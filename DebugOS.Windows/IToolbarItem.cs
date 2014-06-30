using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace DebugOS.Windows
{
    public interface IToolbarItem : DebugOS.IToolbarItem
    {
        new BitmapSource Icon { get; set; }
    }
}
