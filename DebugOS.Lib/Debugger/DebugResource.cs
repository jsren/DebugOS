using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DebugOS
{
    public interface IDebugResource
    {

    }

    public static class DebugResourceEx
    {
        private static Assembly coreAssembly;

        static DebugResourceEx()
        {
            coreAssembly = Assembly.GetAssembly(typeof(DebugResourceEx));
        }

        /// <summary>
        /// Gets whether the given debug resource is a built-in type.
        /// </summary>
        /// <param name="res">The resource for which to check.</param>
        /// <returns>True if the resource is of a built-in type.</returns>
        public static bool GetTypeIsBuiltin(this IDebugResource res)
        {
            return res.GetType().Assembly == coreAssembly;
        }
    }
}
