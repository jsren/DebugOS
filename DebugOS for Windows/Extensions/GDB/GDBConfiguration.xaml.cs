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
using System.Windows.Shapes;

namespace DebugOS
{
	/// <summary>
	/// Interaction logic for GDBConfiguration.xaml
	/// </summary>
	public partial class GDBConfiguration : Window
	{
		public GDBConfiguration()
		{
			this.InitializeComponent();
		}
		
		public string Host
		{
			get { return this.hostText.Text; }
			set { this.hostText.Text = value; }
		}

		public string Port
		{
			get { return this.portText.Text; }
			set { this.portText.Text = value; }
		}
		
		private void OnContinue(object sender, System.Windows.RoutedEventArgs e)
		{
			this.DialogResult = true;
			this.Close();
		}
	}
}