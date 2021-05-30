using Sceelix.Core.Parameters;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Parameters.Infos;

namespace Sceelix.Mathematics.Parameters
{
    public class ColorParameter : PrimitiveParameter<Color>
    {
        /*public IntParameter RedParameter = new IntParameter("Red", 0) {MinValue = 0,MaxValue = 255};
        public IntParameter GreenParameter = new IntParameter("Green", 0) { MinValue = 0, MaxValue = 255 };
        public IntParameter BlueParameter = new IntParameter("Blue", 0) { MinValue = 0, MaxValue = 255 };
        public IntParameter AlphaParameter = new IntParameter("Alpha", 0) { MinValue = 0, MaxValue = 255 };*/



        public ColorParameter(string label)
            : base(label, Color.White)
        {
        }



        public ColorParameter(string label, Color color)
            : base(label, color)
        {
            Value = color;
        }



        protected override ParameterInfo ToParameterInfo()
        {
            return new ColorParameterInfo(this);
        }



        /*public new Color Value
        {
            get { return new Color((byte)RedParameter.Value, (byte)GreenParameter.Value, (byte)BlueParameter.Value, (byte)AlphaParameter.Value); }
            set
            {
                RedParameter.Value = value.R;
                GreenParameter.Value = value.G;
                BlueParameter.Value = value.B;
                AlphaParameter.Value = value.A;
            }
        }*/
    }
}