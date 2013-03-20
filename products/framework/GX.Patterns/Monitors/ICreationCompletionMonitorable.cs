using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GX.Patterns
{
    public interface ICreationCompletionMonitorable
    {
        event EventHandler Create;
        event EventHandler Complete;
    }
}
