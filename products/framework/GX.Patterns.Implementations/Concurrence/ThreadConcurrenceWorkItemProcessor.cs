using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GX.Patterns.Concurrence;
using System.Threading;

namespace GX.Patterns.Implementations.Concurrence
{
    public class ThreadConcurrenceWorkItem<T> : ThreadSafeQueueProcessor<T>
    {
        private IWorkItemProcessor<T> workitemProcessor;

        public ThreadConcurrenceWorkItem(IWorkItemProcessor<T> workitemProcessor, Queue<T> queue, EventWaitHandle newWorkItem, Func<bool> queryContinueCallback)
            : base(queue, newWorkItem, queryContinueCallback)
        {
            this.workitemProcessor = workitemProcessor;
            this.workitemProcessor.NewWorkItem += new EventHandler<NewWorkItemEventArgs<T>>(workitemProcessor_OnNewWorkItem);
        }

        void workitemProcessor_OnNewWorkItem(object sender, NewWorkItemEventArgs<T> e)
        {
            QueueWorkItem(e.WorkItem);
        }

        protected override void ProcessWorkItem(T workItem)
        {
            workitemProcessor.ProcessWorkItem(workItem);
        }

        protected override bool IsValidWorkItem(T workItem)
        {
            return workitemProcessor.IsValidWorkItem(workItem);
        }
    }

    public class ThreadConcurrenceWorkItemProcessor<T>
    {

        class ThreadConcurrenceWorkItemWaithandle : IWaitHandle
        {
            public WaitHandle WaitHandle { get; private set; }
            public ThreadConcurrenceWorkItemWaithandle(WaitHandle waitHandle)
            {
                this.WaitHandle = waitHandle;
            }

            #region IWaitHandle Members

            public void WaitOne()
            {
                WaitHandle.WaitOne();
            }

            #endregion
        }
        AutoResetEvent newWorkItemEvent = new AutoResetEvent(false);

        public IWaitHandle QueueUserWorkItem(IWorkItemProcessor<T> workItemProcessor, T workItem)
        {
            Queue<T> queue = new Queue<T>();

            ThreadConcurrenceWorkItemWaithandle waitHandle = new ThreadConcurrenceWorkItemWaithandle(new ManualResetEvent(false));

            ThreadConcurrenceWorkItem<T> threadConcurrenceWorkItem = new ThreadConcurrenceWorkItem<T>(workItemProcessor, queue, newWorkItemEvent, OnQueryContinue);

            WorkItemThreadPool.QueueUserWorkItem(threadConcurrenceWorkItem);
            return waitHandle;
        }

        private bool OnQueryContinue()
        {
            return false;
        }
        private void OnWorkItemStart()
        {
        }
        private void OnWorkItemComplete()
        {
        }

    }
}
