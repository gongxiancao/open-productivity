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
    public class MultiThreadCopyCommand : MultiThreadFileCommand<CopyFileWorkItem>
    {
        public ConfirmCopyCallback ConfirmCopy { get; set; }
        public ConfirmCreateDirectoryCallback ConfirmCreateDirectory { get; set; }
        public NotifyCopyCallback NotifyCopy { get; set; }
        public NotifyCreateDirectoryCallback NotifyCreateDirectory { get; set; }

        public string[] Destinations { get; set; }

        public event EventHandler<ProgressEventArgs<CopyFileWorkItem>> CopyFileProgressUpdate
        {
            add { WorkItemProgressUpdate += value; }
            remove { WorkItemProgressUpdate -= value; }
        }
        public event EventHandler<WorkItemEventArgs<CopyFileWorkItem>> CopyFileStart
        {
            add { WorkItemStart += value; }
            remove { WorkItemStart -= value; }
        }
        public event EventHandler<WorkItemEventArgs<CopyFileWorkItem>> CopyFileComplete
        {
            add { WorkItemComplete += value; }
            remove { WorkItemComplete -= value; }
        }

        public MultiThreadCopyCommand()
        {
        }

        public override IWorkItemProcessor<CopyFileWorkItem> CreateWorkItemProcessor()
        {
            return new CopyFileWorkItemProcessor
            {
                ConfirmCopy = ConfirmCopy,
                ConfirmCreateDirectory = ConfirmCreateDirectory,
                NotifyCopy = NotifyCopy,
                NotifyCreateDirectory = NotifyCreateDirectory
            };
        }

        protected override CopyFileWorkItem CreateWorkItem(FileSystemInfo source, int index, double weight)
        {
            return new CopyFileWorkItem
            {
                Destination = Destinations[index < Destinations.Length ? index : (Destinations.Length - 1)],
                Item = source,
                ProgressWeight = weight,
                FinishedSize = 0
            };
        }
    }
}
