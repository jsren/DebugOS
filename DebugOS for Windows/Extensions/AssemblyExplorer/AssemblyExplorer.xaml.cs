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

            this.RefreshItems();
		}

        private void RefreshItems()
        {
            this.assemblyList.Items.Clear();

            // Add current images to UI
            if (Application.Session != null)
            {
                foreach (ObjectCodeFile assembly in Application.Session.LoadedImages)
                {
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
                    try
                    {
                        Loader.LoadImage(modulePath, null);
                    }
                    catch (Exception x)
                    {
                        var msg = String.Format("An error occured and the module '{0}' "
                            + "has not been loaded:\n{1}", modulePath, x.ToString());

                        MessageBox.Show(msg, "Error Loading Module", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
                this.RefreshItems();
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
            if (Application.Session != null)
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

                    assemblies.Add(asmCtrl.Assembly);
                }

                // Remove assemblies as necessary
                foreach (ObjectCodeFile assembly in Application.Session.LoadedImages)
                {
                    if (!assemblies.Contains(assembly))
                    {
                        Application.Session.RemoveImage(assembly);
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

                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("No session is currently active. \n"
                    + "A new session must be started before changes can be made.");

                this.DialogResult = false;
            }

            // Close the dialog to finish
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
            // Add the new item
            this.assemblyList.Items.Add(new LoadedAssemblyItem(assembly, "Module"));
        }
	}
}