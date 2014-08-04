using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugOS
{
    internal sealed class MainPanel : IMainPanel
    {
        private TabItem tab;

        public string Title
        {
            get { return tab.Title; }
            set { tab.Title = value; }
        }

        public object Content
        {
            get { return tab.Content; }
            set { tab.Content = value; }
        }

        public bool IsOpen { get; internal set; }
        public bool IsVisible { get; private set; }

        internal PanelLocation Location { get; set; }

        internal TabItem GetTabItem() { return this.tab; }

        public event Action<object> GainedFocus;
        public event Action<object> LostFocus;
        public event Action<object> Closed;


        public MainPanel()
        {
            this.tab = new TabItem();
            this.tab.CloseClicked += (t) =>
            {
                this.IsOpen = false;
                if (Closed != null) { Closed(this); }
            };
            this.tab.Selected += (t) => 
            {
                this.IsVisible = true;
                if (GainedFocus != null) { GainedFocus(this); }
            };
            this.tab.Deselected += (t) => 
            {
                this.IsVisible = false;
                if (LostFocus != null) { LostFocus(this); }
            };
        }
        
    }
}
