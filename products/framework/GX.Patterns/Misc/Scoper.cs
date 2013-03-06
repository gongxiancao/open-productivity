using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GX.Patterns.Misc
{
    public abstract class Scoper : IDisposable
    {
        private bool disposed = false;

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        ~Scoper()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    RestoreState();
                }
            }
        }

        protected abstract void RestoreState();
    }
}
