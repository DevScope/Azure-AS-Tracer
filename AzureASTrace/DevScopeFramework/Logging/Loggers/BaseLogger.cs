using DevScope.Framework.Common.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace DevScope.Framework.Common.Logging
{
    [SkipLogging]
    public abstract class BaseLogger : ILogger
    {
        private static string format;
        private static LogEventTypeEnum currentLogLevel;

        public BaseLogger()
        {
            currentLogLevel = GetTraceLevelFromConfig();
            format = AppSettingsHelper.GetAppSetting("logger.format", false, "[%prt][%tid][%dat][%cls.%mtd][%usr]: %msg ");
        }

        public abstract void WriteLog(LogEventTypeEnum evtType, string message);

        public void Write(LogEventTypeEnum evtType, string message, Exception ex)
        {
            if (evtType < currentLogLevel)
                return;

            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException("message");

            var parsedMessage = ParseMessage(evtType, message);

            if (ex != null)
            {
                parsedMessage = string.Format("{0}{1}{2}{1}", parsedMessage, System.Environment.NewLine, ex.ToString());
            }

            WriteLog(evtType, parsedMessage);            
        }

        private static LogEventTypeEnum GetTraceLevelFromConfig()
        {
            var text = AppSettingsHelper.GetAppSetting("logger.level", false, "debug");

            var text2 = text.ToLower();

            if (text2 == "alert")
            {
                return LogEventTypeEnum.Alert;
            }
            if (text2 == "debug")
            {
                return LogEventTypeEnum.Debug;
            }
            if (text2 == "error")
            {
                return LogEventTypeEnum.Error;
            }
            if (text2 == "fatal")
            {
                return LogEventTypeEnum.Fatal;
            }
            if (text2 == "log")
            {
                return LogEventTypeEnum.Log;
            }
            if (text2 == "warning")
            {
                return LogEventTypeEnum.Log;
            }
            if (text2 == "off")
            {
                throw new ApplicationException("Invalid Log Level: " + text);
            }

            return (LogEventTypeEnum)0;
        }

        private string ParseMessage(LogEventTypeEnum evtType, string message)
        {
            string className = null;
            bool flag = false;

            var builder = new StringBuilder(format);

            builder.Replace("%usr", GetCurrentUsername());
            builder.Replace("%prt", evtType.ToString());
            builder.Replace("%dat", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"));
            builder.Replace("%msg", message);
            builder.Replace("%tab", "\t");
            builder.Replace("%nln", "\r\n");

            if (format.IndexOf("%host") != -1)
            {
                builder.Replace("%host", Environment.MachineName);
            }

            builder.Replace("%tid", System.Threading.Thread.CurrentThread.ManagedThreadId.ToString());

            if (format.IndexOf("%mtd") != -1)
            {
                className = GetClassName();
                flag = true;
                builder.Replace("%mtd", className);
            }

            if (format.IndexOf("%cls") != -1)
            {
                if (!flag)
                {
                    className = GetClassName();
                }
                builder.Replace("%cls", className);
            }

            return builder.ToString();
        }

        private static string GetCurrentUsername()
        {
            var username = System.Threading.Thread.CurrentPrincipal.Identity.Name;

            if (string.IsNullOrEmpty(username))
            {
                username = string.Format(@"{0}\{1}", System.Environment.UserDomainName, System.Environment.UserName);
            }

            return username;
        }

        #region Helpers

        internal static string GetClassName()
        {
            var stackTraceInfo = GetStackTraceInfo();

            if (stackTraceInfo != null)
            {
                return stackTraceInfo.MethodName;            
            }

            return null;
        }

        private static StackTraceInfo GetStackTraceInfo()
        {
            var index = 1;
            string className, methodName;
            MethodBase method;
            var trace = new StackTrace(Thread.CurrentThread, false);

            var frame = trace.GetFrame(index);
            method = frame.GetMethod();

            while (frame != null)
            {
                method = frame.GetMethod();

                if (method.ReflectedType == null)
                {
                    break;
                }

                var customAttributes = method.ReflectedType.GetCustomAttributes(typeof(SkipLogging), false);

                var objArray2 = method.GetCustomAttributes(typeof(SkipLogging), false);

                if (((customAttributes == null) || (customAttributes.Length == 0)) && ((objArray2 == null) || (objArray2.Length == 0)))
                {
                    break;
                }

                index++;

                frame = trace.GetFrame(index);
            }

            methodName = "(not found)";
            className = "(not found)";

            if (method != null)
            {
                methodName = method.Name;

                if (method.DeclaringType != null)
                {
                    className = method.DeclaringType.FullName;
                }                
            }       

            return new StackTraceInfo { ClassName = className, MethodName = methodName };
        }

        #endregion

        public class StackTraceInfo
        {
            public string ClassName;
            public string MethodName;
        }
    }
}
