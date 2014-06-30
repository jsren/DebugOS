using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Controls;
using System.Windows.Interop;

namespace DebugOS
{
    public class WindowHost : HwndHost
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndParent);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int W, int H, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool DestroyWindow(IntPtr hwnd);


        public HandleRef WindowHandle { get; private set; }


        public WindowHost(HandleRef windowHandle)
        {
            this.WindowHandle = windowHandle;
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            IntPtr childHandle = WindowHost.SetParent(this.WindowHandle.Handle, hwndParent.Handle);
            var res2 = WindowHost.SetWindowPos(this.WindowHandle.Handle, IntPtr.Zero, 0, 0, 0, 0, 0x27);

            if (childHandle == IntPtr.Zero || res2 == false)
            {
                throw new ExternalException("Error hosting child window", Marshal.GetLastWin32Error());
            }
            return new HandleRef(this.WindowHandle, childHandle);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            WindowHost.DestroyWindow(hwnd.Handle);
        }
    }
}
