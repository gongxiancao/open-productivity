using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GX.Patterns.Concurrence;
using System.Threading;

namespace GX.Patterns.Implementations.Concurrence
{
    public abstract class ThreadSafeQueueProcessor<T> : ConcurrenceSafeQueueProcessor<T>
    {
        private Queue<T> queue;
        EventWaitHandle newWorkItemEvent;

        public ThreadSafeQueueProcessor(Queue<T> queue, EventWaitHandle newWorkItemEvent, Func<bool> queryContinueCallback)
            : base(queryContinueCallback)
        {
            this.queue = queue;
            this.newWorkItemEvent = newWorkItemEvent;
        }

        protected override void QueueWorkItem(T workitem)
        {
            queue.Enqueue(workitem);
            newWorkItemEvent.Set();
        }

        protected override T DequeueWorkItem()
        {
            while (Continue())
            {
                if (queue.Count > 0)
                {
                    lock (queue)
                    {
                        if (queue.Count > 0)
                        {
                            return queue.Dequeue();
                        }
                    }
                }
                newWorkItemEvent.WaitOne();
            }
            return default(T);
        }
    }
}
