using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GX.Patterns
{
    public interface ICompletionMonitorable
    {
        event EventHandler Complete;
    }
}
