/* Extension.cs - (c) James S Renwick 2014
 * ---------------------------------------
 * Version 1.4.0
 */
using System;

namespace DebugOS
{
    public sealed class Extension
    {
        private IExtension extension;
        
        /// <summary>
        /// Gets whether the extension was successfully initialised.
        /// </summary>
        public bool LoadedSuccessfully { get; private set; }
        /// <summary>
        /// Gets the public name of the extension.
        /// </summary>
        public string Name { get { return extension.Name; } }
        /// <summary>
        /// Gets the interfaces supported by the extension
        /// </summary>
        public Type[] SupportedInterfaces { get; private set; }
        


        /// <summary>
        /// Creates a new Extension from the given IExtension.
        /// </summary>
        public Extension(IExtension extension)
        {
            this.extension = extension;

            // Warn on 3rd-party use
            Utils.AssertInternal(warn: true);
        }

        /// <summary>
        /// Attempts to initialise the extension. Exception neutral.
        /// </summary>
        /// <param name="args">Parameters to pass to the extension.</param>
        public void Initialise(string[] args)
        {
            try
            {
                this.extension.Initialise(args);
                this.LoadedSuccessfully = true;
            }
            catch
            {
                this.LoadedSuccessfully = false;
                throw;
            }
        }

        /// <summary>
        /// Gets the interface of the given type for this extension.
        /// </summary>
        /// <typeparam name="T">The interface instance to get.</typeparam>
        /// <returns>The required interface or null if not implemented.</returns>
        public T GetInterface<T>() where T : class, IExtension
        {
            return this.extension as T;
        }

    }
}
