using System.Xml;
using Sceelix.Core.Parameters;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Mathematics.Data;

namespace Sceelix.Mathematics.Parameters.Infos
{
    public class Vector2DParameterInfo : PrimitiveParameterInfo<Vector2D>
    {
        public Vector2DParameterInfo(string label)
            : base(label)
        {
            /*Fields.Add(new FloatParameterInfo("X"));
            Fields.Add(new FloatParameterInfo("Y"));*/
        }



        public Vector2DParameterInfo(Vector2DParameter parameter)
            : base(parameter)
        {
        }



        public Vector2DParameterInfo(XmlElement xmlNode)
            : base(xmlNode)
        {
        }



        public override string ValueLiteral
        {
            get
            {
                var fixedValue = FixedValue;
                return "new Vector2D(" + fixedValue.X + "," + fixedValue.Y + ")";
            }
        }



        /*public Vector2D FixedValue
        {
            get
            {
                float x = Fields[0].CastTo<FloatParameterInfo>().FixedValue;
                float y = Fields[1].CastTo<FloatParameterInfo>().FixedValue;

                return new Vector2D(x, y);
            }
            set
            {
                Fields[0].CastTo<FloatParameterInfo>().FixedValue = value.X;
                Fields[1].CastTo<FloatParameterInfo>().FixedValue = value.Y;
            }
        }*/



        public override Parameter ToParameter()
        {
            return new Vector2DParameter(Label, FixedValue);
        }
    }
}