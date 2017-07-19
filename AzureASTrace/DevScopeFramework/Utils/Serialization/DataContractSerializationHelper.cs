using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace DevScope.Framework.Common.Utils
{
    public static partial class DataContractSerializationHelper
    {
        public static string Serialize<T>(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            var serializer = new DataContractSerializer(typeof(T));

            using (var backing = new System.IO.StringWriter())
            {
                using (var writer = new XmlTextWriter(backing))
                {
                    serializer.WriteObject(writer, obj);

                    return backing.ToString();
                }
            }
        }

        public static T Deserialize<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                throw new ArgumentNullException("xml");

            var serializer = new DataContractSerializer(typeof(T));

            using (var backing = new System.IO.StringReader(xml))
            {
                using (var reader = new System.Xml.XmlTextReader(backing))
                {
                    return (T)serializer.ReadObject(reader);
                }
            }
        }
    }
}
