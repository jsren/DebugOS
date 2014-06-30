using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugOS.ELF
{
    public enum SectionFlags
    {
        /// <summary>
        /// The section contains data that should be writable during process execution.
        /// </summary>
        Writable,
        /// <summary>
        /// The section occupies memory during process execution.
        /// </summary>
        Allocated,
        /// <summary>
        /// The section contains executable machine instructions.
        /// </summary>
        Executable
    }
}
