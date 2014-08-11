using System;
using System.Windows.Controls;

namespace DebugOS
{
	public partial class SmartSearchBox : UserControl
	{
		public SmartSearchBox()
		{
			this.InitializeComponent();
		}

        public bool AddCategory(ISearchCategory category)
        {
            foreach (SmartSearchCategory cat in this.resultsStack.Children)
            {
                if (cat.Category == category) return false;
            }
            this.resultsStack.Children.Add(new SmartSearchCategory(category));
            return true;
        }

        public bool RemoveCategory(ISearchCategory category)
        {
            for (int i = 0; i < this.resultsStack.Children.Count; i++)
            {
                if (((SmartSearchCategory)this.resultsStack.Children[i]).Category == category)
                {
                    this.resultsStack.Children.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(this.searchTextBox.Text))
            {
                this.promptText.Visibility = System.Windows.Visibility.Visible;

                foreach (SmartSearchCategory cat in this.resultsStack.Children)
                {
                    cat.Reset();
                }
            }
            else
            {
                this.promptText.Visibility = System.Windows.Visibility.Collapsed;

                foreach (SmartSearchCategory cat in this.resultsStack.Children)
                {
                    cat.Search(this.searchTextBox.Text);
                }
            }
        }

        private void OnLoseFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            this.ResultsBox.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void OnGainFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            this.ResultsBox.Visibility = System.Windows.Visibility.Visible;
        }
	}
}