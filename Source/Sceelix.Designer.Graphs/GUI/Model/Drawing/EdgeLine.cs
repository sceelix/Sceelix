using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Graphs.GUI.Basic;
using Sceelix.Designer.Utils;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Sceelix.Designer.Graphs.GUI.Model.Drawing
{
    public class EdgeLine
    {
        private const int SelectionExtension = 10;
        private const float MinLineSize = 10f;
        private readonly TransitionColor _color = new TransitionColor(Color.Black, 100);

        private readonly List<Rectangle> _drawableLines = new List<Rectangle>();
        private readonly List<RectangleF> _intersectableLines = new List<RectangleF>();

        private readonly Texture2D _texture;
        private readonly Texture2D _textureBold;
        //private float _previousDistance = 0f;
        private Path2F _bezierPath;
        private RectangleF _boundingRectangle = new RectangleF();
        private Vector2 _midPoint;

        private Vector2 _previousBegin = new Vector2(float.NegativeInfinity), _previousEnd = new Vector2(float.NegativeInfinity);



        public EdgeLine(ContentManager content)
        {
            _texture = EmbeddedResources.Load<Texture2D>("Resources/Graphs/Nodes/EdgeCircle.png");
            _textureBold = EmbeddedResources.Load<Texture2D>("Resources/Graphs/Nodes/EdgeCircleBold.png");
        }



        public Color Color
        {
            get { return _color.Value; }
            set { _color.Value = value; }
        }



        public int TransitionSpeed
        {
            set { _color.TransitionSpeed = value; }
        }



        public Vector2 MidPoint
        {
            get { return _midPoint; }
        }



        public RectangleF BoundingRectangle
        {
            get { return _boundingRectangle; }
        }



        public bool Update(TimeSpan deltaTime, Vector2 positionBegin, Vector2 positionEnd)
        {
            //UpdateLine2(positionBegin, positionEnd);
            bool updatedLine = false;

            if (Math.Abs(_previousBegin.LengthSquared() - positionBegin.LengthSquared()) > float.Epsilon ||
                Math.Abs(_previousEnd.LengthSquared() - positionEnd.LengthSquared()) > float.Epsilon)
            {
                UpdatePoints(positionBegin, positionEnd);
                updatedLine = true;
            }

            _previousBegin = positionBegin;
            _previousEnd = positionEnd;


            /*var currentDistance = (positionBegin - positionEnd).Length();
            if (Math.Abs(currentDistance - _previousDistance) > float.Epsilon)
            {
                
                _previousDistance = currentDistance;
            }*/

            //update the line color
            return _color.Update(deltaTime) || updatedLine;
        }



        private void UpdatePoints(Vector2 positionBegin, Vector2 positionEnd)
        {
            var height = (int) Math.Abs(positionBegin.Y - positionEnd.Y);

            List<Vector2> points = new List<Vector2>();

            var offset = Math.Min(height/2f, 100);

            points.Add(positionBegin);
            points.Add(positionBegin + new Vector2(0, offset));

            if (positionBegin.Y > positionEnd.Y)
            {
                var middlePosition = (positionBegin + positionEnd)/2f;

                points.Add(middlePosition + new Vector2(0, height/2f + offset));
                points.Add(middlePosition);
                points.Add(middlePosition + new Vector2(0, -(height/2f + offset)));
            }

            points.Add(positionEnd + new Vector2(0, -offset));
            points.Add(positionEnd);

            //calculate the boundingrectangle
            _boundingRectangle = new RectangleF(points);

            //now calculate the bezier path
            _bezierPath = new Path2F();

            foreach (var vector2 in points)
            {
                _bezierPath.Add(new PathKey2F() {Point = new Vector2F(vector2.X, vector2.Y), Interpolation = SplineInterpolation.BSpline});
            }
            //bezierPath.First().Interpolation = SplineInterpolation.Linear;
            //bezierPath.Last().Interpolation = SplineInterpolation.Linear;

            _bezierPath.ParameterizeByLength(20, 0.001f);

            
        }



        private System.Drawing.PointF ToPointF(Vector2 v)
        {
            return new System.Drawing.PointF((int) v.X, (int) v.Y);
        }



        private void UpdateLine(Vector2 positionBegin, Vector2 positionEnd)
        {
            _drawableLines.Clear();
            _intersectableLines.Clear();

            //don't draw a line with a small size...
            if ((positionBegin - positionEnd).Length() < MinLineSize)
                return;

            _midPoint = (positionBegin + positionEnd)/2;

            //most common expected case
            if (positionBegin.X < positionEnd.X)
            {
                CreateHorizontalLine(positionBegin.X, _midPoint.X, positionBegin.Y);
                CreateHorizontalLine(_midPoint.X, positionEnd.X, positionEnd.Y);
                CreateVerticalLine(positionBegin.Y, positionEnd.Y, _midPoint.X);
            }
            else
            {
                const int initialSize = 30;

                CreateHorizontalLine(positionBegin.X, positionBegin.X + initialSize, positionBegin.Y);
                CreateHorizontalLine(positionEnd.X - initialSize, positionEnd.X, positionEnd.Y);
                CreateVerticalLine(positionBegin.Y, _midPoint.Y, positionBegin.X + initialSize);
                CreateVerticalLine(_midPoint.Y, positionEnd.Y, positionEnd.X - initialSize);
                CreateHorizontalLine(positionEnd.X - initialSize, positionBegin.X + initialSize, _midPoint.Y);
            }
        }



        private void UpdateLine2(Vector2 positionBegin, Vector2 positionEnd)
        {
            _drawableLines.Clear();
            _intersectableLines.Clear();

            //don't draw a line with a small size...
            if ((positionBegin - positionEnd).Length() < MinLineSize)
                return;


            _midPoint = (positionBegin + positionEnd)/2;

            //most common expected case
            if (positionBegin.Y < positionEnd.Y)
            {
                CreateVerticalLine(positionBegin.Y, _midPoint.Y, positionBegin.X);
                CreateVerticalLine(_midPoint.Y, positionEnd.Y, positionEnd.X);
                CreateHorizontalLine(positionBegin.X, positionEnd.X, _midPoint.Y);
            }
            else
            {
                const int initialSize = 30;

                CreateVerticalLine(positionBegin.Y, positionBegin.Y + initialSize, positionBegin.X);
                CreateVerticalLine(positionEnd.Y - initialSize, positionEnd.Y, positionEnd.X);
                CreateHorizontalLine(positionBegin.X, _midPoint.X, positionBegin.Y + initialSize);
                CreateHorizontalLine(_midPoint.X, positionEnd.X, positionEnd.Y - initialSize);
                CreateVerticalLine(positionEnd.Y - initialSize, positionBegin.Y + initialSize, _midPoint.X);
            }
        }



        private void CreateHorizontalLine(float x1, float x2, float y)
        {
            float startingX = Math.Min(x1, x2);

            Rectangle rectangle = new Rectangle((int) startingX, (int) (y - _texture.Height/2f), (int) Math.Abs(x1 - x2) + _texture.Height, _texture.Height);
            RectangleF boundingRectangle = new RectangleF(rectangle);
            boundingRectangle.Expand(SelectionExtension);

            _drawableLines.Add(rectangle);
            _intersectableLines.Add(boundingRectangle);
        }



        private void CreateVerticalLine(float y1, float y2, float x)
        {
            float startingY = Math.Min(y1, y2);

            Rectangle rectangle = new Rectangle((int) x, (int) (startingY), _texture.Width, (int) Math.Abs(y1 - y2));
            RectangleF boundingRectangle = new RectangleF(rectangle);
            boundingRectangle.Expand(SelectionExtension);


            _drawableLines.Add(rectangle);
            _intersectableLines.Add(boundingRectangle);
        }



        public void DrawLine(SpriteBatch spriteBatch, bool inflate, bool drawDotted = false)
        {
            Texture2D selectedTexture = inflate ? _textureBold : _texture;

            //if (_bezierPath != null)
            {
                var increment = drawDotted ? 15 : 1;
                for (int i = 0; i < _bezierPath.Last().Parameter; i += increment)
                {
                    var point = _bezierPath.GetPoint(i);
                    spriteBatch.Draw(selectedTexture, new Vector2(point.X - selectedTexture.Width / 2f, point.Y - selectedTexture.Height / 2f), _color.Value);
                }
            }

            //LinePath linePath = new LinePath(positionBegin);
            /*foreach (Rectangle lineRectangle in _drawableLines)
            {
                var newRectangle = lineRectangle;

                if(inflate)
                    newRectangle.Inflate(_texture.Width / 2f,_texture.Height/2f);

                newRectangle.X -= (int)(_texture.Width/2f);

                spriteBatch.Draw(_texture, newRectangle, _color.Value);
            }*/
        }



        public bool ContainsPoint(Vector2 mouseModelLocation)
        {
            //we can spare a lot of calculation this way
            if (!_boundingRectangle.ContainsPoint(mouseModelLocation))
                return false;

            //otherwise, go over all points
            for (int i = 0; i < _bezierPath.Last().Parameter; i += 1)
            {
                var point = _bezierPath.GetPoint(i).ToXna();
                if ((point - mouseModelLocation).Length() < _texture.Width)
                    return true;
            }

            return false;

            //return _intersectableLines.Any(val => val.ContainsPoint(mouseModelLocation));
        }
    }
}