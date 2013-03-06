using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using GX.Patterns;
using GX.Architecture.Configuration.CommandLine;

namespace mtcopy
{
    public class ArgumentPosition : Attribute
    {
        public int Position { get; private set;}
        public ArgumentPosition(int position)
        {
            Position = position;
        }
    }
    internal enum Config
    {
        [ArgumentPosition(0)]
        Source,
        [ArgumentPosition(1)]
        Destination
    }

    internal static class ConfigExension
    {
        static ISettingsProvider _s_settings = new CommandLineParser(typeof(Config)).Parse(Environment.GetCommandLineArgs());

        public static string GetValue(this Config config, string defaultValue)
        {
            string value = _s_settings.GetValue(config.ToString());
            if (value == null)
            {
                value = defaultValue;
            }
            return value;
        }

        public static bool IsSet(this Config config)
        {
            return GetValue(config, null) != null;
        }

        public static string GetValue(this Config config)
        {
            string value = _s_settings.GetValue(config.ToString());
            if (value == null)
            {
                throw new ArgumentException("Failed to get argument", config.ToString());
            }
            return value;
        }
    }
}
