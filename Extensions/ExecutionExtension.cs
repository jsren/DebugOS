using System;
using System.Drawing;

using DebugOS;
using System.Collections.Generic;

namespace DebugOS.Extensions
{
    /// <summary>
    /// Built-in extension providing execution-related UI controls.
    /// </summary>
    public class ExecutionExtension : IUIExtension
    {
        public void Initialise(string[] args)
        {
            return;
        }

        public string Name { get { return "Execution UI"; } }

        public DebugSession Session { get; set; }


        public Dictionary<IMainPanel, int> panels
            = new Dictionary<IMainPanel, int>();



        private bool CheckDebugger()
        {
            return Application.Debugger != null &&
                Application.Debugger.CurrentStatus != DebugStatus.Disconnected;
        }

        private void Step(object _)
        {
            if (CheckDebugger()) Application.Debugger.Step();
        }

        private void Continue(object _)
        {
            if (CheckDebugger()) Application.Debugger.Continue();
        }

        private void End(object _)
        {
            if (CheckDebugger()) Application.Debugger.Disconnect();
        }

        #region Symbol Search Category

        private class SymbolSearchCategory : ISearchCategory
        {
            private static readonly SearchResult[] resetResults = new SearchResult[] {
                new SearchResult("Search string must be > 2 characters", null)
            };

            public string Header { get { return "Symbols"; } }

            public IEnumerable<SearchResult> Reset()
            {
                return resetResults;
            }

            public IEnumerable<SearchResult> GetResults(string searchString)
            {
                if (searchString.Length > 2 && Application.Session != null)
                {
                    var output = new List<SearchResult>();

                    foreach (ObjectCodeFile file in Application.Session.LoadedImages)
                    {
                        foreach (SymbolEntry symbol in file.SymbolTable)
                        {
                            if (symbol.Name.Contains(searchString))
                            {
                                string title = '<' + symbol.Name + ">\t";
                                title += Utils.GetHexString((ulong)symbol.Value, prefix: true);

                                output.Add(new SearchResult(title, Properties.Resources.symbol));
                            }
                        }
                    }
                    return output;
                }
                else return resetResults;
            }
        }

        #endregion

        #region File Search Category

        private class FileSearchCategory : ISearchCategory
        {
            private IDebugUI UI;

            public string Header { get { return "Source Files"; } }

            public FileSearchCategory(IDebugUI ui)
            {
                this.UI = ui;
            }

            public IEnumerable<SearchResult> Reset()
            {
                return new SearchResult[0];
            }

            private void OnFileItemClicked(SearchResult result)
            {
                this.UI.OpenSourceView(((FileSearchResult)result).Filepath);
            }

            private class FileSearchResult : SearchResult
            {
                public string Filepath { get; private set; }

                public FileSearchResult(string path, ResultClickCallback click) : base
                    (
                        System.IO.Path.GetFileName(path), 
                        Properties.Resources.file,
                        click
                    )
                {
                    this.Filepath = path;
                }
            }


            public IEnumerable<SearchResult> GetResults(string searchString)
            {
                if (Application.Session != null)
                {
                    var output = new List<SearchResult>();

                    foreach (ObjectCodeFile image in Application.Session.LoadedImages)
                    {
                        foreach (string file in image.SourceFiles)
                        {
                            if (file.ToLowerInvariant().Contains(searchString.ToLowerInvariant()))
                            {
                                output.Add(new FileSearchResult(file, OnFileItemClicked));
                            }
                        }
                    }
                    return output;
                }
                else return new SearchResult[0];
            }
        }

        #endregion

        public void SetupUI(IDebugUI UI)
        {
            #region Menu Bar

            var stepMenuItem = UI.NewMenuItem();
            {
                stepMenuItem.Label    = "Step One";
                stepMenuItem.Icon     = Properties.Resources.arrow_right;
                stepMenuItem.Clicked += this.Step;
            }
            var continueMenuItem = UI.NewMenuItem();
            {
                continueMenuItem.Label    = "Continue";
                continueMenuItem.Icon     = Properties.Resources.right_circular;
                continueMenuItem.Clicked += this.Continue;
            }

            UI.AddMenuItem(stepMenuItem, "Debug");
            UI.AddMenuItem(continueMenuItem, "Debug");

            #endregion

            #region Toolbar Panel

            var panel = UI.NewToolbarPanel();
            panel.Title = "Execution";

            var stepItem = UI.NewToolbarItem();
            {
                stepItem.ToolTip  = "Step";
                stepItem.Icon     = Properties.Resources.arrow_right;
                stepItem.Clicked += this.Step;
            }
            var continueItem = UI.NewToolbarItem();
            {
                continueItem.ToolTip  = "Continue";
                continueItem.Icon     = Properties.Resources.right_circular;
                continueItem.Clicked += this.Continue;
            }
            var stopItem = UI.NewToolbarItem();
            {
                stopItem.ToolTip  = "End Debugging";
                stopItem.Icon     = Properties.Resources.stop;
                stopItem.Clicked += this.End;
            }
            panel.AddToolbarItem(stepItem);
            panel.AddToolbarItem(continueItem);
            panel.AddToolbarItem(stopItem);

            UI.AddToolbarPanel(panel);
            #endregion

            UI.AddSearchCategory(new FileSearchCategory(UI));
            UI.AddSearchCategory(new SymbolSearchCategory());
        }


        
    }
}
