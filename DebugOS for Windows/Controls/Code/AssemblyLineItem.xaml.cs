using System;
using System.Linq;
using System.Windows.Controls;

namespace DebugOS
{
	/// <summary>
	/// Interaction logic for AssemblyLineItem.xaml
	/// </summary>
	public partial class AssemblyLineItem : UserControl
	{
		AssemblyLine line;

        public bool IsCurrentItem { get; private set; }
        public Breakpoint Breakpoint { get; private set; }
		

		public AssemblyLineItem()
		{
			this.InitializeComponent();
		}
		public AssemblyLineItem(AssemblyLine asmLine)
		{
			this.InitializeComponent();

            this.line        = asmLine;
            this.opcode.Text = asmLine.Instruction;
            this.meta.Text   = asmLine.Metadata;

            if (asmLine.Parameters.Length != 0) {
                this.operands.Text = asmLine.Parameters.Aggregate((x, y) => x + ", " + y);
            }

            var bp = Application.Debugger.Breakpoints.GetBreakpoint(this.line.Offset);

            // Set breakpoint
            this.OnDebuggerBPSet(null, new BreakpointChangedEventArgs(bp));

            // Watch for breakpoint changed events
            Application.Debugger.BreakpointSet     += OnDebuggerBPSet;
            Application.Debugger.BreakpointCleared += OnDebuggerBPCleared;
		}

        private void OnDebuggerBPCleared(object sender, BreakpointChangedEventArgs e)
        {
            if (e.Breakpoint != null && e.Breakpoint.Address == this.line.Offset)
            {
                this.Breakpoint = null;
                this.Dispatcher.BeginInvoke((Action)this.UpdateBPDisplay);
            }
        }

        private void OnDebuggerBPSet(object sender, BreakpointChangedEventArgs e)
        {
            if (e.Breakpoint != null && e.Breakpoint.IsActive && 
                e.Breakpoint.Address == this.line.Offset)
            {
                this.Breakpoint = e.Breakpoint;
                this.Dispatcher.BeginInvoke((Action)this.UpdateBPDisplay);
            }
        }
		
		public void UpdateStep(uint currentAddress)
        {
            if (line.Offset <= currentAddress && line.Offset + line.MachineCode.Length > currentAddress)
            {
                currentGrid.Visibility = System.Windows.Visibility.Visible;
                this.IsCurrentItem = true;
            }
            else
            {
                currentGrid.Visibility = System.Windows.Visibility.Collapsed;
                this.IsCurrentItem = false;
            }
        }

        private void OnBreakpointClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Enable/Disable breakpoint when clicked
            if (this.Breakpoint == null)
            {
                Application.Debugger.SetBreakpoint(this.line.Offset);
            }
            else
            {
                Application.Debugger.ClearBreakpoint(this.line.Offset);
            }
            this.UpdateBPDisplay();
        }

        /// <summary>
        /// Updates whether the breakpoint icon is displayed.
        /// </summary>
        internal void UpdateBPDisplay()
        {
            if (this.Breakpoint != null)
            {
                this.breakpointFill.Visibility = System.Windows.Visibility.Visible;
            }
            else this.breakpointFill.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void OnBPHitTestEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.breakpointOutline.Visibility = System.Windows.Visibility.Visible;
        }
        private void OnBPHitTestLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.breakpointOutline.Visibility = System.Windows.Visibility.Collapsed;
        }
	}
}