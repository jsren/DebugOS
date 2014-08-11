using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugOS
{
    public sealed class SymbolExplorerExtension : IUIExtension
    {
        private IDebugUI UI;
        private UIHandle<IMainPanel> explorerPanel;

        public string Name
        { 
            get { return "Symbol Explorer"; }
        }


        public void Initialise(string[] args)
        {
            // Do nothing
        }

        public void SetupUI(IDebugUI UI)
        {
            this.UI = UI;

            // Menu Item
            var menuItem = UI.NewMenuItem(isToggle: true, label: "Show Symbol Explorer");
            menuItem.CheckChanged += menuItem_CheckChanged;
            UI.AddMenuItem(menuItem, "View");
        }

        void AddPanel()
        {
            var panel = UI.NewMainPanel("Symbol Explorer", new SymbolExplorer());

            panel.Closed += OnClosed;

            this.explorerPanel = new UIHandle<IMainPanel>(
                UI.AddMainPanel(panel, new PanelLocation(PanelSide.Right)),
                panel);
        }

        void menuItem_CheckChanged(object obj)
        {
            if (((IMenuItem)obj).IsChecked)
            {
                this.AddPanel();
            }
            else if (this.explorerPanel.Instance.IsOpen)
            {
                UI.RemoveMainPanel(this.explorerPanel.Handle);
                this.explorerPanel = new UIHandle<IMainPanel>();
            }
        }

        void OnClosed(object obj)
        {
            this.explorerPanel = default(UIHandle<IMainPanel>);
        }
    }
}
