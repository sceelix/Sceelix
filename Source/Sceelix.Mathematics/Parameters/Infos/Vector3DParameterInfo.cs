using System.Xml;
using Sceelix.Core.Parameters;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Mathematics.Data;

namespace Sceelix.Mathematics.Parameters.Infos
{
    public class Vector3DParameterInfo : PrimitiveParameterInfo<Vector3D> //CompoundParameterInfo
    {
        public Vector3DParameterInfo(string label)
            : base(label)
        {
            /*Fields.Add(new FloatParameterInfo("X"));
            Fields.Add(new FloatParameterInfo("Y"));
            Fields.Add(new FloatParameterInfo("Z"));*/
        }



        public Vector3DParameterInfo(Vector3DParameter parameter)
            : base(parameter)
        {
        }



        public Vector3DParameterInfo(XmlElement xmlNode)
            : base(xmlNode)
        {
        }



        public override string ValueLiteral
        {
            get
            {
                var fixedValue = FixedValue;
                return "new Vector3D(" + fixedValue.X + "," + fixedValue.Y + "," + fixedValue.Z + ")";
            }
        }



        /*public Vector3D FixedValue
        {
            get
            {
                float x = Fields[0].CastTo<FloatParameterInfo>().FixedValue;
                float y = Fields[1].CastTo<FloatParameterInfo>().FixedValue;
                float z = Fields[2].CastTo<FloatParameterInfo>().FixedValue;

                return new Vector3D(x, y, z);
            }
            set
            {
                Fields[0].CastTo<FloatParameterInfo>().FixedValue = value.X;
                Fields[1].CastTo<FloatParameterInfo>().FixedValue = value.Y;
                Fields[2].CastTo<FloatParameterInfo>().FixedValue = value.Z;
            }
        }*/



        public override Parameter ToParameter()
        {
            return new Vector3DParameter(Label, FixedValue);
        }
    }
}