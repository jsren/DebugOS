using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DebugOS
{
    public interface ISmartContextHandler
    {
        IEnumerable<UIElement> GetContextualUI(string content, UIElement source);
    }
}
