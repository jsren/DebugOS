using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DebugOS
{
	public partial class ToolbarItem : UserControl, Windows.IToolbarItem
	{
        private System.Drawing.Bitmap bitmap;

        public bool IsToggle { get; private set; }

        public event Action<object> Clicked;


		public ToolbarItem(bool toggle = false)
        {
			this.InitializeComponent();
            this.LayoutRoot.Background.Opacity = 0;

            this.IsToggle = toggle;

            this.MouseEnter        += (s, e) => this.LayoutRoot.Background.Opacity = 0.67;
            this.MouseLeave        += (s, e) => this.LayoutRoot.Background.Opacity = 0;
            this.MouseLeftButtonUp += (s, e) => { if (this.Clicked != null) this.Clicked(this); };
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
        public ToolbarItem(string tooltip, System.Drawing.Bitmap bitmap) : this()
        {
            this.ToolTip = tooltip;
            ((IToolbarItem)this).Icon = bitmap;
        }

        public BitmapSource Icon
        {
            get { return this.icon.Source as BitmapSource; }

            set
            {
                if (this.icon.Source != value) {
                    this.bitmap = Windows.Interop.ConvertImage((BitmapSource)(this.icon.Source = value));
                }
            }
        }

        System.Drawing.Bitmap IToolbarItem.Icon
        {
            get { return this.bitmap; }

            set
            {
                if (this.bitmap != value) {
                    this.icon.Source = Windows.Interop.ConvertImage(this.bitmap = value);
                }
            }
        }
        
    }
}