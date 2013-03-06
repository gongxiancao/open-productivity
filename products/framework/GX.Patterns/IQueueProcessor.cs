using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GX.Patterns
{
    public interface IQueueProcessor : IWorkItem
    {
        event EventHandler Start;
        event EventHandler Complete;
    }
}
