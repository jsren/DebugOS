﻿using System;
using System.Linq;

namespace DebugOS
{
    /// <summary>
    /// An object representing a set of instructions - generally a function.
    /// </summary>
    public sealed class CodeUnit
    {
        /// <summary>The size in bytes of the code source.</summary>
        public int Size { get; private set; }
        /// <summary>The offset within the object file at which the source is present.</summary>
        public uint Offset { get; private set; }
        /// <summary>The name of the source.</summary>
        public String Name { get; private set; }
        /// <summary>The source's code.</summary>
        public CodeLine[] Lines { get; private set; }

        /// <summary>
        /// Creates a code unit object.
        /// </summary>
        /// <param name="Offset">The size in bytes of the code source.</param>
        /// <param name="Name">The offset within the object file at which the source is present.</param>
        /// <param name="Lines">The source's code.</param>
        public CodeUnit(uint Offset, String Name, CodeLine[] Lines)
        {
            this.Offset = Offset;
            this.Name   = Name;
            this.Lines  = Lines;
            this.Size   = this.Lines.Sum(line => line.Size);
        }

        /// <summary>
        /// Gets the code line at the specified address.
        /// If not found, returns null.
        /// </summary>
        /// <param name="Address">The address within the code line to retrieve</param>
        /// <returns>The code line, or null</returns>
        public CodeLine GetLine(uint Address)
        {
            for (int i = 0; i < Lines.Length; i++)
            {
                CodeLine l = Lines[i];

                if (l.Offset <= Address && l.Offset + l.Size > Address) {
                    return l;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the line of assembly at the specified address.
        /// If not found, returns null.
        /// </summary>
        /// <param name="Address">The address at which to get the assembly.</param>
        /// <returns>The assembly at the specified address, or null.</returns>
        public AssemblyLine GetAssembly(uint Address)
        {
            CodeLine line = this.GetLine(Address);

            if (line == null) return null;
            else              return line.GetAssembly(Address);
        }

        /// <summary>
        /// Gets the string source of the code unit.
        /// </summary>
        /// <returns>A string containing the source code of the unit.</returns>
        public override string ToString() {
            return string.Join<CodeLine>("\n", this.Lines);
        }

        /// <summary>
        /// Gets the assembly string of the code unit.
        /// </summary>
        /// <returns>A string containing the assembly code of the unit.</returns>
        public string GetAssemblyString() {
            return string.Join("\n", this.Lines.Select(line => line.GetAssemblyString()));
        }
    }
}
