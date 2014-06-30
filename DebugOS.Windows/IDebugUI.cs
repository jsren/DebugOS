using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugOS.Windows
{
    public interface IDebugUI : DebugOS.IDebugUI
    {
        new IToolbarItem NewToolbarItem();
        new IToolbarItem NewToolbarItem(bool isToggle);
    }
}
