using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GX.Patterns.Progress
{
    public class ProgressEventArgs<T> : EventArgs
    {
        public long Finished { get; private set; }
        public long Total { get; private set; }
        public T WorkItem { get; private set; }

        public ProgressEventArgs(long finished, long total, T workItem)
        {
            this.Finished = finished;
            this.Total = total;
            this.WorkItem = workItem;
        }
    }
}
