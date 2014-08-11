using Microsoft.Win32;
using System;
using System.Windows;

namespace DebugOS
{
	/// <summary>
	/// Interaction logic for DebuggerSelector.xaml
	/// </summary>
	public partial class NewSessionDialog : Window
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

        /// <summary>
        /// Gets or sets the name of the architecture to use.
        /// </summary>
        public string Architecture
        {
            get { return this.archCombo.Text; }

            set
            {
                this.archCombo.SelectedIndex =
                    this.archCombo.Items.IndexOf(value);
            }
        }

		public NewSessionDialog()
		{
			this.InitializeComponent();

            // Get the names of the available debuggers
            foreach (Extension e in Application.LoadedExtensions)
            {
                var ext = e.GetInterface<IDebuggerExtension>();

                if (ext != null)
                {
                    this.debuggerCombo.Items.Add(ext.Name);
                }
            }
            // Select the first item
            if (this.debuggerCombo.Items.Count != 0)
            {
                this.debuggerCombo.SelectedIndex = 0;
            }

            // Get the names of the available architectures
            foreach (Architecture a in Application.LoadedArchitectures)
            {
                this.archCombo.Items.Add(a.ID);
            }
            // Select the first item
            if (this.archCombo.Items.Count != 0)
            {
                this.archCombo.SelectedIndex = 0;
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
            bool? result = dialog.ShowDialog();

            if (result.HasValue && result.Value) {
                this.imgPathText.Text = dialog.FileName;
            }
		}

		private void OnBeginClick(object sender, System.Windows.RoutedEventArgs e)
		{
            // Actually begin a new session
            Loader.NewSession((string)this.debuggerCombo.SelectedItem, 
                (string)this.archCombo.SelectedItem);

            // Load the initial binary
            Loader.LoadImage(this.imgPathText.Text);

            this.DialogResult = true;
            this.Close();
		}

		private void OnCloseClick(object sender, System.Windows.RoutedEventArgs e)
		{
			this.DialogResult = false;
			this.Close();
		}

        private void OnLoadClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                AddExtension    = true,
                CheckFileExists = true,
                Filter          = "DebugOS Session Files (*.dbs)|*.dbs",
                FilterIndex     = 0,
                Multiselect     = false,
                Title           = "Load Saved Session"
            };

            bool? result = dialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                try
                {
                    Loader.LoadSession(dialog.FileName);
                }
                catch (Exception x)
                {
                    MessageBox.Show("An error occured while loading the session: " + x.ToString(),
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                this.Close();
            }
        }
	}
}