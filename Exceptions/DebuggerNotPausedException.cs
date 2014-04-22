using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebugOS
{
    /// <summary>
    /// Exception thrown when attemping an operation when
    /// execution has not been paused.
    /// </summary>
    public class DebuggerNotPausedException : Exception { }
}
