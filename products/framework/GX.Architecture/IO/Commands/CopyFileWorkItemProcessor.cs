using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GX.Patterns;
using System.IO;
using GX.IO;
using GX.Patterns.Progress;

namespace GX.Architecture.IO.Commands
{
    public class CopyFileWorkItemProcessor : IWorkItemProcessor<CopyFileWorkItem>, IProgressMonitorable<CopyFileWorkItem>
    {
        public ConfirmCopyCallback ConfirmCopy { get; set; }
        public ConfirmCreateDirectoryCallback ConfirmCreateDirectory { get; set; }
        public NotifyCopyCallback NotifyCopy { get; set; }
        public NotifyCreateDirectoryCallback NotifyCreateDirectory { get; set; }
        public int RetryCount { get; set; }

        public CopyFileWorkItemProcessor()
        {
        }

        public void ProcessWorkItem(CopyFileWorkItem workItem)
        {
            WorkItemResult result = WorkItemResult.Completed;
            if (!OnStart(new CancelWorkItemEventArgs<CopyFileWorkItem>(workItem)))
            {
                result = WorkItemResult.Cancelled;
            }
            else
            {
                try
                {
                    FileSystemInfo fsi = workItem.Item;
                    if (fsi is FileInfo)
                    {
                        int cancel = 0;
                        GX.IO.Utilities.CopyFile(fsi as FileInfo, workItem.Destination, RetryCount, ref cancel, workItem, CopyFileProgressUpdate, ConfirmCopy, NotifyCopy);
                    }
                    else if (fsi is DirectoryInfo)
                    {
                        DirectoryInfo di = fsi as DirectoryInfo;

                        bool destDirectoryExists = Directory.Exists(workItem.Destination);
                        if (!destDirectoryExists)
                        {
                            destDirectoryExists = GX.IO.Utilities.CreateDirectory(workItem.Destination, ConfirmCreateDirectory, NotifyCreateDirectory);
                        }
                        if (destDirectoryExists)
                        {
                            FileSystemInfo[] children = di.GetFileSystemInfos();
                            List<FileSystemInfo> orderedChildren = new List<FileSystemInfo>();

                            foreach (FileSystemInfo cfsi in children)
                            {
                                if (cfsi is FileInfo)
                                    orderedChildren.Add(cfsi);
                            }

                            foreach (FileSystemInfo cfsi in children)
                            {
                                if (cfsi is DirectoryInfo)
                                    orderedChildren.Add(cfsi);
                            }

                            workItem.ProgressWeight = workItem.ProgressWeight / (orderedChildren.Count + 1);
                            foreach (FileSystemInfo cfi in orderedChildren)
                            {
                                string destDirectoryName = Path.Combine(workItem.Destination, cfi.Name);
                                OnNewWorkItem(new NewWorkItemEventArgs<CopyFileWorkItem>(workItem, new CopyFileWorkItem()
                                {
                                    Destination = destDirectoryName,
                                    Item = cfi,
                                    ProgressWeight = workItem.ProgressWeight,
                                    FinishedSize = 0
                                }));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    workItem.FailedReason = ex;
                    result = WorkItemResult.Failed;
                }
            }
            OnComplete(new WorkItemResultEventArgs<CopyFileWorkItem>(workItem, result));
        }

        public event EventHandler<CancelWorkItemEventArgs<CopyFileWorkItem>> Start;
        public event EventHandler<WorkItemResultEventArgs<CopyFileWorkItem>> Complete;
        public event EventHandler<NewWorkItemEventArgs<CopyFileWorkItem>> NewWorkItem;
        public event EventHandler<ProgressEventArgs<CopyFileWorkItem>> ProgressUpdate;

        public bool IsValidWorkItem(CopyFileWorkItem workItem)
        {
            return workItem != null;
        }

        private CopyProgressResult CopyFileProgressUpdate(CopyFileWorkItem state, long total, long finished)
        {
            if (ProgressUpdate != null)
            {
                ProgressUpdate(this, new ProgressEventArgs<CopyFileWorkItem>(finished, total, state));
            }
            state.FinishedSize = finished;
            return CopyProgressResult.PROGRESS_CONTINUE;
        }

        protected virtual bool OnStart(CancelWorkItemEventArgs<CopyFileWorkItem> e)
        {
            if (Start != null)
            {
                Start(this, e);
            }
            return !e.Cancel;
        }

        protected virtual void OnComplete(WorkItemResultEventArgs<CopyFileWorkItem> e)
        {
            if (Complete != null)
            {
                Complete(this, e);
            }
        }

        protected virtual void OnNewWorkItem(NewWorkItemEventArgs<CopyFileWorkItem> e)
        {
            if (NewWorkItem != null)
            {
                NewWorkItem(this, e);
            }
        }
    }
}
