using System.Text.RegularExpressions;

namespace System.Windows.Controls
{
    public class FormattedTextBox : TextBox
    {
        /// <summary>
        /// Dependency property governing the regex with which the text box is formatted.
        /// </summary>
        public static DependencyProperty FormatPattenProperty = DependencyProperty.Register("FormatPattern",
            typeof(string), typeof(FormattedTextBox), new PropertyMetadata(".*"));


        // Holds the regex
        private Regex regex;

        /// <summary>
        /// Creates a new formatted text box.
        /// </summary>
        public FormattedTextBox() : base()
        {
            this.PreviewTextInput += OnPreviewTextInput;
        }

        /// <summary>
        /// Gets or sets the regex by which the textbox will be formatted.
        /// </summary>
        [System.ComponentModel.Category("Common")]
        public string FormatPattern
        {
            get { return (string)this.GetValue(FormatPattenProperty); }
            set { this.SetValue(FormatPattenProperty, value); }
        }

        /// <summary>
        /// Called when input has been detected but not entered.
        /// </summary>
        private void OnPreviewTextInput(object sender, Input.TextCompositionEventArgs e)
        {
            // Get the resulting text
            string newText = this.Text + e.Text;

            // If handled, the input will be ignored
            e.Handled = !this.regex.IsMatch(newText);
        }

        /// <summary>
        /// Called when a dependency property has been changed.
        /// </summary>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == FormatPattenProperty)
            {
                this.regex = new Regex((string)e.NewValue);

                if (!this.regex.IsMatch(this.Text))
                {
                    this.Text = "";
                }
            }
            else base.OnPropertyChanged(e);
        }        

    }
}
