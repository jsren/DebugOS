using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;

namespace DebugOS
{
	/// <summary>
	/// Interaction logic for TabControl.xaml
	/// </summary>
	public partial class TabControl : UserControl
	{
        int selectedIndex;
        TabPlacement placement;
        List<TabItem> tabs;

		public TabControl()
		{
            this.selectedIndex = 0;
			this.InitializeComponent();
            this.tabs = new List<TabItem>();
		}

        public TabPlacement Placement
        {
            get { return this.placement; }

            set
            {
                this.placement = value;

                switch (value)
                {
                    case TabPlacement.Top:
                        Grid.SetRow(content, 2); break;
                    case TabPlacement.Bottom:
                        Grid.SetRow(content, 0); break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public int SelectedIndex
        {
            get { return selectedIndex; }

            set
            {
                if (value > tabs.Count || value < 0) { 
                    throw new IndexOutOfRangeException();
                }
                this.SelectTab(value);
            }
        }

        public void SelectTab(TabItem item)
        {
            this.SelectTab(this.tabs.IndexOf(item));
        }
        private void SelectTab(int tabIndex)
        {
            // Skip if already selected
            if (selectedIndex != tabIndex)
            {
                // Deselect previous if applicable
                if (selectedIndex >= 0 && selectedIndex < tabs.Count)
                {
                    tabs[selectedIndex].IsSelected = false;
                }
                // Select next
                if (tabIndex != -1)
                {
                    tabs[tabIndex].IsSelected = true;
                    this.content.Content      = tabs[tabIndex].Content;
                }
                else this.content.Content = null;
                // Update index
                selectedIndex = tabIndex;
            }
            if (tabIndex != -1)
            {
                // Bring the tab into view if it's scrolled
                tabs[selectedIndex].BringIntoView();
            }
        }

        public void AddTab(string title, object content)
        {
            var newTab = new TabItem(title);

            newTab.Content         = content;
            newTab.Placement       = this.placement;

            this.AddTab(newTab);
        }
        public void AddTab(TabItem newTab)
        {
            this.tabStack.Children.Add(newTab);
            this.tabs.Add(newTab);

            newTab.Selected       += OnTabSelected;
            newTab.CloseClicked   += OnTabClosed;
            newTab.ContentChanged += OnTabContentChanged;

            // If the only tab, select
            if (this.tabs.Count == 1) this.SelectTab(0);
        }

        void OnTabContentChanged(TabItem tab, object content)
        {
            if (tabs[selectedIndex] == tab) {
                this.content.Content = content;
            }
        }

        void OnTabClosed(TabItem tab)
        {
            this.tabStack.Children.Remove(tab);
            tabs.RemoveAt(tabs.IndexOf(tab));

            if (tab.IsSelected)
            {
                if (tabs.Count == 0) {
                    this.content.Content = null;
                }
                else this.SelectTab(0);
            }
            else this.selectedIndex--;
        }

        void OnTabSelected(TabItem tab)
        {
            if (!tab.IsSelected)
            {
                this.SelectTab(tabs.IndexOf(tab));
            }
        }

        public void RemoveTab(TabItem tabItem)
        {
            this.OnTabClosed(tabItem);
        }
    }
}