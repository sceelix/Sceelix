using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Sceelix.Loading
{
    /// <summary>
    /// Loads comment text from types, methods and fields based on the XML documentation that accompanies the Assembly .dll files.
    /// </summary>
    public class CommentLoader
    {
        /// <summary>
        /// Cache of xml files, per assembly.
        /// </summary>
        private static readonly Dictionary<Assembly, Dictionary<string, ItemComment>> AssemblyDocuments = new Dictionary<Assembly, Dictionary<string, ItemComment>>();


        /// <summary>
        /// Cache of comments, per object (type, methodinfo or fieldinfo).
        /// </summary>
        private static readonly Dictionary<object, ItemComment> CommentCache = new Dictionary<object, ItemComment>();



        /// <summary>
        /// Find the actual name of a type, replacing "+" (for nested classes) with ".".
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static string ActualFullName(Type type)
        {
            //The "+" occurs in items inside nested classes, so let's replace it
            return type.FullName.Replace("+", ".");
        }



        /// <summary>
        /// Gets the comment for the given method.
        /// </summary>
        /// <param name="methodInfo">The MethodInfo that reflects the desired method.</param>
        /// <returns>Comment text written for the given method.</returns>
        public static ItemComment GetComment(MethodInfo methodInfo)
        {
            ItemComment comment;

            //fastest: try to get comment from an object cache
            if (CommentCache.TryGetValue(methodInfo, out comment))
                return comment;

            //if it wasn't possible, determine the member identifier for a search inside the documentation file
            Type declaringType = methodInfo.DeclaringType;
            if (declaringType != null)
            {
                string memberName = "M:" + ActualFullName(declaringType) + "." + methodInfo.Name;

                string args = string.Join(",", methodInfo.GetParameters().Select(x => x.ParameterType.FullName));
                if (!string.IsNullOrWhiteSpace(args))
                    memberName += "(" + args + ")";

                return GetComment(methodInfo, memberName, declaringType.Assembly);
            }

            return new ItemComment();
        }



        /// <summary>
        /// Gets the comment for the given field.
        /// </summary>
        /// <param name="fieldInfo">The FieldInfo that reflects the desired field.</param>
        /// <returns>Comment text written for the given field.</returns>
        public static ItemComment GetComment(FieldInfo fieldInfo)
        {
            ItemComment comment;

            //fastest: try to get comment from an object cache
            if (CommentCache.TryGetValue(fieldInfo, out comment))
                return comment;

            //if it wasn't possible, determine the member identifier for a search inside the documentation file
            Type declaringType = fieldInfo.DeclaringType;
            if (declaringType != null) return GetComment(fieldInfo, "F:" + ActualFullName(declaringType) + "." + fieldInfo.Name, declaringType.Assembly);

            return new ItemComment();
        }



        /// <summary>
        /// Gets the comment for the given property.
        /// </summary>
        /// <param name="propertyInfo">The FieldInfo that reflects the desired field.</param>
        /// <returns>Comment text written for the given field.</returns>
        public static ItemComment GetComment(PropertyInfo propertyInfo)
        {
            ItemComment comment;

            //fastest: try to get comment from an object cache
            if (CommentCache.TryGetValue(propertyInfo, out comment))
                return comment;

            //if it wasn't possible, determine the member identifier for a search inside the documentation file
            Type declaringType = propertyInfo.DeclaringType;
            if (declaringType != null) return GetComment(propertyInfo, "P:" + ActualFullName(declaringType) + "." + propertyInfo.Name, declaringType.Assembly);

            return new ItemComment();
        }



        /// <summary>
        /// Gets the comment for the given type.
        /// </summary>
        /// <param name="type">The Type that reflects the desired type.</param>
        /// <returns>Comment text written for the given type.</returns>
        public static ItemComment GetComment(Type type)
        {
            ItemComment comment;

            //fastest: try to get comment from an object cache
            if (CommentCache.TryGetValue(type, out comment))
                return comment;

            return GetComment(type, "T:" + ActualFullName(type), type.Assembly);
        }



        /// <summary>
        /// Gets the comment.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        private static ItemComment GetComment(object data, string memberName, Assembly assembly)
        {
            Dictionary<string, ItemComment> content = GetDocumentContent(assembly);

            //otherwise, try to fetch the comment
            ItemComment comment;
            if (content != null && content.TryGetValue(memberName, out comment))
            {
                //keep it in the cache, to ensure faster access in the future
                CommentCache.Add(data, comment);

                return comment;
            }

            //return an empty comment
            return new ItemComment();
        }



        /// <summary>
        /// Gets the content of the document.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        private static Dictionary<string, ItemComment> GetDocumentContent(Assembly assembly)
        {
            Dictionary<string, ItemComment> documentComments = null;
            if (!AssemblyDocuments.TryGetValue(assembly, out documentComments))
            {
                var xmlDocumentFile = Path.ChangeExtension(assembly.Location, "xml");

                if (File.Exists(xmlDocumentFile))
                {
                    var document = new XmlDocument();
                    document.Load(xmlDocumentFile);

                    documentComments = new Dictionary<string, ItemComment>();
                    foreach (XmlElement xmlElement in document["doc"]["members"].GetElementsByTagName("member"))
                    {
                        var newItemComment = new ItemComment(xmlElement);
                        documentComments.Add(newItemComment.Name, newItemComment);
                    }

                    AssemblyDocuments.Add(assembly, documentComments);
                }
                else
                {
                    AssemblyDocuments.Add(assembly, null);
                }
            }

            return documentComments;
        }



        public class ItemComment
        {
            public ItemComment()
            {
            }



            public ItemComment(XmlElement xmlElement)
            {
                Name = xmlElement.Attributes["name"].InnerXml;

                if (xmlElement["summary"] != null)
                    Summary = CleanString(xmlElement["summary"].InnerXml);

                if (xmlElement["remarks"] != null)
                    Remarks = CleanString(xmlElement["remarks"].InnerXml);
            }



            internal string Name
            {
                get;
            }


            public string Remarks
            {
                get;
            } = string.Empty;


            public string Summary
            {
                get;
            } = string.Empty;



            /// <summary>
            /// Cleans the string.
            /// </summary>
            /// <param name="innerText">The inner text.</param>
            /// <returns></returns>
            private static string CleanString(string innerText)
            {
                //clean the string, removing the 12 white spaces (and no more!) at the beginning of every line
                if (!string.IsNullOrWhiteSpace(innerText))
                {
                    var splittedLines = innerText.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
                    string totalString = splittedLines[0];

                    for (int i = 1; i < splittedLines.Length; i++)
                        totalString += splittedLines[i].Substring(12) + Environment.NewLine;

                    totalString = totalString.Trim('\n', '\r', ' ');

                    return totalString;
                }

                return innerText;
            }
        }
    }
}