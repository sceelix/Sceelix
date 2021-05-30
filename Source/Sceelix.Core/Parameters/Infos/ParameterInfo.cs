using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Sceelix.Conversion;
using Sceelix.Core.Annotations;
using Sceelix.Core.Code;
using Sceelix.Core.Environments;
using Sceelix.Core.Graphs;
using Sceelix.Core.Graphs.Expressions;
using Sceelix.Extensions;

namespace Sceelix.Core.Parameters.Infos
{
    public abstract class ParameterInfo : ICloneable
    {
        private Guid? _guid;

        protected List<InputPort> _inputPorts = new List<InputPort>();
        protected List<OutputPort> _outputPorts = new List<OutputPort>();



        protected ParameterInfo(string label)
        {
            Label = label;
            Description = string.Empty;
            IsPublic = true;
        }



        protected ParameterInfo(string label, string description, bool isPublic)
        {
            Label = label;
            Description = description;
            IsPublic = isPublic;
        }



        protected ParameterInfo(Parameter parameter)
        {
            Label = parameter.Label;
            Description = parameter.Description;
            IsPublic = parameter.IsPublic;
            EntityEvaluation = parameter.EntityEvaluation;
            Section = parameter.Section;

            if (parameter.IsExpression)
            {
                IsExpression = true;
                ParsedExpression = ParsedExpression.Empty;
            }

            _inputPorts = parameter.SubInputs.Select(p => p.ToInputPort(true)).ToList();
            _outputPorts = parameter.SubOutputs.Select(p => p.ToOutputPort(true)).ToList();

            var parameterAttribute = parameter.GetType().GetCustomAttribute<ParameterAttribute>();
            if (parameterAttribute != null && !string.IsNullOrWhiteSpace(parameterAttribute.Guid))
                _guid = ConvertHelper.Convert<Guid>(parameterAttribute.Guid);
        }



        /// <summary>
        /// Creates a new parameterinfo from an XML node
        /// </summary>
        /// <param name="xmlNode"></param>
        protected ParameterInfo(XmlNode xmlNode)
        {
            Label = xmlNode.Attributes["Label"].InnerText;
            IsPublic = xmlNode.GetAttributeOrDefault("IsPublic", true);
            Description = xmlNode.GetAttributeOrDefault("Description", string.Empty);
            EntityEvaluation = xmlNode.GetAttributeOrDefault("EntityEvaluation", false);
            Section = xmlNode.GetAttributeOrDefault<string>("Section");

            //if the IsExpression is not defined, we assume that is false
            if (xmlNode.GetAttributeOrDefault("IsExpression", false))
            {
                ParsedExpression = new ParsedExpression(xmlNode["ParsedExpression"]);
                IsExpression = true;
            }
        }



        /// <summary>
        /// Indicates if this parameterInfo can include recursive infos. Usually applies to lists only.
        /// </summary>
        public virtual bool CanHaveRecursiveItem => false;


        /// <summary>
        /// Indicates whether ParameterInfos associated to this editor can have sub items (like lists and compounds do).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can have sub items; otherwise, <c>false</c>.
        /// </value>
        public virtual bool CanHaveSubItems => false;


        public string Description
        {
            get;
            set;
        }


        public bool EntityEvaluation
        {
            get;
            set;
        }


        public string FullName => Root.GetFullLabel(this);



        public Guid? Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }



        /// <summary>
        /// Identifier representation of the Label (no spaces)
        /// </summary>
        public string Identifier => Parameter.GetIdentifier(Label);



        /// <summary>
        /// Gets the input ports directly defined in this parameter.
        /// </summary>
        public virtual IEnumerable<InputPort> InputPorts
        {
            get
            {
                foreach (var inputPort in _inputPorts)
                    yield return inputPort;
            }
        }



        public bool IsExpression
        {
            get;
            set;
        }


        public bool IsPublic
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the SubParameters that serve as models. Used for documentation generation purposes.
        /// For instance, for listparameters, the models are returned. For compoundparameters, the fields are returned. 
        /// </summary>
        public virtual List<ParameterInfo> ItemModels => new List<ParameterInfo>();


        public string Label
        {
            get;
            set;
        }


        /// <summary>
        /// Gets a human readable name for the parameter (ex. "Int", "File", "List")
        /// </summary>
        public abstract string MetaType
        {
            get;
        }



        /// <summary>
        /// Gets the output ports directly defined in this parameter.
        /// </summary>
        public virtual IEnumerable<OutputPort> OutputPorts
        {
            get
            {
                foreach (var outputPort in _outputPorts)
                    yield return outputPort;
            }
        }



        public virtual List<ParameterInfo> Parameters => new List<ParameterInfo>();


        public ParameterInfo Parent
        {
            get;
            set;
        }


        public ParsedExpression ParsedExpression
        {
            get;
            set;
        }



        /// <summary>
        /// Root parameter that lies on the base of this one.
        /// </summary>
        public ParameterInfo Root
        {
            get
            {
                var parameter = this;

                while (parameter.Parent != null)
                    parameter = parameter.Parent;

                return parameter;
            }
        }



        public string Section
        {
            get;
            set;
        }



        /// <summary>
        /// Gets the input ports defined in this parameter and subparameters.
        /// </summary>
        public virtual IEnumerable<InputPort> SubInputPortTree
        {
            get
            {
                foreach (var inputPort in _inputPorts)
                    yield return inputPort;
            }
        }



        /// <summary>
        /// Gets the output ports defined in this parameter and subparameters.
        /// </summary>
        public virtual IEnumerable<OutputPort> SubOutputPortTree
        {
            get
            {
                foreach (var outputPort in _outputPorts)
                    yield return outputPort;
            }
        }



        public virtual string ValueLiteral => null;



        /// <summary>
        /// Goes down the parameter tree from this parameter, setting all the parameters to fixed values (i.e. removing all the expressions).
        /// </summary>
        public void ClearExpressions()
        {
            foreach (ParameterInfo parameterInfo in GetThisAndSubtree(false))
            {
                parameterInfo.IsExpression = false;
                parameterInfo.ParsedExpression = ParsedExpression.Empty;
            }
        }



        public virtual object Clone()
        {
            ParameterInfo clone = (ParameterInfo) MemberwiseClone();

            clone.Label = Label;
            clone.Description = Description;
            clone.IsPublic = IsPublic;
            clone.IsExpression = IsExpression;
            clone.EntityEvaluation = EntityEvaluation;
            clone.Parent = Parent;
            clone.Section = Section;

            clone._inputPorts = _inputPorts.Select(x => (InputPort) x.Clone()).ToList();
            clone._outputPorts = _outputPorts.Select(x => (OutputPort) x.Clone()).ToList();

            if (ParsedExpression != null)
                clone.ParsedExpression = (ParsedExpression) ParsedExpression.Clone();

            return clone;
        }



        /// <summary>
        /// Clones the parameter. Only to be used by parameters that handle parameter info MODELS (such as the ListParameterInfo).
        /// Only needs to be overriden by parameters that have children (such as the ListParameterInfo or the CompoundParameterInfo).
        /// </summary>
        /// <param name="resolveRecursiveParameter">if set to <c>true</c> [resolve recursive parameter].</param>
        /// <returns></returns>
        public virtual object CloneModel(bool resolveRecursiveParameter)
        {
            return Clone();
        }



        public void CreateCSharpCode(CodeBuilder codeBuilder, string varName, bool skipParameterName = false)
        {
            var parameterReference = skipParameterName ? $"{varName}" : $"{varName}.Parameters[{Label.Quote()}]";

            if (IsExpression)
                codeBuilder.AppendLine($"{parameterReference}.SetExpression({ParsedExpression.OriginalExpression.Quote()});");
            else
                CreateSpecificCSharpCode(codeBuilder, parameterReference);
        }



        protected virtual void CreateSpecificCSharpCode(CodeBuilder codeBuilder, string varName)
        {
            if (!CanHaveSubItems)
                codeBuilder.AppendLine($"{varName}.Set({ValueLiteral});");
        }



        protected internal virtual string GetFullLabel(ParameterInfo parameterInfo)
        {
            if (parameterInfo == this)
                return Label;

            return null;
        }



        public virtual IEnumerable<string> GetReferencedPaths()
        {
            yield break;
        }



        /// <summary>
        /// Gets an flat enumeration of all items and subitems of this parameter.
        /// </summary>
        /// <param name="stopAtExpressionParameters">If true, it will not return subitems if the parameter is an expression.</param>
        /// <returns></returns>
        public virtual IEnumerable<ParameterInfo> GetSubtree(bool stopAtExpressionParameters)
        {
            yield break;
        }



        /// <summary>
        /// Gets an flat enumeration of all items and subitems of this parameter, starting with itself.
        /// </summary>
        /// <param name="stopAtExpressionParameters">If true, it will not return subitems if the parameter is an expression.</param>
        /// <returns></returns>
        public IEnumerable<ParameterInfo> GetThisAndSubtree(bool stopAtExpressionParameters)
        {
            yield return this;

            foreach (var parameterInfo in GetSubtree(stopAtExpressionParameters))
                yield return parameterInfo;
        }



        public virtual bool HasReferenceToParameter(string label)
        {
            if (IsExpression && ParsedExpression != null)
                return ParsedExpression.HasReferenceToParameter(label);

            return false;
        }



        /// <summary>
        /// Reads the values/expressions set as arguments of a parameter
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <param name="procedureEnvironment"></param>
        public virtual void ReadArgumentXML(XmlNode xmlNode, IProcedureEnvironment procedureEnvironment)
        {
            //if the IsExpression is not defined, we assume that is false
            if (xmlNode.GetAttributeOrDefault("IsExpression", false))
            {
                ParsedExpression = new ParsedExpression(xmlNode["ParsedExpression"]);
                IsExpression = true;
            }
        }



        public virtual void RefactorGlobalParameters(string oldLabel, string newLabel)
        {
            if (ParsedExpression != null)
                ParsedExpression.RefactorGlobalParameters(oldLabel, newLabel);
        }



        /// <summary>
        /// Refactors references to the indicated file.
        /// </summary>
        /// <param name="procedureEnvironment">The procedure environment (used for loading resources).</param>
        /// <param name="originalPath">The original file path.</param>
        /// <param name="replacementPath">The new file path.</param>
        /// <returns>True if the reference was found, false otherwise.</returns>
        internal virtual bool RefactorReferencedFile(IProcedureEnvironment procedureEnvironment, string originalPath, string replacementPath)
        {
            var foundReference = false;

            foreach (var parameterInfo in Parameters)
                foundReference |= parameterInfo.RefactorReferencedFile(procedureEnvironment, originalPath, replacementPath);

            return foundReference;
        }



        /// <summary>
        /// Refactors references to the indicated folder.
        /// </summary>
        /// <param name="procedureEnvironment">The procedure environment (used for loading resources).</param>
        /// <param name="originalPath">The original folder path.</param>
        /// <param name="replacementPath">The new folder path.</param>
        /// <returns>True if the reference was found, false otherwise.</returns>
        internal virtual bool RefactorReferencedFolder(IProcedureEnvironment procedureEnvironment, string originalPath, string replacementPath)
        {
            var foundReference = false;

            foreach (var parameterInfo in Parameters)
                foundReference |= parameterInfo.RefactorReferencedFolder(procedureEnvironment, originalPath, replacementPath);

            return foundReference;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameterInfo"></param>
        /// <returns></returns>
        public virtual bool StructurallyEqual(ParameterInfo parameterInfo)
        {
            //TODO: We have to consider a more thorough search
            //for now, let's just consider the type and label
            return GetType() == parameterInfo.GetType() && Label == parameterInfo.Label;
        }



        public abstract Parameter ToParameter();



        public virtual void WriteArgumentXML(XmlWriter writer)
        {
            writer.WriteAttributeString("Label", Label);

            if (_guid.HasValue)
                writer.WriteAttributeString("Guid", _guid.Value.ToString());

            if (IsExpression && ParsedExpression != null)
            {
                writer.WriteAttributeString("IsExpression", IsExpression.ToString());
                ParsedExpression.WriteXML(writer);
            }
        }



        /// <summary>
        /// Writes the whole parameter definition (for the graph parameters).
        /// </summary>
        /// <param name="writer"></param>
        public virtual void WriteXMLDefinition(XmlWriter writer)
        {
            writer.WriteAttributeString("Label", Label);
            writer.WriteAttributeString("Type", GetType().AssemblyQualifiedName);

            //reduce the size of the file by not saving data explicitly if
            //their values match their defaults
            if (!string.IsNullOrEmpty(Description))
                writer.WriteAttributeString("Description", Description);

            if (!IsPublic)
                writer.WriteAttributeString("IsPublic", IsPublic.ToString());

            if (EntityEvaluation)
                writer.WriteAttributeString("EntityEvaluation", EntityEvaluation.ToString());

            if (!string.IsNullOrEmpty("Section"))
                writer.WriteAttributeString("Section", Section);

            if (IsExpression && ParsedExpression != null)
            {
                writer.WriteAttributeString("IsExpression", IsExpression.ToString());
                ParsedExpression.WriteXML(writer);
            }
        }
    }
}