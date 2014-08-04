﻿/* BochsDebugger.cs - (c) James S Renwick 2014
 * -------------------------------------------
 * Version 1.3.2
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace DebugOS.Bochs
{
    public class BochsDebugger : IDebugger
    {
        // Private variables
        private LiveCodeUnit         liveCodeUnit;
        private BochsConnector       connector;
        private List<Breakpoint>     breakpoints;
        private List<ObjectCodeFile> objectFiles;

        private bool breakOnModeChange; // When true, breaks when the CPU changes mode

        // Public properties
        public bool CanReadMemory  { get { return true; } }
        public bool CanWriteMemory { get { return false; } }

        public long           CurrentAddress      { get; private set; }
        public Architecture   CurrentArchitecture { get; private set; }
        public Breakpoint     CurrentBreakpoint   { get; private set; }
        public CodeUnit       CurrentCodeUnit     { get; private set; }
        public ObjectCodeFile CurrentObjectFile   { get; private set; }
        public DebugStatus    CurrentStatus       { get; private set; }

        public RegisterSet Registers { get; private set; }
        
        
        public event EventHandler Continued;
        public event EventHandler Disconnected;
        public event EventHandler Suspended;
        public event EventHandler ArchitectureChanged;
        
        public event EventHandler<RegistersChangedEventArgs> RefreshRegisters;

        public IEnumerable<Breakpoint> Breakpoints
        {
            get { return this.breakpoints.ToArray(); }
        }
        public ObjectCodeFile[] IncludedObjectFiles
        {
            get { return this.objectFiles.ToArray(); }
        }

        /// <summary>
        /// Initializes and connects to a new Bochs debugger.
        /// </summary>
        public BochsDebugger()
        {
            // Load properties
            string bochsPath  = Application.Session.Properties["BochsDebugger.BochsPath"];
            string configPath = Application.Session.Properties["BochsDebugger.ConfigPath"];

            // Set the current architecture to the session one
            this.CurrentArchitecture = Application.Session.Architecture;

            // Initialise locals
            this.breakpoints  = new List<Breakpoint>();
            this.objectFiles  = new List<ObjectCodeFile>();
            this.liveCodeUnit = new LiveCodeUnit();

            // Make initial register listing with all dismissive
            Register[] regs = this.CurrentArchitecture.Registers;
            this.Registers  = new RegisterSet(regs, new bool[regs.Length], new bool[regs.Length]);

            // Load Bochs and begin
            this.connector = new BochsConnector(bochsPath, configPath);

            // Register handlers
            this.connector.Terminated      += this.OnBochsTerminated;
            this.connector.Stepped         += this.OnBochsStepped;
            this.connector.RegisterUpdated += this.OnRegistersUpdated;
        }




        private void AssertPaused()
        {
            if (this.CurrentStatus != DebugStatus.Suspended)
            {
                throw new DebuggerNotPausedException();
            }
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

        public void ClearBreakpoint(Breakpoint breakpoint)
        {
            this.AssertPaused();

            // Get the breakpoint
            int index = this.breakpoints.IndexOf(breakpoint);
            if (index == -1)
            {
                throw new Exception("Unknown breakpoint, cannot clear");
            }
            // Clear the breakpoint
            this.breakpoints[index].IsActive = false;
            this.connector.ClearBreakpoint(index);
        }

        public void Continue()
        {
            this.AssertPaused();

            // Update status and then continue
            this.CurrentStatus = DebugStatus.Executing;
            this.connector.Continue();

            // Fire event
            if (this.Continued != null) this.Continued(this, null);
        }

        public void Disconnect()
        {
            this.CurrentStatus = DebugStatus.Disconnected;
            this.connector.Disconnect();
        }

        public void ExcludeObjectFile(ObjectCodeFile file)
        {
            if (this.CurrentObjectFile == file)
            {
                this.CurrentObjectFile = null;
            }
            this.objectFiles.Remove(file);
        }

        public void IncludeObjectFile(ObjectCodeFile file)
        {
            if (!this.objectFiles.Contains(file))
            {
                this.objectFiles.Add(file);
            }
        }

        public byte[] ReadRegister(string register)
        {
            this.AssertPaused();

            // Check access before reading
            if (this.Registers.CanRead(register))
            {
                return this.connector.ReadRegister(this.Registers[register]);
            }
            else throw new InvalidOperationException();
        }

        public void BeginReadMemory(Address address, int length, Action<byte[]> callback)
        {
            this.AssertPaused();
            this.connector.BeginReadMemory(address, length, callback);
        }

        public void SetBreakpoint(Breakpoint breakpoint)
        {
            this.connector.SetBreakpoint(breakpoint);
        }

        public void Step()
        {
            this.Step(1);
        }
        public void Step(int count)
        {
            this.AssertPaused();

            this.CurrentStatus = DebugStatus.Executing;
            this.connector.Step(1);
        }

        public void WriteMemory(Address address, byte[] data)
        {
            this.AssertPaused();

            throw new NotImplementedException();
        }

        public void WriteRegister(string register, byte[] data)
        {
            this.AssertPaused();

            throw new NotImplementedException();
        }

        /// <summary>
        /// Called when Bochs is stepped.
        /// </summary>
        private void OnBochsStepped(Address address, AssemblyLine assembly)
        {
            // Update status
            this.CurrentStatus  = DebugStatus.Suspended;
            this.CurrentAddress = address.Value;

            // Give default values for current props
            this.CurrentBreakpoint = null;
            this.CurrentObjectFile = null;
            this.CurrentCodeUnit   = this.liveCodeUnit;

            // Update breakpoint
            foreach (Breakpoint bp in this.breakpoints)
            {
                if (bp.Address == address) {
                    this.CurrentBreakpoint = bp; break;
                }
            }

            // Update object file
            foreach (ObjectCodeFile file in this.objectFiles)
            {
                if (file.Sections[0].LoadMemoryAddress <= address.Value &&
                    file.Sections[0].LoadMemoryAddress + file.Size > address.Value)
                {
                    this.CurrentObjectFile = file; break;
                }
            }

            // If object file found, try and load current code unit
            if (this.CurrentObjectFile != null)
            {
                this.CurrentCodeUnit = this.CurrentObjectFile.GetCode(address.Value);
            }
            // Otherwise, use the live code unit
            else this.liveCodeUnit.AddAssemblyLine(assembly);

            // Fire the event
            if (this.Suspended != null) this.Suspended(this, null);
        }

        /// <summary>
        /// Called when Bochs is terminated.
        /// </summary>
        private void OnBochsTerminated()
        {
            // Update status
            this.CurrentStatus = DebugStatus.Disconnected;

            // Fire the event
            if (this.Disconnected != null)
            {
                this.Disconnected(this, null);
            }
        }

        /// <summary>
        /// Called when one or more registers have been refreshed.
        /// </summary>
        private void OnRegistersUpdated(RegistersChangedEventArgs e)
        {
            Register[] regs = this.connector.AvailableRegisters;

            // Update the register listing
            this.Registers = new RegisterSet(regs, regs.Select((_)=>true).ToArray(), 
                new bool[regs.Length]);

            if (this.RefreshRegisters != null)
            {
                this.RefreshRegisters(this, e);
            }
        }
    }
}
