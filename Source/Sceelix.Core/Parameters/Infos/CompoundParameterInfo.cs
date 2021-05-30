using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Sceelix.Core.Code;
using Sceelix.Core.Environments;
using Sceelix.Core.Graphs;
using Sceelix.Extensions;

namespace Sceelix.Core.Parameters.Infos
{
    public class CompoundParameterInfo : ParameterInfo
    {
        public CompoundParameterInfo(string label)
            : base(label)
        {
            Fields = new List<ParameterInfo>();
            Expanded = true;
        }



        public CompoundParameterInfo(CompoundParameter parameter) //, List<ParameterInfo> parameterInfos, List<InputPort> toList, List<OutputPort> list
            : base(parameter)
        {
            Fields = parameter.Fields.Select(p => p.ToParameterInfo()).ToList();
            Expanded = parameter.IsExpandedAsDefault;
            ArrangeInSingleLine = parameter.ArrangeInSingleLine;

            foreach (var parameterInfo in Fields)
                parameterInfo.Parent = this;
        }



        public CompoundParameterInfo(XmlElement xmlNode)
            : base(xmlNode)
        {
            Fields = new List<ParameterInfo>();
            _inputPorts = new List<InputPort>();
            _outputPorts = new List<OutputPort>();

            Expanded = xmlNode.GetAttributeOrDefault("Expanded", true);
            ArrangeInSingleLine = xmlNode.GetAttributeOrDefault("ArrangeInSingleLine", false);

            XmlNodeList parameterList = xmlNode["Fields"].ChildNodes;
            foreach (XmlElement parameterNode in parameterList)
            {
                Type parameterType = parameterNode.GetAttributeOrDefault<Type>("Type", null);

                var parameterInfo = (ParameterInfo) Activator.CreateInstance(parameterType, parameterNode);
                parameterInfo.Parent = this;
                Fields.Add(parameterInfo);
            }
        }



        public bool ArrangeInSingleLine
        {
            get;
            set;
        }


        public override bool CanHaveSubItems => true;


        public bool Expanded
        {
            get;
            set;
        }


        /*public override Argument ToArgument()
        {
            return new CompoundArgument(this, _parameterInfos.Select(val => val.ToArgument()).ToList());
        }*/


        public List<ParameterInfo> Fields
        {
            get;
            private set;
        }


        public override List<ParameterInfo> ItemModels => Fields;


        public override string MetaType => "Compound";


        public override List<ParameterInfo> Parameters => Fields;



        public override IEnumerable<InputPort> SubInputPortTree
        {
            get
            {
                if (IsExpression)
                    yield break;

                foreach (var inputPort in _inputPorts)
                    yield return inputPort;

                foreach (var inputPort in Fields.SelectMany(item => item.SubInputPortTree))
                    yield return inputPort;
            }
        }



        public override IEnumerable<OutputPort> SubOutputPortTree
        {
            get
            {
                if (IsExpression)
                    yield break;

                foreach (var outputPort in _outputPorts)
                    yield return outputPort;

                foreach (var outputPort in Fields.SelectMany(item => item.SubOutputPortTree))
                    yield return outputPort;
            }
        }



        public override object Clone()
        {
            var clone = (CompoundParameterInfo) base.Clone();

            //clone each subparameter info
            clone.Fields = Fields.Select(x => (ParameterInfo) x.Clone()).ToList();

            foreach (var parameterInfo in clone.Fields)
                parameterInfo.Parent = clone;

            return clone;
        }



        public override object CloneModel(bool resolveRecursiveParameter)
        {
            var clone = (CompoundParameterInfo) base.Clone();

            //clone each subparameter info
            clone.Fields = Fields.Select(x => (ParameterInfo) x.CloneModel(resolveRecursiveParameter)).ToList();

            foreach (var parameterInfo in clone.Fields)
                parameterInfo.Parent = clone;

            return clone;
        }



        protected override void CreateSpecificCSharpCode(CodeBuilder codeBuilder, string varName)
        {
            var publicSubParameters = Parameters.Where(x => x.IsPublic).ToList();

            if (publicSubParameters.Count > 1 || InputPorts.Count() > 0)
            {
                var shortVarName = Parameter.GetIdentifier(Label).FirstLetterToLower() + "Parameter";
                codeBuilder.AppendLine("{");
                codeBuilder.TabIndentation++;
                codeBuilder.AppendLine($"var {shortVarName} = {varName};");

                foreach (var subParameterInfo in publicSubParameters)
                    subParameterInfo.CreateCSharpCode(codeBuilder, shortVarName);

                foreach (var port in InputPorts)
                {
                    var portEntityVariable = Parameter.GetIdentifier(port.ObjectType.Name).FirstLetterToLower();
                    codeBuilder.AppendLine($"{shortVarName}.Inputs[{port.Label.Quote()}].Enqueue({portEntityVariable});");
                }

                codeBuilder.TabIndentation--;
                codeBuilder.AppendLine("}");
            }
            else
            {
                foreach (var subParameterInfo in publicSubParameters)
                    subParameterInfo.CreateCSharpCode(codeBuilder, varName);
            }

            if (OutputPorts.Count() > 0)
                foreach (var outputPort in OutputPorts)
                    codeBuilder.AppendLineToFooter($"var output{codeBuilder.VarCounter++} = {varName}.Outputs[{outputPort.Label.Quote()}].Dequeue();");
        }



        protected internal override string GetFullLabel(ParameterInfo parameterInfo)
        {
            if (parameterInfo == this)
                return Label;

            foreach (var field in Fields)
            {
                var fullLabel = field.GetFullLabel(parameterInfo);
                if (fullLabel != null)
                    return Label + "." + fullLabel;
            }

            return null;
        }



        public override IEnumerable<string> GetReferencedPaths()
        {
            return Fields.SelectMany(val => val.GetReferencedPaths());
        }



        public override IEnumerable<ParameterInfo> GetSubtree(bool stopAtExpressionParameters)
        {
            if (stopAtExpressionParameters && IsExpression)
                yield break;

            foreach (var field in Fields)
            {
                yield return field;

                foreach (var subTreeItem in field.GetSubtree(stopAtExpressionParameters))
                    yield return subTreeItem;
            }
        }



        /*public override object CloneModel()
        {
            var clone = (CompoundParameterInfo) base.CloneModel();

            //clone each subparameter info
            clone._fields = _fields.Select(x => (ParameterInfo)x.CloneModel()).ToList();

            foreach (var parameterInfo in clone._fields)
                parameterInfo.Parent = clone;

            return clone;
        }*/



        public override bool HasReferenceToParameter(string label)
        {
            if (base.HasReferenceToParameter(label))
                return true;

            return Fields.Any(x => x.HasReferenceToParameter(label));
        }



        public override void ReadArgumentXML(XmlNode xmlNode, IProcedureEnvironment procedureEnvironment)
        {
            base.ReadArgumentXML(xmlNode, procedureEnvironment);

            Expanded = xmlNode.GetAttributeOrDefault("Expanded", true);

            XmlNodeList argumentNodeList = xmlNode["Fields"].ChildNodes;
            foreach (XmlElement argumentNode in argumentNodeList)
            {
                string argumentLabel = argumentNode.Attributes["Label"].InnerText;
                ParameterInfo argument = Fields.FirstOrDefault(val => val.Label == argumentLabel);
                if (argument != null) argument.ReadArgumentXML(argumentNode, procedureEnvironment);
            }
        }



        public override void RefactorGlobalParameters(string oldLabel, string newLabel)
        {
            base.RefactorGlobalParameters(oldLabel, newLabel);

            foreach (var parameterInfo in Fields)
                parameterInfo.RefactorGlobalParameters(oldLabel, newLabel);
        }



        public override Parameter ToParameter()
        {
            return new CompoundParameter(this, Fields.Select(x => x.ToParameter()));
        }



        public override void WriteArgumentXML(XmlWriter writer)
        {
            writer.WriteAttributeString("Expanded", Convert.ToString(Expanded, CultureInfo.InvariantCulture));

            base.WriteArgumentXML(writer);

            writer.WriteStartElement("Fields");
            {
                foreach (ParameterInfo parameterInfo in Fields)
                {
                    writer.WriteStartElement("Field");

                    parameterInfo.WriteArgumentXML(writer);

                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
        }



        public override void WriteXMLDefinition(XmlWriter writer)
        {
            writer.WriteAttributeString("Expanded", Convert.ToString(Expanded, CultureInfo.InvariantCulture));
            writer.WriteAttributeString("ArrangeInSingleLine", Convert.ToString(ArrangeInSingleLine, CultureInfo.InvariantCulture));

            base.WriteXMLDefinition(writer);

            writer.WriteStartElement("Fields");
            {
                foreach (ParameterInfo parameterInfo in Fields)
                {
                    writer.WriteStartElement("Field");

                    parameterInfo.WriteXMLDefinition(writer);

                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
        }
    }
}