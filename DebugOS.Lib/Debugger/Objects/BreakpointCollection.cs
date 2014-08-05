using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugOS
{
    public sealed class BreakpointCollection : IEnumerable<Breakpoint>
    {
        private IEnumerable<Breakpoint> list;

        public BreakpointCollection(IEnumerable<Breakpoint> backingCollection)
        {
            if (backingCollection == null)
                throw new ArgumentNullException("backingCollection");

            this.list = backingCollection;
        }

        public bool Contains(Address breakpointAddress)
        {
            foreach (Breakpoint bp in this.list)
            {
                if (bp.Address == breakpointAddress) return true;
            }
            return false;
        }

        public Breakpoint GetBreakpoint(Address address)
        {
            foreach (Breakpoint bp in this.list)
            {
                if (bp.Address == address && bp.IsActive) return bp;
            }
            return null;
        }

        public IEnumerator<Breakpoint> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }
}
