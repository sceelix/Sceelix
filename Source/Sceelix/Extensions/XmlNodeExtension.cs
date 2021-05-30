using System.Xml;
using Sceelix.Conversion;

namespace Sceelix.Extensions
{
    public static class XmlNodeExtension
    {
        /// <summary>
        /// Gets the string value stored in a xmlnode attribute and converts it to the indicated type.
        /// </summary>
        /// <typeparam name="T">Type of the value, to which </typeparam>
        /// <param name="node">Xmlnode containing attributes.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <returns>The corresponding value or a default value (default(T)) if the attribute does not exist.</returns>
        public static T GetAttributeOrDefault<T>(this XmlNode node, string attributeName)
        {
            if (node.Attributes != null && node.Attributes[attributeName] != null) return (T) ConvertHelper.Convert(node.Attributes[attributeName].InnerText, typeof(T));

            return default(T);
        }



        /// <summary>
        /// Gets the string value stored in a xmlnode attribute and converts it to the indicated type.
        /// </summary>
        /// <typeparam name="T">Type of the value, to which </typeparam>
        /// <param name="node">Xmlnode containing attributes.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="defaultValue">Value to return in case the attribute does not exist.</param>
        /// <returns>The corresponding value the indicated default value if the attribute does not exist.</returns>
        public static T GetAttributeOrDefault<T>(this XmlNode node, string attributeName, T defaultValue)
        {
            if (node.Attributes != null && node.Attributes[attributeName] != null) return (T) ConvertHelper.Convert(node.Attributes[attributeName].InnerText, typeof(T));

            return defaultValue;
        }



        /// <summary>
        /// Gets the string value stored in a xmlnode attribute and converts it to the indicated type.
        /// </summary>
        /// <typeparam name="T">Type of the value, to which </typeparam>
        /// <param name="node">Xmlnode containing attributes.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="defaultValue">Value to return in case the attribute does not exist.</param>
        /// <returns>The corresponding value the indicated default value if the attribute does not exist.</returns>
        public static T? GetAttributeOrNullable<T>(this XmlNode node, string attributeName) where T : struct
        {
            if (node.Attributes != null && node.Attributes[attributeName] != null) return (T) ConvertHelper.Convert(node.Attributes[attributeName].InnerText, typeof(T));

            return null;
        }
    }
}