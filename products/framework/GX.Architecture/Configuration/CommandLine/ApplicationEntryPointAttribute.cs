using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GX.Architecture.Configuration.CommandLine
{
    [AttributeUsage(AttributeTargets.Method)]
    class ApplicationEntryPointAttribute : Attribute
    {
    }
}
