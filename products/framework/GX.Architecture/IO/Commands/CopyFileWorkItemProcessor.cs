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
        private ConfirmCopyCallback confirmCopy;
        private ConfirmCreateDirectoryCallback confirmCreateDirectory;
        private NotifyCopyCallback notifyCopy;
        private NotifyCreateDirectoryCallback notifyCreateDirectory;
        private readonly int retryCount = 0;
        public CopyFileWorkItemProcessor(int retryCount, ConfirmCopyCallback confirmCopy, ConfirmCreateDirectoryCallback confirmCreateDirectory, NotifyCopyCallback notifyCopy, NotifyCreateDirectoryCallback notifyCreateDirectory)
        {
            this.retryCount = retryCount;
            this.confirmCopy = confirmCopy;
            this.confirmCreateDirectory = confirmCreateDirectory;
            this.notifyCopy = notifyCopy;
            this.notifyCreateDirectory = notifyCreateDirectory;
        }

        public void ProcessWorkItem(CopyFileWorkItem workItem)
        {
            OnStart(new WorkItemEventArgs<CopyFileWorkItem>(workItem));
            try
            {
                FileSystemInfo fsi = workItem.Item;
                if (fsi is FileInfo)
                {
                    int cancel = 0;
                    GX.IO.Utilities.CopyFile(fsi as FileInfo, workItem.Destination, retryCount, ref cancel, workItem, CopyFileProgressUpdate, confirmCopy, notifyCopy);
                }
                else if (fsi is DirectoryInfo)
                {
                    DirectoryInfo di = fsi as DirectoryInfo;

                    bool destDirectoryExists = Directory.Exists(workItem.Destination);
                    if (!destDirectoryExists)
                    {
                        destDirectoryExists = GX.IO.Utilities.CreateDirectory(workItem.Destination, confirmCreateDirectory, notifyCreateDirectory);
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
                            OnNewWorkItem(this, new NewWorkItemEventArgs<CopyFileWorkItem>(workItem, new CopyFileWorkItem()
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
            }
            OnComplete(new WorkItemEventArgs<CopyFileWorkItem>(workItem));
        }

        public event EventHandler<WorkItemEventArgs<CopyFileWorkItem>> Start;
        public event EventHandler<WorkItemEventArgs<CopyFileWorkItem>> Complete;
        public event EventHandler<NewWorkItemEventArgs<CopyFileWorkItem>> OnNewWorkItem;
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

        protected virtual void OnStart(WorkItemEventArgs<CopyFileWorkItem> e)
        {
            if (Start != null)
            {
                Start(this, e);
            }
        }

        protected virtual void OnComplete(WorkItemEventArgs<CopyFileWorkItem> e)
        {
            if (Complete != null)
            {
                Complete(this, e);
            }
        }
    }
}
