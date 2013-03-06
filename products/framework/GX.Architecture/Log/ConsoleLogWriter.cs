using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GX.Patterns.Misc;

namespace GX.Architecture.Log
{

    public class ConsoleLogWriter : ILogWriter
    {
        class LogConsoleColorScoper : Scoper
        {
            ConsoleColor color;

            public LogConsoleColorScoper(ConsoleColor color)
            {
                this.color = Console.ForegroundColor;
                Console.ForegroundColor = color;
            }

            protected override void RestoreState()
            {
                Console.ForegroundColor = color;
            }
        }
        #region ILogWriter Members

        public void Write(string message, System.Diagnostics.EventLogEntryType type)
        {
            switch (type)
            {
                case System.Diagnostics.EventLogEntryType.Error:
                case System.Diagnostics.EventLogEntryType.FailureAudit:
                    using (new LogConsoleColorScoper(ConsoleColor.Red))
                    {
                        Console.Error.WriteLine(message);
                    }
                    break;
                case System.Diagnostics.EventLogEntryType.Information:
                case System.Diagnostics.EventLogEntryType.SuccessAudit:
                    using (new LogConsoleColorScoper(ConsoleColor.Green))
                    {
                        Console.WriteLine(message);
                    }
                    break;
                case System.Diagnostics.EventLogEntryType.Warning:
                    using (new LogConsoleColorScoper(ConsoleColor.Yellow))
                    {
                        Console.WriteLine(message);
                    }
                    break;
            }
        }

        public void WriteProgress(string message)
        {
            Console.WriteLine(message);
        }

        #endregion
    }
}
