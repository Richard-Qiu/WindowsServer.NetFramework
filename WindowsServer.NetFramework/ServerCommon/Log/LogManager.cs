using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Log
{
    public sealed class LogManager
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Logger GetCurrentClassLogger()
        {
            string loggerName;
            Type declaringType;
            int framesToSkip = 1;
            do
            {
#if SILVERLIGHT
                StackFrame frame = new StackTrace().GetFrame(framesToSkip);
#else
                StackFrame frame = new StackFrame(framesToSkip, false);
#endif
                var method = frame.GetMethod();
                declaringType = method.DeclaringType;
                if (declaringType == null)
                {
                    loggerName = method.Name;
                    break;
                }

                framesToSkip++;
                loggerName = declaringType.FullName;
            } while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

            return new Logger(NLog.LogManager.GetLogger(loggerName));
        }

        public static Logger GetLogger(string loggerName)
        {
            return new Logger(NLog.LogManager.GetLogger(loggerName));
        }

        public static void UseConsoleLoggingConfiguration(bool enableDebugLog = false)
        {
            var logConfig = new LoggingConfiguration();
            var errorTarget = new ConsoleTarget()
            {
                Layout = new SimpleLayout()
                {
                    Text = "${longdate}|${level}|${logger}|${message}|${exception:tostring}",
                },
                Error = true,
            };
            LoggingRule rule1 = new LoggingRule("*", LogLevel.Error, errorTarget);
            logConfig.LoggingRules.Add(rule1);

            var outputTarget = new ConsoleTarget()
            {
                Layout = new SimpleLayout()
                {
                    Text = "${longdate}|${level}|${logger}|${message}|${exception:tostring}",
                },
                Error = false,
            };
            LoggingRule rule2 = new LoggingRule("*", (enableDebugLog ? LogLevel.Debug : LogLevel.Info), outputTarget);
            logConfig.LoggingRules.Add(rule2);

            NLog.LogManager.Configuration = logConfig;
        }

    }
}
