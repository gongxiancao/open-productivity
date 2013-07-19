using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GX.Patterns.Progress;

namespace GX.Patterns
{

    public class WorkItemEventArgs<T> : EventArgs
    {
        public T WorkItem { get; private set; }
        public WorkItemEventArgs(T workItem)
        {
            this.WorkItem = workItem;
        }
    }

    public enum WorkItemResult
    {
        Completed,
        Cancelled,
        Failed
    }

    public class WorkItemResultEventArgs<T> : WorkItemEventArgs<T>
    {
        public WorkItemResult Result { get; private set; }
        public WorkItemResultEventArgs(T workItem, WorkItemResult result)
            : base(workItem)
        {
            this.Result = result;
        }
    }

    public class CancelWorkItemEventArgs<T> : WorkItemEventArgs<T>
    {
        public bool Cancel { get; set; }
        public CancelWorkItemEventArgs(T workItem)
            : base(workItem)
        {
        }
    }

    public class NewWorkItemEventArgs<T> : EventArgs
    {
        public T Parent { get; private set; }
        public T WorkItem { get; private set; }
        public NewWorkItemEventArgs(T parent, T workItem)
        {
            this.Parent = parent;
            this.WorkItem = workItem;
        }
    }

    public interface IWorkItemProcessor<T>
    {
        int RetryCount { get; set; }
        void ProcessWorkItem(T workItem);
        bool IsValidWorkItem(T workItem);
        event EventHandler<CancelWorkItemEventArgs<T>> Start;
        event EventHandler<NewWorkItemEventArgs<T>> NewWorkItem;
        event EventHandler<WorkItemResultEventArgs<T>> Complete;
        event EventHandler<ProgressEventArgs<T>> ProgressUpdate;
    }
}
