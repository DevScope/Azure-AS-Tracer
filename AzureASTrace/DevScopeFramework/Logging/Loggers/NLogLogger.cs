using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NLog;

namespace DevScope.Framework.Common.Logging
{
    [SkipLogging]
    public class NLogLogger : ILogger
    {
        private static NLog.Logger Logger { get; set; }   

        public NLogLogger()
        {
        }        

        public virtual void ExtendLogEvent(NLog.LogEventInfo logEvent)
        {
            logEvent.Properties.Add("classname", BaseLogger.GetClassName()); 
        }

        public void Write(LogEventTypeEnum evtType, string message, Exception ex)
        {
            if (Logger == null)
            {
                Logger = NLog.LogManager.GetLogger("AppDomainLogger");      
            }
            
            var logLevel = LogLevel.Info;

            switch (evtType)
            {
                case LogEventTypeEnum.Warning:
                    logLevel = LogLevel.Warn;
                    break;
                case LogEventTypeEnum.Error:
                    logLevel = LogLevel.Error;
                    break;
                case LogEventTypeEnum.Debug:
                    logLevel = LogLevel.Debug;
                    break;                
            }

            var formatProvider = System.Globalization.CultureInfo.InvariantCulture;

            var logEvent = new LogEventInfo(logLevel, Logger.Name, formatProvider, message, null, ex);

            ExtendLogEvent(logEvent);

            Logger.Log(logEvent);
        }
    }
}
