using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DevScope.Framework.Common.Extensions
{
    public static class AssemblyExtensions
    {
        public static byte[] GetResourceAsBytes(this Assembly assembly, string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName))
                throw new ArgumentNullException("resourceName");

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    return null;

                var bytes = new byte[stream.Length];
                
                stream.Read(bytes, 0, bytes.Length);
                
                return bytes;  
            }
        }

        public static string GetResourceAsString(this Assembly assembly, string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName))
                throw new ArgumentNullException("resourceName");

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    return null;

                using (var sr = new StreamReader(stream))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        public static string GetAssemblyDateStr(this Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            var dateStr = string.Format("{0:dd-MM-yyyy HH:mm:ss}", assembly.GetAssemblyDate());

            return dateStr;
        }

        public static DateTime GetAssemblyDate(this Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            var asbName = assembly.GetName();

            var asbDate = new DateTime(2000, 01, 01, 0, 0, 0);
            asbDate = asbDate.AddDays(asbName.Version.Build);
            asbDate = asbDate.AddSeconds(asbName.Version.Revision * 2);

            if (TimeZone.IsDaylightSavingTime(asbDate, TimeZone.CurrentTimeZone.GetDaylightChanges(asbDate.Year)))
            {
                asbDate = asbDate.Add(TimeZone.CurrentTimeZone.GetDaylightChanges(asbDate.Year).Delta);
            }

            return asbDate;
        }

        public static string GetAssemblyVersion(this Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            var version = string.Empty;

            var asbName = assembly.GetName();

            version = string.Format("{0}", asbName.Version.ToString());

            return version;
        }
    }
}
