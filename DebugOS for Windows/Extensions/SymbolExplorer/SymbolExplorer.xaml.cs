using System.Collections.Generic;
using System.Windows.Controls;

namespace DebugOS
{

	public partial class SymbolExplorer : UserControl
	{
		public SymbolExplorer()
		{
			this.InitializeComponent();

            Application.SessionChanged  += Application_SessionChanged;
            Application.DebuggerChanged += Application_DebuggerChanged;

            this.Application_SessionChanged();
            this.Application_DebuggerChanged();
		}

        private void Application_DebuggerChanged()
        {
            if (Application.Debugger != null)
            {
                Application.Debugger.BreakpointSet     += (s, bp) => UpdateBreakpoints();
                Application.Debugger.BreakpointCleared += (s, bp) => UpdateBreakpoints();
            }
            UpdateBreakpoints();
        }

        private void UpdateBreakpoints()
        {
            IEnumerable<Breakpoint> breakpoints;

            if (Application.Debugger == null) {
                breakpoints = new Breakpoint[0];
            }
            else breakpoints = Application.Debugger.Breakpoints;

            foreach (SymbolItem item in this.itemStack.Children)
            {
                item.ClearBreakpoint();
                foreach (Breakpoint bp in breakpoints)
                {
                    if (item.symbol.Value == bp.Address && bp.IsActive)
                    {
                        item.SetBreakpoint();
                        break;
                    }
                }
            }
        }

        void Application_SessionChanged()
        {
            this.itemStack.Children.Clear();

            if (Application.Session != null)
            {
                foreach (ObjectCodeFile file in Application.Session.LoadedImages)
                {
                    foreach (SymbolEntry symbol in file.SymbolTable)
                    {
                        if ((symbol.Flags & SymbolFlags.Function) != 0)
                        {
                            this.itemStack.Children.Add(new SymbolItem(symbol));
                        }
                    }
                }
            }
        }
	}
}