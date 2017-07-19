using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.XPath;

namespace DevScope.Framework.Common.Utils
{
    public static partial class BinarySerializationHelper
   {

       public static byte[] BinSerialize<T>(T obj)
       {
           if (obj == null)
               throw new ArgumentNullException("obj");

           using (MemoryStream stream = new MemoryStream())
           {
               BinaryFormatter formater = new BinaryFormatter();

               formater.Serialize(stream, obj);

               return stream.ToArray();
           }
       }

       public static string BinSerializeToString<T>(T obj)
       {
           if (obj == null)
               throw new ArgumentNullException("obj");

           using (MemoryStream stream = new MemoryStream())
           {
               BinaryFormatter formater = new BinaryFormatter();

               formater.Serialize(stream, obj);

               return Convert.ToBase64String(stream.ToArray());
           }
       }

       public static T BinDeserializeFromString<T>(string objStr)
       {
           if (string.IsNullOrEmpty(objStr))
               throw new ArgumentNullException("objStr");

           object obj = null;

           using (MemoryStream sr = new MemoryStream(Convert.FromBase64String(objStr)))
           {
               BinaryFormatter formater = new BinaryFormatter();

               obj = formater.Deserialize(sr);
           }

           return (T)obj;
       }

       public static T BinDeserialize<T>(byte[] byteArray)
       {
           if (byteArray == null)
               throw new ArgumentNullException("byteArray");

           object obj = null;

           using (MemoryStream sr = new MemoryStream(byteArray))
           {
               BinaryFormatter formater = new BinaryFormatter();

               obj = formater.Deserialize(sr);
           }

           return (T)obj;
       }

    }
}
