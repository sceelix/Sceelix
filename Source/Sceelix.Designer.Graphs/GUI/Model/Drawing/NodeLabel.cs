using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Core.Graphs;
using Sceelix.Designer.Graphs.GUI.Basic;

namespace Sceelix.Designer.Graphs.GUI.Model.Drawing
{
    public class NodeLabel
    {
        private readonly SpriteFont _font;
        private readonly SpriteFont _fontSmall;
        //model references
        private readonly Node _node;

        private readonly TransitionColor _textColor;

        private readonly string _typeText;
        private string _actualText = String.Empty;

        //private readonly string _extraText = string.Empty;
        private Vector2 _fontMinSize;
        private Vector2 _fontSmallMinSize;
        private Vector2 _minimumSize;



        public NodeLabel(Node node, ContentManager content, Color color)
        {
            _node = node;

            _textColor = new TransitionColor(color, 500);
            _font = content.Load<SpriteFont>("Fonts/Arial18");
            _fontSmall = content.Load<SpriteFont>("Fonts/ArialSmall");

            _typeText = _node.DefaultLabel;
            if (_node.IsObsolete)
                _typeText += " [Obsolete]";

            DetermineActualTextAndSizes();
        }



        /// <summary>
        /// Actual rendered node text
        /// </summary>
        public string Text
        {
            get { return _node.Label; } //.Replace("\\n", "\n")
            set
            {
                _node.Label = value;
                DetermineActualTextAndSizes();
            }
        }



        /// <summary>
        /// Color of the label text
        /// </summary>
        public Color TextColor
        {
            get { return _textColor.Value; }
            set { _textColor.Value = value; }
        }



        /// <summary>
        /// Minimum size the node should have to contain this label.
        /// </summary>
        public Vector2 MinimumSize
        {
            get { return _minimumSize; }
        }



        public void DetermineActualTextAndSizes()
        {
            _actualText = _node.DefaultLabel != _node.Label ? _node.Label.Replace("\\n", "\n") : String.Empty;

            if (!String.IsNullOrWhiteSpace(_actualText))
            {
                _fontMinSize = _font.MeasureString(_actualText);
                _fontSmallMinSize = _fontSmall.MeasureString(_typeText);

                _minimumSize = Vector2.Max(_fontMinSize, _fontSmallMinSize);
            }
            else
            {
                _minimumSize = _font.MeasureString(_typeText);
            }
        }



        /// <summary>
        /// Updates text color transtions.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="deltaTime"></param>
        public bool UpdateColors(TimeSpan deltaTime)
        {
            return _textColor.Update(deltaTime);
        }



        /// <summary>
        /// Draws the text centered in the rectangle.
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="nodeRectangle"></param>
        public void Draw(SpriteBatch spriteBatch, RectangleF nodeRectangle, float alpha = 1f)
        {
            var textColor = new Color(_textColor.Value, alpha);

            if (!String.IsNullOrWhiteSpace(_actualText))
            {
                Vector2 fontPosition = new Vector2(nodeRectangle.Min.X + nodeRectangle.Width/2f - _fontMinSize.X/2f, nodeRectangle.Min.Y + nodeRectangle.Height/2f - _fontMinSize.Y/2f - 10);
                Vector2 fontSmallPosition = new Vector2(nodeRectangle.Min.X + nodeRectangle.Width/2f - _fontSmallMinSize.X/2f, nodeRectangle.Min.Y + nodeRectangle.Height - _fontSmallMinSize.Y - _fontSmallMinSize.Y); //nodeRectangle.Min.Y + nodeRectangle.Height / 2f + _fontSmallMinSize.Y / 2f

                spriteBatch.DrawString(_font, _actualText, fontPosition, textColor);
                spriteBatch.DrawString(_fontSmall, _typeText, fontSmallPosition, textColor);
            }
            else
            {
                Vector2 position = new Vector2(nodeRectangle.Min.X + nodeRectangle.Width/2f - MinimumSize.X/2f, nodeRectangle.Min.Y + nodeRectangle.Height/2f - MinimumSize.Y/2f);
                spriteBatch.DrawString(_font, _typeText, position, textColor);
            }
        }
    }
}