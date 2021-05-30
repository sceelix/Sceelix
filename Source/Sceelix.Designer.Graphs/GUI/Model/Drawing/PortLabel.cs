using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Core.Graphs;

namespace Sceelix.Designer.Graphs.GUI.Model.Drawing
{
    public class PortLabel
    {
        private const float PortToLabelSpacing = 5;
        private readonly SpriteFont _font;

        //model references
        private readonly Port _port;
        private readonly float _portSize;
        private readonly Vector2 _textOffset;
        private readonly Vector2 _countOffset;



        public PortLabel(Port port, ContentManager content, float portSize)
        {
            _port = port;
            _portSize = portSize;
            _font = content.Load<SpriteFont>("Fonts/ArialSmall");

            Vector2 measuredString = _font.MeasureString(_port.Label);

            _textOffset = new Vector2(0, 2); //new Vector2(0, portSize/2f - measuredString.Y /2);
            _textOffset.X = _port is InputPort ? -measuredString.X - PortToLabelSpacing : PortToLabelSpacing + _portSize;


            _countOffset = _port is InputPort ? new Vector2(20,-20) : new Vector2(20, 20);
        }



        public void Draw(SpriteBatch spriteBatch, Vector2 centerPosition)
        {
            spriteBatch.DrawString(_font, _port.Label, centerPosition + _textOffset, Color.CornflowerBlue);
        }



        public void Draw(SpriteBatch spriteBatch, Vector2 centerPosition, int value)
        {
            spriteBatch.DrawString(_font, value.ToString(), centerPosition + _countOffset, Color.Black);
        }
    }
}