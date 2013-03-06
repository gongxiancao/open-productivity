using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace GX.Architecture.Configuration.CommandLine
{
    public static class CommandBindingCall
    {
        public static object Invoke(object obj, MethodInfo methodInfo, string commandLine)
        {
            MehtodParamterConfigurationDefinitionGenerator gen = new MehtodParamterConfigurationDefinitionGenerator();

            ConfigurationInfo configuartionInfo = gen.Generate(methodInfo);
            CommandLineParser target = new CommandLineParser(configuartionInfo);
            object[] parameters = target.Parse(commandLine);
            return methodInfo.Invoke(obj, parameters);
        }

        public static object Invoke(Delegate del, string commandLine)
        {
            return Invoke(del.Target, del.Method, commandLine);
        }

    }
}
