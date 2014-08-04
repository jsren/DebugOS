using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugOS.GDB
{
    public enum StopReason
    {
        Unknown,
        Trap,
        Watchpoint,
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

            if (data[0] != 'F' && data[0] != 'O')
            {
                // Remove delimiting spaces if present
                while (data[1] == ' ') { data = data.Remove(1, 1); }

                // Get the signal no.
                this.Signal = int.Parse(data.Substring(1, 2));
            }

            if (data[0] == 'S' || data[0] == 'T')
            {
                this.Reason = StopReason.Trap;
                /* TODO: Watchpoints */
            }
            else if (data[0] == 'W' || data[0] == 'X')
            {
                this.Reason = StopReason.Termination;
            }
            else this.Reason = StopReason.Unknown;
        }
    }
}
