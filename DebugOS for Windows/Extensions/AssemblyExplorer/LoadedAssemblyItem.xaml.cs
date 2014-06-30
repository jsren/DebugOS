using System.Windows.Controls;

namespace DebugOS.Extensions
{
	/// <summary>
	/// Interaction logic for LoadedAssemblyItem.xaml
	/// </summary>
	public partial class LoadedAssemblyItem : UserControl
	{
        public ObjectCodeFile Assembly { get; private set; }
        
        private LoadedAssemblyItem()
		{
			this.InitializeComponent();
		}

        public LoadedAssemblyItem(ObjectCodeFile assembly, string type) : this()
        {
            this.Assembly        = assembly;
            this.typeText.Text   = type;
            this.baseText.Text   = Utils.GetHexString((ulong)assembly.ActualLoadAddress, prefix: false);
            this.sourceText.Text = assembly.Filepath;
        }
	}
}