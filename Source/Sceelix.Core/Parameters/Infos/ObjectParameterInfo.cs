using System.Xml;
using Sceelix.Core.Graphs.Expressions;

namespace Sceelix.Core.Parameters.Infos
{
    public class ObjectParameterInfo : PrimitiveParameterInfo<object>
    {
        public ObjectParameterInfo(string label)
            : base(label)
        {
            IsExpression = true;
            ParsedExpression = new ParsedExpression(new ConstantExpressionNode(""));
        }



        public ObjectParameterInfo(ObjectParameter parameter)
            : base(parameter)
        {
        }



        public ObjectParameterInfo(XmlElement xmlNode)
            : base(xmlNode)
        {
        }



        public override Parameter ToParameter()
        {
            return new ObjectParameter(this);
        }
    }
}