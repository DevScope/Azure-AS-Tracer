using System;
using System.Collections.Generic;
using System.Text;

namespace DevScope.Framework.Common.Logging
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class SkipLogging : Attribute
    {
    }

    public enum LogEventTypeEnum
    {
        Debug = 100,
        Log = 200,
        Alert = 300,
        Warning = 400,
        Error = 500,
        Fatal = 600,
    }

    public interface ILogger
    {
        void Write(LogEventTypeEnum evtType, string message, Exception ex);
    }
}
