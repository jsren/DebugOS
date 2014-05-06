using Microsoft.Win32;
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
	/// Interaction logic for DebuggerSelector.xaml
	/// </summary>
	public partial class DebuggerSelector : Window
	{
        /// <summary>
        /// Gets or sets the path to the image file to load.
        /// </summary>
        public string ImagePath
        {
            get { return this.imgPathText.Text; }
            set { this.imgPathText.Text = value; }
        }

        public string DebuggerName
        {
            get { return this.debuggerCombo.Text; }
        }

		public DebuggerSelector()
		{
			this.InitializeComponent();
		}

		private void OnBrowseClick(object sender, System.Windows.RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog()
            {
                CheckPathExists = true,
                Multiselect     = false,
                Filter          = "Binary Files (*.bin, *.exe, *.img, *.iso, *.o)|*.bin;*.exe;*.img;*.iso;*.o|"
                                + "All Files (*.*)|*.*|Image Files (*.img, *.iso)|*.img;*.iso|"
                                + "Object Files (*.bin, *.o, *.exe)|*.bin;*.o;*.exe",
                FilterIndex     = 0,
                Title           = "Select Debug Binary",
            };
            bool? result = dialog.ShowDialog(this);

            if (result.HasValue && result.Value) {
                this.imgPathText.Text = dialog.FileName;
            }
		}

		private void OnBeginClick(object sender, System.Windows.RoutedEventArgs e)
		{
            this.DialogResult = true;
            this.Close();
		}

		private void OnCloseClick(object sender, System.Windows.RoutedEventArgs e)
		{
			this.DialogResult = false;
			this.Close();
		}
	}
}