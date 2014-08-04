using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace DebugOS.GDB
{
    public class GDBDebugger : IDebugger
    {
        private GDBConnector connector;

        private List<Breakpoint>     breakpoints;
        private List<ObjectCodeFile> objectFiles;

        private Dictionary<Register, byte[]> registers;

        public bool CanReadMemory  { get { return true; } }
        public bool CanWriteMemory { get { return false; } }

        public long           CurrentAddress      { get; private set; }
        public Architecture   CurrentArchitecture { get; private set; }
        public DebugStatus    CurrentStatus       { get; private set; }
        public ObjectCodeFile CurrentObjectFile   { get; private set; }
        public CodeUnit       CurrentCodeUnit     { get; private set; }
        public Breakpoint     CurrentBreakpoint   { get; private set; }

        public RegisterSet Registers { get; private set; }

        public event EventHandler Suspended;
        public event EventHandler Continued;
        public event EventHandler Disconnected;
        public event EventHandler ArchitectureChanged;

        public event EventHandler<RegistersChangedEventArgs> RefreshRegisters;

        public GDBDebugger()
        {
            string gdbHost = Application.Session.Properties["GDBDebugger.Host"];
            int    gdbPort = int.Parse(Application.Session.Properties["GDBDebugger.Port"]);

            // Initialise locals
            this.breakpoints = new List<Breakpoint>();
            this.objectFiles = new List<ObjectCodeFile>();

            // Set the current architecture to the session one
            this.CurrentArchitecture = Application.Session.Architecture;

            // Connect to GDB stub
            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            sock.Connect(gdbHost, gdbPort);

            // Make empty initial register listing
            this.Registers  = new RegisterSet(new Register[0], new bool[0], new bool[0]);

            // Create connector and begin debugging
            this.connector = new GDBConnector(sock);
            try
            {
                this.connector.EstablishConnection();
                this.CurrentStatus = DebugStatus.Suspended;

                this.connector.Paused += OnGDBPaused;
            }
            catch
            {
                this.CurrentStatus = DebugStatus.Disconnected;
                this.connector.Dispose();
                throw;
            }
        }

        private void OnGDBPaused(StopReply response)
        {
            this.CurrentBreakpoint = null;
            this.CurrentObjectFile = null;

            // Update Registers
            this.registers = this.connector.ReadRegisters();
            var regs       = this.registers.Keys.ToArray();
            var trues      = regs.Select((_) => true).ToArray();
            this.Registers = new RegisterSet(regs, trues, new bool[regs.Length]);

            // Fire event
            if (this.RefreshRegisters != null)
            {
                // Do a batch update of all regs
                this.RefreshRegisters(this, new RegistersChangedEventArgs(
                    this.registers.Keys.Select((reg) => reg.Name).ToArray()));
            }

             
            // Update current address
            foreach (var pair in this.registers)
            {
                if (pair.Key.Type == RegisterType.InstructionPointer)
                {
                    this.CurrentAddress = Utils.LongFromBytes(pair.Value);
                    break;
                }
            }

            // Update breakpoint
            foreach (Breakpoint bp in this.breakpoints)
            {
                if (bp.Address == this.CurrentAddress)
                {
                    this.CurrentBreakpoint = bp; break;
                }
            }

            // Update object file
            foreach (ObjectCodeFile file in this.objectFiles)
            {
                if (file.Sections[0].LoadMemoryAddress <= this.CurrentAddress &&
                    file.Sections[0].LoadMemoryAddress + file.Size > this.CurrentAddress)
                {
                    this.CurrentObjectFile = file; break;
                }
            }

            // If object file found, try and load current code unit
            if (this.CurrentObjectFile != null)
            {
                this.CurrentCodeUnit = this.CurrentObjectFile.GetCode(this.CurrentAddress);
            }

            // Fire stepped event
            if (this.Suspended != null) this.Suspended(this, null);

            // Reset any disabled breakpoints
            this.EnableBreakpoints();

            // Qemu gets stuck on the current line unless the breakpoint is disabled
            if (this.CurrentBreakpoint != null)
            {
                connector.ClearBreakpoint(this.CurrentBreakpoint.Address.Value);
                this.CurrentBreakpoint.IsActive = false;
            }
        }

        private void EnableBreakpoints()
        {
            foreach (Breakpoint bp in this.breakpoints)
            {
                if (!bp.IsActive)
                {
                    connector.SetBreakpoint(bp.Address.Value);
                }
            }
        }

        public void Step()
        {
            connector.Step();

            this.CurrentStatus = DebugStatus.Executing;
        }

        public void Continue()
        {
            connector.Continue();

            this.CurrentStatus = DebugStatus.Executing;
        }

        public void Disconnect()
        {
            connector.Disconnect();

            this.CurrentStatus = DebugStatus.Disconnected;
        }

        public void SetBreakpoint(Breakpoint breakpoint)
        {
            if (breakpoint.Address.Type == AddressType.Logical) {
                throw new InvalidOperationException();
            }
            connector.SetBreakpoint(breakpoint.Address.Value);
            this.breakpoints.Add(breakpoint);
        }

        public void ClearBreakpoint(Breakpoint breakpoint)
        {
            if (breakpoint.Address.Type == AddressType.Logical) {
                throw new InvalidOperationException();
            }
            connector.ClearBreakpoint(breakpoint.Address.Value);
            this.breakpoints.Remove(breakpoint);
        }

        public IEnumerable<Breakpoint> Breakpoints
        {
            get { return this.breakpoints.ToArray(); }
        }

        public ObjectCodeFile[] IncludedObjectFiles
        {
            get { return this.objectFiles.ToArray(); }
        }

        public byte[] ReadRegister(string register)
        {
            byte[] data   = this.registers[this.Registers[register]];
            byte[] output = new byte[data.Length];

            data.CopyTo(output, 0);
            return output;
        }

        public void WriteRegister(string register, byte[] data)
        {
            throw new NotImplementedException();
        }

        public void BeginReadMemory(Address address, int length, Action<byte[]> callback)
        {
            if (callback == null) return;

            string mem    = connector.ReadMemory(address.Value, length);
            byte[] output = new byte[mem.Length / 2];

            for (int i = 0; i < output.Length; i++)
            {
                output[i] = Utils.ParseHex8(mem[i*2] + "" + mem[i*2+1]);
            }
            callback(output);
        }

        public void WriteMemory(Address address, byte[] data)
        {
            throw new NotImplementedException();
        }

        public void IncludeObjectFile(ObjectCodeFile file)
        {
            this.objectFiles.Add(file);

            // Set a breakpoint at the EP if it's the primary image
            if (PathComparer.OSIndependentPath.Equals(file.Filepath,
                Application.Session.ImageFilepath))
            {
                var sym = file.SymbolTable.GetSymbol("_start");

                if (sym != null) {
                    this.SetBreakpoint(new Breakpoint(true, sym.Value));
                }
            }

        }

        public void ExcludeObjectFile(ObjectCodeFile file)
        {
            this.objectFiles.Remove(file);
        }
    }
}
