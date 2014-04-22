using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace DebugOS.Bochs
{
    internal delegate void AddressEventHandler(Address address);

    internal sealed partial class BochsConnector
    {
        /* STDOUT Regexes */
        /* ========================================================================================= */
        const string addr = @"0x[\da-fA-F_]+";
        static readonly Regex counterRegex  = new Regex(@"Next at t=.*");
        static readonly Regex registerRegex = new Regex(@"(\w+?)\s*: (" + addr + ")");
        static readonly Regex stepRegex     = new Regex(@"\(.*\) \[(" + addr + @")\][^:]*:[^:]*:\s*(.*)\s*; (.*)");
        static readonly Regex breakRegex    = new Regex(@"\(.*\) Breakpoint.*\, (" + addr + @").*");
        static readonly Regex memoryRegex   = new Regex(addr + @" <.*\+.*>:\s+(.*)");
        /* ========================================================================================= */

        public event EventHandler Terminated;
        public event AddressEventHandler BreakpointHit;
        public event EventHandler<SteppedEventArgs> Stepped;
        public event EventHandler<RegisterUpdateEventArgs> RegisterUpdated;

        // Holds the bochs process with which to communicate
        private Process bochsProcess;
        // Holds the current set of registers
        public Dictionary<Register, byte[]> registers;

        private List<byte> memoryBuffer;
        private System.Threading.AutoResetEvent memoryReadLock;

        public BochsConnector(Process bochsProcess)
        {
            this.memoryBuffer = new List<byte>();
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

        public byte[] readMemory(Address start, int length, int timeout = 3000)
        {
            if (start.Type == AddressType.Logical)
            {
                throw new NotImplementedException();
            }
            int carry;
            int count = Math.DivRem(length, 4, out carry) + (carry == 0 ? 0 : 1);

            if (count == 0) return new byte[0];

            StringBuilder cmdBuilder = new StringBuilder();
            cmdBuilder.Append(start.Type == AddressType.Physical ? "xp /" : "x /");
            cmdBuilder.Append(count);
            cmdBuilder.Append("wx ");
            cmdBuilder.Append(Utils.GetHexString((ulong)start.Value, prefix: true));

            int    index  = 0;
            byte[] buffer = new byte[count * 4];

            Stopwatch sw = new Stopwatch();
            sw.Start();
            lock (this)
            {
                this.memoryReadLock = new System.Threading.AutoResetEvent(false);
                this.writeLine(cmdBuilder.ToString());

                while (index < length)
                {
                    if (sw.ElapsedMilliseconds > timeout) {
                        throw new TimeoutException();
                    }

                    this.memoryReadLock.WaitOne(timeout);

                    byte[] data;
                    lock (this.memoryBuffer)
                    {
                        this.memoryReadLock.Reset();

                        // Take a copy of the buffer
                        lock (this.memoryBuffer) { data = this.memoryBuffer.ToArray(); }
                    }
                    Array.Copy(data, 0, buffer, index, data.Length);
                    index += data.Length;
                }
                sw.Stop();
            }
            Array.Resize(ref buffer, length);
            return buffer;
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
                if ((match = stepRegex.Match(line)).Success)
                    tryHandleStep(match);

                else if ((match = breakRegex.Match(line)).Success)
                    tryHandleBreak(match);
                else if ((match = memoryRegex.Match(line)).Success)
                    tryHandleMemory(match);

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

        void tryHandleMemory(Match match)
        {
            string[] addresses = match.Groups[1].Value.Split('\t', ' ');

            lock (this.memoryBuffer)
            {
                this.memoryBuffer.Clear();

                foreach (String hex in addresses)
                {
                    for (int i = hex.Length; i != 2; i -= 2) {
                        this.memoryBuffer.Add(Utils.ParseHex8(hex[i - 1] + "" + hex[i]));
                    }
                }
            }
            this.memoryReadLock.Set();
            System.Threading.Thread.Yield();
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
                for (int i = 2; i < value.Length; i += 2)
                {
                    if (value[i] == '_') { i -= 1; continue; }

                    data.Add(Utils.ParseHex8(value[i] + "" + value[i + 1]));
                }
                this.registers[reg] = data.ToArray();

                return reg;
            }
            catch (Exception) { return null; }
        }

        #endregion
    }
}
