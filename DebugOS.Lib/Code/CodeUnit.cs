using System;
using System.Linq;

namespace DebugOS
{
    /// <summary>
    /// An object representing a set of instructions - generally a function.
    /// </summary>
    public class CodeUnit
    {
        /// <summary>The size in bytes of the code source.</summary>
        public virtual long Size { get; private set; }
        /// <summary>The offset within the object file at which the source is present.</summary>
        public virtual long Offset { get; private set; }
        /// <summary>The name of the source.</summary>
        public virtual String Name { get; private set; }
        /// <summary>The source's code.</summary>
        public virtual CodeLine[] Lines { get; private set; }

        /// <summary>
        /// Creates a code unit object.
        /// </summary>
        /// <param name="Offset">The size in bytes of the code source.</param>
        /// <param name="Name">The offset within the object file at which the source is present.</param>
        /// <param name="Lines">The source's code.</param>
        public CodeUnit(long Offset, String Name, CodeLine[] Lines)
        {
            this.Offset = Offset;
            this.Name   = Name;
            this.Lines  = Lines;
            this.Size   = this.Lines.Sum(line => (long)line.Size);
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
