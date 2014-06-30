using System;
using System.Windows.Controls;

using CtrlVisible = System.Windows.Visibility;

namespace DebugOS
{
	/// <summary>
	/// Interaction logic for TabItem.xaml
	/// </summary>
	public partial class TabItem : UserControl
	{
        private object content;
        private TabPlacement placement;

		public TabItem()
		{
			this.InitializeComponent();

            hitBox.MouseLeftButtonUp  += (s, e) => { if (Selected != null)     Selected(this); };
            close.MouseLeftButtonDown += (s, e) => { if (CloseClicked != null) CloseClicked(this); };
		}
        public TabItem(string title) : this()
        {
            this.Title = title;
        }

        public new object Content
        {
            get { return this.content; }

            set
            {
                this.content = value;
                if (ContentChanged != null) ContentChanged(this, value);
            }
        }

        public string Title
        {
            get { return titleText.Text; }
            set { this.ToolTip = (titleText.Text = value); }
        }

        public bool IsSelected
        {
            get { return activeBG.Visibility == CtrlVisible.Visible; }

            set
            {
                activeBG.Visibility = value ? CtrlVisible.Visible : CtrlVisible.Collapsed;

                if (value  && Selected   != null) Selected(this);
                if (!value && Deselected != null) Deselected(this);
            }
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
                        Grid.SetRow(highlights, 2); break;
                    case TabPlacement.Bottom:
                        Grid.SetRow(highlights, 0); break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public event Action<TabItem> Selected;
        public event Action<TabItem> Deselected;
        public event Action<TabItem> CloseClicked;
        public event Action<TabItem, object> ContentChanged;
	}
}