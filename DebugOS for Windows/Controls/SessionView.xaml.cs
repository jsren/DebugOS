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

            Application.DebuggerChanged += () => this.UpdateInfo();

            this.UpdateInfo();
		}

        public void UpdateInfo()
        {
            if (Application.Debugger == null)
            {
                this.fileList.Items.Clear();
            }
            else
            {
                HashSet<string> files = new HashSet<string>(PathComparer.OSIndependentPath);

                // Search for source files
                foreach (ObjectCodeFile obj in Application.Debugger.IncludedObjectFiles)
                {
                    foreach (CodeUnit unit in obj.Code) {
                        files.Add(unit.SourceFilepath.Trim());
                    }
                }

                // Add each file item
                foreach (string file in files.OrderBy(x=>x, PathComparer.OSIndependentFilename))
                {
                    // Ignore empty entries
                    if (!string.IsNullOrWhiteSpace(file))
                    {
                        var newItem = new SourceFileItem(file);
                        newItem.MouseDoubleClick += FileItemDoubleClick;

                        this.fileList.Items.Add(newItem);
                    }
                }
            }
        }

        void FileItemDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Open a new or existing code view
            
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