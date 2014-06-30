using System;
using System.Drawing;

namespace DebugOS
{
    public interface IMenuItem
    {
        bool   IsEnabled { get; set; }
        bool   IsChecked { get; set; }
        Bitmap Icon      { get; set; }
        string Shortcut  { get; set; }
        string Label     { get; set; }

        event Action<object> Clicked;
        event Action<object> CheckChanged;
    }
}
