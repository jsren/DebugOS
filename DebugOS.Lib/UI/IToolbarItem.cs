using System;
using System.Windows.Input;
using System.Windows.Media;

namespace DebugOS
{
    public interface IToolbarItem
    {
        bool   IsEnabled { get; set; }
        object ToolTip   { get; set; }
        bool   IsToggle  { get; }

        ImageSource Icon { get; set; }

        event MouseButtonEventHandler Clicked;
    }
}
