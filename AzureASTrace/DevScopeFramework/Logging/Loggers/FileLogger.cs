using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using System.IO;
using System.Configuration;
using DevScope.Framework.Common.Utils;

namespace DevScope.Framework.Common.Logging
{
    /// <summary>
    /// Implementação de logger em ficheiro com substituição de tokens.
    /// </summary>
    public class FileLogger : ConsoleLogger
    {
        private static object locker = new object();

        private string GetPath(LogEventTypeEnum evtType, bool isErrorPath)
        {
            string loggerPath;

            if (isErrorPath)
            {
                loggerPath = AppSettingsHelper.GetAppSetting("logger.errorpath", false, @"%appdir\Logs\%dat\Errors\Logs.%usr.log");
            }
            else
            {
                loggerPath = AppSettingsHelper.GetAppSetting("logger.path", false, @"%appdir\Logs\%dat\Logs.%usr.log");
            }

            var builder = new StringBuilder(loggerPath);
            builder.Replace("%appdir", AppDomain.CurrentDomain.BaseDirectory);
            builder.Replace("%tempdir", Path.GetTempPath());
            builder.Replace("%usr", this.GetValidPathName(System.Threading.Thread.CurrentPrincipal.Identity.Name));
            builder.Replace("%prt", this.GetValidPathName(evtType.ToString()));
            builder.Replace("%dat", this.GetValidPathName(DateTime.Now.ToString("yyyy-MM-dd")));

            if (loggerPath.IndexOf("%host") != -1)
            {
                builder.Replace("%host", this.GetValidPathName(Environment.MachineName));
            }

            builder.Replace("/", @"\");

            return builder.ToString();
        }

        private string GetValidPathName(string strPath)
        {
            StringBuilder builder = new StringBuilder(strPath);
            builder.Replace(@"\", "_");
            builder.Replace("/", "_");
            builder.Replace(":", "_");
            builder.Replace("*", "_");
            builder.Replace("?", "_");
            builder.Replace("\"", "_");
            builder.Replace("<", "_");
            builder.Replace(">", "_");
            builder.Replace("|", "_");
            builder.Replace(".", "_");
            return builder.ToString();
        }

        public override void WriteLog(LogEventTypeEnum evtType, string message)
        {
            base.WriteLog(evtType, message);

            this.TrySaveLog(this.GetPath(evtType, false), message);

            if (evtType >= LogEventTypeEnum.Error)
            {
                this.TrySaveLog(this.GetPath(evtType, true), message);
            }
        }        

        private void SaveLog(string path, string message)
        {
            lock (locker)
            {
                File.AppendAllLines(path, new string[] { message });
            }
        }

        private void TrySaveLog(string path, string message)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            try
            {
                this.SaveLog(path, message);
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                this.SaveLog(path, message);
            }
        }

        public static void CleanLogFileBySize(string filePath, int fileSizeLimit)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return;
                }

                var fi = new FileInfo(filePath);
                
                if (fileSizeLimit != -1 && fi.Length >= fileSizeLimit)
                {
                    File.Delete(filePath);

                    var warning = string.Format("This file has been cleaned, because exceeded the size limit of {0} bytes\r\n", fileSizeLimit);

                    File.WriteAllText(filePath, warning);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
            }
        }
    }
}
