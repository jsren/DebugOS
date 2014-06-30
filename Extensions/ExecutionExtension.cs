using System;
using System.Drawing;

using DebugOS;
using System.Collections.Generic;

namespace DebugOS.Extensions
{
    /// <summary>
    /// Built-in extension providing execution-related UI controls.
    /// </summary>
    public class ExecutionExtension : IUIExtension
    {
        public void Initialise(string[] args)
        {
            return;
        }

        public string Name { get { return "Execution UI"; } }

        public DebugSession Session { get; set; }


        public Dictionary<IMainPanel, int> panels
            = new Dictionary<IMainPanel, int>();



        private bool CheckDebugger()
        {
            return Application.Debugger != null &&
                Application.Debugger.CurrentStatus != DebugStatus.Disconnected;
        }

        private void Step(object _)
        {
            if (CheckDebugger()) Application.Debugger.Step();
        }

        private void Continue(object _)
        {
            if (CheckDebugger()) Application.Debugger.Continue();
        }

        private void End(object _)
        {
            if (CheckDebugger()) Application.Debugger.Disconnect();
        }

        public void SetupUI(IDebugUI UI)
        {
            #region Menu Bar

            var stepMenuItem = UI.NewMenuItem();
            {
                stepMenuItem.Label    = "Step One";
                stepMenuItem.Icon     = Properties.Resources.arrow_right;
                stepMenuItem.Clicked += this.Step;
            }
            var continueMenuItem = UI.NewMenuItem();
            {
                continueMenuItem.Label    = "Continue";
                continueMenuItem.Icon     = Properties.Resources.right_circular;
                continueMenuItem.Clicked += this.Continue;
            }

            UI.AddMenuItem("Debug", stepMenuItem);
            UI.AddMenuItem("Debug", continueMenuItem);

            #endregion

            #region Toolbar Panel

            var panel = UI.NewToolbarPanel();
            panel.Title = "Execution";

            var stepItem = UI.NewToolbarItem();
            {
                stepItem.ToolTip  = "Step";
                stepItem.Icon     = Properties.Resources.arrow_right;
                stepItem.Clicked += this.Step;
            }
            var continueItem = UI.NewToolbarItem();
            {
                continueItem.ToolTip  = "Continue";
                continueItem.Icon     = Properties.Resources.right_circular;
                continueItem.Clicked += this.Continue;
            }
            var stopItem = UI.NewToolbarItem();
            {
                stopItem.ToolTip  = "End Debugging";
                stopItem.Icon     = Properties.Resources.stop;
                stopItem.Clicked += this.End;
            }
            panel.AddToolbarItem(stepItem);
            panel.AddToolbarItem(continueItem);
            panel.AddToolbarItem(stopItem);

            UI.AddToolbarPanel(panel);
            #endregion

        }


        
    }
}
