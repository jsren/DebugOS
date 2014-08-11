/* IDebugger.cs - (c) James S Renwick 2013-2014 
 * --------------------------------------------
 * Version 1.7.0
 */
using System;
using System.Collections.Generic;

namespace DebugOS
{
    /// <summary>
    /// Interface defining the basic implementation set of a debugger.
    /// </summary>
    public interface IDebugger
    {
        /// <summary>
        /// Gets an BreakpointCollection containing the breakpoints currently set on the 
        /// debugger.
        /// </summary>
        BreakpointCollection Breakpoints { get; }
        /// <summary>
        /// Gets whether the debugger can read blocks of the debuggee's RAM.
        /// </summary>
        bool CanReadMemory { get; }
        /// <summary>
        /// Gets whether the debugger can write blocks of the debuggee's RAM.
        /// </summary>
        bool CanWriteMemory { get; }
        /// <summary>
        /// Gets the address of the current instruction. This returns an undefined value
        /// while the debugger is not in a suspended state.
        /// </summary>
        long CurrentAddress { get; }
        /// <summary>
        /// Gets the architecture under which the debugger is currently executing.
        /// This only applies while the debugger is in a suspended state.
        /// </summary>
        Architecture CurrentArchitecture { get; }
        /// <summary>
        /// Gets the breakpoint defined at the current instruction, or null if none
        /// is present. This returns an undefined value while the debugger is not
        /// in a suspended state.
        /// </summary>
        Breakpoint CurrentBreakpoint { get; }
        /// <summary>
        /// Gets the current code unit, if applicable. Returns null (Nothing in Visual Basic) 
        /// otherwise. This should only be accessed while debugging is suspended.
        /// </summary>
        CodeUnit CurrentCodeUnit { get; }
        /// <summary>
        /// Gets the current object file, if applicable. Returns null (Nothing in Visual Basic)
        /// otherwise. This should only be accessed while debugging is suspended.
        /// </summary>
        ObjectCodeFile CurrentObjectFile { get; }
        /// <summary>
        /// Gets the debugger's current status. This can be accessed at any time.
        /// </summary>
        DebugStatus CurrentStatus { get; }
        /// <summary>
        /// Gets the set of available registers as specified by the current architecture.
        /// Allows querying accessibility for each register.
        /// </summary>
        RegisterSet Registers { get; }

        /// <summary>
        /// Occurrs when execution of the debuggee is continued.
        /// </summary>
        event EventHandler Continued;
        /// <summary>
        /// Occurrs when the debugger has been disconnected.
        /// </summary>
        event EventHandler Disconnected;
        /// <summary>
        /// Occurrs when the debugger has been suspended by a step,
        /// breakpoint or other trap.
        /// </summary>
        event EventHandler Suspended;
        /// <summary>
        /// Occurrs when the execution architecture has changed.
        /// </summary>
        event EventHandler ArchitectureChanged;
        /// <summary>
        /// Occurrs when one or more debuggee register values have been changed.
        /// </summary>
        event EventHandler<RegistersChangedEventArgs> RefreshRegisters;
        /// <summary>
        /// Occurrs when a new breakpoint has been set on the debugger.
        /// </summary>
        event EventHandler<BreakpointChangedEventArgs> BreakpointSet;
        /// <summary>
        /// Occurrs when an existing breakpoint has been cleared on the debugger.
        /// </summary>
        event EventHandler<BreakpointChangedEventArgs> BreakpointCleared;

        /// <summary>
        /// Clears the given breakpoint if set.
        /// </summary>
        /// <param name="address">The address of the breakpoint to clear.</param>
        void ClearBreakpoint(Address address);
        /// <summary>
        /// Continues execution until a breakpoint or other trap is hit
        /// or the debugging is terminated.
        /// </summary>
        void Continue();
        /// <summary>
        /// Forcibly ceases debugging and disconnects the debugger.
        /// </summary>
        void Disconnect();
        /// <summary>
        /// Attempts to read the contents of a debuggee register.
        /// </summary>
        /// <param name="register">The name of the register from which to read.</param>
        /// <returns>A byte array containing the contents of the register.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when the requested register cannot be read from.
        /// </exception>
        /// <exception cref="System.Exception">
        /// Thrown when the given register is not present.
        /// </exception>
        byte[] ReadRegister(string register);
        /// <summary>
        /// Asynchronously reads the contents of a section of debuggee RAM.
        /// The result will be delivered by the callback.
        /// </summary>
        /// <param name="address">The base address of the memory section.</param>
        /// <param name="length">The length, in bytes, of the memory to read.</param>
        /// <param name="callback">The callback to execute once the memory has been read.</param>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when the debuggee memory cannot be read from at this time.
        /// </exception>
        void BeginReadMemory(Address address, int length, Action<byte[]> callback);
        /// <summary>
        /// Sets the given breakpoint on the debugger.
        /// </summary>
        /// <param name="address">The address at which to set the breakpoint.</param>
        /// <returns>An object for interacting with the breakpoint.</returns>
        Breakpoint SetBreakpoint(Address address);
        /// <summary>
        /// Performs a single forwards step, executing an instrution and 
        /// returning to the debugger.
        /// </summary>
        void Step();
        /// <summary>
        /// Writes to the contents of a section of debuggee RAM.
        /// </summary>
        /// <param name="address">The base address of the memory section.</param>
        /// <param name="data">The byte array to write to the section.</param>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when the debuggee memory cannot be written to at this time.
        /// </exception>
        void WriteMemory(Address address, byte[] data);
        /// <summary>
        /// Attempts to write to the contents of a debuggee register.
        /// </summary>
        /// <param name="register">The name of the register to which to write.</param>
        /// <param name="data">A byte array containing the data to write.</param>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when the requested register cannot be written to.
        /// </exception>
        /// <exception cref="System.Exception">
        /// Thrown when the given register is not present.
        /// </exception>
        void WriteRegister(string register, byte[] data);
    }
}
