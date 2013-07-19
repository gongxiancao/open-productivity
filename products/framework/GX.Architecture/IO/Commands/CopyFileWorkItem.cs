using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GX.Architecture.IO.Commands
{
     public class CopyFileWorkItem : FileWorkItem
    {
        public string Destination { get; set; }
        public double ProgressWeight { get; set; }
        public long FinishedSize { get; set; }
        public object Tag { get; set; }
        public object FailedReason { get; set; }

        public override string ToString()
        {
            return string.Format("{{Item:{0},Destination:{1}}}", Item.FullName, Destination);
        }
    }
}
