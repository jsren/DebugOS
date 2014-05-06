using System;
using System.Collections.Generic;
using System.Linq;
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

namespace DebugOS.Controls.Stack
{
    /// <summary>
    /// Interaction logic for StackViewer.xaml
    /// </summary>
    public partial class StackViewer : UserControl
    {
        private byte[] BP;
        private byte[] SP;

        public StackViewer()
        {
            InitializeComponent();

            App.DebuggerRegistered += OnDebuggerRegistered;
        }

        void OnDebuggerRegistered()
        {
            if (App.Debugger.CanReadMemory)
            {
                App.Debugger.RefreshRegister += OnRegisterUpdate;
                App.Debugger.Stepped         += Debugger_Stepped;
            }
        }

        void OnRegisterUpdate(object sender, RegisterUpdateEventArgs e)
        {
            if (BP != null && SP != null)
            {
                this.requestUpdate();
                BP = SP = null;
            }
            else if (e.Register.Type == RegisterType.BasePointer) {
                BP = e.Value;
            }
            else if (e.Register.Type == RegisterType.StackPointer) {
                SP = e.Value;
            }
        }

        void Debugger_Stepped(object sender, SteppedEventArgs e)
        {
            BP = SP = null;
            this.Dispatcher.Invoke((Action)delegate() { this.stack.Children.Clear(); });
        }


        void requestUpdate()
        {
            ulong bpValue, spValue;

            if (BP.Length == 4)
            {
                bpValue = (uint)Utils.IntFromBytes(BP);
                spValue = (uint)Utils.IntFromBytes(SP);
            }
            else if (BP.Length == 8)
            {
                bpValue = (ulong)Utils.LongFromBytes(BP);
                spValue = (ulong)Utils.LongFromBytes(SP);
            }
            else return; // Invalid register size - cannot compute

            int diff = Math.Max(Math.Max((int)(bpValue - spValue), 20), 96);

            // Read stack data
#warning Must be logical address from SS
            App.Debugger.BeginReadMemory(new Address(AddressType.Linear, (long)spValue), diff, (UInt32[] data) =>
            {
                this.Dispatcher.Invoke((Action)delegate()
                {
                    if (App.Debugger.AddressWidth == 4)
                    {
                        for (int i = 0; i < data.Length; i++)
                        {
                            this.stack.Children.Add(new StackValueItem(data[i]) { IsReturnAddress = (i == 0) });
                        }
                    }
                });
            });
        }
    }
}
