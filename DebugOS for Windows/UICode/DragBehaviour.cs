using System;
using System.Windows;
using System.Windows.Media;

namespace DebugOS
{
    public class DragBehaviour
    {
        private IInputElement handle;
        private UIElement targetControl;
        private Window    targetWindow;
        private Point     lastPoint;

        public bool Dragging { get; private set; }

        public DragBehaviour(Window window) : this((UIElement)window) {
            this.targetWindow = window;
        }
        public DragBehaviour(UIElement control) : this(control, control) { 
            // Empty
        }
        public DragBehaviour(IInputElement handle, Window target) : this(handle, (UIElement)target) {
            this.targetWindow = target;
        }
        public DragBehaviour(IInputElement handle, UIElement target)
        {
            this.handle        = handle;
            this.targetControl = target;

            handle.PreviewMouseMove           += MouseMove;
            handle.PreviewMouseLeftButtonDown += MouseLeftButtonDown;
            handle.PreviewMouseLeftButtonUp   += MouseLeftButtonUp;
        }

        void MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (this.Dragging)
            {
                e.Handled = true;

                Point  loc = e.GetPosition(null);
                Vector mov = loc - this.lastPoint;

                if (this.targetWindow != null)
                {
                    this.targetWindow.Top += mov.Y;
                    this.targetWindow.Left += mov.X;
                }
                else {
                    this.targetControl.RenderTransform = new TranslateTransform(mov.X, mov.Y);
                }
            }
        }

        void MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1) return; // Ignore double(and so on)-clicks

            this.handle.CaptureMouse();
            this.lastPoint = e.GetPosition(null);
            this.Dragging  = true;
        }

        void MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!this.Dragging) return;

            e.Handled     = true;
            this.Dragging = false;
            this.handle.ReleaseMouseCapture();
        }

    }
}
