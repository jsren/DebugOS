/* BreakpointExtension.cs - (c) James S Renwick 2014
 * -------------------------------------------------
 * Version 1.2.0
 */
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
            UI.AddMenuItem("Debug", clearAllMenu);


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
                bp.IsActive = false;
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
