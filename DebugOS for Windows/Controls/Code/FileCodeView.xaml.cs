using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DebugOS
{
	/// <summary>
	/// Interaction logic for FileCodeView.xaml
	/// </summary>
	public partial class FileCodeView : UserControl, IDisposable
	{
        private Stream stream;
        public string Filepath { get; private set; }

		private FileCodeView()
		{
			this.InitializeComponent();
		}

        public FileCodeView(string filepath) : this()
        {
            try
            {
                this.Filepath = Path.GetFullPath(filepath);
                this.stream   = File.OpenRead(this.Filepath);

                var reader = new StreamReader(this.stream);

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    this.codeStack.Children.Add(new TextBlock()
                    {
                        Text = line,
                        FontFamily = new System.Windows.Media.FontFamily("Consolas")
                    });
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("An error occurred while opening the source file '{0}': \n{1}",
                    filepath, e.ToString()), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (this.Filepath != null)
            {
                Application.Debugger.Suspended += this.UpdateStep;
            }
        }

        /// <summary>
        /// Performs a UI update following a step.
        /// </summary>
        private void UpdateStep(object sender, EventArgs e)
        {
            Dispatcher.Invoke((Action)delegate
            {
                /* Test if we're the current code unit and update as appropriate */
                var currentUnit = Application.Debugger.CurrentCodeUnit;

                if (currentUnit == null) return; // Return if we're lost

                // Ignore empty filepaths
                if (string.IsNullOrWhiteSpace(currentUnit.SourceFilepath)) return;

                // Get the filepath
                var currentFile = Path.GetFullPath(currentUnit.SourceFilepath);

                if (this.Filepath == currentFile)
                {
                    long     currentAddress = Application.Debugger.CurrentAddress;
                    CodeLine currentLine    = currentUnit.GetLine(currentAddress);

                    if (currentLine != null && currentLine.LineNumber != -1
                        && currentLine.LineNumber <= codeStack.Children.Count)
                    {
                        // Get the line object
                        var block = (TextBlock)codeStack.Children[(int)currentLine.LineNumber - 1];

                        // Get the point offset of the line object
                        Point p = block.TranslatePoint(new Point(), codeStack);

                        // Position the current rect as necessary
                        this.currentGrid.Margin = new Thickness(0, p.Y - 2, 0, 0);
                        this.currentGrid.Height = block.ActualHeight + 2;
                    }
                }
            });
        }

        private TextBlock GetTextBlock(string line)
        {
            return null;
        }

        public void Dispose()
        {
            this.stream.Dispose();
        }
    }
}