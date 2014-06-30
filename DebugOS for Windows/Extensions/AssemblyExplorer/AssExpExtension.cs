using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebugOS.Extensions
{
    public class AssemblyExplorerExtension : IUIExtension
    {
        public string Name { get { return "Assembly Explorer"; } }

        public void Initialise(string[] args)
        {

        }

        public void SetupUI(DebugOS.IDebugUI @interface)
        {
            if (@interface.Type != GUIType.WPF) {
                throw new Exception("This extension must be run under WPF.");
            }

            IDebugUI UI = @interface as IDebugUI;

            /* == Assembly Explorer == */
            IMenuItem assemblyExplorerMenuItem = UI.NewMenuItem();
            assemblyExplorerMenuItem.Label     = "Configure Loaded Assemblies...";
            assemblyExplorerMenuItem.Clicked  += (o) => new AssemblyExplorer().ShowDialog();

            UI.AddMenuItem("Debug", assemblyExplorerMenuItem);
        }

    }
}
