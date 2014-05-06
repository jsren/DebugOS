using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugOS.Bochs
{
    public interface IBochsRequest
    {
        bool isComplete { get; }
        bool feedLine(string line);

        void handleComplete();
    }
}
