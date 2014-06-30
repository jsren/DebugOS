using System;
using System.Collections.Generic;

namespace DebugOS
{
    public interface IDebugger
    {
        void Step();
        void StepOver();
        void Continue();
        void Disconnect();

        void SetBreakpoint(Breakpoint breakpoint);
        void ClearBreakpoint(Breakpoint breakpoint);

        Register[] AvailableRegisters { get; }
        IEnumerable<Breakpoint> Breakpoints { get; }
        ObjectCodeFile[] IncludedObjectFiles { get; }

        byte[] ReadRegister(Register register);
        void WriteRegister(Register register, byte[] data);

        void BeginReadMemory(Address address, int length, Action<UInt32[]> callback);
        void WriteMemory(Address address, byte[] data);

        void IncludeObjectFile(ObjectCodeFile file);
        void ExcludeObjectFile(ObjectCodeFile file);

        CodeUnit CurrentCodeUnit { get; }
        long     CurrentAddress  { get; }

        int AddressWidth { get; }
        string Name { get; }

        ObjectCodeFile CurrentObjectFile { get; }
        Breakpoint     CurrentBreakpoint { get; }
        DebugStatus    CurrentStatus     { get; }

        bool CanReadMemory  { get; }
        bool CanWriteMemory { get; }

        event EventHandler Continued;
        event EventHandler Disconnected;
        event EventHandler<SteppedEventArgs> Stepped;
        event EventHandler<BreakpointHitEventArgs> BreakpointHit;

        event EventHandler<RegisterUpdateEventArgs> RefreshRegister;
        event EventHandler RefreshMemory;
    }
}
