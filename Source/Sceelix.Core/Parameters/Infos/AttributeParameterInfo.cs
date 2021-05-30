using System.Xml;
using Sceelix.Core.Environments;
using Sceelix.Extensions;

namespace Sceelix.Core.Parameters.Infos
{
    public class AttributeParameterInfo : ParameterInfo
    {
        public AttributeParameterInfo(string label)
            : base(label)
        {
        }



        public AttributeParameterInfo(string label, string description, bool isPublic, string attributeString)
            : base(label, description, isPublic)
        {
            AttributeString = attributeString;
        }



        public AttributeParameterInfo(AttributeParameter parameter)
            : base(parameter)
        {
            Access = parameter.Access;
            AttributeString = parameter.AttributeString;
        }



        public AttributeParameterInfo(XmlElement xmlNode)
            : base(xmlNode)
        {
            Access = xmlNode.GetAttributeOrDefault("Access", Access);
            AttributeString = xmlNode.GetAttributeOrDefault("FixedValue", string.Empty);
        }



        public AttributeAccess Access
        {
            get;
            set;
        }


        public string AttributeString
        {
            get;
            set;
        }


        public override string MetaType => "Attribute";


        public override string ValueLiteral => AttributeString.Quote();



        public override void ReadArgumentXML(XmlNode xmlNode, IProcedureEnvironment procedureEnvironment)
        {
            base.ReadArgumentXML(xmlNode, procedureEnvironment);

            AttributeString = xmlNode.GetAttributeOrDefault("FixedValue", string.Empty);
        }



        public override Parameter ToParameter()
        {
            return new AttributeParameter(this);
        }



        public override void WriteArgumentXML(XmlWriter writer)
        {
            writer.WriteAttributeString("FixedValue", AttributeString);

            base.WriteArgumentXML(writer);
        }



        public override void WriteXMLDefinition(XmlWriter writer)
        {
            base.WriteXMLDefinition(writer);

            writer.WriteAttributeString("Access", Access.ToString());
            writer.WriteAttributeString("FixedValue", AttributeString);
        }
    }
}