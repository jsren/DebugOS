/* BochsConnector.cs (c) James S Renwick 2013
 * ------------------------------------------
 * Version 1.6.6
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using DebugOS.Bochs.Requests;

namespace DebugOS.Bochs
{
    internal sealed partial class BochsConnector
    {
        /* STDOUT Regexes */
        /* ========================================================================================= */
        internal const string addr = @"(?:0x)?[\da-fA-F_]+";
        static readonly Regex counterRegex  = new Regex(@"Next at t=.*");
        static readonly Regex registerRegex = new Regex(@"(\w+?)\s*: (" + addr + ")");
        static readonly Regex stepRegex     = new Regex(@"\(.*\) \[(" + addr + @")\][^:]*:[^:]*:\s*(.*)\s*; (.*)");
        static readonly Regex breakRegex    = new Regex(@"\(.*\) Breakpoint.*\, (" + addr + @").*");
        
        /* ========================================================================================= */

        public event Action                            Terminated;
        public event Action<Address, AssemblyLine>     Stepped;
        public event Action<MemoryReadEventArgs>       MemoryRead;
        public event Action<RegistersChangedEventArgs> RegisterUpdated;
        

        private Process                      bochsProcess;
        private Queue<IBochsRequest>         requests;
        private Dictionary<Register, byte[]> registers;

        /// <summary>
        /// Gets the handle of the current BOCHS process' main window.
        /// </summary>
        public HandleRef WindowHandle
        { 
            get 
            {
                System.Threading.Thread.Sleep(1000); // Sleep for a bit to improve chances
                bochsProcess.Refresh();
                return new HandleRef(bochsProcess, bochsProcess.MainWindowHandle);
            } 
        }

        /// <summary>
        /// Creates a new Bochs connector.
        /// </summary>
        public BochsConnector(string bochsPath, string configPath)
        {
            // Initialise locals
            this.requests  = new Queue<IBochsRequest>();
            this.registers = new Dictionary<Register, byte[]>();

            // Configure bochs process
            var startInfo = new ProcessStartInfo()
            {
                CreateNoWindow         = true,
                RedirectStandardOutput = true,
                RedirectStandardInput  = true,
                UseShellExecute        = false,

                FileName  = bochsPath,
                Arguments = "-q -f \"" + configPath + '"',
            };
            // Add BOCHSHOME env. var. in case the config needs it
            startInfo.EnvironmentVariables["BOCHSHOME"] = Path.GetDirectoryName(bochsPath);

            // Create bochs process
            this.bochsProcess = new Process() { StartInfo = startInfo };
            this.bochsProcess.OutputDataReceived += handleOutputData;

            // Let's-a go!
            bochsProcess.Start();
            bochsProcess.BeginOutputReadLine();
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

        private string GetAddressString(Address address)
        {
            if (address.Type != AddressType.Logical)
            {
                string output = String.Empty;
                switch (address.Segment)
                {
                    case Segment.Code:
                        output = "cs:"; break;
                    case Segment.Data:
                        output = "ds:"; break;
                    case Segment.Stack:
                        output = "ss:"; break;
                    case Segment.Extended1:
                        output = "es:"; break;
                    case Segment.Extended2:
                        output = "fs:"; break;
                    case Segment.Extended3:
                        output = "gs:"; break;
                }
                return output + address.Value.ToString();
            }
            else return address.Value.ToString();
        }

        public Register[] AvailableRegisters
        {
            get { return this.registers.Keys.ToArray(); }
        }

        public void Quit()     { this.writeLine("q"); }
        public void Continue() { this.writeLine("continue"); }

        public void Step(int count)
        { 
            this.writeLine("step " + count.ToString());
        }

        public void UpdateRegisters()
        {
            this.writeLine("regs");
            this.writeLine("sregs");
        }

        public void SetBreakpoint(Address address)
        {
            // Get the address string
            string addressStr = this.GetAddressString(address);

            if (address.Type == AddressType.Physical)
            {
                this.writeLine("pb " + addressStr);
            }
            else if (address.Type == AddressType.Logical)
            {
                this.writeLine("lb " + addressStr);
            }
            else this.writeLine("vb " + addressStr);
        }

        public void EnableBreakpoint(int index) {
            this.writeLine("bpe " + (index+1).ToString());
        }

        public void DisableBreakpoint(int index) {
            this.writeLine("bpd " + (index + 1).ToString());
        }

        public void ClearBreakpoint(int index) {
            this.writeLine("del " + (index+1).ToString());
        }

        public void ToggleBreakOnModeChange() {
            this.writeLine("modebp");
        }

        public void Disconnect()
        {
            this.Quit();
            this.bochsProcess.Kill();
        }

        public byte[] ReadRegister(Register register)
        {
            byte[] value  = this.registers[register];
            byte[] output = new byte[value.Length];

            value.CopyTo(output, 0);
            return output;
        }

        public void BeginReadMemory(Address start, int length, Action<byte[]> callback)
        {
            // If empty callback, just return
            if (callback == null) { return; }

            // If no bytes requested, just return
            else if (length == 0)
            {
                callback(new byte[0]);
                return;
            }

            // Build the Bochs command
            StringBuilder cmdBuilder = new StringBuilder();
            cmdBuilder.Append(start.Type == AddressType.Physical ? "xp /" : "x /");
            cmdBuilder.Append(length);
            cmdBuilder.Append("xb ");

            cmdBuilder.Append(GetAddressString(start));

            this.requests.Enqueue(new ReadMemoryRequest(length, callback));
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
                if (this.Terminated != null)
                {
                    ThreadPool.QueueUserWorkItem((o) => this.Terminated());
                }
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
                    List<string> regsToUpdate = new List<string>();

                    foreach (Match m in matches)
                    {
                        // Try and parse register
                        Register reg = this.tryHandleRegister(m);

                        // Add name if successful
                        if (reg != null) regsToUpdate.Add(reg.Name);
                    }
                    // Fire event
                    if (this.RegisterUpdated != null)
                    {
                        this.RegisterUpdated(new RegistersChangedEventArgs(
                            regsToUpdate.ToArray()));
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

            // Update regs
            this.UpdateRegisters();

            // Fire event
            if (this.Stepped != null) this.Stepped(address, asm);

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
            return true;
        }

        Register tryHandleRegister(Match match)
        {
            if (Application.Debugger == null) return null;

            try
            {
                string name  = match.Groups[1].Value;
                string value = match.Groups[2].Value;

                Register reg;

                // Create or get existing register
                if (!Application.Debugger.Registers.HasRegister(name))
                {
                    RegisterType type = RegisterType.GeneralPurpose;

                         if (name.EndsWith("bp")) type = RegisterType.FramePointer;
                    else if (name.EndsWith("sp")) type = RegisterType.StackPointer;

                    reg = new Register(name, (value.Length - 2) / 2, type);
                }
                else reg = Application.Debugger.Registers[name];

                // Sanitize the value for easier parsing
                value = Utils.SanitizeHex(value);

                List<byte> data = new List<byte>(8);
                for (int i = value.Length - 1; i >= 1; i -= 2)
                {
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
