using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DebugOS
{
	/// <summary>
	/// Interaction logic for AssemblyLineItem.xaml
	/// </summary>
	public partial class AssemblyLineItem : UserControl
	{
		AssemblyLine line;

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
	}
}