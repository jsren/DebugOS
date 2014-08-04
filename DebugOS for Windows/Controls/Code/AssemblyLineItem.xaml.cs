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
        Breakpoint breakpoint;

        
        public bool IsCurrentItem { get; private set; }
		

		public AssemblyLineItem()
		{
			this.InitializeComponent();
		}
		public AssemblyLineItem(AssemblyLine asmLine)
		{
			this.InitializeComponent();

            this.opcode.Text = asmLine.Instruction;
            this.meta.Text   = asmLine.Metadata;

            if (asmLine.Parameters.Length != 0) {
                this.operands.Text = asmLine.Parameters.Aggregate((x, y) => x + ", " + y);
            }

			this.line = asmLine;
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
            if (this.Breakpoint == null)
            {
                this.Breakpoint = new Breakpoint(true, this.line.Offset);
            }
            else
            {
                this.Breakpoint = null;
            }
        }

        public Breakpoint Breakpoint
        {
            get { return this.breakpoint; }

            set
            {
                // The breakpoint has been cleared
                if (value == null && this.breakpoint != null)
                {
                    try {
                        Application.Debugger.ClearBreakpoint(this.breakpoint);
                    }
                    finally
                    {
                        this.breakpoint = null;
                        this.breakpointFill.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
                // A new breakpoint has been set
                else
                {
                    if (this.line.MachineCode.Length != 0)
                    {
                        try
                        {
                            Application.Debugger.SetBreakpoint(this.breakpoint = value);
                            this.breakpointFill.Visibility = System.Windows.Visibility.Visible;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("[ERROR] Error setting breakpoint at '{0}': {1}",
                                Utils.GetHexString((ulong)this.line.Offset, prefix: true), e);

                            this.breakpoint = null;
                            this.breakpointFill.Visibility = System.Windows.Visibility.Collapsed;
                        }
                    }
                }
            }
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