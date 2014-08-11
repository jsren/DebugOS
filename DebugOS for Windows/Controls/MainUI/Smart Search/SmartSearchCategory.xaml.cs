using System.Collections.Generic;
using System.Windows.Controls;


namespace DebugOS
{
	public partial class SmartSearchCategory : UserControl
	{
        public ISearchCategory Category { get; private set; }

#if DEBUG
		public SmartSearchCategory()
		{
			this.InitializeComponent();
		}
#endif
        public SmartSearchCategory(ISearchCategory category)
        {
            this.InitializeComponent();

            this.Category        = category;
            this.headerText.Text = category.Header;

            this.Reset();
        }

        public void AddResult(SearchResult result)
        {
            this.itemsStack.Children.Add(new SmartSearchResult(result));
        }

        public void Reset()
        {
            this.itemsStack.Children.Clear();

            foreach (SearchResult result in this.Category.Reset())
            {
                this.AddResult(result);
            }
        }

        public void Search(string searchString)
        {
            this.itemsStack.Children.Clear();

            foreach (SearchResult result in this.Category.GetResults(searchString))
            {
                this.AddResult(result);
            }
        }
    }
}