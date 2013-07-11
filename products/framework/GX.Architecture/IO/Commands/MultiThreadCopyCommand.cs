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

        public int RetryCount
        {
            get { return processor.RetryCount; }
            set { processor.RetryCount = value; }
        }
        ManualResetEvent completeEvent = new ManualResetEvent(false);


        CopyFileWorkItemProcessor processor;
        WorkItemPool<CopyFileWorkItem> processorThreadPool;

        public MultiThreadCopyCommand()
        {
            processor = new CopyFileWorkItemProcessor
            {
                ConfirmCopy = ConfirmCopy,
                ConfirmCreateDirectory = ConfirmCreateDirectory,
                NotifyCopy = NotifyCopy,
                NotifyCreateDirectory = NotifyCreateDirectory
            };

            processorThreadPool = new WorkItemPool<CopyFileWorkItem>(processor);

            processor.Start += new EventHandler<CancelWorkItemEventArgs<CopyFileWorkItem>>(processor_Start);
            processor.Complete += new EventHandler<WorkItemResultEventArgs<CopyFileWorkItem>>(processor_Complete);
            processor.ProgressUpdate += new EventHandler<ProgressEventArgs<CopyFileWorkItem>>(processor_ProgressUpdate);
        }

        private List<FileSystemInfo> GetOrderedSources()
        {
            List<FileSystemInfo> sources = new List<FileSystemInfo>();
            for (int i = 0; i < Sources.Length; ++i)
            {
                FileSystemInfo source = null;
                string src = Sources[i];
                if (File.Exists(src))
                {
                    source = new FileInfo(src);
                    sources.Add(source);
                }
                else if (Directory.Exists(src))
                {
                    source = new DirectoryInfo(src);
                    sources.Add(source);
                }

                if (source == null)
                {
                    OnError(new GX.Patterns.ErrorEventArgs(new FileNotFoundException(string.Format("Source file {0} cannot be found.", src), src)));
                }
            }
            return sources;
        }

        private List<FileSystemInfo> QueueWorkItemsForSources(List<FileSystemInfo> sources)
        {
            for (int i = 0; i < sources.Count; ++i)
            {
                FileSystemInfo source = sources[i];
                double weight = 1.0 / sources.Count;
                processorThreadPool.QueueUserWorkitem(new CopyFileWorkItem()
                {
                    Destination = Destinations[i < Destinations.Length ? i : (Destinations.Length - 1)],
                    Item = source,
                    ProgressWeight = weight,
                    FinishedSize = 0
                }).Complete += new EventHandler(MultiThreadCopyCommand_Complete);
            }
            return sources;
        }

        private List<FileSystemInfo> QueueWorkItemsForSources()
        {
            List<FileSystemInfo> sources = GetOrderedSources();
            return QueueWorkItemsForSources(sources);
        }

        private void PrepareForRun()
        {
            this.excludes.Clear();
            this.excludes.UnionWith(Excludes);
            if (!string.IsNullOrEmpty(IncludePattern))
            {
                this.includePattern = new Regex(IncludePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            if (!string.IsNullOrEmpty(ExcludePattern))
            {
                this.excludePattern = new Regex(ExcludePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }

        #region IAsyncCommand Members

        public void Do()
        {
            OnStart(new EventArgs());

            PrepareForRun();

            var sources = QueueWorkItemsForSources();

            if (sources.Count <= 0)
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
