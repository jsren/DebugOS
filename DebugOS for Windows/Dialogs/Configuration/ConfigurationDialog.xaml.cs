using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DebugOS
{
	public partial class ConfigurationDialog : Window
	{
        private int nextID;
        private Dictionary<int, ConfigCategoryItem> pages;
        private SessionProperties tempProperties;

		public ConfigurationDialog()
		{
            this.InitializeComponent();

            this.pages = new Dictionary<int, ConfigCategoryItem>();

            // Create a temporary copy of the SessionProperties to allow
            // rolling back the changes.
            if (Application.Session != null)
            {
                this.tempProperties = new SessionProperties(
                    Application.Session.Properties);
            }
            else this.tempProperties = new SessionProperties();

            // Set the data context as the temporary session properties
            this.DataContext = this.tempProperties;
		}

        public int AddPage(string name, UIElement page)
        {
            var item = new ConfigCategoryItem(name, page);

            item.Selected += (s,e) =>
            {
                this.pageContent.Content = item.Page;
            };

            this.itemStack.Children.Add(item);

            this.pages.Add(nextID, item);
            return nextID++;
        }

        public void RemovePage(int handle)
        {
            if (this.pages.ContainsKey(handle))
            {
                this.itemStack.Children.Remove(this.pages[handle]);
                this.pages.Remove(handle);
            }
        }
	}
}