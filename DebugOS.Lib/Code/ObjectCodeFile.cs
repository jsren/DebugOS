﻿/* ObjectCodeFile.cs - (c) James S Renwick 2014
 * --------------------------------------------
 * Version 1.3.0
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace DebugOS
{
    /// <summary>
    /// An object representing an object code file.
    /// </summary>
    public sealed class ObjectCodeFile : IDebugResource
    {
        /// <summary>The actual load address.</summary>
        private long loadAddress;

        /// <summary>The requested load address of the object file in memory.</summary>
        public long RequestedLoadAddress { get; private set; }
        /// <summary>The name of the object file.</summary>
        public String Filepath { get; private set; }
        /// <summary>The targeted architecture of the object file.</summary>
        public String Architecture { get; private set; }
        /// <summary>The sections of the object file.</summary>
        public Section[] Sections { get; private set; }
        /// <summary>The symbol table of the object file.</summary>
        public SymbolTable SymbolTable { get; private set; }
        /// <summary>The source code of the object file.</summary>
        public CodeUnit[] Code { get; private set; }
        /// <summary>The total runtime length of the object file in bytes.</summary>
        public long Size { get; private set; }
        /// <summary>An array of paths to source files.</summary>
        public String[] SourceFiles { get; private set; }

        /// <summary>Gets or sets the actual load address of the object file.</summary>
        public long ActualLoadAddress
        {
            get { return this.loadAddress; }
            set
            {
                this.loadAddress = value;

                // Adjust the actual load address of each code unit
                long delta = this.loadAddress - this.RequestedLoadAddress;
                if (delta != 0)
                {
                    foreach (CodeUnit unit in Code) { unit.Rebase(delta); }
                }
            }
        }

        /// <summary>
        /// Gets the code for the specified symbol, or null if not found.
        /// </summary>
        /// <param name="SymbolName">The name of the symbol.</param>
        /// <returns>A CodeUnit object containing the code for the specified symbol, or null.</returns>
        public CodeUnit GetCode(string SymbolName)
        {
            foreach (CodeUnit c in Code) {
                if (c.Name == SymbolName) return c;
            }
            return null;
        }
        /// <summary>
        /// Gets the source at the given address, or null if not found.
        /// </summary>
        /// <param name="Address">The address with which to find the code source.</param>
        /// <returns>A CodeUnit object containing the code for the specified address, or null.</returns>
        public CodeUnit GetCode(long Address)
        {
            for (int i = 0; i < Code.Length; i++)
            {
                if (Code[i].Offset <= Address && Code[i].Offset + Code[i].Size > Address) {
                    return Code[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the line at the given address, or null if not found.
        /// </summary>
        /// <param name="Address">The address with which to find the code line</param>
        /// <returns>An object containing the line of code which has the given address, or null.</returns>
        public CodeLine GetCodeLine(long Address)
        {
            for (int i = 0; i < Code.Length; i++)
            {
                var line = Code[i].GetLine(Address);
                if (line != null) return line;
            }
            return null;
        }

        /// <summary>
        /// Gets the assembly instruction at the given address, or null if not found.
        /// </summary>
        /// <param name="Address">The address of the instruction</param>
        /// <returns>An object containing the assembly and machine code at the given address, or null.</returns>
        public AssemblyLine GetAssembly(uint Address)
        {
            for (int i = 0; i < Code.Length; i++)
            {
                var asm = Code[i].GetAssembly(Address);
                if (asm != null) return asm;
            }
            return null;
        }

        /// <summary>
        /// Creates a new object code file object.
        /// </summary>
        /// <param name="StartAddress">The load address of the object file.</param>
        /// <param name="Filename">The name of the object file.</param>
        /// <param name="Architecture">The targeted architecture of the object file.</param>
        /// <param name="Sections">The sections of the object file.</param>
        /// <param name="SymbolTable">The symbol table of the object file.</param>
        /// <param name="Code">The source code of the object file.</param>
        public ObjectCodeFile(long StartAddress, String Filename, String Architecture,
            Section[] Sections, SymbolTable SymbolTable, CodeUnit[] Code)
        {
            this.loadAddress  = StartAddress;
            this.Filepath     = Filename;
            this.Architecture = Architecture;
            this.Sections     = Sections;
            this.SymbolTable  = SymbolTable;
            this.Code         = Code;

            this.RequestedLoadAddress = StartAddress;

            // Sum loaded sections' sizes
            if (this.Sections != null)
            this.Size = this.Sections.Sum(
                sec => sec.Flags.HasFlag(SectionFlags.Load) ? (long)sec.Size : 0);

            // Get unique source file paths
            var tmpFiles = new HashSet<string>();

            foreach (CodeUnit unit in this.Code) {
                tmpFiles.Add(unit.SourceFilepath.Trim());
            }
            this.SourceFiles = tmpFiles.ToArray();
        }
    }
}
