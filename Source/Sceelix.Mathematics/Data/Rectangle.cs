using Sceelix.Mathematics.Spatial;

namespace Sceelix.Mathematics.Data
{
    public struct Rectangle
    {
        public Rectangle(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }



        public Rectangle(Vector2D min, Vector2D max)
        {
            X = min.X;
            Y = min.Y;
            Width = max.X - min.X;
            Height = max.Y - min.Y;
        }



        public Rectangle(BoundingBox boundingBox)
        {
            X = boundingBox.Min.X;
            Y = boundingBox.Min.Y;
            Width = boundingBox.Width;
            Height = boundingBox.Length;
        }



        public float X
        {
            get;
        }


        public float Y
        {
            get;
        }


        public float Width
        {
            get;
        }


        public float Height
        {
            get;
        }


        public Vector2D Min => new Vector2D(X, Y);


        public Vector2D Max => new Vector2D(X + Width, Y + Height);



        public override string ToString()
        {
            return string.Format("X: {0}, Y: {1}, Width: {2}, Height: {3}", X, Y, Width, Height);
        }



        public Rectangle Merge(Rectangle rectangle)
        {
            return new Rectangle(Vector2D.Minimize(rectangle.Min, Min), Vector2D.Maximize(rectangle.Max, Max));
        }



        public static implicit operator System.Drawing.Rectangle(Rectangle rectangle)
        {
            return new System.Drawing.Rectangle((int) rectangle.X, (int) rectangle.Y, (int) rectangle.Width, (int) rectangle.Height);
        }



        public static implicit operator Rectangle(System.Drawing.Rectangle rectangle)
        {
            return new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }
    }
}