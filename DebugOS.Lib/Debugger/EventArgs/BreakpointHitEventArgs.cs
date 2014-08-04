using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugOS
{
    public class BreakpointHitEventArgs : EventArgs
    {
        public virtual Breakpoint Breakpoint { get; protected set; }

        public BreakpointHitEventArgs(Breakpoint Breakpoint)
        {
            this.Breakpoint = Breakpoint;
        }
    }
}
