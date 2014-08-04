using System;

namespace DebugOS
{
    public class RegistersChangedEventArgs : EventArgs
    {
        public string[] AffectedRegisters { get; private set; }

        public RegistersChangedEventArgs(string[] registers)
        {
            this.AffectedRegisters = registers;
        }
    }
}
