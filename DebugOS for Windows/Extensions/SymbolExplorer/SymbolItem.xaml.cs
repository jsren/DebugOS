using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace DebugOS
{
	/// <summary>
	/// Interaction logic for SymbolItem.xaml
	/// </summary>
	public partial class SymbolItem : UserControl
	{
        internal SymbolEntry symbol;

#if DEBUG
		public SymbolItem()
		{
			this.InitializeComponent();
		}
#endif
        public SymbolItem(SymbolEntry symbol)
        {
            if ((this.symbol = symbol) == null)
                throw new ArgumentNullException("symbol");

            this.InitializeComponent();

            this.nameText .Text = symbol.Name;
            this.valueText.Text = Utils.GetHexString((ulong)symbol.Value, prefix:true);
            this.typeText .Text = symbol.Flags.ToString();
        }

        internal void SetBreakpoint()
        {
            this.bpEllipse.Visibility = System.Windows.Visibility.Visible;    
        }

        internal void ClearBreakpoint()
        {
            this.bpEllipse.Visibility = System.Windows.Visibility.Hidden;
        }

		private void OnToggleBP(object sender, MouseButtonEventArgs e)
		{
            if (this.bpEllipse.Visibility != System.Windows.Visibility.Visible)
            {
                Application.Debugger.SetBreakpoint(this.symbol.Value);
            }
            else
            {
                Application.Debugger.ClearBreakpoint(this.symbol.Value);
            }
		}
	}
}