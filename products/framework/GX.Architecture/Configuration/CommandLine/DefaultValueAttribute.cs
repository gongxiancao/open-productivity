using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GX.Architecture.Configuration.CommandLine
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class DefaultValueAttribute:Attribute
    {
        public object DefaultValue { get; set; }
        public DefaultValueAttribute(object defaultValue)
        {
            this.DefaultValue = defaultValue;
        }
    }
}
