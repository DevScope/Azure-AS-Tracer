using DevScope.Framework.Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevScope.Framework.Common.Logging
{
    public class ConsoleLogger : TraceLogger
    {
        public override void WriteLog(LogEventTypeEnum evtType, string message)
        {
            base.WriteLog(evtType, message);

            Console.WriteLine(message);
        }
    }
}
