using Sceelix.Core.Annotations;

namespace Sceelix.Surfaces.Data
{
    [Entity("Hole Layer", TypeBrowsable = false)]
    public class HoleLayer : GenericSurfaceLayer<bool>
    {
        public HoleLayer(bool[,] values)
            : base(values)
        {
        }



        protected override bool Add(bool valueA, bool valueB)
        {
            return valueA;
        }



        public override SurfaceLayer CreateEmpty(int numColumns, int numRows)
        {
            return new HoleLayer(new bool[numColumns, numRows]);
        }



        protected override bool InvertValue(bool value)
        {
            return !value;
        }



        protected override bool Minus(bool valueA, bool valueB)
        {
            return valueA;
        }



        protected override bool Multiply(bool value1, bool value2)
        {
            return value1;
        }



        protected override bool MultiplyScalar(bool value, float scalar)
        {
            return value;
        }

        
        public override void Update()
        {
            //do nothing
        }
    }
}