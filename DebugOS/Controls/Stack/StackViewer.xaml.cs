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
        public StackViewer()
        {
            InitializeComponent();

            if (App.Debugger != null) {
                App.Debugger.Stepped += Debugger_Stepped;
            }
        }

        void Debugger_Stepped(object sender, SteppedEventArgs e)
        {
            // == Refresh ==
            this.Dispatcher.Invoke((Action)delegate() { this.stack.Children.Clear(); });

            if (!App.Debugger.CanReadMemory) return;

            // Get Base Pointer:
            Register? bp = null, sp = null;

            foreach (Register reg in App.Debugger.AvailableRegisters)
            {
                if (reg.Type == RegisterType.BasePointer)
                {
                    bp = reg;
                }
                else if (reg.Type == RegisterType.StackPointer)
                {
                    sp = reg;
                }
            }

            // Pointers not available - just quit
            if (bp == null || sp == null) return;

            byte[] bpBytes = App.Debugger.ReadRegister(bp.Value);
            byte[] spBytes = App.Debugger.ReadRegister(sp.Value);

            long bpValue, spValue;

            if (bpBytes.Length == 4)
            {
                bpValue = Utils.IntFromBytes(bpBytes);
                spValue = Utils.IntFromBytes(spBytes);
            }
            else if (bpBytes.Length == 8)
            {
                bpValue = Utils.LongFromBytes(bpBytes);
                spValue = Utils.LongFromBytes(spBytes);
            }
            else return; // Invalid register size - cannot compute

            int diff = (int)Math.Max((ulong)bpValue - (ulong)spValue, 20);

            // Read stack data
#warning Must be logical address from SS
            byte[] data = App.Debugger.ReadMemory(new Address(AddressType.Linear, spValue), diff);

            this.Dispatcher.Invoke((Action)delegate()
            {
                if (App.Debugger.AddressWidth == 4)
                {
                    byte[] buffer = new byte[4];
                    for (int i = 0; i < data.Length; i += 4)
                    {
                        Array.Copy(data, i, buffer, 0, 4);
                        this.stack.Children.Add(new StackValueItem() { Value = (ulong)Utils.IntFromBytes(buffer) });
                    }
                }
                else if (App.Debugger.AddressWidth == 8)
                {
                    byte[] buffer = new byte[8];
                    for (int i = 0; i < data.Length; i += 8)
                    {
                        Array.Copy(data, i, buffer, 0, 8);
                        this.stack.Children.Add(new StackValueItem() { Value = (ulong)Utils.LongFromBytes(buffer) });
                    }
                }
            });
        }
    }
}
