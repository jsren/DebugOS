using System;

namespace DebugOS
{
    public sealed class SteppedEventArgs : EventArgs
    {
        public Address Address { get; private set; }
        public AssemblyLine Assembly { get; private set; }

        public SteppedEventArgs(Address address) {
            this.Address = address;
        }
        public SteppedEventArgs(Address address, AssemblyLine asm)
        {
            this.Address  = address;
            this.Assembly = asm;
        }
    }
}
