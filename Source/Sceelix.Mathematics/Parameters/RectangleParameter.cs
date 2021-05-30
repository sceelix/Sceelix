using Sceelix.Core.Parameters;
using Sceelix.Mathematics.Data;

namespace Sceelix.Mathematics.Parameters
{
    public class RectangleParameter : CompoundParameter
    {
        public FloatParameter Height = new FloatParameter("Height", 0);
        public FloatParameter Width = new FloatParameter("Width", 0);
        public FloatParameter X = new FloatParameter("X", 0);
        public FloatParameter Y = new FloatParameter("Y", 0);



        public RectangleParameter(string label)
            : base(label)
        {
        }



        public RectangleParameter(string label, Rectangle rectangle)
            : base(label)
        {
            Value = rectangle;
        }



        public Rectangle Value
        {
            get { return new Rectangle(X.Value, Y.Value, Width.Value, Height.Value); }
            set
            {
                X.Value = value.X;
                Y.Value = value.Y;
                Width.Value = value.Width;
                Height.Value = value.Height;
            }
        }
    }
}