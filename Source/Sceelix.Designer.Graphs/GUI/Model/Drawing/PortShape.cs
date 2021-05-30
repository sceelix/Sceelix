using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Core.Graphs;
using Sceelix.Designer.Graphs.GUI.Basic;
using Sceelix.Designer.Graphs.GUI.Basic.Animations;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Graphs.GUI.Model.Drawing
{
    public class PortShape
    {
        private const float DefaultSize = 22;

        private readonly TransitionColor _borderColor = new TransitionColor(Color.Black, 250);
        private readonly TransitionColor _fillColor = new TransitionColor(Color.White, 250);

        private readonly Port _port;

        private readonly Texture2D _textureBorder;
        private readonly Texture2D _textureBorderNormal;
        private readonly Texture2D _textureFill;

        private PortAnimation _animation;
        private float _extraSize = 0;
        private Rectangle _rectangle;



        public PortShape(Port port, ContentManager content)
        {
            _port = port;
            //_textureBorder = EmbeddedResources.Load<Texture2D>("Resources/Graphs.Nodes." + port.Shape + "Border.png");  //content.Load<Texture2D>("Graphs/Nodes/" + port.Shape + "Border");
            //_textureFill = EmbeddedResources.Load<Texture2D>("Resources/Graphs.Nodes." + port.Shape + "Fill.png");    //content.Load<Texture2D>("Graphs/Nodes/" + port.Shape + "Fill");
            //_textureBorder = content.Load<Texture2D>("Graphs/Nodes/" + port.Shape + "Border");
            //_textureFill = content.Load<Texture2D>("Graphs/Nodes/" + port.Shape + "Fill");
            bool isOptional = port is InputPort && ((InputPort) port).IsOptional;

            //String optionalSuffix = port is InputPort && ((InputPort) port).IsOptional ? "Optional" : String.Empty;

            _textureBorder = EmbeddedResources.Load<Texture2D>("Resources/Graphs/Nodes/" + port.Shape + "Border"+ (isOptional ? "Optional" : "") + ".png");
            _textureBorderNormal = isOptional ? EmbeddedResources.Load<Texture2D>("Resources/Graphs/Nodes/" + port.Shape + "Border.png") : _textureBorder;
            _textureFill = EmbeddedResources.Load<Texture2D>("Resources/Graphs/Nodes/" + port.Shape + "Fill.png");
        }



        public Color BorderColor
        {
            get { return _borderColor.Value; }
            set { _borderColor.Value = value; }
        }



        public Color FillColor
        {
            get { return _fillColor.Value; }
            set { _fillColor.Value = value; }
        }



        public float Width
        {
            get { return DefaultSize; }
        }



        public float Height
        {
            get { return DefaultSize; }
        }



        public PortAnimation Animation
        {
            get { return _animation; }
            set { _animation = value; }
        }



        public Rectangle Rectangle
        {
            get { return _rectangle; }
        }



        public float ExtraSize
        {
            get { return _extraSize; }
            set { _extraSize = value; }
        }



        public bool UpdateColors(TimeSpan deltaTime)
        {
            var updated = _borderColor.Update(deltaTime);
            updated |= _fillColor.Update(deltaTime);

            if (_animation != null)
            {
                _animation.Process(deltaTime);
                return true;
            }

            return updated;
        }



        public void DrawBorderFill(SpriteBatch spriteBatch, Vector2 position)
        {
            float amount = _animation != null ? _animation.Amount : _extraSize;
            //float extraInflate = _animation != null ? _animation.Amount : _extraSize;

            _rectangle = new Rectangle((int) (position.X - amount/2), (int) (position.Y - amount/2), (int) (DefaultSize + amount), (int) (DefaultSize + amount));

            if (_port.PortState == PortState.Gate)
            {
                Rectangle rectangleToInflate = _rectangle;
                rectangleToInflate.Inflate(5 + amount/5f, 5 + amount/5f);

                spriteBatch.Draw(_textureFill, rectangleToInflate, _fillColor.Value);
                spriteBatch.Draw(_textureBorderNormal, rectangleToInflate, _borderColor.Value);

                spriteBatch.Draw(_textureFill, _rectangle, _fillColor.Value);
                spriteBatch.Draw(_textureBorder, _rectangle, _borderColor.Value);
            }
            else if (_port.PortState == PortState.Blocked)
            {
                spriteBatch.Draw(_textureFill, _rectangle, _borderColor.Value);
                spriteBatch.Draw(_textureBorder, _rectangle, _borderColor.Value);
            }
            else
            {
                spriteBatch.Draw(_textureFill, _rectangle, _fillColor.Value);
                spriteBatch.Draw(_textureBorder, _rectangle, _borderColor.Value);
            }
        }
    }
}