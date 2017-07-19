using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevScope.Framework.Common.Logging;

namespace DevScope.Framework.Common.Utils
{
    [SkipLogging()]
    public sealed class TimeWatcher : IDisposable
    {

        private int maxSecondsWarning;
        private string message;
        private DateTime started;

        public TimeWatcher(string message, params object[] args)
            : this(-1, message, args)
        {
        }

        public TimeWatcher(int maxSecondsWarning, string message, params object[] args)
        {            
            this.maxSecondsWarning = maxSecondsWarning;
            this.message = string.Format(message, args);
            this.started = DateTime.Now;
        }

        public TimeSpan CalculateTimeSpan()
        {
            return DateTime.Now.Subtract(started);
        }

        #region IDisposable Members

        public void Dispose()
        {
            var time = CalculateTimeSpan();

            if (maxSecondsWarning != -1 && time.TotalSeconds > maxSecondsWarning)
            {
                Logger.Warning("Time of Operation '{1}' exceeded time limit ({2}s): {0}s", time.TotalSeconds, message, maxSecondsWarning);
            }
            else
            {
                Logger.Debug("Time of Operation '{0}': {1}s ", message, time.TotalSeconds);
            }
        }

        #endregion
    }
}
