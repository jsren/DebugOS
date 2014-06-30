using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugOS.GDB
{
    public enum StopReason
    {
        Unknown,
        Watchpoint,
        Breakpoint,
        Termination
    }

    public sealed class StopReply
    {
        public StopReason Reason { get; private set; }

        public int Core { get; private set; }
        public int ThreadID { get; private set; }

        public int Signal { get; private set; }

        public Tuple<int, long>[] Registers { get; private set; }

        public StopReply(string data)
        {
            this.Core      = -1;
            this.ThreadID  = -1;
            this.Registers = new Tuple<int, long>[0];
            this.Reason    = StopReason.Unknown;

            if (data[0] == 'T')
            {
                
            }
        }
    }
}
