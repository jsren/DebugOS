﻿using System;
using System.Linq;

namespace DebugOS
{
    /// <summary>
    /// An object representing a single line of assembly.
    /// </summary>
    public sealed class AssemblyLine
    {
        /// <summary>The syntax of the assembly instruction.</summary>
        public AssemblySyntax Syntax { get; private set; }

        /// <summary>The offset in bytes of the instruction within the object file.</summary>
        public uint Offset { get; private set; }
        /// <summary>The disassembled instruction.</summary>
        public String Instruction { get; private set; }
        /// <summary>The disassembled parameters of the instruction.</summary>
        public String[] Parameters  { get; private set; }
        /// <summary>The line's raw machine code.</summary>
        public byte[] MachineCode { get; private set; }

        /// <summary>
        /// Gets the string source of the assembly.
        /// </summary>
        /// <returns>A string containing the assembly source.</returns>
        public override string ToString() {
            return Instruction + '\t' + string.Join(",", this.Parameters.Select(param => param.ToString()));
        }

        /// <summary>
        /// Creates a new assembly line object.
        /// </summary>
        /// <param name="Syntax">The syntax of the assembly instruction.</param>
        /// <param name="Offset">The offset in bytes of the instruction within the object file.</param>
        /// <param name="Instruction">The disassembled instruction.</param>
        /// <param name="Parameters">The disassembled parameters of the instruction.</param>
        /// <param name="MachineCode">The line's raw machine code.</param>
        public AssemblyLine(AssemblySyntax Syntax, uint Offset, 
            String Instruction, String[] Parameters, byte[] MachineCode)
        {
            this.Syntax      = Syntax;
            this.Offset      = Offset;
            this.Instruction = Instruction;
            this.Parameters  = Parameters;
            this.MachineCode = MachineCode;
        }
    }
}
