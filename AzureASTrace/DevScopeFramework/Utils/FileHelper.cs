using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DevScope.Framework.Common.Utils
{
    public static class FileHelper
    {
        public static byte[] ReadAllBytesWithoutLock(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");

            byte[] bytes;

            using (var fs = System.IO.File.Open(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
            {
                var totalBytes = Convert.ToInt32(fs.Length);
                bytes = new byte[totalBytes];
                fs.Read(bytes, 0, totalBytes);
            }

            return bytes;
        }

        public static void EnsureFolder(string folder)
        {
            if (Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

        }

        public static void WriteAllTextAndEnsureFolder(string path, string contents)
        {
            try
            {
                File.WriteAllText(path, contents);
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                WriteAllTextAndEnsureFolder(path, contents);
            }
        }
    }
}
