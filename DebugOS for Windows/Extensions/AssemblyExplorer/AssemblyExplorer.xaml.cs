using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;

namespace DebugOS.Extensions
{
	/// <summary>
	/// Interaction logic for AssemblyExplorer.xaml
	/// </summary>
	public partial class AssemblyExplorer : Window
	{
		public AssemblyExplorer()
		{
			this.InitializeComponent();

            if (Application.Debugger != null)
            {
                foreach (ObjectCodeFile assembly in Application.Debugger.IncludedObjectFiles) {
                    this.AddAssemblyItem(assembly);
                }
            }
		}

		private void OnLoadModule(object sender, System.Windows.RoutedEventArgs e)
		{
            OpenFileDialog dialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                Filter          = "Binary Files (*.bin, *.exe, *.img, *.iso, *.o)|*.bin;*.exe;*.img;*.iso;*.o|"
                                + "All Files (*.*)|*.*|Image Files (*.img, *.iso)|*.img;*.iso|"
                                + "Object Files (*.bin, *.o, *.exe)|*.bin;*.o;*.exe",
                FilterIndex     = 0,
                Title           = "Select Debug Binary",
                Multiselect     = true,
            };
            bool? result = dialog.ShowDialog(this);

            if (result.HasValue && result.Value)
            {
                foreach (string modulePath in dialog.FileNames)
                {
                    ObjectCodeFile file = this.TryLoadModule(modulePath);

                    if (file != null) {
                        this.AddAssemblyItem(file);
                    }
                }
            }
		}

		private void OnRemoveModule(object sender, System.Windows.RoutedEventArgs e)
		{
            if (this.assemblyList.SelectedItem as LoadedAssemblyItem != null)
            {
                this.assemblyList.Items.RemoveAt(this.assemblyList.SelectedIndex);
            }
		}

		private void OnOkay(object sender, System.Windows.RoutedEventArgs e)
		{
            if (Application.Debugger != null)
            {
                var assemblies = new List<ObjectCodeFile>();

                foreach (object item in this.assemblyList.Items)
                {
                    // Ignore invalid controls
                    var asmCtrl = item as LoadedAssemblyItem;
                    if (asmCtrl == null) continue;

                    // Rebase as necessary
                    long @base = (long)Utils.ParseHex64(asmCtrl.baseText.Text);
                    asmCtrl.Assembly.ActualLoadAddress = @base;

                    // Add if not already present
                    if (!Application.Debugger.IncludedObjectFiles.Contains(asmCtrl.Assembly)) {
                        Application.Debugger.IncludeObjectFile(asmCtrl.Assembly);
                    }
                    assemblies.Add(asmCtrl.Assembly);
                }
                // Remove assemblies as necessary
                foreach (ObjectCodeFile assembly in Application.Debugger.IncludedObjectFiles)
                {
                    if (!assemblies.Contains(assembly)) {
                        Application.Debugger.ExcludeObjectFile(assembly);
                    }
                }

                // Update Session Storage
                if (Application.Session != null)
                {
                    StringBuilder builder = new StringBuilder();

                    foreach (ObjectCodeFile assembly in assemblies)
                    {
                        builder.Append(assembly.Filepath);
                        builder.Append("*");
                        builder.Append(assembly.ActualLoadAddress);
                        builder.Append("?");
                    }
                    Application.Session.Properties["DebugOS.LoadedAssemblies"] = builder.ToString();
                }
            }
            else
            {
                MessageBox.Show("Loaded assemblies cannot be altered while no debugger is selected.\n"
                    + "Please configure a debugger and try again." );
            }
            this.DialogResult = true;
            this.Close();
		}

		private void OnCancel(object sender, System.Windows.RoutedEventArgs e)
		{
            // Do nothing and return
            this.DialogResult = false;
            this.Close();
		}

        private void AddAssemblyItem(ObjectCodeFile assembly)
        {
            // Check if the assembly is the primary executable
            bool isExecImg = Application.Session != null && 
                Application.Session.ImageFilepath == assembly.Filepath;

            // Add the new item
            this.assemblyList.Items.Add(new LoadedAssemblyItem(assembly, isExecImg ? "Primary Executable" : "Module"));
        }

        private ObjectCodeFile TryLoadModule(string path)
        {
            ObjectCodeFile output = null;
            try 
            {
                output = Loaders.ObjectFileLoader.Load(path, AssemblySyntax.Intel);
            }
            catch (Exception e)
            {
                string msg = String.Format("An error occurred while loading an object file. " +
                    "\n('{0}' while loading {1})", e, path);

                MessageBox.Show(msg, "Error Loading Assembly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return output;
        }
	}
}