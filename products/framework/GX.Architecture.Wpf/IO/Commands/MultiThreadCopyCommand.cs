using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GX.IO;
using GX.Patterns;
using System.Threading;
using GX.Architecture.Wpf.IO.Commands;
using System.IO;
using GX.Patterns.Concurrence;
using GX.Architecture.IO.Commands;
using GX.Patterns.Progress;
using GX.Architecture.Wpf;

namespace GX.Architecture.UI.IO.Commands
{
    public class MultiThreadCopyUICommand : MultiThreadCopyCommand
    {
        public MultiThreadCopyUICommand(string source, string destination, ConfirmCopyCallback confirmCopy, ConfirmCreateDirectoryCallback confirmCreateDirectory, NotifyCopyCallback notifyCopy, NotifyCreateDirectoryCallback notifyCreateDirectory)
            :base(source, destination, confirmCopy, confirmCreateDirectory, notifyCopy, notifyCreateDirectory)
        {
        }

        #region IAsyncCommand Members

        void processor_ProgressUpdate(object sender, ProgressEventArgs<CopyFileWorkItem> e)
        {
            if (CopyFileProgressUpdate != null)
            {
                CopyFileProgressUpdate(this, e);
            }
        }

        void processor_Complete(object sender, WorkItemEventArgs<CopyFileWorkItem> e)
        {
            if (CopyFileComplete != null)
            {
                CopyFileComplete(this, e);
            }
        }

        void processor_Start(object sender, WorkItemEventArgs<CopyFileWorkItem> e)
        {
            if (CopyFileStart != null)
            {
                CopyFileComplete(this, e);
            }
        }

        #endregion

        public event EventHandler<ProgressEventArgs<CopyFileWorkItem>> CopyFileProgressUpdate;
        public event EventHandler<WorkItemEventArgs<CopyFileWorkItem>> CopyFileStart;
        public event EventHandler<WorkItemEventArgs<CopyFileWorkItem>> CopyFileComplete;

        public event EventHandler Start;
        public event EventHandler Complete;
        public event EventHandler<ProgressEventArgs<MultiThreadCopyCommand>> ProgressUpdate;

    }
}
