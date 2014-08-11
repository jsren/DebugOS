using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugOS.Extensions
{
    public sealed class SessionViewExtension : IUIExtension
    {
        private IDebugUI   ui;
        private Int32      panelHandle;
        private IMainPanel viewPanel;
        private IMenuItem  viewMenuToggle;

        public string Name
        {
            get { return "Session View Extension"; }
        }


        public void Initialise(string[] args)
        {
            this.panelHandle = -1;
            Application.SessionChanged += OnSessionChanged;
        }

        public void SetupUI(IDebugUI UI)
        {
            this.ui = UI;

            this.viewMenuToggle = UI.NewMenuItem(isToggle: true, label: "Show Session Details");
            viewMenuToggle.CheckChanged += OnShowToggle;

            UI.AddMenuItem(viewMenuToggle, "View");
        }

        private void OnSessionChanged()
        {
            if (Application.Session == null)
            {
                this.viewMenuToggle.IsChecked = false;
            }
            else this.viewMenuToggle.IsChecked = true;
        }

        private void viewPanel_Closed(object obj)
        {
            this.viewPanel = null;
            this.viewMenuToggle.IsChecked = false;
        }

        private void ShowViewPanel()
        {
            if (this.viewPanel != null)
            {
                ui.RemoveMainPanel(this.panelHandle);
            }

            this.viewPanel = ui.NewMainPanel(title:"Session Details", 
                content:new SessionView());

            this.viewPanel.Closed += viewPanel_Closed;
            this.panelHandle = ui.AddMainPanel(this.viewPanel, new PanelLocation());

            this.viewMenuToggle.IsChecked = true;
        }

        private void OnShowToggle(object obj)
        {
            bool @checked = ((IMenuItem)obj).IsChecked;

            if (viewPanel != null && viewPanel.IsOpen && !@checked)
            {
                ui.RemoveMainPanel(panelHandle);
                this.viewPanel = null;
            }
            else if (viewPanel == null && @checked)
            {
                this.ShowViewPanel();
            }
        }
    }
}
