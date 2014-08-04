using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DebugOS.Extensions
{
    public sealed class SessionExtension : IUIExtension
    {
        public string Name { get { return "Session Manager"; } }


        public void Initialise(string[] args)
        {
            return;
        }

        public void SetupUI(IDebugUI UI)
        {
            var newSessionMenu    = UI.NewMenuItem();
            var saveSessionMenu   = UI.NewMenuItem();
            var saveAsSessionMenu = UI.NewMenuItem();
            var loadSessionMenu   = UI.NewMenuItem();

            newSessionMenu.Label    = "New Session";
            saveSessionMenu.Label   = "Save Session";
            saveAsSessionMenu.Label = "Save Session As...";
            loadSessionMenu.Label   = "Load Session...";

            newSessionMenu.Clicked    += OnNewSession;
            saveSessionMenu.Clicked   += OnSaveSession;
            saveAsSessionMenu.Clicked += OnSaveAsSession;
            loadSessionMenu.Clicked   += OnLoadSession;

            UI.AddMenuItem("Debug", newSessionMenu);
            UI.AddMenuItem("Debug", saveSessionMenu);
            UI.AddMenuItem("Debug", saveAsSessionMenu);
            UI.AddMenuItem("Debug", loadSessionMenu);
        }

        void OnNewSession(object sender)
        {
            this.InitChangeSession(sender);

            try
            {
                /* This should be handled by the main window
                 * as a request to create a new session. */
                Application.Session = null; 
            }
            catch (Exception e)
            {
                MessageBox.Show("An error occured while loading the session: " + e.ToString(),
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void OnSaveSession(object sender)
        {
            if (Application.Session != null)
            {
                if (Application.Session.BackingFile != null)
                {
                    try
                    {
                        Application.Session.Save(Application.Session.BackingFile);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("An error occured while saving the session: " + e.ToString(),
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else this.OnSaveAsSession(sender);
            }
            else return;
        }

        void OnSaveAsSession(object sender)
        {
            if (Application.Session != null)
            {
                using (var dialog = new SaveFileDialog()
                {
                    AddExtension    = true,
                    Filter          = "DebugOS Session Files (*.dbs)|*.dbs",
                    FilterIndex     = 0,
                    OverwritePrompt = true,
                    Title           = "Save Session",
                })
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            Application.Session.Save(dialog.FileName);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("An error occured while saving the session: " + e.ToString(),
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else return;
        }

        void OnLoadSession(object sender)
        {
            this.InitChangeSession(sender);

            using (var dialog = new OpenFileDialog()
            {
                AddExtension    = true,
                CheckFileExists = true,
                Filter          = "DebugOS Session Files (*.dbs)|*.dbs",
                FilterIndex     = 0,
                Multiselect     = false,
                Title           = "Load Saved Session"
            })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Application.Session = DebugSession.Load(dialog.FileName);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("An error occured while loading the session: " + e.ToString(),
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        void InitChangeSession(object sender)
        {
            if (Application.Debugger != null &&
                Application.Debugger.CurrentStatus != DebugStatus.Disconnected)
            {
                MessageBox.Show("The session cannot be altered while the debugger is still attached",
                    "New Session", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (Application.Session != null)
            {
                var result = MessageBox.Show("Would you like to save the current session before creating a new one?",
                    "New Session", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    this.OnSaveAsSession(sender);
                }
            }
        }

    }
}
