using System;

namespace DebugOS
{
    public sealed class SymbolEntry
    {
        /// <summary>The value of the symbol - often an address.</summary>
        public long Value { get; private set; }

        /// <summary>The symbol's flags.</summary>
        public SymbolFlags Flags { get; private set; }

        /// <summary>The name of the section in which the symbol is declared.</summary>
        public String Section { get; private set; }

        /// <summary>The size or alignment of the symbol, or 0x00000000.</summary>
        public int Size { get; private set; }

        /// <summary>The name of the symbol.</summary>
        public String Name { get; private set; }


        public SymbolEntry(long Value, SymbolFlags Flags, 
            String Section, int Size, String Name)
        {
            this.Value     = Value;
            this.Flags     = Flags;
            this.Section   = Section;
            this.Size      = Size;
            this.Name      = Name;
        }

        public override string ToString() {
            return string.Format("[{0}] {1}: \"{2}\" {3} ", this.Flags, this.Section, this.Name, this.Value); 
        }
    }
}
