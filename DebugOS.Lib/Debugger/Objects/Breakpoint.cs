
namespace DebugOS
{
    public class Breakpoint
    {
        public virtual bool IsActive { get; set; }
        public virtual Address Address  { get; protected set; }

        protected Breakpoint() { }
        public Breakpoint(bool active, Address address)
        {
            this.IsActive = active;
            this.Address  = address;
        }
    }
}
