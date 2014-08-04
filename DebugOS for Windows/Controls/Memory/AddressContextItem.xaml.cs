using System;
using System.Windows;
using System.Windows.Controls;

namespace DebugOS
{
	/// <summary>
	/// Interaction logic for AddressContextItem.xaml
	/// </summary>
	public partial class AddressContextItem : UserControl
	{
        private Address address;
        
#if DEBUG
		public AddressContextItem()
		{
			this.InitializeComponent();
		}
#endif

        public AddressContextItem(Address address)
        {
            this.InitializeComponent();

            this.address = address;
            this.addressText.Text = Utils.GetHexString((ulong)address.Value, prefix: true);
        }

        private void OnRefreshClick(object sender, RoutedEventArgs e)
        {
            if (Application.Debugger != null && Application.Debugger.CanReadMemory)
            {
                int width = Application.Session.Architecture.AddressWidth;

                try
                {
                    Application.Debugger.BeginReadMemory(this.address, width, (data) =>
                    {
                        this.Dispatcher.Invoke((Action)delegate
                        {
                            this.valueText.Text = Utils.GetHexString((ulong)Utils.LongFromBytes(data), width*2);
                        });
                    });
                }
                catch { this.valueText.Text = ""; }
            }
        }

        private void OnValueChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
        	if (string.IsNullOrWhiteSpace(valueText.Text))
			{
				this.refreshText.Visibility = System.Windows.Visibility.Visible;
			}
			else this.refreshText.Visibility = System.Windows.Visibility.Collapsed;
        }
	}
}