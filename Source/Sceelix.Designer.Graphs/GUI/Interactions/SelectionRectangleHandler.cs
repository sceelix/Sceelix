using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Graphs.GUI.Basic;
using Sceelix.Designer.Graphs.GUI.Model;

namespace Sceelix.Designer.Graphs.GUI.Interactions
{
    public class SelectionRectangleHandler
    {
        private const int BorderSize = 4;
        private readonly GraphControl _graphControl;
        private readonly Color _selectionBorderColor = Color.Blue;

        private readonly Color _selectionFillColor = Color.Blue;

        private Texture2D _dot;
        private Rectangle _drawRectangle;
        private Vector2 _firstClickedPosition;
        private Vector2 _lastMouseScreenPosition;



        public SelectionRectangleHandler(GraphControl graphControl)
        {
            _graphControl = graphControl;
            _selectionFillColor = Color.FromNonPremultiplied(_selectionFillColor.R, _selectionFillColor.G, _selectionFillColor.B, 100);
        }







        public void StartDragging(Vector2 mouseScreenPosition)
        {
            _firstClickedPosition = _graphControl.Camera.ToModelPosition(mouseScreenPosition);

            _drawRectangle.X = (int) _firstClickedPosition.X;
            _drawRectangle.Y = (int) _firstClickedPosition.Y;
        }



        public void LoadContent(ContentManager manager)
        {
            _dot = manager.Load<Texture2D>("Graphs/Selection/Dot");
        }



        public void Update(TimeSpan deltaTime, Vector2 mouseScreenPosition)
        {
            if (mouseScreenPosition != _lastMouseScreenPosition)
            {
                _graphControl.ShouldRender = true;
                _lastMouseScreenPosition = mouseScreenPosition;
            }

            Vector2 mouseModelPosition = _graphControl.Camera.ToModelPosition(mouseScreenPosition);
            
            _drawRectangle.Width = (int) Math.Abs(mouseModelPosition.X - _firstClickedPosition.X);
            _drawRectangle.Height = (int) Math.Abs(mouseModelPosition.Y - _firstClickedPosition.Y);

            _drawRectangle.X = (int) Math.Min(_firstClickedPosition.X, mouseModelPosition.X);
            _drawRectangle.Y = (int) Math.Min(_firstClickedPosition.Y, mouseModelPosition.Y);
        }



        public void Draw(SpriteBatch spriteBatch, ControlState interactionState)
        {
            spriteBatch.Draw(_dot, _drawRectangle, _selectionFillColor);

            //draw the borders
            spriteBatch.Draw(_dot, new Rectangle(_drawRectangle.X, _drawRectangle.Y, _drawRectangle.Width, BorderSize), _selectionBorderColor); //top border
            spriteBatch.Draw(_dot, new Rectangle(_drawRectangle.X, _drawRectangle.Y + _drawRectangle.Height - BorderSize, _drawRectangle.Width, BorderSize), _selectionBorderColor); //bottom border
            spriteBatch.Draw(_dot, new Rectangle(_drawRectangle.X + _drawRectangle.Width - BorderSize, _drawRectangle.Y, BorderSize, _drawRectangle.Height), _selectionBorderColor); //right border
            spriteBatch.Draw(_dot, new Rectangle(_drawRectangle.X, _drawRectangle.Y, BorderSize, _drawRectangle.Height), _selectionBorderColor); //left border
        }



        public void ApplySelection()
        {
            VisualGraph.SelectHoveredUnits(false);
        }

        public RectangleF? BoundingRectangle
        {
            get { return new RectangleF(_drawRectangle); }
        }



        public VisualGraph VisualGraph
        {
            get { return _graphControl.VisualGraph; }
        }
    }
}