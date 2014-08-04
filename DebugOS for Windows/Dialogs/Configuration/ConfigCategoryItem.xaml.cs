using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace DebugOS
{
	public partial class ConfigCategoryItem : UserControl
	{
		
		public static DependencyProperty HeaderProperty = 
			DependencyProperty.Register("Header", typeof(string), typeof(ConfigCategoryItem));
		
        public UIElement Page { get; private set; }
		
        [Category("Common")]
		public string Header
		{
			get { return (string)this.GetValue(HeaderProperty); }
			set { this.SetValue(HeaderProperty, value); }
		}
		
		public event EventHandler Selected;

		public ConfigCategoryItem()
		{
			this.InitializeComponent();
		}

        public ConfigCategoryItem(string name, UIElement page) : this()
        {
            this.Header = name;
            this.Page   = page;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == HeaderProperty)
            {
                this.headerText.Text = (string)e.NewValue;
            }
            base.OnPropertyChanged(e);
        }
		
		public void Select()
		{
			if (this.Selected != null) this.Selected(this, null);
		}

        private void OnSelected(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	this.Select();
        }
	}
}