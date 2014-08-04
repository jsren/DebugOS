using System;
using System.Windows.Controls;

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
                Application.Debugger.RefreshRegisters += (s, r) =>
                {
                    this.Dispatcher.Invoke((Action)delegate
                    {
                        foreach (string regName in r.AffectedRegisters)
                        {
                            bool addRegister = true;

                            foreach (RegisterItem ctrl in this.registerWrap.Children)
                            {
                                // If we've the register already displayed, just update the value
                                if (StringComparer.CurrentCultureIgnoreCase.Equals(ctrl.nameText.Text, regName))
                                {
                                    ctrl.SetValue(Application.Debugger.ReadRegister(regName));
                                    addRegister = false;
                                    break;
                                }
                            }
                            // Add new as necessary
                            if (addRegister)
                            {
                                this.registerWrap.Children.Add(new RegisterItem(regName, 
                                    Application.Debugger.ReadRegister(regName)));
                            }
                        }
                    });
                };
            }
        }
	}
}