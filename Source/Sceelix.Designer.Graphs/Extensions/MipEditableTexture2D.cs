using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sceelix.Designer.Graphs.Extensions
{
    /// <summary>
    /// A texture2D that can be edited.
    /// 
    /// This could inherit from Texture2D, but then every change on the [x,y] would have to set data.
    /// </summary>
    public class MipEditableTexture2D
    {
        private readonly SurfaceFormat _format;
        //private readonly Color[] texture2DPixels;
        private readonly List<EditableSubTexture> _subTextures;
        private readonly GraphicsDevice graphicsDevice;
        private readonly int height;
        private readonly int width;
        private readonly int _levelCount;



        public MipEditableTexture2D(GraphicsDevice graphicsDevice, int width, int height, int levelCount, SurfaceFormat format)
        {
            this.graphicsDevice = graphicsDevice;
            this.width = width;
            this.height = height;
            this._levelCount = levelCount;
            _format = format;

            _subTextures = new List<EditableSubTexture>(levelCount);
            //texture2DPixels = new Color[width * height];
        }



        public MipEditableTexture2D(Texture2D texture2D)
            : this(texture2D.GraphicsDevice, texture2D.Width, texture2D.Height, texture2D.LevelCount, texture2D.Format)
        {
            for (int i = 0; i < texture2D.LevelCount; i++)
            {
                _subTextures.Add(new EditableSubTexture(texture2D, i));
            }
        }



        public Color this[int x, int y, int level]
        {
            get { return _subTextures[level][x, y]; }
            set { _subTextures[level][x, y] = value; }
        }



        public EditableSubTexture this[int level]
        {
            get { return _subTextures[level]; }
        }



        public int Width
        {
            get { return width; }
        }



        public int Height
        {
            get { return height; }
        }



        public int LevelCount
        {
            get { return _levelCount; }
        }



        public GraphicsDevice GraphicsDevice
        {
            get { return graphicsDevice; }
        }



        public Texture2D ToTexture2D()
        {
            Texture2D texture2D = new Texture2D(graphicsDevice, width, height, true, _format);
            for (int i = 0; i < texture2D.LevelCount; i++)
            {
                _subTextures[i].SetData(texture2D, i);
            }
            //texture2D.SetData(texture2DPixels);

            return texture2D;
        }
    }

    public class EditableSubTexture
    {
        private readonly int _height;

        private readonly Color[] _texture2DPixels;
        private readonly int _width;



        public EditableSubTexture(Texture2D texture2D, int level)
        {
            _width = (int) Math.Max(1, texture2D.Width >> level);
            _height = (int) Math.Max(1, texture2D.Height >> level);

            int size = _width*_height;

            _texture2DPixels = new Color[size];

            texture2D.GetData(level, null, _texture2DPixels, 0, size);
        }



        public Color this[int x, int y]
        {
            get { return _texture2DPixels[y*_width + x]; }
            set { _texture2DPixels[y*_width + x] = value; }
        }



        public int Width
        {
            get { return _width; }
        }



        public int Height
        {
            get { return _height; }
        }



        public void SetData(Texture2D texture2D, int level)
        {
            texture2D.SetData(level, null, _texture2DPixels, 0, _width*_height);
        }
    }
}