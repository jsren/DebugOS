using System;

namespace DebugOS
{
    public class RegisterUpdateEventArgs : EventArgs
    {
        public Register Register { get; private set; }
        public byte[] Value { get; private set; }

        public RegisterUpdateEventArgs(Register Register, byte[] Value)
        {
            this.Register = Register;
            this.Value    = Value;
        }
    }
}
