/* SymbolEntry.cs - (c) James S Renwick 2013
 * -----------------------------------------
 * Version 1.0.1
 */
using System;

namespace DebugOS
{
    /// <summary>
    /// Represents a symbol (a named constant) in the scope of the
    /// executable.
    /// </summary>
    public class SymbolEntry
    {
        /// <summary>The value of the symbol - often an address.</summary>
        public long Value { get; private set; }

        /// <summary>The symbol's flags.</summary>
        public SymbolFlags Flags { get; private set; }

        /// <summary>The name of the section in which the symbol is declared.</summary>
        public string Section { get; private set; }

        /// <summary>The size or alignment of the symbol, or 0x00000000.</summary>
        public int Size { get; private set; }

        /// <summary>The name of the symbol.</summary>
        public String Name { get; private set; }

        /// <summary>
        /// Creates a new symbol entry.
        /// </summary>
        /// <param name="value">The symbol's integer value.</param>
        /// <param name="flags">The symbol's flags.</param>
        /// <param name="section">The section in which the symbol is declared.</param>
        /// <param name="size">The size or alignment of the symbol.</param>
        /// <param name="name">The unique name of the symbol.</param>
        public SymbolEntry(long value, SymbolFlags flags, 
            String section, int size, String name)
        {
            this.Value   = value;
            this.Flags   = flags;
            this.Section = section;
            this.Size    = size;
            this.Name    = name;
        }

        public override string ToString()
        {
            return string.Format("[{0}] {1}: \"{2}\" {3} ", 
                this.Flags, this.Section, this.Name, this.Value); 
        }
    }
}
