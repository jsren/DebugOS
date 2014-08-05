
using System;

namespace DebugOS
{
    public class Breakpoint
    {
        public virtual bool IsActive { get; internal set; }
        public virtual Address Address  { get; protected set; }

        protected Breakpoint() { }
        public Breakpoint(Address address, bool active = true)
        {
            this.IsActive = active;
            this.Address  = address;
        }

        public virtual void MarkDeactivated()
        {
            this.IsActive = false;
        }
    }
}
