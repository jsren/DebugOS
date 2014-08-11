using System;
using System.Windows.Controls;

using Bitmap      = System.Drawing.Bitmap;
using WPFMenuItem = System.Windows.Controls.MenuItem;

namespace DebugOS
{
    public class MenuItem : WPFMenuItem, IMenuItem
    {
        Image  icon;
        Bitmap bitmap;

        public MenuItem(bool toggle = false, bool top = false)
        {
            base.IsCheckable = toggle;

            base.Click     += (s, e) => { if (this.Clicked != null)      this.Clicked(this); };
            base.Checked   += (s, e) => { if (this.CheckChanged != null) this.CheckChanged(this); };
            base.Unchecked += (s, e) => { if (this.CheckChanged != null) this.CheckChanged(this); };

            if (!top) base.IsVisibleChanged += App.OnPopupVisChange;
        }

        public event Action<object> Clicked;
        public event Action<object> CheckChanged;

        public string Shortcut
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                if (value != null)
                    throw new NotImplementedException();
            }
        }

        public string Label
        {
            get { return base.Header as string; }
            set { base.Header = value; }
        }

        public new Bitmap Icon
        {
            get { return bitmap; }
            set
            {
                if (value == null)
                {
                    base.Icon = this.icon = null;
                }
                else
                {
                    if (this.icon == null) {
                        base.Icon = this.icon = new Image() { Width = 16, Height = 16 };
                    }
                    this.icon.Source = Windows.Interop.ConvertImage((bitmap = value));
                }
            }
        }

        internal bool HeaderEquals(string value)
        {
            return ((string)base.Header).Replace("_", "") == value;
        }
        
    }
}
