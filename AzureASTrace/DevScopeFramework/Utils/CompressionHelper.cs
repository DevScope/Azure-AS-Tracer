using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace DevScope.Framework.Common.Utils
{
    public static class CompressionHelper
    {
        public static byte[] Compress(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");

            var bytes = Encoding.Unicode.GetBytes(text);

            using (var msi = new MemoryStream(bytes))
            {
                using (var mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(mso, CompressionMode.Compress))
                    {
                        msi.CopyTo(gs);
                    }

                    //return Convert.ToBase64String(mso.ToArray());           
                    return mso.ToArray();
                }
            }
        }

        public static string Decompress(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");            

            using (var msi = new MemoryStream(bytes))
            {
                using (var mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                    {
                        gs.CopyTo(mso);
                    }

                    return Encoding.Unicode.GetString(mso.ToArray());
                }
            }
        }
    }
}
