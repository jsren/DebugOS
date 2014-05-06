using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DebugOS
{
    public static class ExtensionLoader
    {
        private static Extension checkType(Type type)
        {
            foreach (Type @interface in type.GetInterfaces())
            {
                if (@interface == typeof(IExtension))
                {
                    if (type.ContainsGenericParameters || type.IsAbstract){
                        return null;
                    }
                    var constructor = type.GetConstructor(System.Type.EmptyTypes);

                    if (constructor == null){
                        return null;
                    }
                    else return new Extension((IExtension)constructor.Invoke(null));
                }
            }
            return null;
        }

        public static Extension[] loadExtensionAssembly(Assembly assembly)
        {
            var extensions = new List<Extension>();

            foreach (Type type in assembly.GetExportedTypes())
            {
                try
                {
                    var ext = checkType(type);
                    if (ext != null) extensions.Add(ext);
                }
                catch (Exception e)
                {
                    Console.WriteLine("[ERROR] Error loading possible extension '{0}': ", type, e);
                }
            }
            return extensions.ToArray();
        }
    }
}
