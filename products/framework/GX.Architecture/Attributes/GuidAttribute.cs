using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GX.Architecture.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public class GuidAttribute : Attribute
    {
        public Guid Guid { get; set; }

        public GuidAttribute(Guid guid)
        {
            this.Guid = guid;
        }

        public GuidAttribute(string guid)
        {
            this.Guid = Guid.Parse(guid);
        }
    }
}
