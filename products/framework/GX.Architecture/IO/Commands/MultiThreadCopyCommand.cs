using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using GX.IO;
using GX.Patterns.Concurrence;
using GX.Patterns;
using GX.Patterns.Progress;

namespace GX.Architecture.IO.Commands
{
    public class MultiThreadCopyCommand : ICommand, IStartCompletionMonitorable, IProgressMonitorable<MultiThreadCopyCommand>, IErrorMonitorable
    {
        private ConfirmCopyCallback confirmCopy;
        private ConfirmCreateDirectoryCallback confirmCreateDirectory;
        private NotifyCopyCallback notifyCopy;
        private NotifyCreateDirectoryCallback notifyCreateDirectory;
        public string[] Sources { get; private set; }
        public string[] Destinations { get; private set; }
        public int RetryCount { get; private set; }
        ManualResetEvent completeEvent = new ManualResetEvent(false);
        public MultiThreadCopyCommand(string[] sources, string[] destinations, int retryCount,  ConfirmCopyCallback confirmCopy, ConfirmCreateDirectoryCallback confirmCreateDirectory, NotifyCopyCallback notifyCopy, NotifyCreateDirectoryCallback notifyCreateDirectory)
        {
            this.RetryCount = retryCount;
            this.Sources = sources;
            this.Destinations = destinations;
            this.confirmCopy = confirmCopy;
            this.confirmCreateDirectory = confirmCreateDirectory;
            this.notifyCopy = notifyCopy;
            this.notifyCreateDirectory = notifyCreateDirectory;
        }

        #region IAsyncCommand Members

        public void Do()
        {
            OnStart(new EventArgs());
            CopyFileWorkItemProcessor processor = new CopyFileWorkItemProcessor(RetryCount, confirmCopy, confirmCreateDirectory, notifyCopy, notifyCreateDirectory);

            processor.Start += new EventHandler<WorkItemEventArgs<CopyFileWorkItem>>(processor_Start);
            processor.Complete += new EventHandler<WorkItemEventArgs<CopyFileWorkItem>>(processor_Complete);
            processor.ProgressUpdate += new EventHandler<ProgressEventArgs<CopyFileWorkItem>>(processor_ProgressUpdate);
            int workItemQueued = 0;
            for (int i = 0; i < Sources.Length; ++i )
            {
                FileSystemInfo source = null;
                string src = Sources[i];
                if (File.Exists(src))
                {
                    source = new FileInfo(src);
                }
                else if (Directory.Exists(src))
                {
                    source = new DirectoryInfo(src);
                }
                if (source != null)
                {
                    WorkItemProcessorThreadPool.QueueUserWorkItem(processor, new CopyFileWorkItem()
                    {
                        Destination = Destinations[i< Destinations.Length? i : (Destinations.Length - 1)],
                        Item = source,
                        ProgressWeight = 1.0,
                        FinishedSize = 0
                    }).Complete += new EventHandler(MultiThreadCopyCommand_Complete);
                    workItemQueued++;
                }
                else
                {
                    OnError(new GX.Patterns.ErrorEventArgs(new FileNotFoundException(string.Format("Source file {0} cannot be found.", src), src)));
                }
            }
            if (workItemQueued <= 0)
            {
                OnComplete(new EventArgs());
            }
        }

        void MultiThreadCopyCommand_Complete(object sender, EventArgs e)
        {
            OnComplete(e);
        }

        protected virtual void OnCopyFileProgressUpdate(ProgressEventArgs<CopyFileWorkItem> e)
        {
            if (CopyFileProgressUpdate != null)
            {
                CopyFileProgressUpdate(this, e);
            }
        }

        void processor_ProgressUpdate(object sender, ProgressEventArgs<CopyFileWorkItem> e)
        {
            OnCopyFileProgressUpdate(e);
        }

        protected virtual void OnCopyFileComplete(WorkItemEventArgs<CopyFileWorkItem> e)
        {
            if (CopyFileComplete != null)
            {
                CopyFileComplete(this, e);
            }
        }
        void processor_Complete(object sender, WorkItemEventArgs<CopyFileWorkItem> e)
        {
            OnCopyFileComplete(e);
        }

        protected virtual void OnCopyFileStart(WorkItemEventArgs<CopyFileWorkItem> e)
        {
            if (CopyFileStart != null)
            {
                CopyFileStart(this, e);
            }
        }
        void processor_Start(object sender, WorkItemEventArgs<CopyFileWorkItem> e)
        {
            OnCopyFileStart(e);
        }

        #endregion

        protected virtual void OnStart(EventArgs e)
        {
            if (Start != null)
            {
                Start(this, e);
            }
        }

        protected virtual void OnComplete(EventArgs e)
        {
            if (Complete != null)
            {
                Complete(this, e);
            }
        }

        protected virtual void OnError(GX.Patterns.ErrorEventArgs e)
        {
            if (Error != null)
            {
                Error(this, e);
            }
        }

        public event EventHandler<ProgressEventArgs<CopyFileWorkItem>> CopyFileProgressUpdate;
        public event EventHandler<WorkItemEventArgs<CopyFileWorkItem>> CopyFileStart;
        public event EventHandler<WorkItemEventArgs<CopyFileWorkItem>> CopyFileComplete;

        public event EventHandler Start;
        public event EventHandler Complete;
        public event EventHandler<ProgressEventArgs<MultiThreadCopyCommand>> ProgressUpdate;


        #region IErrorMonitorable Members

        public event EventHandler<Patterns.ErrorEventArgs> Error;

        #endregion
    }
}
