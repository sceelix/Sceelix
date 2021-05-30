using System;
using System.Collections;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Utils;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaPoint = Microsoft.Xna.Framework.Point;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using GDIColor = System.Drawing.Color;

namespace Sceelix.Designer.Extensions
{
    public enum Texture2DFile
    {
        Png,
        Jpeg
    }

    public static class Texture2DExtender
    {
        /// <summary>
        /// Clones a texture.
        /// </summary>
        /// <param name="texture2D">Texture to be colored</param>
        /// <param name="position"></param>
        /// <returns>Colored texture</returns>
        public static Texture2D Clone(this Texture2D texture2D)
        {
            //starts a new spritebatch and prepares a new rendertarget where it will be used in
            SpriteBatch spriteBatch = new SpriteBatch(texture2D.GraphicsDevice);
            RenderTarget2D renderTarget2D = new RenderTarget2D(texture2D.GraphicsDevice, texture2D.Width, texture2D.Height);
            texture2D.GraphicsDevice.SetRenderTarget(renderTarget2D);

            //sets the background transparent
            texture2D.GraphicsDevice.Clear(XnaColor.Transparent);

            //draws the texture, with the new color, to the rendertarget 
            spriteBatch.Begin();
            spriteBatch.Draw(texture2D, Vector2.Zero, XnaColor.White);
            spriteBatch.End();

            //resets the graphics device to render normally
            texture2D.GraphicsDevice.SetRenderTarget(null);

            return renderTarget2D;
        }



        /// <summary>
        /// Colors a texture of a certain texture
        /// </summary>
        /// <param name="texture2D">Texture to be colored</param>
        /// <param name="position"></param>
        /// <returns>Colored texture</returns>
        public static Texture2D Translate(this Texture2D texture2D, Vector2 position)
        {
            //calculates the final size of the image
            int newWidth = (int) (texture2D.Width + position.X);
            int newHeight = (int) (texture2D.Height + position.Y);

            //starts a new spritebatch and prepares a new rendertarget where it will be used in
            SpriteBatch spriteBatch = new SpriteBatch(texture2D.GraphicsDevice);
            RenderTarget2D renderTarget2D = new RenderTarget2D(texture2D.GraphicsDevice, newWidth, newHeight);
            texture2D.GraphicsDevice.SetRenderTarget(renderTarget2D);

            //sets the background transparent
            texture2D.GraphicsDevice.Clear(XnaColor.Transparent);

            //draws the texture, with the new color, to the rendertarget 
            spriteBatch.Begin();
            spriteBatch.Draw(texture2D, position, XnaColor.White);
            spriteBatch.End();

            //resets the graphics device to render normally
            texture2D.GraphicsDevice.SetRenderTarget(null);

            return renderTarget2D;
        }



        /// <summary>
        /// Colors a texture of a certain texture
        /// </summary>
        /// <param name="texture2D">Texture to be colored</param>
        /// <param name="color">Color to apply</param>
        /// <returns>Colored texture</returns>
        public static Texture2D Color(this Texture2D texture2D, XnaColor color)
        {
            //starts a new spritebatch and prepares a new rendertarget where it will be used in
            SpriteBatch spriteBatch = new SpriteBatch(texture2D.GraphicsDevice);
            RenderTarget2D renderTarget2D = new RenderTarget2D(texture2D.GraphicsDevice, texture2D.Width, texture2D.Height);
            texture2D.GraphicsDevice.SetRenderTarget(renderTarget2D);

            //sets the background transparent
            texture2D.GraphicsDevice.Clear(XnaColor.Transparent);

            //draws the texture, with the new color, to the rendertarget 
            spriteBatch.Begin();
            spriteBatch.Draw(texture2D, Vector2.Zero, color);
            spriteBatch.End();

            //resets the graphics device to render normally
            texture2D.GraphicsDevice.SetRenderTarget(null);

            return renderTarget2D;
        }



        /// <summary>
        /// Colors a texture of a certain texture
        /// </summary>
        /// <param name="texture2D">Texture to be colored</param>
        /// <param name="color">Color to apply</param>
        /// <returns>Colored texture</returns>
        public static Texture2D Color2(this Texture2D texture2D, XnaColor color)
        {
            EditableTexture2D editableTexture2D = new EditableTexture2D(texture2D);
            for (int i = 0; i < editableTexture2D.Width; i++)
            {
                for (int j = 0; j < editableTexture2D.Height; j++)
                {
                    XnaColor xnaColor = editableTexture2D[i, j];
                    byte red = (byte) ((xnaColor.R/255f)*color.R);
                    byte green = (byte) ((xnaColor.G/255f)*color.G);
                    byte blue = (byte) ((xnaColor.B/255f)*color.B);
                    byte alpha = (byte) ((xnaColor.A/255f)*color.A);

                    editableTexture2D[i, j] = new XnaColor(red, green, blue, alpha);
                }
            }

            return editableTexture2D.ToTexture2D();
        }



        /// <summary>
        /// Changes the hue of a certain texture.
        /// </summary>
        /// <param name="texture2D">Texture whose hue is to be changed</param>
        /// <param name="hue">Hue, inthe range 0..360</param>
        /// <returns>Colored texture</returns>
        public static Texture2D ModifyHue(this Texture2D texture2D, int hue)
        {
            EditableTexture2D editableTexture2D = new EditableTexture2D(texture2D);
            for (int i = 0; i < editableTexture2D.Width; i++)
            {
                for (int j = 0; j < editableTexture2D.Height; j++)
                {
                    XnaColor xnaColor = editableTexture2D[i, j];

                    var hsvColor = xnaColor.ColorToHSV();
                    hsvColor[0] = hue;

                    editableTexture2D[i, j] = ColorExtension.HsvToRgb(hsvColor);

                    /*var gdiColor = xnaColor.ToGDIColor();

                    var sat = gdiColor.GetSaturation();
                    var bright = gdiColor.GetBrightness();
                    
                    editableTexture2D[i, j] = HsvToRgb(hue,sat,bright).ToXNAColor();*/
                    //HSLColor gdiColor = 

                    //gdiColor.Hue = hue / 360d;
                    //hue / 360d
                    //System.Drawing.Color changedColor = gdiColor;

                    //editableTexture2D[i, j] = changedColor.ToXNAColor();
                }
            }

            return editableTexture2D.ToTexture2D();
        }



        public static GDIColor HsvToRgb(double h, double s, double v)
        {
            int hi = (int) Math.Floor(h/60.0)%6;
            double f = (h/60.0) - Math.Floor(h/60.0);

            double p = v*(1.0 - s);
            double q = v*(1.0 - (f*s));
            double t = v*(1.0 - ((1.0 - f)*s));

            GDIColor ret;

            switch (hi)
            {
                case 0:
                    ret = GetRgb(v, t, p);
                    break;
                case 1:
                    ret = GetRgb(q, v, p);
                    break;
                case 2:
                    ret = GetRgb(p, v, t);
                    break;
                case 3:
                    ret = GetRgb(p, q, v);
                    break;
                case 4:
                    ret = GetRgb(t, p, v);
                    break;
                case 5:
                    ret = GetRgb(v, p, q);
                    break;
                default:
                    ret = GDIColor.FromArgb(0xFF, 0x00, 0x00, 0x00);
                    break;
            }
            return ret;
        }



        public static GDIColor GetRgb(double r, double g, double b)
        {
            return GDIColor.FromArgb(255, (byte) (r*255.0), (byte) (g*255.0), (byte) (b*255.0));
        }



        /// <summary>
        /// Joins a texture to another
        /// </summary>
        /// <param name="texture2D">First Texture</param>
        /// <param name="texture2DToAdd">Texture to be added on top</param>
        /// <returns>Joined texture</returns>
        public static Texture2D Join(this Texture2D texture2D, Texture2D texture2DToAdd)
        {
            //starts a new spritebatch and prepares a new rendertarget where it will be used in
            SpriteBatch spriteBatch = new SpriteBatch(texture2D.GraphicsDevice);
            RenderTarget2D renderTarget2D = new RenderTarget2D(texture2D.GraphicsDevice, texture2D.Width, texture2D.Height);
            texture2D.GraphicsDevice.SetRenderTarget(renderTarget2D);

            //sets the background transparent
            texture2D.GraphicsDevice.Clear(XnaColor.Transparent);

            //draws the two textures, one after the other, to the rendertarget 
            spriteBatch.Begin();
            spriteBatch.Draw(texture2D, Vector2.Zero, XnaColor.White);
            spriteBatch.Draw(texture2DToAdd, Vector2.Zero, XnaColor.White);
            spriteBatch.End();

            //resets the graphics device to render normally
            texture2D.GraphicsDevice.SetRenderTarget(null);

            return renderTarget2D;
        }



        public static Texture2D Join2(this Texture2D texture2D, Texture2D texture2DToAdd)
        {
            EditableTexture2D editableTexture = new EditableTexture2D(texture2D);
            EditableTexture2D editableTextureTopLayer = new EditableTexture2D(texture2DToAdd);

            for (int i = 0; i < editableTexture.Width; i++)
            {
                for (int j = 0; j < editableTexture.Height; j++)
                {
                    XnaColor dstColor = editableTexture[i, j];
                    XnaColor srcColor = editableTextureTopLayer[i, j];
                    byte red = (byte) (srcColor.R*(srcColor.A/255f) + dstColor.R*(1 - (srcColor.A/255f)));
                    byte green = (byte) (srcColor.G*(srcColor.A/255f) + dstColor.G*(1 - (srcColor.A/255f)));
                    byte blue = (byte) (srcColor.B*(srcColor.A/255f) + dstColor.B*(1 - (srcColor.A/255f)));
                    byte alpha = (byte) (srcColor.A*(srcColor.A/255f) + dstColor.A*(1 - (srcColor.A/255f)));

                    editableTexture[i, j] = new XnaColor(red, green, blue, alpha);
                }
            }

            return editableTexture.ToTexture2D();
        }



        /// <summary>
        /// Joins a texture to another
        /// </summary>
        /// <param name="texture2D">First Texture</param>
        /// <param name="rotation"></param>
        /// <returns>Rotated texture</returns>
        public static Texture2D Rotate(this Texture2D texture2D, float rotation)
        {
            //calculates the final size of the image
            int newWidth = (int) Math.Abs(texture2D.Height*Math.Sin(MathHelper.ToRadians(rotation))) + (int) Math.Abs(texture2D.Width*Math.Cos(MathHelper.ToRadians(rotation)));
            int newHeight = (int) Math.Abs(texture2D.Width*Math.Sin(MathHelper.ToRadians(rotation))) + (int) Math.Abs(texture2D.Height*Math.Cos(MathHelper.ToRadians(rotation)));

            //starts a new spritebatch and prepares a new rendertarget where it will be used in
            SpriteBatch spriteBatch = new SpriteBatch(texture2D.GraphicsDevice);
            RenderTarget2D renderTarget2D = new RenderTarget2D(texture2D.GraphicsDevice, newWidth, newHeight);
            texture2D.GraphicsDevice.SetRenderTarget(renderTarget2D);

            //sets the background transparent
            texture2D.GraphicsDevice.Clear(XnaColor.Transparent);

            //draws the texture, with rotation, to the rendertarget 
            spriteBatch.Begin();
            spriteBatch.Draw(texture2D, Vector2.Zero + new Vector2(newWidth/2f, newHeight/2f), new XnaRectangle(0, 0, texture2D.Width, texture2D.Height),
                XnaColor.White, MathHelper.ToRadians(rotation), new Vector2(texture2D.Width/2f, texture2D.Height/2f), 1, SpriteEffects.None, 0);
            spriteBatch.End();

            //resets the graphics device to render normally
            texture2D.GraphicsDevice.SetRenderTarget(null);

            return renderTarget2D;
        }



        /// <summary>
        /// Resizes a texture
        /// </summary>
        /// <param name="texture2D">First Texture</param>
        /// <param name="newSize"></param>
        /// <returns>Resized texture</returns>
        public static Texture2D Resize(this Texture2D texture2D, XnaPoint newSize)
        {
            //starts a new spritebatch and prepares a new rendertarget where it will be used in
            SpriteBatch spriteBatch = new SpriteBatch(texture2D.GraphicsDevice);
            RenderTarget2D renderTarget2D = new RenderTarget2D(texture2D.GraphicsDevice, newSize.X, newSize.Y);
            texture2D.GraphicsDevice.SetRenderTarget(renderTarget2D);

            //sets the background transparent
            texture2D.GraphicsDevice.Clear(XnaColor.Transparent);

            //draws the texture, with rotation, to the rendertarget 
            spriteBatch.Begin();
            spriteBatch.Draw(texture2D, new XnaRectangle(0, 0, newSize.X, newSize.Y),
                new XnaRectangle(0, 0, texture2D.Width, texture2D.Height), XnaColor.White);
            spriteBatch.End();

            //resets the graphics device to render normally
            texture2D.GraphicsDevice.SetRenderTarget(null);

            return renderTarget2D;
        }



        public static Texture2D[] DivideX(this Texture2D texture2D, int times)
        {
            int tileSize = (texture2D.Width/times);
            Texture2D[] textureTiles = new Texture2D[times];

            //starts a new spritebatch and prepares a new rendertarget where it will be used in
            SpriteBatch spriteBatch = new SpriteBatch(texture2D.GraphicsDevice);

            for (int i = 0; i < times; i++)
            {
                RenderTarget2D renderTarget2D = new RenderTarget2D(texture2D.GraphicsDevice, tileSize, texture2D.Height);
                texture2D.GraphicsDevice.SetRenderTarget(renderTarget2D);

                //sets the background transparent
                texture2D.GraphicsDevice.Clear(XnaColor.Transparent);

                //draws the texture, with rotation, to the rendertarget 
                spriteBatch.Begin();
                spriteBatch.Draw(texture2D, Vector2.Zero, new XnaRectangle(i*tileSize, 0, tileSize, texture2D.Height), XnaColor.White);
                spriteBatch.End();

                textureTiles[i] = renderTarget2D;
            }

            //resets the graphics device to render normally
            texture2D.GraphicsDevice.SetRenderTarget(null);

            return textureTiles;
        }



        public static void SaveToFile(this Texture2D texture2D, String fileName, Texture2DFile fileType)
        {
            FileInfo file = new FileInfo(fileName);
            file.Directory.Create();

            switch (fileType)
            {
                case Texture2DFile.Png:
                    texture2D.SaveAsPng(new FileStream(fileName, FileMode.Create, FileAccess.Write), texture2D.Width, texture2D.Height);
                    break;
                case Texture2DFile.Jpeg:
                    texture2D.SaveAsJpeg(new FileStream(fileName, FileMode.Create, FileAccess.Write), texture2D.Width, texture2D.Height);
                    break;
            }
        }



        public static Texture2D PackX(this Texture2D[] texturePack, XnaRectangle instanceBounds, int maxWidth)
        {
            if (texturePack.Length == 0)
                return null;

            //int lineWidth = 0, height = 0, line = 0, column = 0;

            GraphicsDevice graphicsDevice = texturePack[0].GraphicsDevice;
            int columns = maxWidth/instanceBounds.Width;
            int rows = (texturePack.Length/columns) + 1;

            RenderTarget2D renderTarget2D = new RenderTarget2D(graphicsDevice, columns*instanceBounds.Width, rows*instanceBounds.Height);
            SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
            graphicsDevice.SetRenderTarget(renderTarget2D);

            //sets the background transparent
            graphicsDevice.Clear(XnaColor.Transparent);


            //draws the texture, with rotation, to the rendertarget 
            spriteBatch.Begin();

            for (int i = 0; i < texturePack.Length; i++)
            {
                int x = i%columns;
                int y = i/columns;

                spriteBatch.Draw(texturePack[i], new Vector2(x*instanceBounds.Width, y*instanceBounds.Height), XnaColor.White);
            }


            spriteBatch.End();

            //resets the graphics device to render normally
            graphicsDevice.SetRenderTarget(null);

            return renderTarget2D;
        }



        public static Texture2D PackSpriteSheet(this Texture2D texture2D, int originalNumInstances, int maxWidth)
        {
            Texture2D[] texture2Ds = texture2D.DivideX(originalNumInstances);
            return texture2Ds.PackX(texture2Ds[0].Bounds, maxWidth);
        }



        /// <summary>
        /// Resizes a texture
        /// </summary>
        /// <param name="texture2D">First Texture</param>
        /// <param name="newPercentage"></param>
        /// <returns>Resized texture</returns>
        public static Texture2D Resize(this Texture2D texture2D, Vector2 newPercentage)
        {
            return
                texture2D.Resize(new XnaPoint((int) (texture2D.Width*newPercentage.X),
                    (int) (texture2D.Height*newPercentage.Y)));
        }



        private static XnaPoint MaxPoint(IEnumerable points)
        {
            XnaPoint maxPoint = new XnaPoint(0, 0);
            foreach (XnaPoint point in points)
            {
                if (point.X > maxPoint.X)
                    maxPoint.X = point.X;
                if (point.Y > maxPoint.Y)
                    maxPoint.Y = point.Y;
            }

            return maxPoint;
        }



        public static Texture2D Blur(this Texture2D texture2D)
        {
            EditableTexture2D editableTexture2D = new EditableTexture2D(texture2D);

            for (int i = 1; i < editableTexture2D.Width - 1; i++)
            {
                for (int j = 1; j < editableTexture2D.Height - 1; j++)
                {
                    float r, g, b, a;
                    r = g = b = a = 0;

                    for (int k = -1; k <= 1; k++)
                    {
                        for (int l = -1; l <= 1; l++)
                        {
                            XnaColor newColor = editableTexture2D[i + k, j + l];
                            r += newColor.R;
                            g += newColor.G;
                            b += newColor.B;
                            a += newColor.A;
                        }
                    }

                    editableTexture2D[i, j] = new XnaColor((byte) (r/9f), (byte) (g/9f), (byte) (b/9f), (byte) (a/9f));
                }
            }

            return editableTexture2D.ToTexture2D();
        }



        public static Texture2D IntersectOpaque(this Texture2D texture2D, Texture2D newTexture)
        {
            EditableTexture2D editableTexture2D = new EditableTexture2D(texture2D);
            EditableTexture2D newEditableTexture2D = new EditableTexture2D(newTexture);

            for (int i = 0; i < editableTexture2D.Width; i++)
            {
                for (int j = 0; j < editableTexture2D.Height; j++)
                {
                    //#warning What if the texture sizes are different?
                    if (newEditableTexture2D[i, j].A == 0 || newEditableTexture2D[i, j].R < 200)
                        editableTexture2D[i, j] = XnaColor.Transparent;
                }
            }


            return editableTexture2D.ToTexture2D();
        }



        public static Texture2D ExtendRectangleQuad(this Texture2D texture2D, int newWidth, int newHeight)
        {
            if (newHeight < texture2D.Height || newWidth < texture2D.Width)
                throw new ArgumentException("New height and width values cannot be smaller than the original ones.");

            //load the pixel array of the texture
            EditableTexture2D originalTexture = new EditableTexture2D(texture2D);
            EditableTexture2D extendedTexture = new EditableTexture2D(texture2D.GraphicsDevice, newWidth, newHeight);

            int middleLayerX = texture2D.Width/2;
            int middleLayerY = texture2D.Height/2;

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
                    /*else if (j < middleLayerY && (i >= newWidth - middleLayerX))
                    {
                        extendedTexturePixels[j * newWidth + i] = texture2DPixels[j * texture2D.Width + (i - (newWidth - middleLayerX))];
                    }*/
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
                    /*else if (i >= middleLayerX)
                    {
                        extendedTexture[i, j] = extendedTexture[i - 1, j];
                    }*/


                    /*else if(j < middleLayerY && i > middleLayerX)
                    {
                        extendedTexturePixels[j * newWidth + i] = extendedTexturePixels[j * newWidth + (i-1)];
                    }

                    if(j > middleLayerY && j < (newHeight - middleLayerY))
                    {
                        extendedTexturePixels[j*newWidth + i] = extendedTexturePixels[(j-1)*newWidth + i];
                    }*/
                }
            }

            return extendedTexture.ToTexture2D();
        }



        public static Texture2D CreateColorTexture(GraphicsDevice graphicsDevice, Color color,int width = 1, int height = 1)
        {
            var editableTexture = new EditableTexture2D(graphicsDevice, width, height);
            for (int i = 0; i < editableTexture.Width; i++)
            {
                for (int j = 0; j < editableTexture.Height; j++)
                {
                    editableTexture[i, j] = color;
                }
            }

            return editableTexture.ToTexture2D();
        }



        /*public static Bitmap DrawRoundedRectangle2(Bitmap image, GDIColor boxColor, int xPosition, int yPosition,int height, int width, int cornerRadius)
           {
               Bitmap newBitmap = new Bitmap(image, image.Width, image.Height);
               using (Graphics newGraphics = Graphics.FromImage(newBitmap))
               {
                   using (Pen boxPen = new Pen(boxColor))
                   {
                       using (GraphicsPath Path = new GraphicsPath())
                       {
                           Path.AddLine(xPosition + cornerRadius, yPosition, xPosition + width - (cornerRadius * 2), yPosition);
                           Path.AddArc(xPosition + width - (cornerRadius * 2), yPosition, cornerRadius * 2, cornerRadius * 2, 270, 90);
                           Path.AddLine(xPosition + width, yPosition + cornerRadius, xPosition + width, yPosition + height - (cornerRadius * 2));
                           Path.AddArc(xPosition + width - (cornerRadius * 2), yPosition + height - (cornerRadius * 2), cornerRadius * 2, cornerRadius * 2, 0, 90);
                           Path.AddLine(xPosition + width - (cornerRadius * 2), yPosition + height, xPosition + cornerRadius, yPosition + height);
                           Path.AddArc(xPosition, yPosition + height - (cornerRadius * 2), cornerRadius * 2, cornerRadius * 2, 90, 90);
                           Path.AddLine(xPosition, yPosition + height - (cornerRadius * 2), xPosition, yPosition + cornerRadius);
                           Path.AddArc(xPosition, yPosition, cornerRadius * 2, cornerRadius * 2, 180, 90);
                           Path.CloseFigure();
                           newGraphics.DrawPath(boxPen, Path);
                       }
                   }
               }
               return newBitmap;
           }

        private static void DrawRoundedRectangle(Graphics gfx, GDIRectangle bounds, int cornerRadius, Pen drawPen, GDIColor fillColor)
        {
            int strokeOffset = Convert.ToInt32(Math.Ceiling(drawPen.Width));
            bounds = GDIRectangle.Inflate(bounds ,- strokeOffset, -strokeOffset);

            drawPen.EndCap = drawPen.StartCap = LineCap.Round;

            GraphicsPath gfxPath = new GraphicsPath();
            gfxPath.AddArc(bounds.X, bounds.Y, cornerRadius, cornerRadius, 180, 90);
            gfxPath.AddArc(bounds.X + bounds.Width - cornerRadius, bounds.Y, cornerRadius, cornerRadius, 270, 90);
            gfxPath.AddArc(bounds.X + bounds.Width - cornerRadius, bounds.Y + bounds.Height - cornerRadius, cornerRadius, cornerRadius, 0, 90);
            gfxPath.AddArc(bounds.X, bounds.Y + bounds.Height - cornerRadius, cornerRadius, cornerRadius, 90, 90);
            gfxPath.CloseAllFigures();

            //gfx.FillPath(new SolidBrush(fillColor), gfxPath);
            gfx.DrawPath(drawPen, gfxPath);
        }*/
    }
}