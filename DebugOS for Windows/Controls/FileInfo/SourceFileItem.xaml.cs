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
	/// <summary>
	/// Interaction logic for SourceFileItem.xaml
	/// </summary>
	public partial class SourceFileItem : UserControl
	{
		public string Filepath { get; private set; }
		
		private SourceFileItem()
		{
			this.InitializeComponent();
		}
		public SourceFileItem(string filepath) : this()
		{
			this.Filepath      = filepath;
			this.nameText.Text = System.IO.Path.GetFileName(filepath);
		}
	}
}