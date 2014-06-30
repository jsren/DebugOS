/* BochsExtension.cs - (c) James S Renwick 2014
 * --------------------------------------------
 * Version 1.3.0
 */
using System;
using DebugOS.Bochs;
using System.Windows.Forms;

namespace DebugOS.Extensions
{
    /// <summary>
    /// Bochs Debugger Extension. Provides a bootstrapper for a Bochs instance.
    /// </summary>
    public class BochsExtension : IUIExtension, IDebuggerExtension
    {
        private string configPath;
        private string bochsPath;


        /// <summary>Gets the name of the extension.</summary>
        public string Name { get { return "Bochs Debugger"; } }

        
        /// <summary>
        /// Performs extension initialisation.
        /// 
        /// Here, attempts to fetch Bochs parameters from the command line
        /// or environment variables.
        /// </summary>
        /// <param name="args"></param>
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

        /// <summary>
        /// Builds the extension's user interface.
        /// </summary>
        public void SetupUI(IDebugUI UI)
        {
            return; // No user interface
        }

        /// <summary>
        /// Loads the Bochs debugger.
        /// </summary>
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

            Application.Session.Properties["BochsDebugger.BochsPath"]  = bochsPath;
            Application.Session.Properties["BochsDebugger.ConfigPath"] = configPath;

            return new BochsDebugger();
        }


        /// <summary>
        /// Displays an OpenFileDialog.
        /// </summary>
        private string ShowDialog(string title, string filter)
        {
            var dialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                Multiselect     = false,
                Filter          = filter,
                FilterIndex     = 0
            };

            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK || result == DialogResult.Yes)
            {
                return dialog.FileName;
            }
            else return null;
        }
    }
}
