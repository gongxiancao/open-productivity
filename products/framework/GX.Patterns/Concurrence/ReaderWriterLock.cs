using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GX.Patterns.Misc;
using System.Threading;

namespace GX.Patterns.Concurrence
{
    public class ReaderLock : Scoper
    {
        ReaderWriterLock rwLock;
        public ReaderLock(ReaderWriterLock rwLock, int millisecondsTimeout)
        {
            this.rwLock = rwLock;
            rwLock.AcquireReaderLock(millisecondsTimeout);
        }
        protected override void RestoreState()
        {
            rwLock.ReleaseReaderLock();
        }
    }

    public class WriterLock : Scoper
    {
        ReaderWriterLock rwLock;
        public WriterLock(ReaderWriterLock rwLock, int millisecondsTimeout)
        {
            this.rwLock = rwLock;
            rwLock.AcquireWriterLock(millisecondsTimeout);
        }
        protected override void RestoreState()
        {
            rwLock.ReleaseWriterLock();
        }
    }
}
