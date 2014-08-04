using System;

namespace DebugOS
{
    public class MemoryReadEventArgs : EventArgs
    {
        public Address BaseAddress { get; private set; }
        public byte[]  Data        { get; private set; }

        public MemoryReadEventArgs(Address address, byte[] data)
        {
            this.BaseAddress = address;
            this.Data        = data;
        }
    }
}
