using DebugOS.Bochs.Requests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace DebugOS.Bochs
{
    internal delegate void AddressEventHandler(Address address);

    internal sealed partial class BochsConnector
    {
        /* STDOUT Regexes */
        /* ========================================================================================= */
        internal const string addr = @"0x[\da-fA-F_]+";
        static readonly Regex counterRegex  = new Regex(@"Next at t=.*");
        static readonly Regex registerRegex = new Regex(@"(\w+?)\s*: (" + addr + ")");
        static readonly Regex stepRegex     = new Regex(@"\(.*\) \[(" + addr + @")\][^:]*:[^:]*:\s*(.*)\s*; (.*)");
        static readonly Regex breakRegex    = new Regex(@"\(.*\) Breakpoint.*\, (" + addr + @").*");
        
        /* ========================================================================================= */

        public event EventHandler Terminated;
        public event AddressEventHandler BreakpointHit;
        public event EventHandler<SteppedEventArgs> Stepped;
        public event EventHandler<RegisterUpdateEventArgs> RegisterUpdated;

        

        // Holds the bochs process with which to communicate
        private Process bochsProcess;
        // Holds the current set of registers
        public Dictionary<Register, byte[]> registers;
        // Holds the current queue of async requests
        private Queue<IBochsRequest> requests;

        /// <summary>
        /// Gets the handle of the current BOCHS process' main window.
        /// </summary>
        public HandleRef WindowHandle
        { 
            get 
            {
                System.Threading.Thread.Sleep(1000);
                bochsProcess.Refresh();
                return new HandleRef(bochsProcess, bochsProcess.MainWindowHandle);
            } 
        }

        public BochsConnector(Process bochsProcess)
        {
            this.requests     = new Queue<IBochsRequest>();
            this.registers    = new Dictionary<Register, byte[]>();
            this.bochsProcess = bochsProcess;
            this.bochsProcess.OutputDataReceived += handleOutputData;
        }

        // Writes a line to bochs' stdin
        private void writeLine(string command)
        {
            lock (bochsProcess)
            {
                bochsProcess.StandardInput.WriteLine(command);
                bochsProcess.StandardInput.Flush();
            }
        }

        public void Quit()     { this.writeLine("q"); }
        public void Continue() { this.writeLine("continue"); }

        public void Step(int count)
        { 
            this.writeLine("step " + count.ToString());
            this.UpdateRegisters();
        }

        public void UpdateRegisters()
        {
            this.writeLine("regs");
            this.writeLine("sregs");
        }

        public void SetPhysicalBreakpoint(long address) {
            this.writeLine("pb " + address.ToString());
        }
        public void SetLinearBreakpoint(long address) {
            this.writeLine("lb " + address.ToString());
        }
        public void SetVirtualBreakpoint(long address) {
            this.writeLine("vb " + address.ToString());
        }

        public void EnableBreakpoint(int index) {
            this.writeLine("bpe " + index.ToString());
        }

        public void DisableBreakpoint(int index) {
            this.writeLine("bpd " + index.ToString());
        }

        public void ClearBreakpoint(int index) {
            this.writeLine("del " + index.ToString());
        }

        public void ToggleBreakOnModeChange() {
            this.writeLine("modebp");
        }

        public void BeginReadMemory(Address start, int length, Action<UInt32[]> callback)
        {
            if (callback == null) return;

            int carry;
            int count = Math.DivRem(length, 4, out carry) + (carry == 0 ? 0 : 1);

            if (count == 0)
            {
                callback(new UInt32[0]);
                return;
            }

            StringBuilder cmdBuilder = new StringBuilder();
            cmdBuilder.Append(start.Type == AddressType.Physical ? "xp /" : "x /");
            cmdBuilder.Append(count);
            cmdBuilder.Append("wx ");

            if (start.Type == AddressType.Logical)
            {
                switch (start.Segment)
                {
                    case Segment.Code:
                        cmdBuilder.Append("cs:"); break;
                    case Segment.Data:
                        cmdBuilder.Append("ds:"); break;
                    case Segment.Stack:
                        cmdBuilder.Append("ss:"); break;
                    case Segment.Extended1:
                        cmdBuilder.Append("es:"); break;
                    case Segment.Extended2:
                        cmdBuilder.Append("fs:"); break;
                    case Segment.Extended3:
                        cmdBuilder.Append("gs:"); break;
                }
            }

            cmdBuilder.Append(Utils.GetHexString((ulong)start.Value, prefix: true));

            this.requests.Enqueue(new ReadMemoryRequest(count, callback));
            this.writeLine(cmdBuilder.ToString());
        }

        // Primary method for handling bochs communication
        void handleOutputData(object sender, DataReceivedEventArgs e)
        {
            Match match;
            MatchCollection matches;

            // Hit end of stream - program terminated
            if (e.Data == null)
            { 
                if (this.Terminated != null) this.Terminated(this, null);
                return;
            }

            // Split into lines
            string[] lines = e.Data.Split(new char[] { '\n','\r' }, 
                StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
            carry:

                /* Try handling with a request */
                if (this.requests.Count != 0)
                {
                    IBochsRequest req = this.requests.Peek();

                    bool consumed = req.feedLine(line);

                    if (req.isComplete)
                    {
                        req.handleComplete();
                        this.requests.Dequeue();

                        if (consumed) continue;
                        else          goto carry;
                    }
                }

                /* Otherwise parse with dedicated handlers */
                if ((match = stepRegex.Match(line)).Success)
                    tryHandleStep(match);

                else if ((match = breakRegex.Match(line)).Success)
                    tryHandleBreak(match);

                else if ((matches = registerRegex.Matches(line)).Count != 0)
                {
                    foreach (Match m in matches)
                    {
                        Register? r = this.tryHandleRegister(m);

                        if (r.HasValue && this.RegisterUpdated != null) {
                            this.RegisterUpdated(this, new RegisterUpdateEventArgs(r.Value, this.registers[r.Value]));
                        }
                    }
                }
            }
        }

        #region Handlers

        bool tryHandleStep(Match match)
        {
            Address address;
            AssemblyLine asm;
            try
            {
                // Get the execution address
                address = new Address((long)Utils.ParseHex64(match.Groups[1].Value));
                // Get the disassembly
                string disasm = match.Groups[2].Value;

                // Get the raw bytecode
                string bytecode = match.Groups[3].Value;
                byte[] bytes    = new byte[bytecode.Length / 2];

                for (int i = 0; i < bytes.Length; i++) {
                    bytes[i] = Utils.ParseHex8("" + bytecode[i * 2] + bytecode[i * 2 + 1]);
                }

                // Parse out the assembly instruction & operands
                Match    asmMatch = AssemblyLine.AssemblyRegex.Match(disasm);
                string[] args     = asmMatch.Groups[2].Value.Split(',');
                string   meta     = "";

                for (int i = 0; i < args.Length; i++)
                {
                    args[i] = args[i].Trim();

                    int i1 = args[i].IndexOf('<');
                    int i2 = args[i].IndexOf('>');

                    if (i1 != -1 && i2 != -1)
                    {
                        meta = args[i].Substring(i1, 1 + i2 - i1);
                        args[i].Remove(i1, 1 + i2 - i1);
                    }
                }
                asm = new AssemblyLine(AssemblySyntax.Intel, -1, asmMatch.Groups[1].Value, args, bytes, meta);
            }
            catch
            {
                Console.WriteLine("[ERROR] Error parsing bochs step output.");
                return false;
            }

            // Fire event
            if (this.Stepped != null) 
                this.Stepped(this, new SteppedEventArgs(address, asm));

            return true;
        }

        bool tryHandleBreak(Match match)
        {
            Address address;
            try
            {
                // Get the execution address
                address = new Address((long)Utils.ParseHex64(match.Groups[1].Value));
            }
            catch
            {
                Console.WriteLine("[ERROR] Error parsing bochs break output.");
                return false;
            }
            // Fire event
            if (this.BreakpointHit != null) this.BreakpointHit(address);

            return true;
        }

        Register? tryHandleRegister(Match match)
        {
            try
            {
                string name  = match.Groups[1].Value;
                string value = match.Groups[2].Value;

                RegisterType type = RegisterType.GeneralPurpose;

                     if (name.EndsWith("bp")) type = RegisterType.BasePointer;
                else if (name.EndsWith("sp")) type = RegisterType.StackPointer;

                Register reg = new Register(name, (value.Length - 2) / 2, type);

                List<byte> data = new List<byte>(8);
                for (int i = value.Length - 1; i > 2; i -= 2)
                {
                    if (value[i] == '_') { i += 1; continue; }

                    data.Add(Utils.ParseHex8(value[i-1] + "" + value[i]));
                }
                this.registers[reg] = data.ToArray();

                return reg;
            }
            catch (Exception) { return null; }
        }

        #endregion
    }
}
