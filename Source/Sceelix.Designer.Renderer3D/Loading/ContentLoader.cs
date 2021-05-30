using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Core.Attributes;
using Sceelix.Core.Resources;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.Services;
using Sceelix.Extensions;

namespace Sceelix.Designer.Renderer3D.Loading
{
    public class ContentItem
    {
        public ContentItem(object content)
        {
            Content = content;
            LoadTime = DateTime.Now;
        }



        public Object Content
        {
            get;
            set;
        }



        public DateTime LoadTime
        {
            get;
            private set;
        }
    }

    public class ContentLoader
    {
        private static readonly Dictionary<String, ContentItem> ContentDictionary = new Dictionary<String, ContentItem>();

        private readonly IServiceLocator _services;
        private readonly IResourceManager _resourceManager;

        private readonly TextureLoader _textureLoader;

        private readonly Dictionary<Object, Object> _assetDictionary = new Dictionary<Object, Object>();

        public ContentLoader(IServiceLocator services, IResourceManager resourceManager)
        {
            _services = services;
            _resourceManager = resourceManager;
            _textureLoader = new TextureLoader(services);
        }



        public TextureLoader TextureLoader
        {
            get { return _textureLoader; }
        }



        public T LoadAsset<T>(Object key, Func<T> creationFunc)
        {
            object value;
            if(!_assetDictionary.TryGetValue(key,out value))
                _assetDictionary.Add(key,value = creationFunc());

            return (T)value;
        }



        /// <summary>
        /// Loads the texture.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public Texture2D LoadTexture(string path)
        {
            ContentItem contentItem;

            if (String.IsNullOrWhiteSpace(path))
                return null;

            //var settings = ServiceLocator.Current.GetInstance<Xna3DViewerSettings>();

            lock (ContentDictionary)
            {
                var fullPath = _resourceManager.GetFullPath(path);

                DateTime lastWriteTime = _resourceManager.GetLastWriteTime(fullPath);
                
                //DateTime lastWriteTime = Uri.IsWellFormedUriString(fullPath, UriKind.RelativeOrAbsolute) && File.Exists(fullPath) ? File.GetLastWriteTime(fullPath) : DateTime.MinValue;

                //ContentItem item;
                
                if (!ContentDictionary.TryGetValue(fullPath, out contentItem) || lastWriteTime > contentItem.LoadTime) // || fullPath.StartsWith(Environment.ResourcePrefix))
                {
                    try
                    {
                        var texture = _textureLoader.LoadTextureFromBitmap(_resourceManager.Load<Bitmap>(path).ConvertFormat(PixelFormat.Format32bppArgb));

                        var content = _textureLoader.BuildMipMapGPUSync(texture);
                        //content = _textureLoader.BuildMipMapGPUExtra(texture);
                        //if (settings.CacheContent)

                        //TODO: try to optimize this
                        //if we don't do this check, we might end up trying to add the same object again
                        //this is due to the fact that the BuildMipMapGPUSync occurs on a later date
                        //if (!ContentDictionary.ContainsKey(path))
                        ContentDictionary[fullPath] = contentItem = new ContentItem(content);
                    }
                    catch (FileNotFoundException fex)
                    {
                        _services.Get<MessageManager>().Publish(new ExceptionThrown(fex));
                    }
                    catch (Exception ex)
                    {
                        _services.Get<MessageManager>().Publish(new ExceptionThrown(new Exception("Could not load file '" + fullPath + "'. It might be invalid or corrupted.", ex)));
                    }
                }


                //return the default pink texture if some exception occured before
                if (contentItem == null)
                    return _textureLoader.CreateColorTexture(Microsoft.Xna.Framework.Color.Fuchsia);
            }


            return (Texture2D) contentItem.Content;
        }



        //I guess this can be made much more generic in the future
        public SoundEffect LoadSound(string path)
        {
            ContentItem contentItem;


            //var settings = ServiceLocator.Current.GetInstance<Xna3DViewerSettings>();

            lock (ContentDictionary)
            {
                var fullPath = _resourceManager.GetFullPath(path);

                if (!ContentDictionary.TryGetValue(fullPath, out contentItem))
                {
                    var stream = _resourceManager.Load<Stream>(fullPath);


                    //FileStream fs = new FileStream(fullPath, FileMode.Open);
                    var content = SoundEffect.FromStream(stream);

                    contentItem = new ContentItem(content);

                    //if (settings.CacheContent)
                    //    _contentDictionary.Add(projectRelativePath, content);

                    //stream.Dispose();
                }
            }

            return (SoundEffect)contentItem.Content;
        }



        /*public static RenderTarget2D ToMipmapedTexture(Texture2D toTexture2D)
        {
            RenderTarget2D target = new RenderTarget2D(toTexture2D.GraphicsDevice, toTexture2D.Width, toTexture2D.Height, true, SurfaceFormat.Color, DepthFormat.None,0,RenderTargetUsage.PreserveContents);
            SpriteBatch sb = new SpriteBatch(toTexture2D.GraphicsDevice);
            toTexture2D.GraphicsDevice.SetRenderTarget(target);

            sb.Begin();
            sb.Draw(toTexture2D, Vector2.Zero, Color.White);
            sb.End();

            toTexture2D.GraphicsDevice.SetRenderTarget(null);

            toTexture2D.Dispose();

            return target;
        }*/



        public static void ClearContent()
        {
            lock (ContentDictionary)
            {
                foreach (object value in ContentDictionary.Values)
                {
                    if (value is Texture2D)
                    {
                        //Problem: we can try to dispose a texture while it is being used!
                        ((Texture2D) value).Dispose();
                    }
                }

                ContentDictionary.Clear();
            }
        }



        public Texture2D LoadColorTexture(Color color)
        {
            return _textureLoader.LoadTextureFromBitmap(BitmapExtension.CreateColorBitmap(color).ConvertFormat(PixelFormat.Format32bppArgb));
        }
    }
}