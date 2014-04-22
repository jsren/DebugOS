using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace DebugOS
{
	public partial class ToolbarItem : UserControl, IToolbarItem
	{
        public bool IsToggle { get; private set; }

		public ToolbarItem(bool toggle = false)
        {
			this.InitializeComponent();
            this.LayoutRoot.Background.Opacity = 0;

            this.IsToggle = toggle;

            this.MouseLeftButtonUp += (MouseButtonEventHandler)delegate(object s, MouseButtonEventArgs e)
            {
                if (this.Clicked != null) this.Clicked(s, e);
            };

            this.MouseEnter += (s, e) => this.LayoutRoot.Background.Opacity = 0.67;
            this.MouseLeave += (s, e) => this.LayoutRoot.Background.Opacity = 0;
		}

        public ToolbarItem(string tooltip, string iconPath) : this()
        {
            this.ToolTip = tooltip;
            this.Icon    = new BitmapImage(new Uri(iconPath));
        }
        public ToolbarItem(string tooltip, BitmapSource icon) : this()
        {
            this.ToolTip = tooltip;
            this.Icon    = icon;
        }
        public ToolbarItem(string tooltip, string iconPath, 
            MouseButtonEventHandler clickedHandler) : this()
        {
            this.ToolTip = tooltip;
            this.Icon    = new BitmapImage(new Uri(iconPath, UriKind.Relative));
            this.Clicked += clickedHandler;
        }


        public System.Windows.Media.ImageSource Icon
        {
            get { return this.icon.Source; }
            set { this.icon.Source = value; }
        }

        public event MouseButtonEventHandler Clicked;
        
    }
}