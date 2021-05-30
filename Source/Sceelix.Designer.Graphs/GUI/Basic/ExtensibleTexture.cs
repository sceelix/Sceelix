using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sceelix.Designer.Graphs.GUI.Basic
{
    public class ExtensibleTexture
    {
        private readonly Texture2D _texture2D;



        public ExtensibleTexture(Texture2D texture2D)
        {
            _texture2D = texture2D;
        }



        public void Draw(RectangleF rectangle, Color color)
        {
            if (rectangle.Height < _texture2D.Height || rectangle.Width < _texture2D.Width)
                throw new ArgumentException("New height and width values cannot be smaller than the original ones.");

            /*int middleLayerX = texture2D.Width / 2;
            int middleLayerY = texture2D.Height / 2;

            for (int i = 0; i < newWidth; i++)
            {
                for (int j = 0; j < newHeight; j++)
                {
                    if (j < middleLayerY && i < middleLayerX)
                    {
                        extendedTexture[i, j] = originalTexture[i, j];
                    }
                    else if ((j >= newHeight - middleLayerY) && i < middleLayerX)
                    {
                        extendedTexture[i, j] = originalTexture[i, j - (newHeight - middleLayerY) + middleLayerY];
                    }
                    else if (i < middleLayerX)
                    {
                        extendedTexture[i, j] = extendedTexture[i, j - 1];
                    }
                    else if ((i >= newWidth - middleLayerX) && (j < middleLayerY))
                    {
                        extendedTexture[i, j] = originalTexture[i - (newWidth - middleLayerX) + middleLayerX, j];
                    }
                    else if ((i >= newWidth - middleLayerX) && (j >= newHeight - middleLayerY))
                    {
                        extendedTexture[i, j] = originalTexture[i - (newWidth - middleLayerX) + middleLayerX, j - (newHeight - middleLayerY) + middleLayerY];
                    }
                    else if ((i >= newWidth - middleLayerX))
                    {
                        extendedTexture[i, j] = extendedTexture[i, j - 1];
                    }
                    else if (i >= middleLayerX)
                    {
                        extendedTexture[i, j] = extendedTexture[i - 1, j];
                    }
                }
            }

            return extendedTexture.ToTexture2D();*/
        }
    }
}