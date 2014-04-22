using System;
using System.Windows.Media.Imaging;

namespace DebugOS
{
    /// <summary>
    /// Built-in extension providing execution-related UI controls.
    /// </summary>
    public class ExecutionExtension : IDebugExtension
    {
        public void Initialise(string[] args) {
            return;
        }

        public string Name { get { return "Execution UI"; } }

        public void SetupUI(IDebugUI UI, IDebugger debugger)
        {
            // == First create the toolbar panel ==
            #region Toolbar Panel

            var panel = UI.NewToolbarPanel();
            panel.Title = "Execution";

            var stepItem = UI.NewToolbarItem();
            {
                stepItem.ToolTip =  "Step";
                stepItem.Icon    =  new BitmapImage(new Uri("/Icons/arrow-32.png", UriKind.Relative));
                stepItem.Clicked += (s, e) => debugger.Step();
            }
            var stepOverItem = UI.NewToolbarItem();
            {
                stepOverItem.ToolTip =  "Step Over";
                stepOverItem.Icon    =  new BitmapImage(new Uri("/Icons/right_circular-32.png", UriKind.Relative));
                stepOverItem.Clicked += (s, e) => debugger.StepOver();
            }
            var continueItem = UI.NewToolbarItem();
            {
                continueItem.ToolTip =  "Continue";
                continueItem.Icon    =  new BitmapImage(new Uri("/Icons/right_round-32.png", UriKind.Relative));
                continueItem.Clicked += (s, e) => debugger.Continue();
            }
            panel.AddToolbarItem(stepItem);
            panel.AddToolbarItem(stepOverItem);
            panel.AddToolbarItem(continueItem);

            UI.AddToolbarPanel(panel);
            #endregion
        }
    }
}
