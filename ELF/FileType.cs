using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugOS.ELF
{
    /// <summary>
    /// ELF Object File Type
    /// </summary>
    public enum FileType
    {
        Unknown      = 0,
        Relocatable,
        Executable,
        Shared,
        Core
    }
}
