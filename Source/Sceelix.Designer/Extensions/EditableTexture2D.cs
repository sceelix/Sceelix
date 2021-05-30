using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sceelix.Designer.Extensions
{
    /// <summary>
    /// A texture2D that can be edited.
    /// 
    /// This could inherit from Texture2D, but then every change on the [x,y] would have to set data.
    /// </summary>
    public class EditableTexture2D
    {
        private readonly GraphicsDevice graphicsDevice;
        private readonly int height;
        private readonly Color[] texture2DPixels;
        private readonly int width;



        public EditableTexture2D(GraphicsDevice graphicsDevice, int width, int height)
        {
            this.graphicsDevice = graphicsDevice;
            this.width = width;
            this.height = height;

            texture2DPixels = new Color[width*height];
        }



        public EditableTexture2D(Texture2D texture2D)
            : this(texture2D.GraphicsDevice, texture2D.Width, texture2D.Height)
        {
            texture2D.GetData(texture2DPixels);
        }



        public Color this[int x, int y]
        {
            get { return texture2DPixels[y*width + x]; }
            set { texture2DPixels[y*width + x] = value; }
        }



        public int Width
        {
            get { return width; }
        }



        public int Height
        {
            get { return height; }
        }



        public GraphicsDevice GraphicsDevice
        {
            get { return graphicsDevice; }
        }



        public Texture2D ToTexture2D()
        {
            Texture2D texture2D = new Texture2D(graphicsDevice, width, height);
            texture2D.SetData(texture2DPixels);

            return texture2D;
        }
    }
}