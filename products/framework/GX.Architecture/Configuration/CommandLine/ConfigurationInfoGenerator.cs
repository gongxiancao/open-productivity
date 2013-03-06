using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace GX.Architecture.Configuration.CommandLine
{
    public class ConfigurationInfoGenerator
    {
    }

    public class MehtodParamterConfigurationDefinitionGenerator : ConfigurationInfoGenerator
    {
        public T GetAttribute<T>(ParameterInfo pi) where T : Attribute
        {
            object[] attributes = pi.GetCustomAttributes(typeof(T), false);
            string name = pi.Name;
            if (attributes.Length > 0)
            {
                return (T)attributes[0];
            }
            return null;
        }

        public ConfigurationInfo Generate(MethodInfo methodInfo)
        {
            List<ConfigurationPropertyInfo> properties = new List<ConfigurationPropertyInfo>();
            foreach (ParameterInfo pi in methodInfo.GetParameters())
            {
                string name = pi.Name;
                NameAttribute nameAttribute = GetAttribute<NameAttribute>(pi);
                if (nameAttribute != null)
                {
                    name = nameAttribute.Name;
                }

                string alias = name;

                AliasAttribute aliasAttribute = GetAttribute<AliasAttribute>(pi);
                if (aliasAttribute != null)
                {
                    alias = aliasAttribute.Alias;
                }

                object defaultValue = null;

                DefaultValueAttribute defaultValueAttribute = GetAttribute<DefaultValueAttribute>(pi);
                if (defaultValueAttribute != null)
                {
                    defaultValue = defaultValueAttribute.DefaultValue;
                }

                SwitchAttribute switchAttribute = GetAttribute<SwitchAttribute>(pi);

                if (switchAttribute != null)
                {
                    bool defaultSwitch = false;
                    if (defaultValue != null)
                    {
                        defaultSwitch = (bool)defaultValueAttribute.DefaultValue;
                    }
                    properties.Add(new ConfigurationSwitchPropertyInfo(name, alias, defaultSwitch));
                }
                else
                {
                    properties.Add(new ConfigurationPropertyInfo(name, alias, defaultValue));
                }
            }

            return new ConfigurationInfo(properties);
        }
    }
}
