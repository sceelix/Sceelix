using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Extensions;
using Sceelix.Designer.Graphs.Extensions;
using Sceelix.Designer.Graphs.GUI.Basic;

namespace Sceelix.Designer.Graphs.GUI.Model.Drawing
{
    public class NodeRectangle
    {
        private readonly TransitionColor _borderColor = new TransitionColor(Color.Black, 300);
        private readonly TransitionColor _fillColor;
        //these are the small sized samples of the border and the fill
        private readonly Texture2D _loadedTextureBorder;
        private readonly Texture2D _loadedTextureFill;

        //these are the resized textures of the border and fill
        private Texture2D _textureBorder;
        private Texture2D _textureFill;



        public NodeRectangle(ContentManager content, Color color, string shapeName)
        {
            //_textureBorder = _loadedTextureBorder = EmbeddedResources.Load<Texture2D>("Resources/Graphs.Nodes." + shapeName + "Border.png"); //content.Load<Texture2D>("Graphs/Nodes/" + shapeName + "Border");
            //_textureFill = _loadedTextureFill = EmbeddedResources.Load<Texture2D>("Resources/Graphs.Nodes." + shapeName + "Fill.png");  //content.Load<Texture2D>("Graphs/Nodes/" + shapeName + "Fill");//RectangleFill
            _textureBorder = _loadedTextureBorder = content.Load<Texture2D>("Graphs/Nodes/" + shapeName + "Border");
            _textureFill = _loadedTextureFill = content.Load<Texture2D>("Graphs/Nodes/" + shapeName + "Fill"); //RectangleFill

            _fillColor = new TransitionColor(color, 300);
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



        public void UpdateSize(int width, int height)
        {
            int actualWidth = Math.Max(width, _loadedTextureBorder.Width);
            int actualHeight = Math.Max(height, _loadedTextureBorder.Height);

            _textureBorder = _loadedTextureBorder.ExtendRectangleQuad(actualWidth, actualHeight);
            _textureFill = _loadedTextureFill.ExtendRectangleQuad(actualWidth, actualHeight);
        }



        public bool UpdateColors(TimeSpan deltaTime)
        {
            return _borderColor.Update(deltaTime) || _fillColor.Update(deltaTime);
        }



        public void Draw(SpriteBatch spriteBatch, RectangleF nodeRectangle, float alpha = 1f)
        {
            Rectangle xnaRectangle = nodeRectangle.ToXnaRectangle();

            spriteBatch.Draw(_textureFill, xnaRectangle, new Color(_fillColor.Value, alpha));
            spriteBatch.Draw(_textureBorder, xnaRectangle, new Color(_borderColor.Value, alpha));
        }
    }
}