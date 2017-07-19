using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.IO;

namespace DevScope.Framework.Common.Logging
{
    /// <summary>
    /// Logger usado no addin.
    /// Log simples para ficheiro mas com limite de tamanho.
    /// </summary>
    public sealed class AddInLogger : NLogLogger
    {        
        public string LogFile { get; set; }        

        public AddInLogger(string logFile, int fileMaxSize)
        {
            if (string.IsNullOrEmpty(logFile))
                throw new ArgumentNullException("logFile");            

            if (File.Exists(logFile))
            {
                //No arranque verifica tamanho mas durante a sessão não de forma a não quebrar a sequência se necessário

                FileLogger.CleanLogFileBySize(logFile, fileMaxSize);                
            }

            this.LogFile = logFile;
        }

        public override void ExtendLogEvent(NLog.LogEventInfo logEvent)
        {
            base.ExtendLogEvent(logEvent);

            if (!string.IsNullOrEmpty(this.LogFile))
            {
                logEvent.Properties.Add("logFile", this.LogFile);
            }
        }
  
    }

    //public sealed class AddInLogger : BaseLogger
    //{
    //    private static object locker = new object();
    //    public string LogFile { get; set; }        

    //    public AddInLogger(string logFile, int fileMaxSize)
    //    {
    //        if (string.IsNullOrEmpty(logFile))
    //            throw new ArgumentNullException("logFile");            

    //        if (File.Exists(logFile))
    //        {
    //            //No arranque verifica tamanho mas durante a sessão não de forma a não quebrar a sequência se necessário

    //            FileLogger.CleanLogFileBySize(logFile, fileMaxSize);                
    //        }

    //        this.LogFile = logFile;
    //    }
  
    //    public override void WriteLog(LogEventTypeEnum evtType, string message)
    //    {
    //        //base.WriteLog(evtType, message);

    //        //É expectável muita concorrência melhor usar o lock.

    //        lock (locker)
    //        {
    //            System.Diagnostics.Debug.WriteLine(message);

    //            message = string.Concat(message, System.Environment.NewLine);

    //            try
    //            {
    //                File.AppendAllText(this.LogFile, message);
    //            }
    //            catch (DirectoryNotFoundException)
    //            {
    //                Directory.CreateDirectory(Path.GetDirectoryName(this.LogFile));

    //                File.AppendAllText(this.LogFile, message);
    //            }
    //        }
    //    }
    //}
}
