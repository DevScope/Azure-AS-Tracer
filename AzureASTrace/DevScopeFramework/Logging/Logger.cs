using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using System.IO;
using DevScope.Framework.Common.Utils;

namespace DevScope.Framework.Common.Logging
{
    [SkipLogging]
    public static class Logger
    {
        private static ILogger logger;

        public static ILogger CurrentLogger
        {
            get
            {
                return logger;
            }
            set
            {
                logger = value;
            }
        }

        static Logger()
        {            
        }

        #region Overrides

        public static bool Debug(string message)
        {
            return Debug(null, message, null);
        }

        public static bool Debug(Exception ex, string message)
        {
            return Debug(ex, message, null);
        }

        public static bool Debug(string message, params object[] messageParameters)
        {
            return Debug(null, message, messageParameters);
        }

        public static bool Debug(Exception ex, string message, params object[] messageParameters)
        {
            return WriteLog(LogEventTypeEnum.Debug, ex, message, messageParameters);
        }

        public static bool Error(string message)
        {
            return Error(null, message, null);
        }

        public static bool Error(string message, params object[] messageParameters)
        {
            return Error(null, message, messageParameters);
        }

        public static bool Error(Exception ex)
        {
            return Error(ex, string.Empty, null);
        }

        public static bool Error(Exception ex, string message)
        {
            return Error(ex, message, null);
        }

        public static bool Error(Exception ex, string message, params object[] messageParameters)
        {
            return WriteLog(LogEventTypeEnum.Error, ex, message, messageParameters);
        }

        public static bool Log(string message)
        {
            return Log(message, null);
        }

        public static bool Log(string message, params object[] messageParameters)
        {
            return WriteLog(LogEventTypeEnum.Log, null, message, messageParameters);
        }

        public static bool Warning(string message)
        {
            return Warning(null, message, null);
        }

        public static bool Warning(Exception ex, string message)
        {
            return Warning(ex, message, null);
        }

        public static bool Warning(string message, params object[] messageParameters)
        {
            return Warning(null, message, messageParameters);
        }

        public static bool Warning(Exception ex, string message, params object[] messageParameters)
        {
            return WriteLog(LogEventTypeEnum.Warning, ex, message, messageParameters);
        }

        #endregion

        public static bool WriteLog(LogEventTypeEnum evtType, Exception ex, string message, params object[] messageParameters)
        {
            try
            {
                if (logger == null)
                    throw new ApplicationException("Logger.CurrentLogger cannot be null");
                
                var formattedMessage = Logger.FormatMessage(message, messageParameters);                

                logger.Write(evtType, formattedMessage, ex);
                
                return true;
            }
            catch (Exception iex)
            {
                try
                {
                    Trace.WriteLine(string.Format("{0} - ({1:yyyy-MM-dd HH:mm:ss}): {2}"
                        , evtType
                        , DateTime.Now
                        , string.Format("Logging Error: '{0}'", iex.ToString())
                        ));
                }
                catch
                {
                }

                return false;
            }
        }

        private static string FormatMessage(string message, object[] messageParameters)
        {
            if (string.IsNullOrEmpty(message))
            {
                message = string.Empty;
            }
            else if (messageParameters != null)
            {
                message = string.Format(message, messageParameters);
            }

            return message;
        }
    }
}
