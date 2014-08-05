using System;

namespace DebugOS
{
    public class BreakpointChangedEventArgs : EventArgs
    {
        public Breakpoint Breakpoint { get; private set; }

        public BreakpointChangedEventArgs(Breakpoint breakpoint)
        {
            this.Breakpoint = breakpoint;
        }
    }
}
