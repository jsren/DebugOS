using System;

namespace DebugOS
{
    /// <summary>
    /// The type of address - how the address is to be mapped.
    /// </summary>
    public enum AddressType
    {
        /// <summary>
        /// The direct memory location
        /// </summary>
        Physical = 0,
        /// <summary>
        /// The address within the process' scope, subject to paging
        /// </summary>
        Linear   = 1,
        /// <summary>
        /// The address relative to the current segment, subject to paging
        /// </summary>
        Logical  = 2
    }
}
