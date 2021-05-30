using System.Xml;

namespace Sceelix.Core.Parameters.Infos
{
    public class BoolParameterInfo : PrimitiveParameterInfo<bool>
    {
        public BoolParameterInfo(string label)
            : base(label)
        {
        }



        public BoolParameterInfo(BoolParameter parameter)
            : base(parameter)
        {
        }



        public BoolParameterInfo(XmlElement xmlNode)
            : base(xmlNode)
        {
        }



        public override string ValueLiteral =>
            //C# converts boolean values to True or False, so we need to set the string to lowercase
            FixedValue.ToString().ToLower();



        public override Parameter ToParameter()
        {
            return new BoolParameter(this);
        }
    }
}