using System;

namespace DebugOS
{
    /// <summary>
    /// Flags describing a symbol.
    /// </summary>
    [Flags]
    public enum SymbolFlags
    {
        None,

        Local  = 0x01,
        Global = 0x02,
        Unique = 0x04,
        Weak   = 0x08,

        Constructor   = 0x10,
        Warning       = 0x20,
        Reference     = 0x40,
        RelocFunction = 0x80,

        Debugging = 0x100,
        Dynamic   = 0x200,

        Function = 0x400,
        File     = 0x800,
        Object   = 0x1000,
    }
}
