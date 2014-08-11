using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

using AutoResetEvent = System.Threading.AutoResetEvent;

namespace DebugOS.GDB
{
    public sealed class GDBConnector : IDisposable
    {
        private byte[] buffer;
        private Socket socket;
        private bool   ownsSocket;
        private bool   confirmReceipt;

        private Queue<Request> requests;

        public bool SupportsStep { get; private set; }
        public bool SupportsContinue { get; private set; }

        public event Action Terminated;
        public event Action<StopReply> Paused;
        public event Action<string> ConsoleOutput;

        private const int REQUEST_TIMEOUT = 10000;

        public GDBConnector(Socket socket, bool ownsSocket = true)
        {
            this.ownsSocket = ownsSocket;
            this.socket     = socket;
            this.buffer     = new byte[1024];

            // Initially confirm message receipt
            this.confirmReceipt = true;

            this.requests = new Queue<Request>();
        }

        public bool EstablishConnection()
        {
            // Set an initial timeout
            this.socket.ReceiveTimeout = 5000;

            // Get execution capabilities or fail
            try { if (!this.GetCapabilities()) return false; }
            catch { return false; }

            // Now try to go no-confirm
            try {
                if (this.StartNoAckMode()) this.confirmReceipt = false;
            }
            catch { }

            // Begin message receive loop
            try
            {
                this.socket.BeginReceive(this.buffer, 0, this.buffer.Length,
                    SocketFlags.None, this.MessageLoop, null);
            }
            catch { return false; }

            // We're good to go
            return true;
        }

        public bool SetBreakpoint(long address)
        {
            return this.SendRequest(new Message(String.Format("Z0,{0:X},1", address)), true);
        }

        public bool ClearBreakpoint(long address)
        {
            return this.SendRequest(new Message(String.Format("z0,{0:X},1", address)), true);
        }

        public bool Continue()
        {
            if (!this.SupportsContinue) throw new NotSupportedException();

            return this.SendRequest(new Message("vCont;c"), false);
        }

        public bool Step()
        {
            if (!this.SupportsStep) throw new NotSupportedException();

            return this.SendRequest(new Message("vCont;s:1"), false);
        }

        public Dictionary<Register, byte[]> ReadRegisters()
        {
            var request = new ReadRegistersRequest(new Message("g"));

            this.SendRequest(request);

            if (request.Response.IsError)
            {
                throw new Exception(String.Format(
                    "An error occured while attempting to read registers via a GDB stub: E{0:X}",
                    request.Response.ErrorCode));
            }
            else return request.Result;
        }

        public bool WriteRegisters(string data)
        {
            Request request = null;

            if (!this.SendRequest(new Message("G " + data), false) || request == null)
            {
                return false;
            }
            return request.Response.IsOK;
        }

        public string ReadMemory(long address, int length)
        {
            Request request;

            if (!this.SendRequest(new Message(
                String.Format("m {0:X},{1:X}", address, length)), true, out request))
            {
                throw new IOException();
            }

            if (request.Response.IsError)
            {
                throw new Exception(String.Format(
                    "An error occured while attempting to read memory via a GDB stub: E{0:X}",
                    request.Response.ErrorCode));
            }
            else return request.Response.Data;
        }

        public void Disconnect()
        {
            try { this.SendMessage(new Message("k")); }
            catch { }
            try { this.socket.Close(); }
            catch { }
        }

        private bool GetCapabilities()
        {
            if (SendMessage(new Message("vCont?")))
            {
                string[] caps = ReceiveMessage().Data.Split(';');

                if (caps[0] != "vCont")
                {
                    return false;
                }
                else
                {
                    this.SupportsContinue = caps.Contains("c");
                    this.SupportsStep = caps.Contains("s");
                }
            }
            else return false;

            return true;
        }

        private bool StartNoAckMode()
        {
            if (SendMessage(new Message("QStartNoAckMode")))
            {
                return ReceiveMessage().IsOK;
            }
            else return false;
        }

        /// <summary>
        /// Synchronously sends a message to the connected GDB stub. This cannot
        /// be called while the message loop is active.
        /// </summary>
        /// <param name="msg">The message to send.</param>
        /// <returns>Receipt acknowledgemenet. True if no-ack enabled.</returns>
        private bool SendMessage(Message msg)
        {
            // Send data
            socket.Send(msg.GetMessageBytes());

            // Receive confirmation
            if (this.confirmReceipt)
            {
                lock (this.buffer)
                {
                    int recv = socket.Receive(this.buffer, 0, 1, SocketFlags.None);
                    if (recv != 1) throw new IOException();

                    switch ((char)this.buffer[0])
                    {
                        case '+': return true;
                        case '-': return false;
                        default: throw new IOException();
                    }
                }
            }
            else return true;
        }

        /// <summary>
        /// Sends a message and blocks until the response is complete or
        /// the timeout period passes.
        /// </summary>
        /// <param name="msg">The message to send.</param>
        /// <param name="hasData">Whether the response has data.</param>
        /// <returns>True if sent and acknowledged. False otherwise.</returns>
        private bool SendRequest(Message msg, bool hasData)
        {
            Request _;
            return this.SendRequest(msg, hasData, out _);
        }
        /// <summary>
        /// Sends a message and blocks until the response is complete or
        /// the timeout period passes. Dispose will be called on the resulting
        /// request.
        /// </summary>
        /// <param name="msg">The message to send.</param>
        /// <param name="hasData">Whether the response has data.</param>
        /// <param name="req">The request if completed.</param>
        /// <returns>True if sent and acknowledged. False otherwise.</returns>
        private bool SendRequest(Message msg, bool hasData, out Request req)
        {
            using (req = new Request(msg, new AutoResetEvent(false), hasData))
            {
                return this.SendRequest(req);
            }
        }
        /// <summary>
        /// Sends a message and blocks until the response is complete or
        /// the timeout period passes.
        /// </summary>
        /// <param name="req">The request to submit.</param>
        /// <returns>True if sent and, if blocking, acknowledged. False otherwise.</returns>
        private bool SendRequest(Request req)
        {
            // Lock the request queue
            Monitor.Enter(this.requests);
            
            // Queue message for sending
            this.requests.Enqueue(req);

            // If queue was empty, send manually
            if (this.requests.Count == 1)
            {
                // Send message
                socket.Send(req.Message.GetMessageBytes());
                req.Sent = true;
            }

            // Unlock request queue
            Monitor.Exit(this.requests);

            // Wait for response event
            if (req.Lock != null)
            { 
                req.Lock.WaitOne(REQUEST_TIMEOUT);

                if (this.confirmReceipt)
                {
                    return req.Acknowledgement.GetValueOrDefault();
                }
            }
            return true;
        }

        /// <summary>
        /// Synchronously receives a message from the GDB stub. This cannot
        /// be called while the message loop is active.
        /// </summary>
        /// <returns>The received message.</returns>
        private Message ReceiveMessage()
        {
            lock (this.buffer)
            {
                try
                {
                    var msg = new Message(this.buffer, 0, socket.Receive(this.buffer));
                    socket.Send(new byte[] { (byte)'+' });

                    return msg;
                }
                catch (FormatException)
                {
                    socket.Send(new byte[] { (byte)'-' });
                    throw;
                }
            }
        }

        private void HandleStopReply(Message msg)
        {
            if (msg.Data[0] == 'O')
            {
                // It's actually just console output
                if (this.ConsoleOutput != null)
                {
                    // Use ThreadPool so that we don't interrupt the message loop thread
                    ThreadPool.QueueUserWorkItem((o) => 
                        this.ConsoleOutput(msg.Data.Remove(0, 1)));
                }
            }
            else if (msg.Data[0] == 'F')
            {
                return; // Ignore system call requests
            }
            else // We have an actual stop reply
            {
                StopReply reply = new StopReply(msg.Data);

                if (reply.Reason == StopReason.Trap || reply.Reason == StopReason.Watchpoint)
                {
                    if (this.Paused != null)
                    {
                        // Use ThreadPool so that we don't interrupt the message loop thread
                        ThreadPool.QueueUserWorkItem((o) => this.Paused(reply));
                    }
                }
                else if (reply.Reason == StopReason.Termination)
                {
                    if (this.Terminated != null)
                    {
                        // Use ThreadPool so that we don't interrupt the message loop thread
                        ThreadPool.QueueUserWorkItem((o) => this.Terminated());
                    }
                }
            }
        }

        private void MessageLoop(IAsyncResult token)
        {
            // End receive
            int received = this.socket.EndReceive(token);

            // Yup. This is a goto. For real. Deal with it.
            // If we've received nothing, skip all processing.
            if (received == 0) goto beginReceive;

            bool? ack    = null;
            int   offset = 0;

            // Look for +/- acknowledgements
            if (this.confirmReceipt)
            {
                for (int i = 0; i < received; i++)
                {
                    if (buffer[i] == '+' || buffer[i] == '-')
                    {
                        // If we've already parsed an acknowledgement
                        if (ack.HasValue)
                        {
                            throw new Exception("Double ack in GDB message loop. " +
                            "Please report this to the development team.");
                        }
                        // Set ack accordingly
                        ack = buffer[i] == '+';
                        // Increment offset
                        offset++;
                    }
                    else break;
                }
                // Missing acknowledgements should result in timeouts in requests or 
                // null ack values.
            }

            // Lock request queue
            Monitor.Enter(this.requests);

            // Set the acknowledgement
            if (ack != null && this.requests.Count != 0)
            {
                this.requests.Peek().Acknowledge(ack.Value);
            }

            // Now look for response data
            if (received - offset > 0)
            {
                try
                {
                    var msg = new Message(this.buffer, offset, received);

                    // Send acknowledgement
                    if (this.confirmReceipt) { socket.Send(new byte[] { (byte)'+' }); }

                    // Check for stop replies - we handle those differently
                    if (msg.IsStopReply)
                    {
                        this.HandleStopReply(msg);
                    }
                    else // Handle request. Disconnect on error.
                    {
                        this.requests.Dequeue().HandleResponse(msg);
                    }
                }
                catch (FormatException)
                {
                    // Send acknowledgement
                    if (this.confirmReceipt) { socket.Send(new byte[] { (byte)'-' }); }

                    // Disconnect and throw
                    Monitor.Exit(this.requests);
                    this.Disconnect();
                }
                catch { Monitor.Exit(this.requests); this.Disconnect(); throw; }
            }
            // Handle requests without response datak
            else if (ack != null && this.requests.Count != 0 && !this.requests.Peek().HasResponseData)
            {
                // Handle request. Disconnect on error.
                try   { this.requests.Dequeue().HandleResponse(null); }
                catch { Monitor.Exit(this.requests); this.Disconnect(); throw; }
            }

            try
            {
                // Check for request to send
                if (requests.Count != 0 && !requests.Peek().Sent)
                {
                    this.socket.Send(requests.Peek().Message.GetMessageBytes());
                    requests.Peek().Sent = true;
                }
            }
            finally
            {
                // Unlock request queue
                Monitor.Exit(this.requests);
            }

            // Begin receiving again
        beginReceive:
            this.socket.BeginReceive(this.buffer, 0, this.buffer.Length,
                    SocketFlags.None, this.MessageLoop, null);
        }

        public void Dispose()
        {
            if (this.ownsSocket)
            {
                this.socket.Close();
            }
        }
    }
}
