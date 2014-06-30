using System;
using System.Collections.Generic;
using System.Text;
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
	
	public partial class HexSearchBox : UserControl
	{
		public HexSearchBox()
		{
			this.InitializeComponent();
		}
		
		public string Prompt
		{
			get { return (string)GetValue(PromptProperty); }
			set { SetValue(PromptProperty, value); }
		}
		
		public static readonly DependencyProperty PromptProperty 
			= DependencyProperty.Register("Prompt", typeof(string), typeof(HexSearchBox), new PropertyMetadata("Search", PromptChanged));
		
		private static void PromptChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			((HexSearchBox)sender).promptLabel.Text = e.NewValue as string;
		}

        public string Text
        {
            get { return (string)searchText.GetValue(TextBox.TextProperty); }
            set { searchText.SetValue(TextBox.TextProperty, value); }
        }

		private void OnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			const System.Windows.Visibility visible = System.Windows.Visibility.Visible;
			const System.Windows.Visibility collapsed = System.Windows.Visibility.Collapsed;
			
			promptLabel.Visibility = searchText.Text.Trim() == string.Empty ? visible : collapsed;
			
			int caretPos = searchText.CaretIndex;
			
			string content = searchText.Text.Trim();
			content = content.ToUpper();
			
			if (content.Length > 1 && content[1] == 'X') { 
                content = content.Remove(1,1).Insert(1, "x");
            }
			
			searchText.Text = content;
            searchText.CaretIndex = caretPos;
			
			if (TextChanged != null) TextChanged(this, null);
		}
		
		public event EventHandler TextChanged;
	}
}