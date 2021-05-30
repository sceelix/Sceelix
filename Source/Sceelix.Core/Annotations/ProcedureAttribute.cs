using System;
using System.Linq;
using Sceelix.Loading;

namespace Sceelix.Core.Annotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ProcedureAttribute : Attribute
    {
        /// <summary>
        /// Creates an attribute with default guid.
        /// </summary>
        /// <param name="guid"></param>
        public ProcedureAttribute(string guid)
        {
            Guid = guid;
        }



        internal ProcedureAttribute(ProcedureAttribute procedureAttribute)
        {
            Tags = procedureAttribute.Tags;
            Label = procedureAttribute.Label;
            Description = procedureAttribute.Description;
            Author = procedureAttribute.Author;
            HexColor = procedureAttribute.HexColor;
            Guid = procedureAttribute.Guid;
        }



        /// <summary>
        /// Author/Creator of the procedure.
        /// </summary>
        public string Author
        {
            get;
            set;
        } = "Sceelix";


        /// <summary>
        /// Category of the procedure.
        /// </summary>
        public string Category
        {
            get;
            set;
        } = "Other";


        /// <summary>
        /// Description of the procedure. 
        /// If set, it will overwrite defined code comments.
        /// </summary>
        public string Description
        {
            get;
            internal set;
        } = string.Empty;


        /// <summary>
        /// Unique identifier of the procedure.
        /// Should be a textual representation of a Guid instance.
        /// </summary>
        public string Guid
        {
            get;
            set;
        } = "";


        /// <summary>
        /// Color code associated to the procedure in hexadecimal (without the #). 
        /// Check http://www.2createawebsite.com/build/hex-colors.html for color codes.
        /// </summary>
        public string HexColor
        {
            get;
            set;
        } = "ffffff";


        /// <summary>
        /// Indicates if this procedures is executable or not.
        /// Dummy procedures are excluded from graph executions.
        /// </summary>
        public bool IsDummy
        {
            get;
            set;
        } = false;


        /// <summary>
        /// Default label of the node.
        /// If not defined, the class name of the procedure will be used.
        /// </summary>
        public string Label
        {
            get;
            set;
        } = string.Empty;


        /// <summary>
        /// Obsolete tag defined at the procedure.
        /// </summary>
        public ObsoleteAttribute ObsoleteAttribute
        {
            get;
            internal set;
        }


        /// <summary>
        /// Remarks of the procedure. 
        /// If set, it will overwrite defined code remarks.
        /// </summary>
        public string Remarks
        {
            get;
            internal set;
        } = string.Empty;


        /// <summary>
        /// Tags associated to this procedure
        /// (for indexing and searching)
        /// </summary>
        public string Tags
        {
            get;
            set;
        } = "";



        public static ProcedureAttribute GetAttributeForProcedure(Type type)
        {
            ProcedureAttribute attribute = type.GetCustomAttributes(true).OfType<ProcedureAttribute>().FirstOrDefault(); // ?? new ProcedureAttribute(type.Name)
            if (attribute != null)
            {
                if (string.IsNullOrEmpty(attribute.Label))
                    attribute.Label = type.Name;

                attribute.ObsoleteAttribute = type.GetCustomAttributes(true).OfType<ObsoleteAttribute>().FirstOrDefault();

                //if the description is not set, load from the comments, otherwise keep the default
                var comment = CommentLoader.GetComment(type);

                if (string.IsNullOrWhiteSpace(attribute.Description))
                    attribute.Description = comment.Summary;

                if (string.IsNullOrWhiteSpace(attribute.Remarks))
                    attribute.Remarks = comment.Remarks;
            }

            return attribute;
        }



        /*public static IEnumerable<String> GetTags(Type procedureType)
        {
            foreach (var fieldInfo in procedureType.GetFields().Where(x => typeof(Parameter).IsAssignableFrom(x.FieldType)))
            {
                foreach (string tag in GetTags(fieldInfo.FieldType))
                    yield return tag;
                
                var attribute = fieldInfo.FieldType.GetCustomAttribute<ParameterTagAttribute>();
                if (attribute != null)
                {
                    foreach (var tag in attribute.Tags)
                        yield return tag;
                }
            }
        }*/



        public static bool HasProcedureAttribute(Type type)
        {
            return type.GetCustomAttributes(true).Any(val => val is ProcedureAttribute);
        }
    }
}