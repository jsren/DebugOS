using System;
using System.Linq;
using System.Reflection;

namespace DebugOS
{
    public static partial class Utils
    {
        // This should be evaluated at a time when only internal
        // assemblies are loaded.
        private static readonly Assembly[] internalAsms = 
            AppDomain.CurrentDomain.GetAssemblies();


        public static void AssertInternal(bool warn = false)
        {
            const string message = "A user library has attempted to access "
                + "or instantiate an internal type or member.";

            Assembly currentAsm = Assembly.GetCallingAssembly();

            if (!internalAsms.Contains(currentAsm))
            {
                if (warn) System.Diagnostics.Trace.TraceWarning(message);
                else throw new ApplicationException(message);
            }
        }
    }    
}
