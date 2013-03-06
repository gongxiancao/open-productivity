using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GX.Architecture.Configuration.CommandLine
{
    [AttributeUsage(AttributeTargets.All)]
    public class NameAttribute : Attribute
    {
        public string Name { get; set; }
        public NameAttribute(string name)
        {
            this.Name = name;
        }
    }
}
