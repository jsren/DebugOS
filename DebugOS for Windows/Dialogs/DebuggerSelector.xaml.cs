using Microsoft.Win32;
using System.Windows;

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

        /// <summary>
        /// Gets or sets the name of the debugger to use.
        /// </summary>
        public string DebuggerName
        {
            get { return this.debuggerCombo.Text; }

            set
            {
                this.debuggerCombo.SelectedIndex = 
                    this.debuggerCombo.Items.IndexOf(value);
            }
        }

		public DebuggerSelector()
		{
			this.InitializeComponent();

            // Get the names of the available debuggers
            foreach (Extension e in App.LoadedExtensions)
            {
                if (e.HasDebugger) {
                    this.debuggerCombo.Items.Add(e.Name);
                }
            }
            // Select the first item
            if (this.debuggerCombo.Items.Count != 0)
            {
                this.debuggerCombo.SelectedIndex = 0;
            }
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