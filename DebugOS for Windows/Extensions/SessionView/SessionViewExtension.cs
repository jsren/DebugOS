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

        public void SetupUI(IDebugUI UI)
        {
            this.ui = UI;
        }

        public string Name
        {
            get { return "Session View Extension"; }
        }

        public void Initialise(string[] args)
        {
            this.panelHandle = -1;
            Application.DebuggerChanged += Application_DebuggerChanged;

            if (Application.Debugger != null)
            {
                this.Application_DebuggerChanged();
            }
        }

        void Application_DebuggerChanged()
        {
            if (Application.Debugger == null)
            {
                if (this.viewPanel != null)
                {
                    ui.RemoveMainPanel(this.panelHandle);
                }
            }
            else
            {
                if (this.viewPanel == null)
                {
                    this.viewPanel       = ui.NewMainPanel();
                    this.viewPanel.Title = "Session Details";
                    this.panelHandle     = ui.AddMainPanel(new PanelLocation(), this.viewPanel);
                }
                this.viewPanel.Content = new SessionView();
            }
        }
    }
}
