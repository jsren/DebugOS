using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugOS
{
    public interface IMainPanel
    {
        string Title { get; set; }
        object Content { get; set; }

        bool IsOpen { get; }
        bool IsVisible { get; }

        event Action<object> GainedFocus;
        event Action<object> LostFocus;
        event Action<object> Closed;
    }
}
