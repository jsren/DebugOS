using System;

namespace DebugOS
{
    /// <summary>
    /// The type of address - how the address is to be mapped.
    /// </summary>
    public enum AddressType
    {
        /// <summary>
        /// The direct memory location.
        /// </summary>
        Physical = 0,
        /// <summary>
        /// The address within the process' scope, subject to paging.
        /// </summary>
        Virtual = 1,
        /// <summary>
        /// The address, subject to paging and offset by the given segment.
        /// </summary>
        Logical = 2
    }
}
