using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugOS.ELF
{
    public enum ABI
    {
        SystemV   = 0,
        HPUX      = 1,
        NetBSD    = 2,
        Linux     = 3,
        Solaris   = 6,
        AIX       = 7,
        IRIX      = 8,
        FreeBSD   = 9,
        OpenBSD   = 12,

        Standalone = 255
    }
}
