using System;

namespace DebugOS
{
    [Serializable]
    public sealed class ObjectFrozenException : Exception
    {
        public ObjectFrozenException() : 
            base("An attempt was made to change a read-only field of a frozen object.") { }

        public ObjectFrozenException(string message) : base(message) { }
    }
}
