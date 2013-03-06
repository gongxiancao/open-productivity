using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace GX.Architecture.Data
{
    public static class XmlNodeExt
    {
        public static string GetAttributeValue(this XmlNode node, string attribute, string defaultValue)
        {
            XmlAttribute attr = node.Attributes[attribute];
            if (attr != null)
            {
                return attr.Value;
            }
            return defaultValue;
        }
    }
}
