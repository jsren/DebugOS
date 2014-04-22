using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugOS.Extensions
{
    class BreakpointExtension : IDebugExtension
    {

        public string Name { get { return "Breakpoint UI"; } }

        public void Initialise(string[] args) {
            return;
        }

        public void SetupUI(IDebugUI UI, IDebugger debugger)
        {
            var panel = UI.NewToolbarPanel();
            panel.Title = "Breakpoints";

            var clearAllItem = UI.NewToolbarItem();
            {
                clearAllItem.ToolTip = "Clears all breakpoints";
                clearAllItem.Clicked += (s, e) =>
                {
                    foreach (Breakpoint bp in debugger.Breakpoints)
                    {
                        bp.IsActive = false;
                    }
                    // TODO: get the code views and clear their breakpoints
                };
            }
            var cpuModeToggleItem = UI.NewToolbarItem(isToggle:true);
            {
                cpuModeToggleItem.IsEnabled = debugger is Bochs.BochsDebugger;
                cpuModeToggleItem.ToolTip   = "Toggle breakpoint on CPU mode change";

                cpuModeToggleItem.Clicked  += (s, e) =>
                {
                    Bochs.BochsDebugger bdb  = (Bochs.BochsDebugger)debugger;
                    bdb.BreakOnCPUModeChange = !bdb.BreakOnCPUModeChange;
                };
            }
            panel.AddToolbarItem(clearAllItem);
            panel.AddToolbarItem(cpuModeToggleItem);

            UI.AddToolbarPanel(panel);
        }
    }
}
