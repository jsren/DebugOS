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
                Application.Debugger.Stepped += Debugger_Stepped;
            }
        }

        static void Debugger_Stepped(object sender, SteppedEventArgs e)
        {
            var window = App.MainWindow;

            bool found = false;
            foreach (var pair in codeViews)
            {
                var view = (CodeView)pair.Key.Content;

                if (view.CodeUnit == Application.Debugger.CurrentCodeUnit)
                {
                    window.InvokeMethod((Action)delegate
                    {
                        window.FocusMainPanel(pair.Value);
                    });
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                window.InvokeMethod((Action)delegate
                {
                    var panel = new MainPanel()
                    {
                        Title   = Application.Debugger.CurrentCodeUnit.Name,
                        Content = new CodeView(Application.Debugger.CurrentCodeUnit)
                    };
                    int handle = window.AddMainPanel(new PanelLocation(), panel);
                    codeViews.Add(panel, handle);
                    window.FocusMainPanel(handle);
                });
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
