using System;
using System.Drawing;
using System.Drawing.Imaging;
using DigitalRune.Graphics;
using DigitalRune.Threading;
using ImageProcessor.Imaging;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Services;
using Sceelix.Helpers;

namespace Sceelix.Designer.Utils
{
    public static class GDIExtender
    {
        /*unsafe public class FastBitmap
        {
            private struct PixelData
            {
                public byte blue;
                public byte green;
                public byte red;
                public byte alpha;

                public override string ToString()
                {
                    return "(" + alpha.ToString() + ", " + red.ToString() + ", " + green.ToString() + ", " + blue.ToString() + ")";
                }
            }

            private Bitmap workingBitmap = null;
            private int width = 0;
            private BitmapData bitmapData = null;
            private Int64* pBase = null;

            public FastBitmap(Bitmap inputBitmap)
            {
                workingBitmap = inputBitmap;
            }

            public void LockImage()
            {
                Rectangle bounds = new Rectangle(Point.Empty, workingBitmap.Size);

                width = (int)(bounds.Width * sizeof(PixelData));
                if (width % 4 != 0) width = 4 * (width / 4 + 1);

                //Lock Image
                bitmapData = workingBitmap.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                pBase = (Int64*)bitmapData.Scan0.ToPointer();
            }

            private PixelData* pixelData = null;

            public Color GetPixel(int x, int y)
            {
                pixelData = (PixelData*)(pBase + y * width + x * sizeof(PixelData));
                return Color.FromArgb(pixelData->alpha, pixelData->red, pixelData->green, pixelData->blue);
            }

            public Color GetPixelNext()
            {
                pixelData++;
                return Color.FromArgb(pixelData->alpha, pixelData->red, pixelData->green, pixelData->blue);
            }

            public void SetPixel(int x, int y, Color color)
            {
                PixelData* data = (PixelData*)(pBase + y * width + x * sizeof(PixelData));
                data->alpha = color.A;
                data->red = color.R;
                data->green = color.G;
                data->blue = color.B;
            }

            public void UnlockImage()
            {
                workingBitmap.UnlockBits(bitmapData);
                bitmapData = null;
                pBase = null;
            }
        }*/



        public static Bitmap ToBitmap24(this Texture2D texture2D)
        {
            // This only works for 32bbpARGB for the bitmap and Color for the texture, since these formats match.
            // Because they match, we can simply have Marshal copy over the data, otherwise we'd need to go over
            // each pixel and do the conversion manually (or through some trick I'm unaware off).
            byte[] textureData = new byte[4*texture2D.Width*texture2D.Height];
            texture2D.GetData<byte>(textureData);

            //sawp from BGRA to RGBA
            /*for (int i = 0; i < 4 * texture2D.Width * texture2D.Height; i += 4)
            {
                var temp = textureData[i];

                textureData[i] = textureData[i + 2];
                textureData[i + 2] = temp;
            }*/

            Bitmap bitmap = new Bitmap(texture2D.Width, texture2D.Height, PixelFormat.Format24bppRgb);

            BitmapData bmpData = bitmap.LockBits(
                new Rectangle(0, 0, texture2D.Width, texture2D.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb
            );

            IntPtr safePtr = bmpData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(textureData, 0, safePtr, textureData.Length);
            bitmap.UnlockBits(bmpData);

            return bitmap;
        }



        public static Bitmap ToBitmap(this Texture2D texture2D)
        {
            // This only works for 32bbpARGB for the bitmap and Color for the texture, since these formats match.
            // Because they match, we can simply have Marshal copy over the data, otherwise we'd need to go over
            // each pixel and do the conversion manually (or through some trick I'm unaware off).
            byte[] textureData = new byte[4*texture2D.Width*texture2D.Height];
            texture2D.GetData<byte>(textureData);

            //sawp from BGRA to RGBA
            for (int i = 0; i < 4*texture2D.Width*texture2D.Height; i += 4)
            {
                var temp = textureData[i];

                textureData[i] = textureData[i + 2];
                textureData[i + 2] = temp;
            }

            Bitmap bitmap = new Bitmap(
                texture2D.Width, texture2D.Height,
                PixelFormat.Format32bppArgb
            );

            BitmapData bmpData = bitmap.LockBits(
                new Rectangle(0, 0, texture2D.Width, texture2D.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb
            );

            IntPtr safePtr = bmpData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(textureData, 0, safePtr, textureData.Length);
            bitmap.UnlockBits(bmpData);

            // just some test output
            //bmp.Save(@"c:\workbench\smile.bmp", System.Drawing.Imaging.ImageFormat.Bmp);


/*Bitmap bitmap = new Bitmap(texture2D.Width, texture2D.Height);
                        Color[] texturePixels = new Color[texture2D.Width * texture2D.Height];
                        texture2D.GetData(0, null, texturePixels, 0, texture2D.Width * texture2D.Height);
            
                        FastBitmap fastBitmap = new FastBitmap(bitmap);
                        fastBitmap.LockImage();
            
                        for (int i = 0; i < texture2D.Width; i++)
                        {
                            for (int j = 0; j < texture2D.Height; j++)
                            {
                                Color color = texturePixels[i + texture2D.Width * j];
                                fastBitmap.SetPixel(i, j, color.ToGDIColor());
            
                                //bitmap.SetPixel(i, j, GDIColor.FromArgb(color.A, color.R, color.G, color.B));
                            }
                        }
            
                        fastBitmap.UnlockImage();*/

            return bitmap;
        }



        public static Texture2D ToTexture2D(this Bitmap bitmap, GraphicsDevice graphicsDevice)
        {
            //reads the image data (which is a one-dimensional array)
            Microsoft.Xna.Framework.Color[] texturePixels = new Microsoft.Xna.Framework.Color[bitmap.Width*bitmap.Height];

            using (FastBitmap fastBitmap = new FastBitmap(bitmap))
            {
                ParallelHelper.For(0, fastBitmap.Width, (i) =>
                {
                    for (int j = 0; j < fastBitmap.Height; j++)
                    {
                        //GDIColor color = bitmap.GetPixel(i, j);
                        Color color = fastBitmap.GetPixel(i, j);
                        texturePixels[i + fastBitmap.Width*j] = new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);
                    }
                });
            }

            Texture2D texture2D = new Texture2D(graphicsDevice, bitmap.Width, bitmap.Height);
            texture2D.SetData(texturePixels);

            return texture2D;
        }



        public static Microsoft.Xna.Framework.Color[] ToColorArray(this Bitmap bitmap)
        {
            //reads the image data (which is a one-dimensional array)
            Microsoft.Xna.Framework.Color[] texturePixels = new Microsoft.Xna.Framework.Color[bitmap.Width*bitmap.Height];

            using (FastBitmap fastBitmap = new FastBitmap(bitmap))
            {
                ParallelHelper.For(0, fastBitmap.Width, (i) =>
                {
                    for (int j = 0; j < fastBitmap.Height; j++)
                    {
                        //GDIColor color = bitmap.GetPixel(i, j);
                        Color color = fastBitmap.GetPixel(i, j);
                        texturePixels[i + fastBitmap.Width*j] = new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);
                    }
                });
            }

            return texturePixels;
        }

        /// <summary>
        /// Converts the the bitmap into a array of float values in the [0-1] range. Takes the Red values only.
        /// Useful assuming a grayscale image.
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static float[,] ToFloatArray(this Bitmap bitmap)
        {
            //reads the image data (which is a one-dimensional array)
            float[,] texturePixels = new float[bitmap.Width,bitmap.Height];

            using (FastBitmap fastBitmap = new FastBitmap(bitmap))
            {
                ParallelHelper.For(0, fastBitmap.Width, (i) =>
                {
                    for (int j = 0; j < fastBitmap.Height; j++)
                    {
                        //GDIColor color = bitmap.GetPixel(i, j);
                        Color color = fastBitmap.GetPixel(i, j);
                        texturePixels[i, j] = color.A / 255f;
                    }
                });
            }

            return texturePixels;
        }



        public static Texture2D ToTexture2D(this Image bitmap, GraphicsDevice graphicsDevice)
        {
            //reads the image data (which is a one-dimensional array)
            Microsoft.Xna.Framework.Color[] texturePixels = new Microsoft.Xna.Framework.Color[bitmap.Width*bitmap.Height];

            using (ImageProcessor.Imaging.FastBitmap fastBitmap = new ImageProcessor.Imaging.FastBitmap(bitmap))
            {
                var width = fastBitmap.Width;

                ParallelHelper.For(0, fastBitmap.Height, y =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        Color color = fastBitmap.GetPixel(x, y);
                        texturePixels[x + width*y] = new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);
                    }
                });
            }

            Texture2D texture2D = new Texture2D(graphicsDevice, bitmap.Width, bitmap.Height);
            texture2D.SetData(texturePixels);

            return texture2D;
        }



        public static Texture2D ToTexture2D(this Bitmap bitmap, IServiceLocator services)
        {
            IGraphicsService graphicsService = services.Get<IGraphicsService>();

            return ToTexture2D(bitmap, graphicsService.GraphicsDevice);
        }



        public static Point ToXNAPoint(this Point gdiPoint)
        {
            return new Point(gdiPoint.X, gdiPoint.Y);
        }



        public static Point ToGdiPoint(this Point xnaPoint)
        {
            return new Point(xnaPoint.X, xnaPoint.Y);
        }



        public static Rectangle ToXNARectangle(this Rectangle gdiRectangle)
        {
            return new Rectangle(gdiRectangle.X, gdiRectangle.Y, gdiRectangle.Width, gdiRectangle.Height);
        }



        public static Rectangle ToGDIRectangle(this Rectangle xnaRectangle)
        {
            return new Rectangle(xnaRectangle.X, xnaRectangle.Y, xnaRectangle.Width, xnaRectangle.Height);
        }



        public static Microsoft.Xna.Framework.Color ToXNAColor(this Color gdiColor)
        {
            return new Microsoft.Xna.Framework.Color(gdiColor.R, gdiColor.G, gdiColor.B, gdiColor.A);
        }



        public static Color ToGDIColor(this Microsoft.Xna.Framework.Color xnaColor)
        {
            return Color.FromArgb(xnaColor.A, xnaColor.R, xnaColor.G, xnaColor.B);
        }
    }
}