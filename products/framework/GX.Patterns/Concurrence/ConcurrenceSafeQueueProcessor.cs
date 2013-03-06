using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GX.Patterns.Concurrence
{
    public abstract class ConcurrenceSafeQueueProcessor<T> : IQueueProcessor
    {
        public event EventHandler Start;
        public event EventHandler Complete;

        private Func<bool> queryContinueCallback;

        public ConcurrenceSafeQueueProcessor(Func<bool> queryContinueCallback)
        {
            this.queryContinueCallback = queryContinueCallback;
        }

        #region template methods
        protected abstract T DequeueWorkItem();
        protected abstract void QueueWorkItem(T workitem);
        protected abstract void ProcessWorkItem(T workItem);
        protected abstract bool IsValidWorkItem(T workItem);
        #endregion

        #region implementations
        protected virtual void OnStart(EventArgs e)
        {
            if (Start != null)
            {
                Start(this, e);
            }
        }

        protected virtual void OnComplete(EventArgs e)
        {
            if (Complete != null)
            {
                Complete(this, e);
            }
        }

        protected virtual bool Continue()
        {
            return queryContinueCallback();
        }

        public void Process()
        {
            OnStart(EventArgs.Empty);
            while (Continue())
            {
                T item = DequeueWorkItem();
                if (IsValidWorkItem(item))
                {
                    ProcessWorkItem(item);
                }
            }

            OnComplete(EventArgs.Empty);
        }
        #endregion
    }
}
