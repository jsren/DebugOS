using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace DebugOS
{
    /// <summary>
    /// User interface control displaying the code/disassembly
    /// of a given code unit.
    /// </summary>
	public partial class CodeView : UserControl
	{
        private bool     showAsm = true;
        private CodeUnit codeUnit;

		public CodeView()
		{
			this.InitializeComponent();
            this.ShowAssembly = true;

            Application.Debugger.Stepped += (s, e) => UpdateStep();
		}
        public CodeView(CodeUnit codeUnit) : this()
        {
            this.CodeUnit = codeUnit;
        }

        /// <summary>
        /// Whether to show disassembly in the view.
        /// </summary>
        public bool ShowAssembly
        {
            get { return this.showAsm; }
            set
            { 
                this.showAsm = value;
                this.ReloadItems();
            }
        }

        /// <summary>
        /// Gets or sets the code unit displayed in the view.
        /// </summary>
        public CodeUnit CodeUnit
        {
            get { return this.codeUnit; }
            set
            {
                if (this.codeUnit == value && 
                    value as LiveCodeUnit == null) return;

                this.codeUnit = value;
                this.ReloadItems();
            }
        }


        /// <summary>
        /// Performs a UI update following a step.
        /// </summary>
        internal void UpdateStep()
        {
            Dispatcher.Invoke((Action)delegate
            {
                uint address = (uint)Application.Debugger.CurrentAddress;

                // Update each line item
                foreach (CodeLineItem lItem in this.codeStack.Children) {
                    lItem.UpdateStep(address);
                }

                /* Test if we're the current code unit and update as appropriate */
                var ccu = Application.Debugger.CurrentCodeUnit;

                if (ccu != null && ccu == this.CodeUnit)
                {
                    this.statusText.Text = "[paused]";

                    // Scroll to the current line
                    foreach (CodeLineItem lItem in this.codeStack.Children)
                    {
                        if (lItem.IsCurrentItem) { lItem.BringIntoView(); }

                        if (this.ShowAssembly)
                        {
                            foreach (AssemblyLineItem aItem in lItem.childrenStack.Children)
                            {
                                if (aItem.IsCurrentItem) aItem.BringIntoView();
                            }
                        }
                    }
                }
                else
                {
                    this.statusText.Text = "";
                }
            });
        }

        public void ReloadItems()
        {
            Dispatcher.Invoke((Action)delegate
            {
                // Clear text & load new code unit's code
                this.codeStack.Children.Clear();
                // Make sure we don't have a null value
                if (this.codeUnit == null) return;

                this.codeTitleText.Text = codeUnit.Name;

                foreach (CodeLine line in this.codeUnit.Lines)
                {
                    CodeLineItem item = new CodeLineItem(line, this.ShowAssembly);
                    this.codeStack.Children.Add(item);
                }
                this.UpdateStep();
            });
        }

        private void OnShowAsmChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            this.ShowAssembly = this.asmCheck.IsChecked.Value;
        }

        private void OnMouseDownTitle(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.optionsGrid.Visibility == System.Windows.Visibility.Visible)
            {
                this.optionsGrid.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                this.optionsGrid.Visibility = System.Windows.Visibility.Visible;
            }
        }
	}
}