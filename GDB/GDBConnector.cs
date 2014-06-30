using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace DebugOS.GDB
{
    public sealed class GDBConnector : IDisposable
    {
        private byte[] buffer;
        private Stream stream;
        private bool   confirmReceipt;

        public bool SupportsStep { get; private set; }
        public bool SupportsContinue { get; private set; }

        //public event EventHandler<string> 

        public GDBConnector(Stream stream)
        {
            if (!stream.CanRead || !stream.CanWrite)
            {
                throw new ArgumentException("Stream must have read and write access.");
            }
            this.stream = stream;
        }

        public bool SetBreakpoint(long address)
        {
            if (SendMessage(new Message("Z0,{0:X},1")))
            {
                return ReceiveMessage().IsOK;
            }
            else return false;            
        }

        public bool ClearBreakpoint(long address)
        {
            //if (SendMessage(new Message
            throw new NotImplementedException();
        }

        public string ReadRegisters()
        {
            if (!SendMessage(new Message("g")))
            {
                throw new IOException();
            }

            var response = ReceiveMessage();

            if (response.IsError)
            {
                throw new Exception(String.Format(
                    "An error occured while attempting to read registers via a GDB stub: E{0:X}",
                    response.ErrorCode));
            }
            else return response.Data;
        }

        public bool WriteRegisters(string data)
        {
            if (!SendMessage(new Message("G " + data)))
            {
                return false;
            }
            return ReceiveMessage().IsOK;
        }

        public string ReadMemory(long address, int length)
        {
            if (!SendMessage(new Message(String.Format("m {0:X},{1:X}", address, length))))
            {
                throw new IOException();
            }

            var response = ReceiveMessage();

            if (response.IsError)
            {
                throw new Exception(String.Format(
                    "An error occured while attempting to read memory via a GDB stub: E{0:X}",
                    response.ErrorCode));
            }
            else return response.Data;
        }

        public bool Continue()
        {
            if (!this.SupportsContinue) throw new NotSupportedException();

            return SendMessage(new Message("vCont;c"));
        }

        public bool Step()
        {
            if (!this.SupportsStep) throw new NotSupportedException();

            return SendMessage(new Message("vCont;s"));
        }

        public void Disconnect()
        {
            try { SendMessage(new Message("k")); }
            catch { }
            try { stream.Close(); }
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

        private bool SendMessage(Message msg)
        {
            // Send data
            stream.Write(msg.GetMessageBytes());

            // Receive confirmation
            if (this.confirmReceipt)
            {
                lock (this.buffer)
                {
                    int recv = stream.Read(this.buffer);
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

        private Message ReceiveMessage()
        {
            lock (this.buffer)
            {
                try
                {
                    var msg = new Message(this.buffer, 0, stream.Read(this.buffer));
                    stream.Write(new byte[] { (byte)'+' });

                    return msg;
                }
                catch (FormatException)
                {
                    stream.Write(new byte[] { (byte)'-' });
                    throw;
                }
            }
        }

        public void Dispose()
        {
            this.stream.Dispose();
        }
    }
}
