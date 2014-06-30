using System;

namespace DebugOS.ELF
{
    [Flags]
    public enum SegmentFlags
    {
        CanExecute = 0x1,
        CanWrite   = 0x2,
        CanRead    = 0x4
    }
}
