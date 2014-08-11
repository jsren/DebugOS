using System.Windows.Controls;
using System.Windows.Input;

namespace DebugOS
{

	public partial class SymbolContextItem : UserControl
	{
        private CodeUnit unit;
        private SymbolEntry symbol;
        private Breakpoint breakpoint;

#if DEBUG
		public SymbolContextItem()
		{
			this.InitializeComponent();
		}
#endif

        public SymbolContextItem(CodeUnit unit, SymbolEntry symbol)
        {
            this.InitializeComponent();

            this.unit            = unit;
            this.symbol          = symbol;
            this.symbolText.Text = symbol.Name;

            // Check if breakpoint already set - if so, give option to clear
            this.breakpoint = Application.Debugger.Breakpoints.GetBreakpoint(symbol.Value);

            if (this.breakpoint != null && !this.breakpoint.IsActive)
            {
                this.breakpoint = null;
            }
            this.UpdateBPText();
        }

        private void UpdateBPText()
        {
            if (this.breakpoint != null)
            {
                this.bpHyper.Text = "Clear Breakpoint";
            }
            else this.bpHyper.Text = "Set Breakpoint";
        }

        private void OnViewSrcClick(object sender, MouseButtonEventArgs e)
        {
            if (System.IO.File.Exists(unit.SourceFilepath))
            {
                CodeViewer.OpenCodeView(unit.SourceFilepath);
            }
        }

        private void OnBreakpointClick(object sender, MouseButtonEventArgs e)
        {
            if (this.breakpoint == null)
            {
                this.breakpoint = Application.Debugger.SetBreakpoint(this.symbol.Value);
            }
            else
            {
                Application.Debugger.ClearBreakpoint(this.symbol.Value);
                this.breakpoint = null;
            }
            this.UpdateBPText();
        }

        private void OnViewAsmClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CodeViewer.OpenCodeView(this.unit);
        }
	}
}