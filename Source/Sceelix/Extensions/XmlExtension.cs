using System.Collections.Generic;
using System.Xml;

namespace Sceelix.Extensions
{
    public static class XmlExtension
    {
        public static IEnumerable<XmlElement> GetChildren(this XmlNode node)
        {
            foreach (XmlElement xmlElement in node.ChildNodes)
                yield return xmlElement;
        }
    }
}