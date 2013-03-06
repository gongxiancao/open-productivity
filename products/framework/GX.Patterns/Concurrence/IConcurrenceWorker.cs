using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GX.Patterns.Concurrence
{
    public interface IConcurrenceWorker
    {
        void Spout(IWorkItem workItem);
    }
}
