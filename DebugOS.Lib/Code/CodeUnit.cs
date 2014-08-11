using System;
using System.Linq;

namespace DebugOS
{
    /// <summary>
    /// An object representing a set of instructions - generally a function.
    /// </summary>
    public class CodeUnit : IDebugResource
    {
        /// <summary>The size in bytes of the code unit.</summary>
        public virtual long Size { get; private set; }
        /// <summary>The offset within the object file at which the unit is present.</summary>
        public virtual long Offset { get; private set; }
        /// <summary>The name of the unit.</summary>
        public virtual String Name { get; private set; }
        /// <summary>The symbol by which the unit is referred.</summary>
        public virtual String Symbol { get; private set; }
        /// <summary>The path to the source file which defines this unit.</summary>
        public virtual String SourceFilepath { get; private set; }
        /// <summary>The units's code.</summary>
        public virtual CodeLine[] Lines { get; private set; }

        /// <summary>
        /// Creates a code unit object.
        /// </summary>
        /// <param name="offset">The size in bytes of the code unit.</param>
        /// <param name="name">The offset within the object file at which the unit is present.</param>
        /// <param name="symbol">The symbol by which the unit is referred.</param>
        /// <param name="path">The path to the source file which defines this unit.</param>
        /// <param name="lines">The unit's code.</param>
        public CodeUnit(long offset, String name, String symbol, String path, CodeLine[] lines)
        {
            this.Offset = offset;
            this.Name   = name;
            this.Symbol = symbol;
            this.Lines  = lines;
            this.Size   = this.Lines.Sum(line => (long)line.Size);

            this.SourceFilepath = Utils.GetPlatformPath(path);
        }
        protected CodeUnit() { }

        /// <summary>
        /// Moves the offset of the current code unit by the given address delta.
        /// Negative values will subtract from the offset.
        /// </summary>
        /// <param name="offsetDelta">The number of bytes by which to move the offset.</param>
        public void Rebase(long offsetDelta)
        {
            this.Offset += offsetDelta;
        }

        /// <summary>
        /// Gets the code line at the specified address.
        /// If not found, returns null.
        /// </summary>
        /// <param name="Address">The address within the code line to retrieve</param>
        /// <returns>The code line, or null</returns>
        public CodeLine GetLine(long Address)
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
        public AssemblyLine GetAssembly(long Address)
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
