using System;

namespace DebugOS
{
    public class Extension
    {
        public IExtension Interface { get; private set; }
        
        /// <summary>
        /// Gets whether the extension was successfully initialised.
        /// </summary>
        public bool LoadedSuccessfully { get; private set; }

        /// <summary>
        /// Gets the public name of the extension.
        /// </summary>
        public string Name { get { return Interface.Name; } }

        /// <summary>
        /// Gets whether the extension has a user interface.
        /// </summary>
        public bool HasUI { get; private set; }

        /// <summary>
        /// Gets whether the extension can load a debugger.
        /// </summary>
        public bool HasDebugger { get; private set; }


        /// <summary>
        /// Creates a new Extension from the given IExtension.
        /// </summary>
        public Extension(IExtension extension)
        {
            this.Interface = extension;

            if (extension as IUIExtension != null) {
                HasUI = true;
            }
            if (extension as IDebuggerExtension != null) {
                HasDebugger = true;
            }
        }

        /// <summary>
        /// Attempts to initialise the extension. Exception neutral.
        /// </summary>
        /// <param name="args">Parameters to pass to the extension.</param>
        public void Initialise(string[] args)
        {
            try 
            {
                this.Interface.Initialise(args);
                this.LoadedSuccessfully = true;
            }
            catch 
            {
                this.LoadedSuccessfully = false; 
                throw; 
            }
        }

        /// <summary>
        /// Initialises and configures the extension's UI if one exists.
        /// </summary>
        /// <param name="UI">The GUI interface object.</param>
        /// <param name="debugger">The debugger interface object.</param>
        public void SetupUI(IDebugUI UI)
        {
            if (this.HasUI)
            {
                ((IUIExtension)this.Interface).SetupUI(UI);
            }
        }

        /// <summary>
        /// Attempts to load the extension's debugger if one exists.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the extension is not an IDebuggerExtension.
        /// </exception>
        /// <returns>A debugger.</returns>
        public IDebugger LoadDebugger()
        {
            if (this.HasDebugger)
            {
                return ((IDebuggerExtension)this.Interface).LoadDebugger();
            }
            throw new InvalidOperationException();
        }
    }
}
