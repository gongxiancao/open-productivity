using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GX.Architecture.Configuration.CommandLine
{
    [AttributeUsage(AttributeTargets.All)]
    public class AliasAttribute : Attribute
    {
        public string Alias { get; set; }
        public AliasAttribute(string alias)
        {
            this.Alias = alias;
        }
    }
}
