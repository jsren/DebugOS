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
using System.Linq;

namespace DebugOS
{
	/// <summary>
	/// Interaction logic for SourceViewer.xaml
	/// </summary>
	public partial class SourceViewer : UserControl
	{
        private List<CodeView> codeViews;

		public SourceViewer()
		{
			this.InitializeComponent();
            this.codeViews = new List<CodeView>();

            // Wait until debugger is registered to subscribe
            App.DebuggerRegistered += OnDebuggerRegistered;
		}

        void OnDebuggerRegistered()
        {
            // Subscribe to debugger events
            App.Debugger.BreakpointHit += (s, e) => this.UpdateStep();
            App.Debugger.Stepped       += (s, e) => this.UpdateStep();
            App.Debugger.Continued     += (s, e) => this.OnContinued();
        }

        public void OpenCodeUnit(CodeUnit unit)
        {
            var newView = new CodeView(unit);
            this.codeViews.Add(newView);

            this.mainPanelTabView.Items.Add(new TabItem()
            {
                Content = newView,
                Header  = unit.Name
            });
        }

        private void OnContinued()
        {
            Dispatcher.Invoke((Action)delegate
            {
                int currentIndex = this.mainPanelTabView.SelectedIndex;

                if (currentIndex != -1) {
                    this.codeViews[currentIndex].OnContinued();
                }
            });
        }

        private void UpdateStep()
        {
            Dispatcher.Invoke((Action)delegate
            {
                bool openNew = true;

                for (int i = 0; i < this.codeViews.Count; i++)
                {
                    CodeView view = this.codeViews[i];

                    if (view.CodeUnit == App.Debugger.CurrentCodeUnit)
                    {
                        this.mainPanelTabView.SelectedIndex = i;
                        view.UpdateStep();

                        openNew = false;
                        break;
                    }
                }
                if (openNew)
                {
                    this.OpenCodeUnit(App.Debugger.CurrentCodeUnit);
                    this.mainPanelTabView.SelectedIndex = this.codeViews.Count - 1;
                }
            });
        }

	}
}