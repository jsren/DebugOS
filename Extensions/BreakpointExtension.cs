/* BreakpointExtension.cs - (c) James S Renwick 2014
 * -------------------------------------------------
 * Version 1.3.0
 */
using System;
using System.Collections.Generic;

namespace DebugOS.Extensions
{
    public class BreakpointExtension : IUIExtension
    {
        private IToolbarItem cpuModeToggleItem;

        /// <summary>Gets the name of the extension.</summary>
        public string Name { get { return "Breakpoint UI"; } }

        /// <summary>
        /// Performs extension initialisation.
        /// </summary>
        public void Initialise(string[] args)
        {
            return;
        }

        private sealed class SearchCategory : ISearchCategory
        {
            public string Header { get { return "Breakpoints"; } }

            public IEnumerable<SearchResult> Reset()
            {
                return new SearchResult[0];
            }

            public IEnumerable<SearchResult> GetResults(string searchString)
            {
                const long mask = ~(0x1FL);

                if (Application.Debugger == null)
                {
                    return new SearchResult[0];
                }

                try
                {
                    ulong addr = Utils.ParseHex64(searchString) - 30;

                    var output = new List<SearchResult>();

                    foreach (Breakpoint bp in Application.Debugger.Breakpoints)
                    {
                        if (bp.IsActive && (bp.Address.Type == AddressType.Physical || 
                            (bp.Address.Type == AddressType.Logical && bp.Address.Segment == Segment.Code)))
                        {
                            if ((bp.Address.Value & mask) == ((long)addr & mask))
                            {
                                string symbol = String.Empty;

                                // Get the symbol name for the code at the breakpoint
                                if (Application.Session != null)
                                {
                                    foreach (ObjectCodeFile file in Application.Session.LoadedImages)
                                    {
                                        var unit = file.GetCode(bp.Address.Value);
                                        if (unit != null)
                                        {
                                            symbol = " <" + unit.Symbol + '>';
                                            break;
                                        }
                                    }
                                }

                                // Get the result value string
                                string value = Utils.GetHexString((ulong)bp.Address.Value, prefix: true) 
                                    + symbol ?? String.Empty; 

                                // Add the result
                                output.Add(new SearchResult(value, Properties.Resources.breakpoint));
                            }
                        }
                    }
                    return output;
                }
                catch (FormatException) { return new SearchResult[0]; }
            }
        }


        /// <summary>
        /// Builds the extension's user interface.
        /// </summary>
        public void SetupUI(IDebugUI UI)
        {
            Application.DebuggerChanged += OnDebuggerChanged;

            /* == Menu Items == */
            var clearAllMenu = UI.NewMenuItem();
            {
                clearAllMenu.Label    = "Clear all breakpoints";
                clearAllMenu.Clicked += clearAllBreakpoints;
            }
            UI.AddMenuItem(clearAllMenu, "Debug");


            /* == Toolbar Items == */
            var panel = UI.NewToolbarPanel();
            panel.Title = "Breakpoints";

            // Add "Clear all breakpoints" item
            var clearAllItem = UI.NewToolbarItem();
            {
                clearAllItem.ToolTip = "Clears all breakpoints";
                clearAllItem.Clicked += clearAllBreakpoints;
            }
            // Add "Toggle breakpoint on mode change" item
            this.cpuModeToggleItem = UI.NewToolbarItem(isToggle: true);
            {
                this.cpuModeToggleItem.IsEnabled = Application.Debugger is Bochs.BochsDebugger;
                this.cpuModeToggleItem.ToolTip   = "Toggle breakpoint on CPU mode change";
                this.cpuModeToggleItem.Clicked  += this.toggleBPOnModeChange;
            }
            panel.AddToolbarItem(clearAllItem);
            panel.AddToolbarItem(cpuModeToggleItem);

            // Add the panel to the UI
            UI.AddToolbarPanel(panel);

            // Add the search category
            UI.AddSearchCategory(new SearchCategory());
        }


        /// <summary>
        /// Enables/Disables buttons when a debugger is loaded/unloaded.
        /// </summary>
        private void OnDebuggerChanged()
        {
            cpuModeToggleItem.IsEnabled = (Application.Debugger as Bochs.BochsDebugger != null);
        }

        /// <summary>
        /// Event handler for toolbar item click
        /// </summary>
        private void clearAllBreakpoints(object sender)
        {
            // Ignore if no debugger attached
            if (Application.Debugger == null) return;

            foreach (Breakpoint bp in Application.Debugger.Breakpoints)
            {
                if (bp.IsActive) // Ignore deactivated breakpoints
                {
                    Application.Debugger.ClearBreakpoint(bp.Address);
                }
            }
        }

        /// <summary>
        /// Event handler for toolbar item click
        /// </summary>
        private void toggleBPOnModeChange(object sender)
        {
            // BP on mode change only applicable to Bochs
            var bochs = Application.Debugger as Bochs.BochsDebugger;

            if (bochs != null)
            {
                bochs.BreakOnCPUModeChange = !bochs.BreakOnCPUModeChange;
            }
        }
    }
}
