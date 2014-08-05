/* TabControl.cs - (c) James S Renwick 2014
 * ----------------------------------------
 * Version 1.4.1
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace DebugOS
{
	/// <summary>
	/// A control which contains multiple items between which the user can switch.
	/// </summary>
	public partial class TabControl : UserControl
	{
        private int           selectedIndex;
        private TabPlacement  placement;
        private List<TabItem> tabs;

        /// <summary>
        /// Creates a new tab control.
        /// </summary>
		public TabControl()
		{
            this.selectedIndex = -1;
            this.tabs          = new List<TabItem>();

			this.InitializeComponent();
		}

        /// <summary>
        /// Gets or sets the placement of the control's tabs.
        /// </summary>
        [Category("Appearance")]
        public TabPlacement Placement
        {
            get { return this.placement; }

            set
            {
                this.placement = value;

                switch (value)
                {
                    case TabPlacement.Top:
                        Grid.SetRow(tabScroll, 0); break;
                    case TabPlacement.Bottom:
                        Grid.SetRow(tabScroll, 2); break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the index of the selected tab.
        /// One tab must always be selected: -1 is not a valid value.
        /// </summary>
        [Category("Common")]
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

        /// <summary>
        /// Selects the given tab item.
        /// </summary>
        /// <param name="item">The tab to select.</param>
        public void SelectTab(TabItem item)
        {
            this.SelectTab(this.tabs.IndexOf(item));
        }
        /// <summary>
        /// Selects the tab item at the given index.
        /// </summary>
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
            // Ignore values of -1
        }

        /// <summary>
        /// Adds a new tab to the tab control.
        /// </summary>
        /// <param name="title">The tab's header.</param>
        /// <param name="content">The content to display when the tab is selected.</param>
        public void AddTab(string title, object content)
        {
            this.AddTab(new TabItem(title)
            {
                Content   = content,
                Placement = this.placement
            });
        }
        /// <summary>
        /// Adds the given tab item to the tab control.
        /// </summary>
        /// <param name="newTab">The tab item to add.</param>
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

        /// <summary>
        /// Removes the given tab item from the tab control.
        /// </summary>
        /// <param name="tabItem">The tab to remove.</param>
        public void RemoveTab(TabItem tabItem)
        {
            this.OnTabClosed(tabItem);
        }


        /// <summary>
        /// Called when a tab's content is changed.
        /// </summary>
        private void OnTabContentChanged(TabItem tab, object content)
        {
            if (tabs[selectedIndex] == tab) {
                this.content.Content = content;
            }
        }

        /// <summary>
        /// Called when a tab has been closed.
        /// </summary>
        private void OnTabClosed(TabItem tab)
        {
            // Check that the tab exists
            if (this.tabStack.Children.Contains(tab))
            {
                // Remove the visual tab
                this.tabStack.Children.Remove(tab);

                // Update selection
                if (tab.IsSelected)
                {
                    if (tabs.Count == 0)
                    {
                        this.content.Content = null;
                    }
                    else this.SelectTab(0);
                }
                else this.selectedIndex--;
            }

            // Remove the virtual tab
            tabs.Remove(tab);

            // Clear the tab control if empty
            if (this.SelectedIndex == -1 || this.tabStack.Children.Count == 0)
            {
                this.content.Content = null;
            }
        }

        /// <summary>
        /// Called when a tab has been selected.
        /// </summary>
        private void OnTabSelected(TabItem tab)
        {
            if (!tab.IsSelected)
            {
                this.SelectTab(tabs.IndexOf(tab));
            }
        }
    }
}