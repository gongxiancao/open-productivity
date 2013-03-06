using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using GX.Patterns.Concurrence;

namespace GX.Architecture.IO.Commands
{
    public class MultiThreadCopyCommandProgressMonitor
    {
        private class ProgressSnapshot
        {
            public DateTime Time { get; set; }
            public long TransferedSize { get; set; }
        }

        public DateTime StartTime { get; private set; }
        public DateTime CompleteTime { get; private set; }

        private ProgressSnapshot Snapshot = new ProgressSnapshot() { Time = DateTime.Now, TransferedSize = 0 };

        private Int64 completedSize = 0;

        private ReaderWriterLock inProgressWorkItemsLock = new ReaderWriterLock();

        public Int64 CompletedSize
        {
            get { return completedSize; }
            private set
            {
                if (completedSize != value)
                {
                    completedSize = value;

                    if (Update != null)
                    {
                        Update(this, EventArgs.Empty);
                    }
                }
            }
        }

        public Int64 TransferedSize
        {
            get
            {
                using (new ReaderLock(inProgressWorkItemsLock, -1))
                {
                    return completedSize + InProgressWorkItems.Sum(o => o.FinishedSize);
                }
            }
        }

        private HashSet<CopyFileWorkItem> inProgressWorkItems = new HashSet<CopyFileWorkItem>();
        public HashSet<CopyFileWorkItem> InProgressWorkItems
        {
            get { return inProgressWorkItems; }
        }

        public long Speed
        {
            get
            {
                return (long)(TransferedSize / (DateTime.Now - StartTime).TotalSeconds);
            }
        }

        public long RealTimeSpeed
        {
            get
            {
                long speed = (long)((TransferedSize - Snapshot.TransferedSize) / (DateTime.Now - Snapshot.Time).TotalSeconds);
                CheckUpdateSnapshot();
                return speed;
            }
        }

        public event EventHandler Update;

        public MultiThreadCopyCommandProgressMonitor(MultiThreadCopyCommand target)
        {
            target.Start += new EventHandler(target_Start);
            target.Complete += new EventHandler(target_Complete);

            target.CopyFileStart += new EventHandler<Patterns.WorkItemEventArgs<CopyFileWorkItem>>(target_CopyFileStart);
            target.CopyFileComplete += new EventHandler<Patterns.WorkItemEventArgs<CopyFileWorkItem>>(target_CopyFileComplete);
        }

        void target_CopyFileStart(object sender, Patterns.WorkItemEventArgs<CopyFileWorkItem> e)
        {
            using (new WriterLock(inProgressWorkItemsLock, -1))
            {
                inProgressWorkItems.Add(e.WorkItem);
            }
        }

        void target_Complete(object sender, EventArgs e)
        {
            CompleteTime = DateTime.Now;
        }

        void target_Start(object sender, EventArgs e)
        {
            StartTime = DateTime.Now;
            CompletedSize = 0;
        }

        void target_CopyFileComplete(object sender, Patterns.WorkItemEventArgs<CopyFileWorkItem> e)
        {
            FileInfo fi = e.WorkItem.Item as FileInfo;
            using (new WriterLock(inProgressWorkItemsLock, -1))
            {
                inProgressWorkItems.Remove(e.WorkItem);
            }

            if (fi != null)
            {
                lock (this)
                {
                    CompletedSize += fi.Length;
                }
            }
        }

        void CheckUpdateSnapshot()
        {
            DateTime now = DateTime.Now;
            if (this.Snapshot != null && (now - this.Snapshot.Time).TotalMilliseconds < 100)
            {
                return;
            }
            this.Snapshot = new ProgressSnapshot()
            {
                Time = now,
                TransferedSize = TransferedSize
            };
        }
    }
}
