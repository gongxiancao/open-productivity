using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GX.Architecture.Configuration.CommandLine
{
    public class ConfigurationPropertyInfo
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public object DefaultValue { get; set; }
        public ConfigurationPropertyInfo()
        {
        }

        public ConfigurationPropertyInfo(string name, string alias, object defaultValue)
        {
            Name = name;
            Alias = alias;
            DefaultValue = defaultValue;
        }

    }

    public class ConfigurationSwitchPropertyInfo : ConfigurationPropertyInfo
    {
        public ConfigurationSwitchPropertyInfo()
        {
        }

        public ConfigurationSwitchPropertyInfo(string name, string alias, bool defaultValue)
            : base(name, alias, defaultValue)
        {
        }
    }

    public class ConfigurationInfo
    {
        public IEnumerable<ConfigurationPropertyInfo> Definitions { get; private set; }
        public ConfigurationInfo(IEnumerable<ConfigurationPropertyInfo> definitions)
        {
            Definitions = new List<ConfigurationPropertyInfo>(definitions);
        }
    }
}
