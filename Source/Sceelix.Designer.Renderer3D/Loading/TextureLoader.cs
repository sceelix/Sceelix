using System;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Sceelix.Designer.Renderer3D.Loading
{
    /// <summary>
    /// Based on http://jakepoz.com/jake_poznanski__background_load_xna.html 
    /// </summary>
    public class TextureLoader
    {
        private readonly BlendState BlendAlphaBlendState;
        private readonly BlendState BlendColorBlendState;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatch _spriteBatch;
        //private bool _needsBmp;

        private readonly Synchronizer _synchronizer;



        internal TextureLoader(IServiceLocator services)
        {
            BlendColorBlendState = new BlendState
            {
                ColorDestinationBlend = Blend.Zero,
                ColorWriteChannels = ColorWriteChannels.Red | ColorWriteChannels.Green | ColorWriteChannels.Blue,
                AlphaDestinationBlend = Blend.Zero,
                AlphaSourceBlend = Blend.SourceAlpha,
                ColorSourceBlend = Blend.SourceAlpha
            };

            BlendAlphaBlendState = new BlendState
            {
                ColorWriteChannels = ColorWriteChannels.Alpha,
                AlphaDestinationBlend = Blend.Zero,
                ColorDestinationBlend = Blend.Zero,
                AlphaSourceBlend = Blend.One,
                ColorSourceBlend = Blend.One
            };

            _graphicsDevice = services.Get<IGraphicsDeviceService>().GraphicsDevice;
            _spriteBatch = new SpriteBatch(_graphicsDevice);
            _synchronizer = services.Get<Synchronizer>();
        }

        


        public Texture2D LoadTextureFromBitmap(Bitmap image)
        {
            if (BuildPlatform.IsMacOS)
            {
                var colorArray = image.ToColorArray();

                var texture = (Texture2D)_synchronizer.EnqueueAndWait(delegate
                {
                    Texture2D texture2D = new Texture2D(_graphicsDevice, image.Width, image.Height);
                    texture2D.SetData(colorArray);
                    return texture2D;
                });

                return texture;

            }

            return image.ToTexture2D(_graphicsDevice);
        }

        


        public Texture2D MipMapAndPremultiplyAlpha(Texture2D texture, bool preMultiplyAlpha = true)
        {
            if (preMultiplyAlpha)
            {
                // Setup a render target to hold our final texture which will have premulitplied alpha values
                using (RenderTarget2D renderTarget = new RenderTarget2D(_graphicsDevice, texture.Width, texture.Height, true, SurfaceFormat.Color, DepthFormat.None))
                {
                    Viewport viewportBackup = _graphicsDevice.Viewport;
                    _graphicsDevice.SetRenderTarget(renderTarget);
                    _graphicsDevice.Clear(Color.White);

                    // Multiply each color by the source alpha, and write in just the color values into the final texture
                    _spriteBatch.Begin(SpriteSortMode.Immediate, BlendColorBlendState);
                    _spriteBatch.Draw(texture, texture.Bounds, Color.White);
                    _spriteBatch.End();

                    // Now copy over the alpha values from the source texture to the final one, without multiplying them
                    _spriteBatch.Begin(SpriteSortMode.Immediate, BlendAlphaBlendState);
                    _spriteBatch.Draw(texture, texture.Bounds, Color.White);
                    _spriteBatch.End();
                    /*_spriteBatch.Begin();
                    _spriteBatch.Draw(texture, viewportBackup.Bounds, Color.White);
                    _spriteBatch.End();*/

                    // Release the GPU back to drawing to the screen
                    _graphicsDevice.SetRenderTarget(null);
                    _graphicsDevice.Viewport = viewportBackup;

                    _graphicsDevice.Textures[0] = null;

                    texture = new Texture2D(texture.GraphicsDevice, texture.Width, texture.Height, true, SurfaceFormat.Color);

                    Color[] data = new Color[texture.Width*texture.Height];

                    //Fixed through this
                    //http://www.gamedev.net/topic/636883-xna-generate-mip-maps-with-texturefromstream/#entry5018728
                    //http://www.gamedev.net/topic/598594-xna-4-texture2dgeneratemipmaps-missing/
                    for (int i = 0; i < renderTarget.LevelCount; i++)
                    {
                        int mipWidth = (int) Math.Max(1, texture.Width >> i);
                        int mipHeight = (int) Math.Max(1, texture.Height >> i);
                        int size = mipWidth*mipHeight;

                        renderTarget.GetData<Color>(i, null, data, 0, size);
                        texture.SetData<Color>(i, null, data, 0, size);
                    }

                    //Bitmap bitmap = renderTarget.ToBitmap();
                    //bitmap.Save(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Printscreen " + DateTime.Now.ToFileTimeUtc() + ".png"));
                }
            }

            return texture;
        }



        static Int32 LowerPowerof2(Int32 num)
        {
            Int32 n = num > 0 ? num - 1 : 0;

            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            n++;

            return n;
        }



        Int32 LowerPowerof22(Int32 x)
        {
            x = x | (x >> 1);
            x = x | (x >> 2);
            x = x | (x >> 4);
            x = x | (x >> 8);
            x = x | (x >> 16);
            return x - (x >> 1);
        }



        public Texture2D BuildMipMapGPUSync(Texture2D texture)
        {
            return (Texture2D) _synchronizer.EnqueueAndWait(() => BuildMipMapGPU(texture));
            //return (Texture2D)_synchronizer.EnqueueAndWait(() => BuildMipMapGPU(texture.Resize(new Microsoft.Xna.Framework.Point(LowerPowerof2(texture.Width), LowerPowerof2(texture.Height)))));

            /*ManualResetEvent mevent = new ManualResetEvent(false);

            Texture2D result = null;
            _synchronizer.Enqueue(
                delegate 
                {
                    result = BuildMipMapGPU(texture);
                    mevent.Set();
                });

            mevent.WaitOne();

            return result;*/
        }



        public Texture2D BuildMipMapGPU(Texture2D texture)
        {
            Viewport viewportBackup = _graphicsDevice.Viewport;

            Texture2D previousTexture = texture;
            Texture2D newTexture = new Texture2D(texture.GraphicsDevice, texture.Width, texture.Height, true, SurfaceFormat.Color);

            Color[] data = new Color[texture.Width*texture.Height];

            for (int i = 0; i < newTexture.LevelCount; i++)
            {
                int mipWidth = (int) Math.Max(1, texture.Width >> i);
                int mipHeight = (int) Math.Max(1, texture.Height >> i);
                int size = mipWidth*mipHeight;


                RenderTarget2D renderTarget = new RenderTarget2D(_graphicsDevice, mipWidth, mipHeight, false, SurfaceFormat.Color, DepthFormat.None);

                _graphicsDevice.SetRenderTarget(renderTarget);
                _graphicsDevice.Clear(Color.Transparent);


                _spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.AnisotropicClamp);
                _spriteBatch.Draw(previousTexture, new Rectangle(0, 0, mipWidth, mipHeight), Color.White);
                _spriteBatch.End();
                // Multiply each color by the source alpha, and write in just the color values into the final texture
                /*_spriteBatch.Begin(SpriteSortMode.Immediate, BlendColorBlendState);
                _spriteBatch.Draw(previousTexture, new Rectangle(0, 0, mipWidth, mipHeight), Color.White);
                _spriteBatch.End();

                // Now copy over the alpha values from the source texture to the final one, without multiplying them
                _spriteBatch.Begin(SpriteSortMode.Immediate, BlendAlphaBlendState);
                _spriteBatch.Draw(previousTexture, new Rectangle(0, 0, mipWidth, mipHeight), Color.White);
                _spriteBatch.End();*/

                _graphicsDevice.SetRenderTarget(null);

                //data = new Color[mipWidth * mipHeight];

                renderTarget.GetData<Color>(0, null, data, 0, size);
                newTexture.SetData<Color>(i, null, data, 0, size);

                /*Texture2D text2d = new Texture2D(tex.GraphicsDevice, mipWidth, mipHeight);
                text2d.SetData(data);*/

                /*Bitmap bitmap = renderTarget.ToBitmap();
                bitmap.Save(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Compare/PrintscreenAlternateBitmap " + i + ".png"));
                */
                _graphicsDevice.SetRenderTarget(null);
                //renderTarget.Dispose();
                previousTexture = renderTarget;
            }

            // Release the GPU back to drawing to the screen            
            _graphicsDevice.Viewport = viewportBackup;

            return newTexture;
        }


        public Texture2D CreateColorTexture(Color color)
        {
            Texture2D tex = new Texture2D(_graphicsDevice, 1, 1);
            tex.SetData(new Color[] { color });

            return tex;
        }

        public static Texture2D CreateColorTexture(GraphicsDevice graphicsDevice, Color color)
        {
            Texture2D tex = new Texture2D(graphicsDevice, 1, 1);
            tex.SetData(new Color[] {color});

            return tex;
        }
    }
}