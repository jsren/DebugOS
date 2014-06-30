using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugOS.COFF
{
    public enum SymbolBaseType
    {
        Unknown   = 0x0,
        Void      = 0x1,
        Char      = 0x2,
        Short     = 0x3,
        Int       = 0x4,
        Long      = 0x5,
        Float     = 0x6,
        Double    = 0x7,
        Struct    = 0x8,
        Union     = 0x9,
        Enum      = 0xA,
        EnumValue = 0xB,
        Byte      = 0xC,
        Word      = 0xD,
        UInt      = 0xE,
        Dword     = 0xF
    }
}
