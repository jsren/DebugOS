using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace DebugOS
{
	/// <summary>
	/// Interaction logic for SessionView.xaml
	/// </summary>
	public partial class SessionView : UserControl
	{
		public SessionView()
		{
			this.InitializeComponent();

            if (Application.Session != null)
            {
                Application.Session.ImageLoaded  += (s,e) => this.UpdateInfo();
                Application.Session.ImageRemoved += (s,e) => this.UpdateInfo();

                this.UpdateInfo();
            }
		}

        public void UpdateInfo()
        {
            if (Application.Session == null)
            {
                this.fileList.Items.Clear();
            }
            else
            {
                HashSet<string> files = new HashSet<string>(PathComparer.OSIndependentPath);

                // Search for source files
                foreach (ObjectCodeFile obj in Application.Session.LoadedImages)
                {
                    foreach (string path in obj.SourceFiles)
                    {
                        files.Add(path);
                    }
                }

                // Add each file item
                foreach (string file in files.OrderBy(x=>x, PathComparer.OSIndependentFilename))
                {
                    // Ignore empty entries
                    if (!string.IsNullOrWhiteSpace(file))
                    {
                        var newItem = new SourceFileItem(file);
                        newItem.MouseDoubleClick += OnFileDoubleClick;

                        this.fileList.Items.Add(newItem);
                    }
                }
            }
        }

        private void OnFileDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SourceFileItem fileItem;

            if ((fileItem = fileList.SelectedItem as SourceFileItem) != null)
            {
                CodeViewer.OpenCodeView(fileItem.Filepath, focus: true);
            }
        }
	}
}