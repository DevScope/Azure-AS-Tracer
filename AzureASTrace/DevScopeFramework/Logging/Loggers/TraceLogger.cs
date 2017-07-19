using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using System.IO;
using System.Configuration;

namespace DevScope.Framework.Common.Logging
{
    public class TraceLogger : BaseLogger
    {
        public override void WriteLog(LogEventTypeEnum evtType, string message)
        {
            Trace.WriteLine(message);            
        }     
    }
}
