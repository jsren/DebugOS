using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugOS.ELF
{
    public enum SectionType
    {
        NotPresent,
        InitializedData,
        SymbolTable,
        StringTable,
        RelocWithAddends,
        SymbolHashTable,
        Dynamic,
        Note,
        UninitializedData,
        Relocation,
        [Obsolete]
        SHLIB,
        DynamicSymbols
    }
}
