using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DebugOS
{
	public partial class CodeLineItem : UserControl
	{
        private long address;
        private int  length;
        private bool showAsm;
        private CodeLine code;

        public bool IsCurrentItem { get; private set; }
		
		public CodeLineItem(CodeLine line, bool showAssembly = true)
		{
			this.InitializeComponent();
            this.address = line.Offset;
            this.length  = line.Size;
            this.code    = line;

            this.ShowAssembly = showAssembly;

            // Update the text
            this.codeText.Text = line.Text;
			
			for (int i = 0; i < line.Assembly.Length; i++)
			{
				this.AddItem(new AssemblyLineItem(line.Assembly[i]));
			}
		}

        public bool ShowAssembly
        {
            get { return showAsm; }
            set {
                this.childrenStack.Visibility = (showAsm = value) ? 
                    System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }

        public void UpdateStep(uint currentAddress)
        {
            if (address <= currentAddress && address + length > currentAddress)
            {
                currentGrid.Visibility = System.Windows.Visibility.Visible;
                this.IsCurrentItem = true;
            }
            else
            {
                currentGrid.Visibility = System.Windows.Visibility.Collapsed;
                this.IsCurrentItem = false;
            }

            for (int i = 0; i < childrenStack.Children.Count; i++)
            {
                AssemblyLineItem cli = childrenStack.Children[i] as AssemblyLineItem;
                if (cli == null) continue;

                cli.UpdateStep(currentAddress);
            }
        }
		
		public void AddItem(UIElement item)
		{
			this.childrenStack.Children.Add(item);
		}

        
    }
}