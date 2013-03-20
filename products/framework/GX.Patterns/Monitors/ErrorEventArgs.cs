using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GX.Patterns
{
    public class ErrorEventArgs : EventArgs
    {
        public Exception Error { get; private set; }

        public ErrorEventArgs(Exception error)
        {
            this.Error = error;
        }
    }
}
