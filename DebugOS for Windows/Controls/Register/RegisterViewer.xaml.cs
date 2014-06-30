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
	/// <summary>
	/// Interaction logic for RegisterViewer.xaml
	/// </summary>
	public partial class RegisterViewer : UserControl
	{
		public RegisterViewer()
		{
			this.InitializeComponent();

            Application.DebuggerChanged += OnDebuggerChanged;
		}

        void OnDebuggerChanged()
        {
            if (Application.Debugger != null)
            {
                Application.Debugger.RefreshRegister += (s, r) =>
                {
                    this.Dispatcher.Invoke((Action)delegate
                    {
                        bool addRegister = true;

                        foreach (RegisterItem ctrl in this.registerWrap.Children)
                        {
                            if (ctrl.nameText.Text == r.Register.Name)
                            {
                                ctrl.SetValue(r.Value);
                                addRegister = false;
                                break;
                            }
                        }

                        if (addRegister)
                        {
                            this.registerWrap.Children.Add(new RegisterItem(r.Register.Name, r.Value, r.Register.Width));
                        }
                    });
                };
            }
        }
	}
}