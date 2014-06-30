using System;
using System.Drawing;

namespace DebugOS
{
    public interface IToolbarItem
    {
        bool   IsEnabled { get; set; }
        object ToolTip   { get; set; }
        bool   IsToggle  { get; }

        Bitmap Icon { get; set; }

        event Action<object> Clicked;
    }
}
