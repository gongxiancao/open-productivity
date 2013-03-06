using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace GX.Patterns.Concurrence
{
    public class CountBasedCompeletionMonitor: ICompletionMonitorable
    {
        private int count = 1;

        public event EventHandler Complete;

        public CountBasedCompeletionMonitor(ICreationCompletionMonitorable target)
        {
            target.Create += new EventHandler(target_New);
            target.Complete += new EventHandler(target_Complete);
        }

        void target_Complete(object sender, EventArgs e)
        {
            Interlocked.Decrement(ref count);
            if (count <= 0)
            {
                OnComplete(e);
            }
        }

        protected virtual void OnComplete(EventArgs e)
        {
            if(Complete!=null)
            {
                Complete(this, e);
            }
        }

        void target_New(object sender, EventArgs e)
        {
            Interlocked.Increment(ref count);
        }
    }

    public class MonitorableWorkItem<T> : ICreationCompletionMonitorable
    {
        public IWorkItemProcessor<T> Processor { get; private set; }
        public event EventHandler<NewWorkItemEventArgs<T>> NewWorkItem;

        public MonitorableWorkItem(IWorkItemProcessor<T> processor)
        {
            Processor = processor;
            Processor.OnNewWorkItem += new EventHandler<NewWorkItemEventArgs<T>>(Processor_OnNewWorkItem);
        }

        public void ProcessWorkItem(T workItem)
        {
            Processor.ProcessWorkItem(workItem);
            if (Complete != null)
            {
                Complete(this, EventArgs.Empty);
            }
        }

        public void WaitCallback(object state)
        {
            ProcessWorkItem((T)state);
        }

        private void Processor_OnNewWorkItem(object sender, NewWorkItemEventArgs<T> e)
        {
            if (Create != null)
            {
                Create(this, EventArgs.Empty);
            }
            if (NewWorkItem != null)
            {
                NewWorkItem(this, e);
            }
        }

        #region ICreationCompletionMonitorable Members

        public event EventHandler Create;

        public event EventHandler Complete;

        #endregion
    }

    public class WorkItemProcessorThreadPool<T>
    {
        public IWorkItemProcessor<T> Processor { get; private set; }

        public WorkItemProcessorThreadPool(IWorkItemProcessor<T> processor)
        {
            Processor = processor;
        }

        public ICompletionMonitorable QueueUserWorkitem(T workItem)
        {
            MonitorableWorkItem<T> monitoredWorkItem = new MonitorableWorkItem<T>(Processor);
            CountBasedCompeletionMonitor completionMonitor = new CountBasedCompeletionMonitor(monitoredWorkItem);

            ThreadPool.QueueUserWorkItem(monitoredWorkItem.WaitCallback, workItem);
            monitoredWorkItem.NewWorkItem += new EventHandler<NewWorkItemEventArgs<T>>(tpWorkItem_NewWorkItem);

            return completionMonitor;
        }

        private void tpWorkItem_NewWorkItem(object sender, NewWorkItemEventArgs<T> e)
        {
            MonitorableWorkItem<T> workItem = (MonitorableWorkItem<T>)sender;
            ThreadPool.QueueUserWorkItem(workItem.WaitCallback, e.WorkItem);
        }
    }

    public class WorkItemProcessorThreadPool
    {
        public static ICompletionMonitorable QueueUserWorkItem<T>(IWorkItemProcessor<T> processor, T workItem)
        {
            WorkItemProcessorThreadPool<T> workItemProcessorThreadPool = new WorkItemProcessorThreadPool<T>(processor);
            return workItemProcessorThreadPool.QueueUserWorkitem(workItem);
        }
    }

    public class WorkItemThreadPool
    {
        public static void QueueUserWorkItem(IWorkItem workItem)
        {
            ThreadPool.QueueUserWorkItem(Callback, workItem);
        }

        private static void Callback(object state)
        {
            IWorkItem workItem = (IWorkItem)state;
            workItem.Process();
        }
    }
}
