using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugOS
{
    public enum Segment
    {
        Code,
        Data,
        Stack,
        /// <summary>
        /// ES
        /// </summary>
        Extended1,
        /// <summary>
        /// FS
        /// </summary>
        Extended2,
        /// <summary>
        /// GS
        /// </summary>
        Extended3
    }
}
