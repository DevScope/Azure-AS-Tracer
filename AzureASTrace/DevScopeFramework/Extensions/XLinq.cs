using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Collections;

namespace DevScope.Framework.Common.Extensions
{
    public static class XLinqExtensions
    {
        public static string GetElementValue(this XContainer container, params XName[] nodes)
        {
            var elementValue = container.TryGetElementValue(nodes);

            if (string.IsNullOrEmpty(elementValue))
            {
                throw new ApplicationException(string.Format("Xml Element value: '{0}' cannot be null.", string.Join("/", nodes.Select(n => n.ToString()).ToArray())));
            }

            return elementValue;            
        }

        public static string TryGetElementValue(this XContainer container, params XName[] nodes)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (nodes == null)
                throw new ArgumentNullException("nodes");

            XElement element = container.TryGetElement(nodes);

            if (element == null)
            {
                return null;
            }

            return element.Value;
        }


        public static string GetAttributeValue(this XContainer container, string attributeName, params XName[] nodes)
        {
            var attributeValue = TryGetAttributeValue(container, attributeName, nodes);

            if (string.IsNullOrEmpty(attributeValue))
                throw new ApplicationException(string.Format("Xml Attribute: '{0}' on Element: '{1}' cannot be null.", attributeName, string.Join("/", nodes.Select(n => n.ToString()).ToArray())));

            return attributeValue;
        }

        public static string TryGetAttributeValue(this XContainer container, string attributeName, params XName[] nodes)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (string.IsNullOrEmpty(attributeName))
                throw new ArgumentNullException("attributeName");
            if (nodes == null)
                throw new ArgumentNullException("nodes");

            XElement element = container.TryGetElement(nodes);

            if (element == null)
            {
                return null;
            }

            XAttribute attribute = element.Attribute(attributeName);

            if (attribute == null)
            {      
                return null;
            }

            return attribute.Value;
        }

        public static XElement GetElement(this XContainer container, params XName[] nodes)
        {
            var element = container.TryGetElement(nodes);

            if (element == null)
            {
                throw new ApplicationException(string.Format("Xml Element: '{0}' cannot be null.", string.Join("/", nodes.Select(n => n.ToString()).ToArray())));
            }

            return element;     
        }

        public static XElement TryGetElement(this XContainer container, params XName[] nodes)
        {
            if (nodes == null)
                throw new ArgumentNullException("nodes");
        
            if (container == null)
                return null;            

            if (nodes.Length == 0)
                return (XElement)container;
            
            var node = nodes.First();

            var nodeElement = container.Element(node);
            
            var newXPath = nodes.Skip(1).ToArray();

            return nodeElement.TryGetElement(newXPath);
        }

        private static XName[] RemoveFirst(XName[] nodes)
        {
            var newArray = new XName[nodes.Length - 1];

            Array.Copy(nodes, 1, newArray, 0, nodes.Length - 1);

            return newArray;
        }
    }
}
