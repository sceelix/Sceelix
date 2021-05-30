namespace Sceelix.Mathematics.Data
{
    public struct CoordinateBounds
    {
        public CoordinateBounds(Coordinate min, Coordinate max)
        {
            Min = min;
            Max = max;
        }



        public Coordinate Min
        {
            get;
        }


        public Coordinate Max
        {
            get;
        }
    }
}