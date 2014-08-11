/* ExtensionLoader.cs - (c) James S Renwick
 * ----------------------------------------
 * Version 1.1.0
 */
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DebugOS.Loaders
{
    internal static class ExtensionLoader
    {
        /// <summary>
        /// Check whether the type implements IExtension and if so, 
        /// create and return an Extension wrapper.
        /// </summary>
        private static Extension CheckType(Type type)
        {
            // Iterate through interfaces in the type
            foreach (Type @interface in type.GetInterfaces())
            {
                if (@interface == typeof(IExtension))
                {
                    // If we can't construct the type, it's useless
                    if (type.ContainsGenericParameters || type.IsAbstract) {
                        return null;
                    }
                    // Look for a valid empty constructor
                    var constructor = type.GetConstructor(System.Type.EmptyTypes);
                    if (constructor == null) return null; 

                    // Return the wrapper over a new instance of the type
                    return new Extension((IExtension)constructor.Invoke(null));
                }
            }
            return null;
        }

        /// <summary>
        /// Load all valid extensions from the given assembly.
        /// </summary>
        public static Extension[] LoadExtensionAssembly(string assemblyPath)
        {
            return LoadExtensionAssembly(Assembly.LoadFile(
                Utils.GetPlatformPath(assemblyPath)));
        }

        /// <summary>
        /// Load all valid extensions from the given assembly.
        /// </summary>
        public static Extension[] LoadExtensionAssembly(Assembly assembly)
        {
            var extensions = new List<Extension>();

            // Iterate through exported types
            foreach (Type type in assembly.GetExportedTypes())
            {
                Extension ext;
                try
                {
                    if ((ext = CheckType(type)) != null)
                    {
                        // Name must be valid
                        if (String.IsNullOrWhiteSpace(ext.Name))
                            throw new ArgumentNullException("Null or empty extension names are not permitted");

                        // Add the extension
                        extensions.Add(ext);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("[ERROR] Error loading extension '{0}': ", type, e);
                }
            }
            return extensions.ToArray();
        }
    }
}
