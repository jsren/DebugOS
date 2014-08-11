using System;

namespace DebugOS.Extensions
{
    public sealed class AssemblyExplorerExtension : IUIExtension
    {
        public string Name 
        { 
            get { return "Assembly Explorer"; }
        }

        public void Initialise(string[] args)
        {

        }

        public void SetupUI(DebugOS.IDebugUI UI)
        {
            // Shows a WPF component only
            if (UI.Type != GUIType.WPF)
                throw new Exception("This extension must be run under WPF.");

            // Add the menu item
            IMenuItem assemblyExplorerMenuItem = UI.NewMenuItem(label:"Configure Loaded Assemblies...");
            assemblyExplorerMenuItem.Clicked  += (o) => new AssemblyExplorer().ShowDialog();

            UI.AddMenuItem(assemblyExplorerMenuItem, "Debug");
        }
    }
}
