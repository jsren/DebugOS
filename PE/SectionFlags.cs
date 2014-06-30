using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugOS.PE
{
    [Flags]
    public enum SectionFlags : uint
    {
        [Obsolete]
        DontPad = 0x8,

        Code     = 0x20,
        InitializedData = 0x40,
        UninitializedData = 0x80,
        Other = 0x100,

        Info = 0x200,
        Remove = 0x800,
        COMDAT = 0x1000,
        GlobalPointerData = 0x8000,

        ThumbCode = 0x20000,

        Align1    = 0x100000,
        Align2    = 0x200000,
        Align4    = 0x300000,
        Align8    = 0x400000,
        Align16   = 0x500000,
        Align32   = 0x600000,
        Align64   = 0x700000,
        Align128  = 0x800000,
        Align256  = 0x900000,
        Align512  = 0xA00000,
        Align1024 = 0xB00000,
        Align2048 = 0xC00000,
        Align4096 = 0xD00000,
        Align8192 = 0xE00000,

        ExtendedReloc = 0x1000000,
        CanDiscard    = 0x2000000,
        NotCached     = 0x4000000,
        NotPaged      = 0x8000000,
        Shared        = 0x10000000,
        Exectuable    = 0x20000000,
        Readable      = 0x40000000,
        Writable      = 0x80000000
    }
}
