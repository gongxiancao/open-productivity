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
using System.Text.RegularExpressions;

namespace GX.Architecture.IO.Commands
{
    public class MultiThreadCopyCommand : ICommand, IStartCompletionMonitorable, IProgressMonitorable<MultiThreadCopyCommand>, IErrorMonitorable
    {
        public ConfirmCopyCallback ConfirmCopy { get; set; }
        public ConfirmCreateDirectoryCallback ConfirmCreateDirectory { get; set; }
        public NotifyCopyCallback NotifyCopy { get; set; }
        public NotifyCreateDirectoryCallback NotifyCreateDirectory { get; set; }
        public string[] Sources { get; set; }
        public string[] Destinations { get; set; }
        public string[] Excludes { get; set; }
        public string IncludePattern { get; set; }
        public string ExcludePattern { get; set; }

        private Regex includePattern = null;
        private Regex excludePattern = null;

        private HashSet<string> excludes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public int RetryCount { get; set; }
        ManualResetEvent completeEvent = new ManualResetEvent(false);

        public MultiThreadCopyCommand()
        {
        }

        #region IAsyncCommand Members

        public void Do()
        {
            this.excludes.UnionWith(Excludes);
            if (!string.IsNullOrEmpty(IncludePattern))
            {
                this.includePattern = new Regex(IncludePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(ExcludePattern))
            {
                this.excludePattern = new Regex(ExcludePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }


            OnStart(new EventArgs());
            CopyFileWorkItemProcessor processor = new CopyFileWorkItemProcessor(RetryCount, ConfirmCopy, ConfirmCreateDirectory, NotifyCopy, NotifyCreateDirectory);

            processor.Start += new EventHandler<CancelWorkItemEventArgs<CopyFileWorkItem>>(processor_Start);
            processor.Complete += new EventHandler<WorkItemResultEventArgs<CopyFileWorkItem>>(processor_Complete);
            processor.ProgressUpdate += new EventHandler<ProgressEventArgs<CopyFileWorkItem>>(processor_ProgressUpdate);
            int validSources = 0;
            WorkItemPool<CopyFileWorkItem> processorThreadPool = new WorkItemPool<CopyFileWorkItem>(processor);
            List<FileSystemInfo> sources = new List<FileSystemInfo>();
            for (int i = 0; i < Sources.Length; ++i )
            {
                FileSystemInfo source = null;
                string src = Sources[i];
                if (File.Exists(src))
                {
                    source = new FileInfo(src);
                    ++validSources;
                }
                else if (Directory.Exists(src))
                {
                    source = new DirectoryInfo(src);
                    ++validSources;
                }
                sources.Add(source);
                if(source == null)
                {
                    OnError(new GX.Patterns.ErrorEventArgs(new FileNotFoundException(string.Format("Source file {0} cannot be found.", src), src)));
                }
            }

            for(int i = 0; i < sources.Count; ++i)
            {
                FileSystemInfo source = sources[i];
                if (source != null)
                {
                    double weight = 1.0 / validSources;
                    processorThreadPool.QueueUserWorkitem(new CopyFileWorkItem()
                    {
                        Destination = Destinations[i < Destinations.Length ? i : (Destinations.Length - 1)],
                        Item = source,
                        ProgressWeight = weight,
                        FinishedSize = 0
                    }).Complete += new EventHandler(MultiThreadCopyCommand_Complete);
                }
            }
            if (validSources <= 0)
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

        void processor_Start(object sender, CancelWorkItemEventArgs<CopyFileWorkItem> e)
        {
            if (!e.Cancel)
            {
                e.Cancel = !ShouldCopy(e.WorkItem);
            }

            OnCopyFileStart(e);
        }

        private bool ShouldCopy(CopyFileWorkItem item)
        {
            string fullName = item.Item.FullName;
            return (this.includePattern == null || this.includePattern.Match(fullName).Success)
                && (this.excludePattern == null || !this.excludePattern.Match(fullName).Success)
                && !this.excludes.Contains(fullName);
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
