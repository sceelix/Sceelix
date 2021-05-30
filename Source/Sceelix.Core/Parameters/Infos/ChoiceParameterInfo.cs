using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Sceelix.Extensions;

namespace Sceelix.Core.Parameters.Infos
{
    public class ChoiceParameterInfo : PrimitiveParameterInfo<string>
    {
        public ChoiceParameterInfo(string label)
            : base(label)
        {
            Choices = new string[0];
            FixedValue = string.Empty;
        }



        public ChoiceParameterInfo(ChoiceParameter parameter)
            : base(parameter)
        {
            //create a clone of the strings
            Choices = parameter.Choices.ToArray();
        }



        public ChoiceParameterInfo(PrimitiveParameter<string> parameter, string[] array)
            : base(parameter)
        {
            //create a clone of the strings
            Choices = array.ToArray();
        }



        public ChoiceParameterInfo(XmlElement xmlNode)
            : base(xmlNode)
        {
            List<string> choiceList = new List<string>();

            foreach (XmlElement xmlElement in xmlNode["Choices"].GetElementsByTagName("Choice")) choiceList.Add(xmlElement.InnerText);

            Choices = choiceList.ToArray();
        }



        public string[] Choices
        {
            get;
            set;
        }


        public override string MetaType => "Choice";


        public override string ValueLiteral => FixedValue?.Quote();



        public override object Clone()
        {
            var choiceParameterInfo = (ChoiceParameterInfo) base.Clone();

            choiceParameterInfo.Choices = Choices.Select(x => x).ToArray();

            return choiceParameterInfo;
        }



        public override Parameter ToParameter()
        {
            return new ChoiceParameter(this);
        }



        public override void WriteXMLDefinition(XmlWriter writer)
        {
            base.WriteXMLDefinition(writer);

            writer.WriteStartElement("Choices");
            {
                foreach (string extension in Choices) writer.WriteElementString("Choice", extension);
            }
            writer.WriteEndElement();
        }
    }
}