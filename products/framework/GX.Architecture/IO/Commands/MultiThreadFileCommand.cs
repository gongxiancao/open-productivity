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

    public abstract class MultiThreadFileCommand<TFileWorkItem> :
        ICommand, IStartCompletionMonitorable, IProgressMonitorable<MultiThreadFileCommand<TFileWorkItem>>, IErrorMonitorable
        where TFileWorkItem : FileWorkItem
    {

        public string[] Sources { get; set; }
        public string[] Excludes { get; set; }
        public string IncludePattern { get; set; }
        public string ExcludePattern { get; set; }

        public int RetryCount
        {
            get { return processor.RetryCount; }
            set { processor.RetryCount = value; }
        }

        public event EventHandler<ProgressEventArgs<TFileWorkItem>> WorkItemProgressUpdate;
        public event EventHandler<WorkItemEventArgs<TFileWorkItem>> WorkItemStart;
        public event EventHandler<WorkItemEventArgs<TFileWorkItem>> WorkItemComplete;

        public event EventHandler Start;
        public event EventHandler Complete;
        public event EventHandler<ProgressEventArgs<MultiThreadFileCommand<TFileWorkItem>>> ProgressUpdate;

        private Regex includePattern = null;
        private Regex excludePattern = null;

        private HashSet<string> excludes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        ManualResetEvent completeEvent = new ManualResetEvent(false);

        protected IWorkItemProcessor<TFileWorkItem> processor;
        WorkItemPool<TFileWorkItem> processorThreadPool;

        public MultiThreadFileCommand()
        {
            processor = CreateWorkItemProcessor();

            processorThreadPool = new WorkItemPool<TFileWorkItem>(processor);

            processor.Start += new EventHandler<CancelWorkItemEventArgs<TFileWorkItem>>(processor_Start);
            processor.Complete += new EventHandler<WorkItemResultEventArgs<TFileWorkItem>>(processor_Complete);
            processor.ProgressUpdate += new EventHandler<ProgressEventArgs<TFileWorkItem>>(processor_ProgressUpdate);
        }

        public abstract IWorkItemProcessor<TFileWorkItem> CreateWorkItemProcessor();

        private List<FileSystemInfo> GetSources(IEnumerable<string> sourcePaths)
        {
            List<FileSystemInfo> sources = new List<FileSystemInfo>();
            foreach (var src in sourcePaths)
            {
                FileSystemInfo source = null;
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
                var workItem = CreateWorkItem(source, i, weight);
                var completionMonitorable = processorThreadPool.QueueUserWorkitem(workItem);
                completionMonitorable.Complete += new EventHandler(MultiThreadCopyCommand_Complete);
            }
            return sources;
        }

        protected abstract TFileWorkItem CreateWorkItem(FileSystemInfo source, int index, double weight);

        private List<FileSystemInfo> QueueWorkItemsForSources()
        {
            List<FileSystemInfo> sources = GetSources(Sources);
            return QueueWorkItemsForSources(sources);
        }

        private void PrepareToRun()
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

            PrepareToRun();

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

        protected virtual void OnWorkItemProgressUpdate(ProgressEventArgs<TFileWorkItem> e)
        {
            if (WorkItemProgressUpdate != null)
            {
                WorkItemProgressUpdate(this, e);
            }
        }

        void processor_ProgressUpdate(object sender, ProgressEventArgs<TFileWorkItem> e)
        {
            OnWorkItemProgressUpdate(e);
        }

        protected virtual void OnCopyFileComplete(WorkItemEventArgs<TFileWorkItem> e)
        {
            if (WorkItemComplete != null)
            {
                WorkItemComplete(this, e);
            }
        }
        void processor_Complete(object sender, WorkItemEventArgs<TFileWorkItem> e)
        {
            OnCopyFileComplete(e);
        }

        protected virtual void OnCopyFileStart(WorkItemEventArgs<TFileWorkItem> e)
        {
            if (WorkItemStart != null)
            {
                WorkItemStart(this, e);
            }
        }

        void processor_Start(object sender, CancelWorkItemEventArgs<TFileWorkItem> e)
        {
            if (!e.Cancel)
            {
                e.Cancel = ShouldSkip(e.WorkItem);
            }

            OnCopyFileStart(e);
        }

        private bool ShouldSkip(TFileWorkItem item)
        {
            string fullName = item.Item.FullName;
            return !(
                (this.includePattern == null || this.includePattern.Match(fullName).Success)
                && (this.excludePattern == null || !this.excludePattern.Match(fullName).Success)
                && !this.excludes.Contains(fullName)
                );
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


        #region IErrorMonitorable Members

        public event EventHandler<Patterns.ErrorEventArgs> Error;

        #endregion
    }
}
