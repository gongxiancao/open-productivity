using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GX.Patterns.Progress
{
    public interface IProgressMonitorable<T>
    {
        event EventHandler<ProgressEventArgs<T>> ProgressUpdate;
    }
}
