using System.Collections.Generic;

namespace Sceelix.Mathematics.Data
{
    public struct Coordinate
    {
        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }



        public int X
        {
            get;
        }


        public int Y
        {
            get;
        }



        public IEnumerable<Coordinate> Neighbor4Coordinates
        {
            get
            {
                yield return new Coordinate(X, Y - 1);
                yield return new Coordinate(X - 1, Y);
                yield return new Coordinate(X + 1, Y);
                yield return new Coordinate(X, Y + 1);
            }
        }



        public IEnumerable<Coordinate> Neighbor8Coordinates
        {
            get
            {
                yield return new Coordinate(X - 1, Y - 1);
                yield return new Coordinate(X - 1, Y);
                yield return new Coordinate(X - 1, Y + 1);
                yield return new Coordinate(X, Y + 1);
                yield return new Coordinate(X + 1, Y + 1);
                yield return new Coordinate(X + 1, Y);
                yield return new Coordinate(X + 1, Y - 1);
                yield return new Coordinate(X, Y - 1);
            }
        }
    }
}