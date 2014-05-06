using System;
using Microsoft.Win32;

using DebugOS.Bochs;

namespace DebugOS.Extensions
{
    public class BochsExtension : IUIExtension, IDebuggerExtension
    {
        public void SetupUI(IDebugUI UI) {
            return;
        }

        public string Name { get { return "Bochs Debugger"; } }


        private string ShowDialog(string title, string filter)
        {
            var dialog = new OpenFileDialog()
            {
                CheckPathExists = true,
                Multiselect     = false,
                Filter          = filter,
                FilterIndex     = 0
            };

            bool? result = dialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                return dialog.FileName;
            }
            else return null;
        }

        private string configPath;
        private string bochsPath;


        public void Initialise(string[] args)
        {
            // Check for bochs args
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-bxrc") {
                    configPath = args[i + 1];
                }
                if (args[i] == "-bxpath") {
                    bochsPath = args[i + 1];
                }
            }

            // If not given, try to get bochs path from env. var
            bochsPath = bochsPath ?? System.Environment.GetEnvironmentVariable("BOCHSHOME");

            // If directory, not file
            if (System.IO.Directory.Exists(bochsPath))
            {
                bochsPath = System.IO.Path.Combine(bochsPath, "bochsdbg.exe");
            }
        }

        public IDebugger LoadDebugger()
        {
            if (configPath == null)
            {
                if ((configPath = ShowDialog("Select Bochs Configuration File", 
                    "Bochs Configuration File (*.bxrc)|*.bxrc")) == null)
                {
                    throw new Exception("Config file not specified.");
                }
            }
            if (bochsPath == null)
            {
                if ((bochsPath = ShowDialog("Select Bochs Debug Executable",
                    "Bochs Executable|bochsdbg.exe")) == null)
                {
                    throw new Exception("Bochs installation path not specified.");
                }
            }
            return new BochsDebugger(bochsPath, configPath);
        }
    }
}
