using System.Windows.Controls;

namespace DebugOS
{
	/// <summary>
	/// Interaction logic for ToolbarPanel.xaml
	/// </summary>
	public partial class ToolbarPanel : UserControl, IToolbarPanel
	{
		public ToolbarPanel() {
			this.InitializeComponent();
		}

        public ToolbarPanel(string title) : this() {
            this.Title = title;
        }
        public ToolbarPanel(string title, params IToolbarItem[] items) : this()
        {
            this.Title = title;
            foreach (var item in items) this.AddToolbarItem(item);
        }

        public string Title
        {
            get { return this.title.Text; }
            set { this.title.Text = value; }
        }

        public IToolbarItem[] Items
        {
            get
            {
                int i = 0;
                var output = new IToolbarItem[this.itemHost.Children.Count];

                foreach (IToolbarItem item in this.itemHost.Children) {
                    output[i++] = item;
                }
                return output;
            }
        }

        public void AddToolbarItem(IToolbarItem item) {
            this.itemHost.Children.Add((ToolbarItem)item);
        }

        public void RemoveToolbarItem(IToolbarItem item) {
            this.itemHost.Children.Remove((ToolbarItem)item);
        }
    }
}