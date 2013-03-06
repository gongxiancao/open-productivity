using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GX.Patterns;
using System.IO;
using GX.Algorithm.Parser;

namespace GX.Architecture.Configuration.CommandLine
{
    using TokenType = CommandLineLex.TokenType;
    public class CommandLineParser
    {
        Dictionary<string, ConfigurationPropertyInfo> aliasArgument = new Dictionary<string, ConfigurationPropertyInfo>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, ConfigurationPropertyInfo> nameArgument = new Dictionary<string, ConfigurationPropertyInfo>(StringComparer.OrdinalIgnoreCase);
        ConfigurationInfo configurationInfo;
        public CommandLineParser(ConfigurationInfo configurationInfo)
        {
            this.configurationInfo = configurationInfo;
            Initialize(configurationInfo.Definitions);
        }

        public CommandLineParser(IEnumerable<ConfigurationPropertyInfo> properties)
        {
            Initialize(properties);
        }

        private void Initialize(IEnumerable<ConfigurationPropertyInfo> properties)
        {
            foreach (ConfigurationPropertyInfo argument in properties)
            {
                if (argument.Name != null)
                {
                    nameArgument[argument.Name] = argument;
                }
                if (!string.IsNullOrEmpty(argument.Alias))
                {
                    aliasArgument[argument.Alias] = argument;
                }
            }
        }

        private ConfigurationPropertyInfo GetConfigurationPropertyInfo(string key)
        {
            ConfigurationPropertyInfo argument = null;
            if (aliasArgument.TryGetValue(key, out argument))
            {
                return argument;
            }
            if (nameArgument.TryGetValue(key, out argument))
            {
                return argument;
            }
            throw new ArgumentException("Unrecognized configuration property : " + key);
        }

        string GetParseError(string expected, TokenType type, string actual)
        {
            return string.Format("Expecting {0}, got {1} value = {2}", expected, type, actual);
        }

        public object[] Parse(string source)
        {
            Dictionary<string, object> argumentValue = new Dictionary<string, object>();
            string name = null;
            string value = null;
            List<object> positionalArguments = new List<object>();
            int position = 0;
            int length = source.Length;
            CommandLineLex lex = new CommandLineLex();
            Token<CommandLineLex.TokenType> token = lex.ReadToken(source, ref position);

        ExpectingNewArgument:

            switch (token.Type)
            {
                case TokenType.EOF:
                    goto End;
                case TokenType.ArgumentName:

                    ConfigurationPropertyInfo cpi = GetConfigurationPropertyInfo(token.Value);
                    name = cpi.Name;

                    if (cpi is ConfigurationSwitchPropertyInfo)
                    {
                        token = lex.ReadToken(source, ref position);

                        if (token.Type == TokenType.Colon)
                        {
                            token = lex.ReadToken(source, ref position);
                            goto ExpectingArgumentValue;
                        }
                        argumentValue[name] = true;
                        token = lex.ReadToken(source, ref position);
                        goto ExpectingNewArgument;
                    }
                    token = lex.ReadToken(source, ref position);
                    goto ExpectingArgumentValue;

                case TokenType.Colon:
                    throw new ArgumentException(GetParseError("NewArgument", token.Type, token.Value));
                case TokenType.String:
                    positionalArguments.Add(token.Value);
                    token = lex.ReadToken(source, ref position);
                    goto ExpectingNewArgument;
            }

        ExpectingArgumentValue:

            switch (token.Type)
            {
                case TokenType.ArgumentName:
                    throw new ArgumentException(GetParseError("ArgumentValue", token.Type, token.Value));
                case TokenType.Colon:
                    token = lex.ReadToken(source, ref position);
                    if (token.Type == TokenType.String)
                    {
                        value = token.Value;
                        argumentValue[name] = value;
                        token = lex.ReadToken(source, ref position);
                        goto ExpectingNewArgument;
                    }
                    else
                    {
                        throw new ArgumentException(GetParseError("ArgumentValue", token.Type, token.Value));
                    }
                case TokenType.EOF:
                    throw new ArgumentException(GetParseError("ArgumentValue", token.Type, token.Value));;
                case TokenType.String:
                    value = token.Value;
                    argumentValue[name] = value;
                    token = lex.ReadToken(source, ref position);
                    goto ExpectingNewArgument;
            }

        End:

            List<object> values = new List<object>();
            int positionalIndex = 0;
            foreach (var pi in configurationInfo.Definitions)
            {
                if (pi.Name != null && argumentValue.ContainsKey(pi.Name))
                {
                    values.Add(argumentValue[pi.Name]);
                }
                else if (positionalIndex < positionalArguments.Count)
                {
                    values.Add(positionalArguments[positionalIndex]);
                    ++positionalIndex;
                }
                else
                {
                    values.Add(pi.DefaultValue);
                }
            }

            return values.ToArray();
        }
    }
}
