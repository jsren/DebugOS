using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugOS
{
    public static class CodeViewer
    {
        private static Dictionary<MainPanel, int> codeViews = new Dictionary<MainPanel, int>();

        public static void Initialise()
        {
            Application.DebuggerChanged += OnDebuggerChanged;
            Application.SessionChanged  += OnSessionChanged;
        }

        static void OnDebuggerChanged()
        {
            if (Application.Debugger != null) {
                Application.Debugger.Suspended += Debugger_Suspended;
            }
        }

        public static IMainPanel GetCodeView(CodeUnit unit)
        {
            foreach (MainPanel panel in codeViews.Keys)
            {
                UnitCodeView view = panel.Content as UnitCodeView;

                if (view != null && view.CodeUnit == unit)
                {
                    return panel;
                }
            }
            return null;
        }

        public static IMainPanel GetCodeView(string sourceFilepath)
        {
            if (string.IsNullOrWhiteSpace(sourceFilepath)) return null;

            foreach (MainPanel panel in codeViews.Keys)
            {
                FileCodeView view = panel.Content as FileCodeView;

                if (view != null && 
                    PathComparer.OSIndependentPath.Equals(sourceFilepath, view.Filepath)) 
                {
                    return panel;
                }
            }
            return null;
        }

        public static int OpenCodeView(CodeUnit unit, bool focus = true)
        {
            int handle = -1;

            // Get the panel if it is already open
            MainPanel panel = GetCodeView(unit) as MainPanel;
            if (panel != null) { handle = codeViews[panel]; }

            App.MainWindow.InvokeMethod((Action)delegate
            {
                // Create a new panel if not already open
                if (handle == -1)
                {
                    // Create panel
                    var newPanel = new MainPanel()
                    {
                        Title = Application.Debugger.CurrentCodeUnit.Name,
                        Content = new UnitCodeView(unit),
                        IsOpen = true
                    };
                    // De-register view on close
                    newPanel.Closed += (sender) => codeViews.Remove((MainPanel)sender);
                    // Add panel
                    handle = App.MainWindow.AddMainPanel(new PanelLocation(), newPanel);
                    codeViews.Add(newPanel, handle);
                }
                if (focus) App.MainWindow.FocusMainPanel(handle);
            });

            return handle;
        }

        public static int OpenCodeView(string sourceFilepath, bool focus = true)
        {
            int handle = -1;

            // Get a valid Windows path in case it's Cygwin
            sourceFilepath = Utils.GetWindowsPath(sourceFilepath);

            // Get the panel if it is already open
            MainPanel panel = GetCodeView(sourceFilepath) as MainPanel;
            if (panel != null) { handle = codeViews[panel]; }

            App.MainWindow.InvokeMethod((Action)delegate
            {
                // Create a new panel if not already open
                if (handle == -1)
                {
                    // Create panel
                    var newPanel = new MainPanel()
                    {
                        Title    = System.IO.Path.GetFileName(sourceFilepath),
                        Content  = new FileCodeView(sourceFilepath),
                        IsOpen   = true
                    };
                    // De-register view on close
                    newPanel.Closed += (sender) => codeViews.Remove((MainPanel)sender);
                    // Add panel
                    handle = App.MainWindow.AddMainPanel(new PanelLocation(PanelSide.Right), newPanel);
                    codeViews.Add(newPanel, handle);
                }
                if (focus) App.MainWindow.FocusMainPanel(handle);
            });

            return handle;
        }

        static void Debugger_Suspended(object sender, EventArgs e)
        {
            MainWindow window = App.MainWindow;

            int i = -1;

            if (i != -1)
            {
                Application.Debugger.BeginReadMemory(new Address(i), 0x100, (arr) =>
                    {
                        System.Windows.MessageBox.Show(Utils.IntFromBytes(arr).ToString());
                    }
                );
            }

            MainPanel unitPanel = null;
            MainPanel filePanel = null;

            // Check if the current unit is already displayed
            // First look for a unit view

            if (Application.Debugger.CurrentCodeUnit == null) return;

            unitPanel = (MainPanel)GetCodeView(Application.Debugger.CurrentCodeUnit);
            filePanel = (MainPanel)GetCodeView(Application.Debugger.CurrentCodeUnit.SourceFilepath);

            // If there's already a visible file view, just keep that
            if (filePanel == null || !filePanel.IsVisible)
            {
                // Otherwise, check if a new view must be opened
                OpenCodeView(Application.Debugger.CurrentCodeUnit, focus: true);
            }
        }

        static void OnSessionChanged()
        {
            var window = App.MainWindow;

            foreach (MainPanel view in codeViews.Keys) {
                window.RemoveMainPanel(view);
            }
            codeViews.Clear();
        }


    }
}
