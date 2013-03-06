using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GX.Architecture.Log
{
    public class EventLog
    {
        static ILogWriter _logWriter;
        public static void SetLogWriter(ILogWriter logWriter)
        {
            _logWriter = logWriter;
        }

        public static void Write(System.Diagnostics.EventLogEntryType type, string format, params object[]args)
        {
            _logWriter.Write(string.Format(format, args), type);
        }

        public static void WriteError(string format, params object[] args)
        {
            _logWriter.Write(string.Format(format, args), System.Diagnostics.EventLogEntryType.Error);
        }

        public static void WriteInformation(string format, params object[] args)
        {
            _logWriter.Write(string.Format(format, args), System.Diagnostics.EventLogEntryType.Information);
        }

        public static void WriteWarning(string format, params object[] args)
        {
            _logWriter.Write(string.Format(format, args), System.Diagnostics.EventLogEntryType.Warning);
        }

        public static void WriteProgress(string format, params object[]args)
        {
            _logWriter.WriteProgress(string.Format(format, args));
        }
    }
}
