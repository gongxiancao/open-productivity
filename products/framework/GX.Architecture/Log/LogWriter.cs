using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace GX.Architecture.Log
{
    public interface ILogWriter
    {
        void Write(string message, EventLogEntryType type);
        void WriteProgress(string message);
    }
}
