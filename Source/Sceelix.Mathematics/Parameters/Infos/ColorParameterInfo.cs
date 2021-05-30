using System.Xml;
using Sceelix.Core.Parameters;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Mathematics.Data;

namespace Sceelix.Mathematics.Parameters.Infos
{
    public class ColorParameterInfo : PrimitiveParameterInfo<Color>
    {
        public ColorParameterInfo(string label)
            : base(label)
        {
            FixedValue = Color.White;
            /*Fields.Add(new IntParameterInfo("Red") { MinValue = 0, MaxValue = 255,FixedValue = 255});
            Fields.Add(new IntParameterInfo("Green") { MinValue = 0, MaxValue = 255, FixedValue = 255 });
            Fields.Add(new IntParameterInfo("Blue") { MinValue = 0, MaxValue = 255, FixedValue = 255 });
            Fields.Add(new IntParameterInfo("Alpha") { MinValue = 0, MaxValue = 255, FixedValue = 255 });*/
        }



        public ColorParameterInfo(ColorParameter parameter)
            : base(parameter)
        {
        }



        public ColorParameterInfo(XmlElement xmlNode)
            : base(xmlNode)
        {
        }



        public override string ValueLiteral
        {
            get
            {
                var fixedValue = FixedValue;
                return "new Color(" + fixedValue.R + "," + fixedValue.G + "," + fixedValue.B + "," + fixedValue.A + ")";
            }
        }



        /*public Color FixedValue
        {
            get
            {
                byte r = (byte)Fields[0].CastTo<IntParameterInfo>().FixedValue;
                byte g = (byte)Fields[1].CastTo<IntParameterInfo>().FixedValue;
                byte b = (byte)Fields[2].CastTo<IntParameterInfo>().FixedValue;
                byte a = (byte)Fields[3].CastTo<IntParameterInfo>().FixedValue;

                return new Color(r,g,b,a);
            }
            set
            {
                Fields[0].CastTo<IntParameterInfo>().FixedValue = value.R;
                Fields[1].CastTo<IntParameterInfo>().FixedValue = value.G;
                Fields[2].CastTo<IntParameterInfo>().FixedValue = value.B;
                Fields[3].CastTo<IntParameterInfo>().FixedValue = value.A;
            }
        }*/



        public override Parameter ToParameter()
        {
            return new ColorParameter(Label, FixedValue);
        }
    }
}