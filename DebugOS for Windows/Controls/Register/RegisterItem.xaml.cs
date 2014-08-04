using System;
using System.Text;
using System.Windows.Controls;

namespace DebugOS
{
    /// <summary>
    /// A user interface control representing a processor register.
    /// </summary>
	public partial class RegisterItem : UserControl
	{
		public RegisterItem()
        {
			this.InitializeComponent();
		}

        public RegisterItem(string name, byte[] value) : this()
        {
            this.nameText.Text = name;
            this.SetValue(value);
        }

        public void SetValue(byte[] value)
        {
            StringBuilder text = new StringBuilder();

            // Reverse for display
            Array.Reverse(value);

            for (int i = 0; i < value.Length; i++)
            {
                // Add each byte to the string
                text.Append(Utils.GetHexString(value[i], fixedPlaces: 2, prefix: false));

                // Add a space if not the last item
                if (i != value.Length - 1) text.Append(' ');
            }
            this.valueText.Text = text.ToString();
        }
	}
}