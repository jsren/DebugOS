using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Documents;

namespace DebugOS
{
	/// <summary>
	/// Interaction logic for MenuBar.xaml
	/// </summary>
	public partial class MenuBar : UserControl
	{
		public MenuBar()
		{
			this.InitializeComponent();
		}

        public void AddItem(string path, MenuItem item)
        {
            int      index = 0;
            string[] split = path.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

            // Add a top-level item
            if (split.Length == 0)
            {
                menubar.Items.Add(item);
            }
            // Walk the entire path until we find the new parent
            else
            {
                foreach (object obj in menubar.Items)
                {
                    var topMenu = obj as MenuItem;
                    // Check the current top-level item's header
                    if (topMenu != null && topMenu.HeaderEquals(split[index]))
                    {
                        // If we're at the end of the path
                        if (++index == split.Length)
                        {
                            topMenu.Items.Add(item);
                            return;
                        }
                        // Otherwise, loop through child items
                        else
                        {
                            MenuItem curMenu = topMenu;
                            while (true)
                            {
                                bool found = false;
                                foreach (object o in curMenu.Items)
                                {
                                    var menuitem = o as MenuItem;
                                    // Check the current item's header
                                    if (menuitem != null && menuitem.HeaderEquals(split[index]))
                                    {
                                        // If we're at the end of the path
                                        if (++index == split.Length)
                                        {
                                            menuitem.Items.Add(item);
                                            return;
                                        }
                                        // Otherwise select the next Menu Item for inspection
                                        else { curMenu = menuitem; }
                                        // Break upon find
                                        found = true;
                                        break;
                                    }
                                }
                                // Throw exception if not found
                                if (!found) goto Error;
                            }
                        }
                    }
                }
            Error:
                throw new InvalidOperationException(
                                        "Cannot add menu item - given menu path does not exist");
            }
        }


        public IEnumerable<MenuItem> Items
        {
            get
            {
                List<MenuItem> output = new List<MenuItem>(this.menubar.Items.Count);

                foreach (object obj in this.menubar.Items) {
                    output.Add((MenuItem)obj);
                }
                return output;
            }
        }
	}
}