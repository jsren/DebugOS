using System.Collections.Generic;

namespace DebugOS
{
    /// <summary>
    /// A table holding a set of symbols for an object file.
    /// </summary>
    public sealed class SymbolTable : IEnumerable<SymbolEntry>
    {
        private readonly SymbolEntry[] symbols;

        /// <summary>
        /// Creates a new symbol table.
        /// </summary>
        /// <param name="Symbols">The symbols with which to populate the table.</param>
        public SymbolTable(SymbolEntry[] Symbols) {
            this.symbols = Symbols;
        }

        /// <summary>
        /// Gets the symbol at the specified index.
        /// </summary>
        /// <param name="Index">The index of the symbol to get.</param>
        /// <returns>A symbol entry.</returns>
        public SymbolEntry GetSymbol(int Index) {
            return symbols[Index];
        }

        /// <summary>
        /// Gets the symbol with the given name.
        /// </summary>
        /// <param name="Name">The name of the symbol to get.</param>
        /// <returns>A symbol entry.</returns>
        public SymbolEntry GetSymbol(string Name)
        {
            for (int i = 0; i < symbols.Length; i++)  {
                if (symbols[i].Name == Name) return symbols[i];
            }
            return null;
        }

        /* === Enumeration Methods === */
        #region Enumerator
        private sealed class SymbolTableEnumerator : IEnumerator<SymbolEntry>
        {
            private int index = 0;
            private SymbolTable table;

            public void Dispose() { }
            public void Reset() { index = 0; }
            public bool MoveNext() { return ++index != table.symbols.Length; }

            public SymbolEntry Current {
                get { return table.symbols[index]; }
            }
            object System.Collections.IEnumerator.Current {
                get { return this.Current; }
            }

            public SymbolTableEnumerator(SymbolTable table) {
                this.table = table;
            }
        }

        public IEnumerator<SymbolEntry> GetEnumerator() {
            return new SymbolTableEnumerator(this);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return new SymbolTableEnumerator(this);
        }
        #endregion
    }
}
