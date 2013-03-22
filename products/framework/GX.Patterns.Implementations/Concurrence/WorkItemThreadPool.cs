using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace GX.Patterns.Concurrence
{
    public class CountBasedCompeletionMonitor : ICompletionMonitorable
    {
        private int count = 0;

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
            if (Complete != null)
            {
                Complete(this, e);
            }
        }

        void target_New(object sender, EventArgs e)
        {
            Interlocked.Increment(ref count);
        }
    }

    public class MonitorableProcessor<T> : ICreationCompletionMonitorable
    {
        public IWorkItemProcessor<T> Processor { get; private set; }
        public event EventHandler<NewWorkItemEventArgs<T>> NewWorkItem;

        public MonitorableProcessor(IWorkItemProcessor<T> processor)
        {
            Processor = processor;
            Processor.NewWorkItem += new EventHandler<NewWorkItemEventArgs<T>>(Processor_OnNewWorkItem);
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

    public class WorkItemPool<T> : ICreationCompletionMonitorable
    {
        public IWorkItemProcessor<T> Processor { get; private set; }
        private MonitorableProcessor<T> monitoredProcessor;
        private CountBasedCompeletionMonitor completionMonitor;

        public WorkItemPool(IWorkItemProcessor<T> processor)
        {
            Processor = processor;
            monitoredProcessor = new MonitorableProcessor<T>(Processor);
            completionMonitor = new CountBasedCompeletionMonitor(this);
            processor.NewWorkItem += new EventHandler<NewWorkItemEventArgs<T>>(processor_NewWorkItem);
            processor.Complete += new EventHandler<WorkItemEventArgs<T>>(processor_Complete);
            monitoredProcessor.NewWorkItem += new EventHandler<NewWorkItemEventArgs<T>>(tpWorkItem_NewWorkItem);
        }

        void processor_Complete(object sender, WorkItemEventArgs<T> e)
        {
            OnComplete(e);
        }

        void processor_NewWorkItem(object sender, NewWorkItemEventArgs<T> e)
        {
            OnCreate(e);
        }

        public ICompletionMonitorable QueueUserWorkitem(T workItem)
        {
            ThreadPool.QueueUserWorkItem(monitoredProcessor.WaitCallback, workItem);
            OnCreate(new EventArgs());
            return completionMonitor;
        }

        private void tpWorkItem_NewWorkItem(object sender, NewWorkItemEventArgs<T> e)
        {
            MonitorableProcessor<T> workItem = (MonitorableProcessor<T>)sender;
            ThreadPool.QueueUserWorkItem(workItem.WaitCallback, e.WorkItem);
        }


        protected virtual void OnCreate(EventArgs e)
        {
            if (Create != null)
            {
                Create(this, e);
            }
        }

        protected virtual void OnComplete(EventArgs e)
        {
            if (Complete != null)
            {
                Complete(this, e);
            }
        }

        #region ICreationCompletionMonitorable Members

        public event EventHandler Create;

        public event EventHandler Complete;

        #endregion
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
