using System.Windows;
using System.Windows.Controls;

namespace DebugOS
{
	/// <summary>
	/// Interaction logic for SmartSearchResult.xaml
	/// </summary>
	public partial class SmartSearchResult : UserControl
	{
        public SearchResult Result { get; private set; }

#if DEBUG
		public SmartSearchResult()
		{
			this.InitializeComponent();
		}
#endif
        public SmartSearchResult(SearchResult result)
        {
            this.InitializeComponent();

            this.Result         = result;
            this.valueText.Text = result.Title;

            if (result.Icon != null)
                this.image.Source = Windows.Interop.ConvertImage(result.Icon);
        }

		private void OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			this.valueText.TextDecorations = TextDecorations.Underline;
		}

		private void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			this.valueText.TextDecorations = null;
		}

        private void OnMouseClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.Result.ClickCallback != null)
            {
                this.Result.ClickCallback(this.Result);
            }
        }
		
	}
}