using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GX.Patterns.Concurrence
{
    public abstract class ConcurrenceWorkitem<T> : IWorkItem
    {
        private Func notifyStartCallback;
        private Func notifyCompleteCallback;
        private Func<bool> queryContinueCallback;

        public ConcurrenceWorkitem(Func<bool> queryContinueCallback, Func notifyStartCallback, Func notifyCompleteCallback)
        {
            this.queryContinueCallback = queryContinueCallback;
            this.notifyStartCallback = notifyStartCallback;
            this.notifyCompleteCallback = notifyCompleteCallback;
        }

        protected abstract T DequeueWorkItem();
        protected abstract void QueueWorkItem(T workitem);
        protected abstract void ProcessWorkItem(T workItem);
        protected abstract bool IsValidWorkItem(T workItem);

        protected virtual void OnStart()
        {
            if (notifyStartCallback != null)
            {
                notifyStartCallback();
            }
        }

        protected virtual void OnComplete()
        {
            if (notifyCompleteCallback != null)
            {
                notifyCompleteCallback();
            }
        }

        protected virtual bool Continue()
        {
            return queryContinueCallback();
        }

        #region IWorkItem Members

        public void Process()
        {
            OnStart();
            while (Continue())
            {
                T item = DequeueWorkItem();
                if (IsValidWorkItem(item))
                {
                    ProcessWorkItem(item);
                }
            }

            OnComplete();
        }

        #endregion
    }
}
