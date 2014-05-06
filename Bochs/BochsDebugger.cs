using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using DebugOS;

using Path = System.IO.Path;
using System.Threading.Tasks;

namespace DebugOS.Bochs
{
    public class BochsDebugger : IDebugger
    {
        private Process        bochsProcess;
        private BochsConnector connector;
        private bool           breakOnModeChange;

        private List<Breakpoint>     breakpoints;
        private List<ObjectCodeFile> codeFiles;

        public bool CanReadMemory  { get { return true; } }
        public bool CanWriteMemory { get { return false; } }

        public long           CurrentAddress    { get; private set; }
        public DebugStatus    CurrentStatus     { get; private set; }
        public ObjectCodeFile CurrentObjectFile { get; private set; }
        public CodeUnit       CurrentCodeUnit   { get; private set; }
        public Breakpoint     CurrentBreakpoint { get; private set; }

        public int AddressWidth { get { return 4; } }

        public event EventHandler Continued;
        public event EventHandler Disconnected;
        public event EventHandler RefreshMemory;

        public event EventHandler<SteppedEventArgs>        Stepped;
        public event EventHandler<BreakpointHitEventArgs>  BreakpointHit;
        public event EventHandler<RegisterUpdateEventArgs> RefreshRegister;

        public BochsDebugger(string BochsPath, string ConfigFile)
        {
            // Initialise locals
            this.breakpoints = new List<Breakpoint>();
            this.codeFiles   = new List<ObjectCodeFile>();

            this.breakpoints.Add(new Breakpoint(false, new Address())); // Add a dead breakpoint to create 1-based indices

            // Configure bochs process
            var startInfo = new ProcessStartInfo()
            {
                CreateNoWindow         = true,
                RedirectStandardOutput = true,
                RedirectStandardInput  = true,
                UseShellExecute        = false,

                FileName = BochsPath,
                Arguments = "-q -f \"" + ConfigFile + '"',
            };
            // Add BOCHSHOME env. var. in case the config needs it
            startInfo.EnvironmentVariables["BOCHSHOME"] = Path.GetDirectoryName(BochsPath);

            // Create bochs process
            this.bochsProcess = new Process() { StartInfo = startInfo };
            this.connector    = new BochsConnector(bochsProcess);

            // Register handlers
            this.connector.Terminated += this.OnBochsTerminated;
            this.connector.Stepped    += this.OnBochsStepped;

            this.connector.RegisterUpdated += this.OnRegistersUpdated;

            // Let's-a go!
            bochsProcess.Start();
            bochsProcess.BeginOutputReadLine();
        }

        void OnRegistersUpdated(object sender, RegisterUpdateEventArgs e)
        {
            if (this.RefreshRegister != null) this.RefreshRegister(this, e);
        }

        public Register[] AvailableRegisters {
            get { return this.connector.registers.Keys.ToArray(); }
        }
        public IEnumerable<Breakpoint> Breakpoints {
            get { return breakpoints; }
        }

        public byte[] ReadRegister(Register register) {
            return this.connector.registers[register];
        }
        public void WriteRegister(Register register, byte[] data) {
            throw new NotImplementedException();
        }

        public void BeginReadMemory(Address address, int length, Action<UInt32[]> callback) {
            this.connector.BeginReadMemory(address, length, callback);
        }
        public void WriteMemory(Address address, byte[] data) {
            throw new NotImplementedException();
        }

        public bool BreakOnCPUModeChange
        {
            get { return this.breakOnModeChange; }
            set
            {
                if (this.breakOnModeChange != value)
                {
                    this.breakOnModeChange = value;
                    this.connector.ToggleBreakOnModeChange();
                }
            }
        }

        // Fired when can no longer read output
        void OnBochsTerminated(object sender, EventArgs e)
        {
            this.CurrentStatus = DebugStatus.Disconnected;

            if (this.Disconnected != null) this.Disconnected(this, null);
        }

        void OnBochsStepped(object sender, SteppedEventArgs e)
        {
            this.CurrentStatus  = DebugStatus.Paused;
            this.CurrentAddress = e.Address.Value; // NB: must be physical
            // Update Current---- Properties
            this.UpdateCurrent(e.Address.Value, e.Assembly);
            // Fire public event
            if (this.Stepped != null) this.Stepped(this, e);
        }

        void AssertPaused()
        {
            if (this.CurrentStatus != DebugStatus.Paused) 
                throw new DebuggerNotPausedException();
        }

        void UpdateCurrent(long address, AssemblyLine line)
        {
            this.CurrentAddress    = address;
            this.CurrentBreakpoint = null;
            this.CurrentObjectFile = null;

            // Update breakpoint
            foreach (Breakpoint bp in this.breakpoints)
            {
                if (bp.Address == address) {
                    this.CurrentBreakpoint = bp; break;
                }
            }

            // Update object file
            foreach (ObjectCodeFile file in this.codeFiles)
            {
                if (file.Sections[0].LoadMemoryAddress <= address &&
                    file.Sections[0].LoadMemoryAddress + file.Size > address)
                {
                    this.CurrentObjectFile = file; break;
                }
            }

            // If object file found, try ant load current code unit
            if (this.CurrentObjectFile != null) {
                this.CurrentCodeUnit = this.CurrentObjectFile.GetCode((uint)address);
            }
            // Use live code unit otherwise
            if (this.CurrentObjectFile == null || this.CurrentCodeUnit == null) 
            {
                var liveUnit = this.CurrentCodeUnit as LiveCodeUnit; // Keep previous live code unit
                if (liveUnit == null) this.CurrentCodeUnit = liveUnit = new LiveCodeUnit();

                liveUnit.AddAssemblyLine(line);
            }
        }


        public void Step()
        {
            this.AssertPaused();
            this.connector.Step(1);
        }
        public void Step(int Count)
        {
            this.AssertPaused();
            this.connector.Step(Count);
        }

        public void StepOver()
        {
            this.AssertPaused();
            // TODO: step over current code line

        }

        public void Continue()
        {
            this.AssertPaused();
            this.connector.Continue();
            if (this.Continued != null) this.Continued(this, null);
        }

        public void Disconnect()
        {
            if (this.CurrentStatus == DebugStatus.Paused) {
                this.connector.Quit();
            }
            else if (this.CurrentStatus == DebugStatus.Executing) {
                this.bochsProcess.Kill();
            }
            this.CurrentStatus = DebugStatus.Disconnected;
        }

        public void SetBreakpoint(Breakpoint Breakpoint)
        {
            this.AssertPaused();

            if (Breakpoint.Address.Type == AddressType.Physical) {
                this.connector.SetPhysicalBreakpoint(Breakpoint.Address.Value);
            }
            else if (Breakpoint.Address.Type == AddressType.Linear) {
                this.connector.SetLinearBreakpoint(Breakpoint.Address.Value);
            }
            else if (Breakpoint.Address.Type == AddressType.Logical) {
                throw new NotImplementedException();
            }

            int index = this.breakpoints.Count;
            this.breakpoints.Add(Breakpoint);

            if (!Breakpoint.IsActive) {
                this.connector.DisableBreakpoint(index);
            }
        }

        public void ClearBreakpoint(Breakpoint Breakpoint)
        {
            this.AssertPaused();

            int index = this.breakpoints.IndexOf(Breakpoint);
            if (index == -1) throw new Exception("Unknown breakpoint, cannot clear");

            this.breakpoints[index].IsActive = false;
            this.connector.ClearBreakpoint(index);
        }

        public void IncludeObjectFile(ObjectCodeFile file) {
            this.codeFiles.Add(file);
        }
    }
}
