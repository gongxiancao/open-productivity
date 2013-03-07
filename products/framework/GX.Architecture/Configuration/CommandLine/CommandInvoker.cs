using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace GX.Architecture.Configuration.CommandLine
{
    public static class CommandInvoker
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

        public static object Invoke(object obj, Type type, string commandLine)
        {
            MethodInfo methodInfo = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).First(m =>
            {
                return m.GetCustomAttributes(typeof(ApplicationEntryPointAttribute), false).Length > 0;
            });

            return Invoke(obj, methodInfo, commandLine);
        }

        public static object Invoke(Type type, string commandLine)
        {
            MethodInfo methodInfo = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).First(m =>
            {
                return m.GetCustomAttributes(typeof(ApplicationEntryPointAttribute), false).Length > 0;
            });

            return Invoke(null, methodInfo, commandLine);
        }

        public static object Invoke(object obj, string commandLine)
        {
            return Invoke(obj, obj.GetType(), commandLine);
        }
    }
}
