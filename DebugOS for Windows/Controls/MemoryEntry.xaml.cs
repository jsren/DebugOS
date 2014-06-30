using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DebugOS
{
	public partial class MemoryEntry : UserControl
	{
		public MemoryEntry(string Address, string Value)
		{
			this.InitializeComponent();
			
			this.addressLabel.Text = Address;
			this.valueInput.Text = Value;
		}
	}
}