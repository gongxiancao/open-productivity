using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        void ProcessWorkItem(T workItem);
        bool IsValidWorkItem(T workItem);
        event EventHandler<WorkItemEventArgs<T>> Start;
        event EventHandler<NewWorkItemEventArgs<T>> OnNewWorkItem;
        event EventHandler<WorkItemEventArgs<T>> Complete;
    }
}
