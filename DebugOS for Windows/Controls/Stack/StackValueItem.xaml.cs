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
        public StackValueItem(UInt32 value)
        {
            InitializeComponent();
            this.IsReturnAddress = false;

            string stringValue = Utils.GetHexString(value, Application.Debugger.AddressWidth * 2);

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < stringValue.Length; i += 2)
            {
                builder.Append(stringValue[i]);
                builder.Append(stringValue[i + 1]);

                if (i + 2 < stringValue.Length)
                {
                    builder.Append(' ');
                }
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
