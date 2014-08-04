using System;
using System.Windows.Controls;

namespace DebugOS.Controls.Stack
{
    /// <summary>
    /// Interaction logic for StackViewer.xaml
    /// </summary>
    public partial class StackViewer : UserControl
    {
        private byte[] FP;
        private byte[] SP;

        public StackViewer()
        {
            InitializeComponent();

            Application.DebuggerChanged += OnDebuggerChanged;
        }

        void OnDebuggerChanged()
        {
            if (Application.Debugger != null && Application.Debugger.CanReadMemory)
            {
                Application.Debugger.RefreshRegisters += OnRegisterUpdate;
                Application.Debugger.Suspended       += Debugger_Suspended;
            }
        }

        void Debugger_Suspended(object sender, EventArgs e)
        {
            
        }

        void OnRegisterUpdate(object sender, RegistersChangedEventArgs e)
        {
            bool needUpdate = false;

            // Look for stack/frame pointers to update
            foreach (string regname in e.AffectedRegisters)
            {
                var reg = Application.Debugger.Registers[regname];

                if (reg.Type == RegisterType.FramePointer && 
                    Application.Debugger.Registers.CanRead(regname))
                {
                    this.FP = Application.Debugger.ReadRegister(regname);
                    needUpdate = true;
                }
                else if (reg.Type == RegisterType.StackPointer &&
                    Application.Debugger.Registers.CanRead(regname))
                {
                    this.SP = Application.Debugger.ReadRegister(regname);
                    needUpdate = true;
                }
            }

            if (needUpdate) // Update stack as required
            {
                this.RequestUpdate();
            }
        }


        void RequestUpdate()
        {
            ulong bpValue, spValue;

            // Some architectures have no frame (base) pointer
            if (FP == null && SP != null)
            {
                FP = SP;
            }

            this.Dispatcher.Invoke((Action)delegate() { this.stack.Children.Clear(); });

            if (FP.Length == 4)
            {
                bpValue = (uint)Utils.IntFromBytes(FP);
                spValue = (uint)Utils.IntFromBytes(SP);
            }
            else if (FP.Length == 8)
            {
                bpValue = (ulong)Utils.LongFromBytes(FP);
                spValue = (ulong)Utils.LongFromBytes(SP);
            }
            else return; // Invalid register size - cannot compute

            int length       = Math.Min(Math.Max(Math.Abs((int)(bpValue - spValue)), 20), 96);
            int addressWidth = Application.Debugger.CurrentArchitecture.StackWidth;

            if (Application.Session.Architecture.StackDirection == StackDirection.Down)
            {
                // Read stack data
                Application.Debugger.BeginReadMemory(new Address(Segment.Stack, (long)spValue), length, (byte[] data) =>
                {
                    this.Dispatcher.Invoke((Action)delegate()
                    {
                        byte[] buffer = new byte[addressWidth];
                        int    num    = data.Length / addressWidth;

                        for (int i = 0; i < num; i++)
                        {
                            // Copy a single item's bytes
                            Array.Copy(data, i * addressWidth, buffer, 0, addressWidth);
                            // Add the visual
                            this.stack.Children.Add(new StackValueItem(buffer, isRet: (i == 0)));
                        }
                        "".ToUpper();
                    });
                });
            }
            else
            {
                // Read stack data
                Application.Debugger.BeginReadMemory(new Address(Segment.Stack, (long)spValue - length), length, (byte[] data) =>
                {
                    this.Dispatcher.Invoke((Action)delegate()
                    {
                        byte[] buffer  = new byte[addressWidth];
                        int    num     = data.Length / addressWidth;
                        bool   retAddr = true;
                            
                        for (int i = num - 1; i != -1; i--)
                        {
                            // Copy the single item's bytes
                            Array.Copy(data, i * addressWidth, buffer, 0, addressWidth);
                            // Add the visual
                            this.stack.Children.Add(new StackValueItem(buffer, isRet: retAddr));
                            retAddr = false;
                        }
                    });
                });
            }

            
        }
    }
}
