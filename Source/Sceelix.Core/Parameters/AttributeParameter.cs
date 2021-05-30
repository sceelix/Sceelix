using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sceelix.Conversion;
using Sceelix.Core.Attributes;
using Sceelix.Core.Data;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;
using Sceelix.Logging;

namespace Sceelix.Core.Parameters
{
    public enum AttributeAccess
    {
        Read,
        Write,
        ReadWrite
    }

    public class AttributeParameter : Parameter
    {
        /// <summary>
        /// The attribute name and details.
        /// This is simply a placeholder to avoid doing the value processing all the time.
        /// </summary>
        //private string[] _attributeData;
        private AttributeKey _attributeKey;

        //private bool _isLocal = false;
        //private bool _replace;



        public AttributeParameter(string label)
            : this(label, string.Empty, AttributeAccess.ReadWrite)
        {
        }



        public AttributeParameter(string label, string defaultAttributeValue)
            : this(label, defaultAttributeValue, AttributeAccess.ReadWrite)
        {
        }



        public AttributeParameter(string label, AttributeAccess access)
            : this(label, string.Empty, AttributeAccess.ReadWrite)
        {
            Access = access;
        }



        public AttributeParameter(string label, string defaultAttributeValue, AttributeAccess access)
            : base(label)
        {
            Access = access;
            AttributeString = defaultAttributeValue;
            //_attributeKey = ParseAttributeString(_attributeString);
        }



        internal AttributeParameter(AttributeParameterInfo attributeParameterInfo)
            : base(attributeParameterInfo)
        {
            Access = attributeParameterInfo.Access;

            AttributeString = attributeParameterInfo.AttributeString;
            //_attributeKey = ParseAttributeString(_attributeString);
        }



        public AttributeAccess Access
        {
            get;
            protected set;
        } = AttributeAccess.ReadWrite;



        public AttributeKey AttributeKey
        {
            get
            {
                if (_attributeKey == null && !string.IsNullOrEmpty(AttributeString))
                    _attributeKey = ParseAttributeString(AttributeString);

                return _attributeKey;
            }
        }



        public string AttributeName => AttributeKey != null ? AttributeKey.Name : null;


        public Procedure AttributeOwner => Procedure.Parent;


        public string AttributeString
        {
            get;
            private set;
        }
        /*private set
            {
                
            }*/



        public override bool IsExpression
        {
            get { return _expression != null; }
            set
            {
                if (value)
                    _expression = delegate { return ""; };
                else
                    _expression = null;
            }
        }



        /// <summary>
        /// Indicates if an attribute name was indicated, i.e. if there is actually the intent to store any data to this attribute.
        /// </summary>
        public bool IsMapped => AttributeKey != null; //!String.IsNullOrWhiteSpace(Value); }



        /// <summary>
        /// Returns the value in the entity for this attribute.
        /// </summary>
        /// <param name="obj">Entity which should contain the attribute.</param>
        /// <returns>The value for the attribute, or null, if the attribute is not defined in this entity.</returns>
        public object this[IEntity obj]
        {
            get
            {
                if (!IsMapped)
                    return null;

                return obj.Attributes.TryGet(AttributeKey);
            }
            set
            {
                if (IsMapped)
                {
                    if (Access == AttributeAccess.Read)
                        throw new InvalidOperationException("A read-only attribute cannot be written to.");

                    bool replace = Access == AttributeAccess.ReadWrite || AttributeKey.HasMeta<ReplaceMeta>();
                    //var actualKey = AttributeKey;

                    // ProcessMeta(actualKey, replace);

                    //if (Procedure.Parent != null)
                    //    Procedure.Parent.HasLocalAttributes |= _isLocal;

                    /**/

                    obj.Attributes.TrySet(AttributeKey, value, replace);

                    if (AttributeKey is LocalAttributeKey)
                        ((LocalAttributeKey) AttributeKey).Procedure.HasLocalAttributes = true;

                    //since entities can have many subitems, clearing local variables could result in a huge
                    //overhead. What we do here is store references to the entities for deletion on procedure
                    //finish. An exchange of memory for performance. It doesn't really matter if the entities
                    //are destroyed in the execution of the procedure or not.
                    //if (parent != null)
                    //    parent.LocalEntities.Add(obj.Attributes);
                }
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="attributeValue"></param>
        /// <returns></returns>
        public static string GetAttributeName(string attributeValue)
        {
            var attributeData = attributeValue.Split('/');
            var attributeName = attributeData[0].Replace("@", "").Trim();

            if (IsValidAttributeName(attributeName))
                return attributeName;

            return null;
        }



        protected internal override object GetData()
        {
            return AttributeKey;
        }



        internal static AttributeKey GetKey(string attributeName, Procedure parentProcedure)
        {
            if (attributeName.StartsWithLowerCase())
                return new LocalAttributeKey(attributeName, parentProcedure);

            return new GlobalAttributeKey(attributeName);
        }



        public bool HasAttribute(IEntity entity)
        {
            if (!IsMapped)
                return false;

            return entity.Attributes.ContainsKey(AttributeKey);
        }



        private static bool IsValidAttributeName(string attributeName)
        {
            return Regex.IsMatch(attributeName, "^[a-zA-Z_][a-zA-Z0-9_]*$"); //^(local:)?[a-z
        }



        private AttributeKey ParseAttributeString(string attributeValue)
        {
            if (!string.IsNullOrWhiteSpace(attributeValue))
            {
                //gets name of the attribute
                var attributeData = attributeValue.Split('/').Where(x => !string.IsNullOrEmpty(x)).ToArray();
                for (int i = 0; i < attributeData.Length; i++)
                    attributeData[i] = attributeData[i].Trim();

                //_attributeData.Skip(1).ToArray();
                attributeData[0] = attributeData[0].Replace("@", "");
                if (!IsValidAttributeName(attributeData[0]))
                    throw new Exception("Attribute name '" + attributeData[0] + "' is not valid.");

                attributeData[0] = attributeData[0].Replace("@", "");

                var attributeKey = GetKey(attributeData[0], Procedure.Parent ?? Procedure);

                List<object> metaList = new List<object>();
                for (int i = 1; i < attributeData.Length; i++)
                {
                    string metaString = attributeData[i].ToLower();

                    var metaStringParts = metaString.Split(' ');
                    var metaStringCommand = metaStringParts[0];

                    var meta = MetaParserManager.Parse(metaStringCommand, metaStringParts);
                    if (meta != null)
                        metaList.Add(meta);
                    else
                        ProcedureEnvironment.GetService<ILogger>().Log("There is no supported meta '" + metaString + "'.", LogType.Warning);
                }

                if (metaList.Count > 0)
                    attributeKey.Meta = metaList;

                return attributeKey;
            }

            return null;
        }



        protected internal override void Set(ParameterInfo argument, Procedure masterProcedure, Procedure currentProcedure)
        {
            if (argument.IsExpression)
            {
                SetExpression(argument.ParsedExpression.GetCompiledExpressionTree(masterProcedure, currentProcedure, typeof(AttributeKey)));
            }
            else
            {
                var attributeParameter = (AttributeParameterInfo) argument;

                Set(attributeParameter.AttributeString);
            }
        }



        protected override void SetData(object value)
        {
            if (value is string)
            {
                AttributeString = (string) value;
                _attributeKey = null;
            }
            else if (value is AttributeKey)
            {
                //we don't have a superclass for all types of keys!
                _attributeKey = (AttributeKey) value;
                AttributeString = "You shouldn't be looking at me!";
            }
        }



        protected internal override ParameterInfo ToParameterInfo()
        {
            return new AttributeParameterInfo(this);
        }
    }


    public class AttributeParameter<T> : AttributeParameter
    {
        public AttributeParameter(string label, string defaultAttributeValue)
            : base(label, defaultAttributeValue)
        {
        }



        public AttributeParameter(string label, AttributeAccess access)
            : base(label, string.Empty)
        {
            Access = access;
        }



        public AttributeParameter(string label, string defaultAttributeValue, AttributeAccess access)
            : base(label, defaultAttributeValue)
        {
            Access = access;
        }



        /// <summary>
        /// Gets or sets the value the attribute for the given entity.
        /// </summary>
        /// <param name="obj">Entity whose attribute is to be set/get.</param>
        /// <returns></returns>
        public new T this[IEntity obj]
        {
            get
            {
                var value = base[obj];
                if (value == null)
                    return default(T);

                return ConvertHelper.Convert<T>(value);
            }
            set { base[obj] = value; }
        }
    }
}