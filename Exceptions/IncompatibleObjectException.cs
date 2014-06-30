using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugOS
{
    [Serializable]
    public sealed class IncompatibleObjectException : Exception
    {
        public object Object { get; private set; }

        public IncompatibleObjectException(object obj) 
            : base("An extension attempted to use an object that was created by an incompatible version of this program")
        {
            this.Object = obj;
        }
    }
}
