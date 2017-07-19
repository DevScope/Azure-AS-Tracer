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
    public static partial class XmlSerializationHelper
    {
        public static Encoding DefaultEncoding = Encoding.UTF8;

        public static string Serialize<T>(T obj)
        {
            using (MemoryStream stream = Serialize<T>(obj, null, null))
            {
                return DefaultEncoding.GetString(stream.ToArray());
            }
        }

        public static string Serialize<T>(T obj, XmlSerializerNamespaces ns)
        {
            using (MemoryStream stream = Serialize<T>(obj, null, ns))
            {
                return DefaultEncoding.GetString(stream.ToArray());
            }
        }

        public static string Serialize<T>(T obj, Encoding enc)
        {
            using (MemoryStream stream = Serialize<T>(obj, enc, null))
            {
                return enc.GetString(stream.ToArray());
            }
        }

        public static MemoryStream Serialize<T>(T obj, Encoding enc, XmlSerializerNamespaces ns)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            MemoryStream stream = new MemoryStream();

            //Se o enc for null é serializado em utf-8(default) mas não é colocado nada na declaração a indicar o encoding
            using (XmlWriter writer = new XmlTextWriter(stream, enc))
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));

                if (ns == null)
                    xs.Serialize(writer, obj);
                else
                    xs.Serialize(writer, obj, ns);
            }

            return stream;
        }

        public static void SerializeToFile<T>(T obj, string filePath, FileMode fileMode)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException("filePath");

            using (FileStream fs = new FileStream(filePath, fileMode))
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                xs.Serialize(fs, obj);
            }
        }

        //-------------------------------------------------------------------
        //-------------------------------------------------------------------
        //-------------------------------------------------------------------

        public static T Deserialize<T>(string xml)
        {
            return Deserialize<T>(xml, DefaultEncoding);
        }

        public static T Deserialize<T>(string xml, Encoding enc)
        {
            if (string.IsNullOrEmpty(xml))
                throw new ArgumentNullException("xml");
            if (enc == null)
                throw new ArgumentNullException("enc");

            using (MemoryStream ms = new MemoryStream(enc.GetBytes(xml)))
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                return (T)xs.Deserialize(ms);
            }
        }

        public static T DeserializeFromFile<T>(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException("filePath");

            object obj = null;

            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                obj = xs.Deserialize(fs);
            }

            return (T)obj;
        }
  
        /// <summary>
        /// Serializa um objecto num XmlDocument
        /// </summary>
        /// <param name="objToSerialize">objToSerialize</param>
        /// <param name="xmlRootAttribute">xmlRootAttribute</param>
        /// <returns></returns>
        public static XmlDocument GetXmlDocument(object objToSerialize, string xmlRootAttribute)
        {
            XmlDocument xmlMetadata = new XmlDocument();

            XPathNavigator nav = xmlMetadata.CreateNavigator();
            
            using (XmlWriter writer = nav.AppendChild())
            {
                XmlSerializer ser = new XmlSerializer(objToSerialize.GetType(), new XmlRootAttribute(xmlRootAttribute));
                ser.Serialize(writer, objToSerialize);
            }

            return xmlMetadata;
        }
    }
}
