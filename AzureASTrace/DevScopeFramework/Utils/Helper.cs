using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections.Generic;
using System.Reflection;
using DevScope.Framework.Common.Extensions;

namespace DevScope.Framework.Common.Utils
{
    public static class Helper
    {
        public static string GetAppDirectory()
        {
            return System.AppDomain.CurrentDomain.BaseDirectory;
        }

        public static void OpenUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");

            var startUrlInfo = new ProcessStartInfo();

            startUrlInfo.FileName = url;

            Process.Start(startUrlInfo);
        }

        public static string UrlCombine(string baseUrlPath, string additionalNodes)
        {
            if (baseUrlPath == null)
            {
                throw new ArgumentNullException("baseUrlPath");
            }
            if (baseUrlPath.Length <= 0)
            {
                throw new ArgumentOutOfRangeException("baseUrlPath");
            }
            if (additionalNodes == null)
            {
                throw new ArgumentNullException("additionalNodes");
            }
            if (additionalNodes.Length <= 0)
            {
                throw new ArgumentOutOfRangeException("additionalNodes");
            }
            bool flag = baseUrlPath.EndsWith("/");
            bool flag2 = additionalNodes.StartsWith("/");
            if (flag && flag2)
            {
                return (baseUrlPath + additionalNodes.Substring(1));
            }
            if ((flag || !flag2) && (!flag || flag2))
            {
                return (baseUrlPath + "/" + additionalNodes);
            }
            return (baseUrlPath + additionalNodes);
        }              

        public static DateTime? GetExcelDate(object seconds)
        {
            if (seconds == null)
                return null;

            if (!seconds.IsNumeric())
            {
                return null;
            }

            var secs = Convert.ToDouble(seconds);

            if (secs >= 61)
            {
                // Since 02/29/1900 didn't exist, we can't return it
                --secs;
            }

            return new DateTime(1899, 12, 31, 0, 0, 0, 0).Add(TimeSpan.FromDays(Convert.ToDouble(secs)));
        }
      
        /// <summary>
        /// Execução de processo externo.
        /// RQuintino, 20090327
        /// </summary>
        /// <param name="execFilename"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string ExecuteProcess(string execFilename, string args, ProcessWindowStyle windowStyle = ProcessWindowStyle.Normal, bool ignoreExitCode = false, bool fireAndForget = false)
        {
            if (string.IsNullOrEmpty(execFilename))
                throw new ArgumentNullException("execFilename");

            ProcessStartInfo startInfo = new ProcessStartInfo(execFilename, args);
            startInfo.WindowStyle = windowStyle;

            using (Process proc = new Process())
            {
                
                proc.StartInfo = startInfo;
                proc.StartInfo.UseShellExecute = true;

                string output = null;

                if (!fireAndForget)
                {
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.RedirectStandardError = true;
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    proc.Start();

                    output = proc.StandardOutput.ReadToEnd();
                    var errorOutput = proc.StandardError.ReadToEnd();

                    proc.WaitForExit();

                    if (!ignoreExitCode && proc.ExitCode != 0)
                    {
                        string errorMsg = string.Format("Erro no comando:{0} {1}. exit code: {2}\r\nOutput: {3}\r\nError Output: {4}",
                            execFilename, args,
                            proc.ExitCode, output, errorOutput);

                        throw new ApplicationException(errorMsg);
                    }
                }
                else
                {
                    proc.Start();
                }

                return output;
            }
        }      

        /// <summary>
        /// Devolve data do assembly que está a chamar.
        /// </summary>
        /// <returns></returns>
        public static string AssemblyDate()
        {
            return AssemblyDate(System.Reflection.Assembly.GetCallingAssembly());
        }

        /// <summary>
        /// Devolve data de compilação do assembly associado.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string AssemblyDate(Assembly assembly)
        {
            string version = string.Empty;

            AssemblyName asbName = assembly.GetName();

            DateTime asbDate = new DateTime(2000, 01, 01, 0, 0, 0);
            asbDate = asbDate.AddDays(asbName.Version.Build);
            asbDate = asbDate.AddSeconds(asbName.Version.Revision * 2);

            if (TimeZone.IsDaylightSavingTime(asbDate, TimeZone.CurrentTimeZone.GetDaylightChanges(asbDate.Year)))
            {
                asbDate = asbDate.Add(TimeZone.CurrentTimeZone.GetDaylightChanges(asbDate.Year).Delta);
            }

            version = string.Format("{0:dd-MM-yyyy HH:mm:ss}", asbDate);

            return version;
        }

        public static bool IsNetworkAvaliable()
        {
            return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
        }
    }
}
