using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GX.Architecture.IO.Commands
{
    public class MultiThreadCopyCommandFailureMonitor
    {
        public List<CopyFileWorkItem> FailedWorkItems { get; private set; }

        public MultiThreadCopyCommandFailureMonitor(MultiThreadCopyCommand target)
        {
            target.WorkItemComplete += new EventHandler<Patterns.WorkItemEventArgs<CopyFileWorkItem>>(target_CopyFileComplete);
            this.FailedWorkItems = new List<CopyFileWorkItem>();
        }

        void target_CopyFileComplete(object sender, Patterns.WorkItemEventArgs<CopyFileWorkItem> e)
        {
            if (e.WorkItem.FailedReason != null)
            {
                lock (this)
                {
                    FailedWorkItems.Add(e.WorkItem);
                }
            }
        }
    }
}
