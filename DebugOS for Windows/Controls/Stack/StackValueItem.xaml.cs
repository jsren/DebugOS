using System;
using System.Text;
using System.Windows.Controls;

using Visibility = System.Windows.Visibility;

namespace DebugOS.Controls
{
    /// <summary>
    /// Interaction logic for StackValueItem.xaml
    /// </summary>
    public partial class StackValueItem : UserControl
    {
        public StackValueItem(byte[] value, bool isRet = false)
        {
            InitializeComponent();
            this.IsReturnAddress = isRet;

            // Reverse value for display
            Array.Reverse(value);

            // Build the hex string
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                // Add each byte as a hex string
                builder.Append(Utils.GetHexString(value[i], fixedPlaces: 2, prefix: false));

                // Add spaces for all but the last item
                if (i + 1 != value.Length) builder.Append(' ');
            }
            this.value.Text = builder.ToString();
        }

        public bool IsReturnAddress
        {
            get { return this.returnIndicator.Visibility == Visibility.Visible; }
            set { this.returnIndicator.Visibility = (value ? Visibility.Visible : Visibility.Collapsed); }
        }
    }
}
