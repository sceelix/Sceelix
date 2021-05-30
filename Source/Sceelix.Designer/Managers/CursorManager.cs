using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Helpers;
using Sceelix.Designer.Utils;
using Sceelix.Extensions;

namespace Sceelix.Designer.Managers
{
    public class CursorManager
    {
        private readonly Game _game;
        private readonly IGraphicsService _graphicsService;
        private readonly IUIService _uiService;

        private readonly List<CursorInfo> _cursorInfos = new List<CursorInfo>();

        private Cursor _cursor = null;

#if MACOS
        private Texture2D _cursorSprite;
        private Vector2 _cursorSpriteOffset;
        #endif



        public CursorManager(Game game, IGraphicsService graphicsService, IUIService uiService, List<ThemeCursor> themeCursors)
        {
            _game = game;
            _graphicsService = graphicsService;
            _uiService = uiService;

            if (BuildPlatform.IsMacOS)
            {
                ReadCursorXML(themeCursors);
                game.IsMouseVisible = false;
            }

            _cursor = GetCursor();
            if (_cursor != null)
                SetCursor(_cursor);
        }


        private void WriteCursorXML(List<ThemeCursor> themeCursors)
        {
            XmlWriter writer = XmlWriter.Create("E:\\Desktop\\CursorHotSpots.xml", new XmlWriterSettings() {Indent = true});

            writer.WriteStartDocument();
            writer.WriteStartElement("CursorHotSpots");
            foreach (ThemeCursor themeCursor in themeCursors)
            {
                var cursor = (Cursor) themeCursor.Cursor;

                writer.WriteStartElement("CursorHotSpot");
                writer.WriteAttributeString("Name", themeCursor.Name);
                writer.WriteAttributeString("Value", cursor.HotSpot.X + "," + cursor.HotSpot.Y);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }



        private void ReadCursorXML(List<ThemeCursor> themeCursors)
        {
            XmlDocument doc = new XmlDocument(); // { PreserveWhitespace = true}
            doc.LoadXml(EmbeddedResources.Load<String>("Resources/CursorHotSpots.xml"));

            XmlElement root = doc.DocumentElement;
            XmlNodeList nodeList = root.GetElementsByTagName("CursorHotSpot");
            foreach (XmlElement xmlNode in nodeList)
            {
                var name = xmlNode.GetAttributeOrDefault<String>("Name");
                var positionArray = xmlNode.GetAttributeOrDefault<String>("Value").Split(',');
                var offset = new Vector2(Single.Parse(positionArray[0]), Single.Parse(positionArray[1]));

                var cursor = (Cursor) themeCursors.First(x => x.Name == name).Cursor;
                _cursorInfos.Add(new CursorInfo() {Cursor = cursor, Name = name, Offset = offset});
            }
        }







        private void SetCursor(Cursor cursor)
        {
            Bitmap bitmap = new Bitmap(cursor.Size.Width, cursor.Size.Height, PixelFormat.Format32bppArgb);

            using (Graphics gr = Graphics.FromImage(bitmap))
            {
                gr.Clear(System.Drawing.Color.Transparent);
                //gr.DrawImage(bitmap, new Rectangle(0, 0, clone.Width, clone.Height));
                cursor.Draw(gr, new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height));
            }

#if LINUX

            var nativeForm = _game.Window.GetForm();

            //bitmap.Save(@"E:\Desktop\Coiso.png", ImageFormat.Png);
            //Icon icon = Icon.FromHandle(cursor.Handle);
            //using (Bitmap bitmap = icon.ToBitmap())
            {
                var data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);

                nativeForm.Cursor = new OpenTK.MouseCursor(cursor.HotSpot.X, cursor.HotSpot.Y, cursor.Size.Width, cursor.Size.Height, data.Scan0);
            }
            #elif MACOS

            _cursorSprite = bitmap.ToTexture2D(_graphicsService.GraphicsDevice);
            
            var selectedCursor = _cursorInfos.First(x => x.Cursor.Equals(cursor));
            _cursorSpriteOffset = selectedCursor.Offset;

            #endif
        }



        public Cursor GetCursor()
        {
            var desiredCursor = _uiService.Cursor;

            if (desiredCursor == null)
            {
                // Search screens and check if the control under the mouse wants a special cursor.
                foreach (var screen in _uiService.Screens)
                {
                    if (screen.IsEnabled && screen.IsVisible)
                    {
                        // Search for Cursor beginning at ControlUnderMouse up the control hierarchy.
                        var control = screen.ControlUnderMouse;
                        while (control != null)
                        {
                            if (control.Cursor != null)
                            {
                                desiredCursor = control.Cursor;
                                break;
                            }
                            control = control.VisualParent;
                        }
                    }
                }
            }

            if (desiredCursor == null)
            {
                // Search for a default cursor in screens.
                foreach (var screen in _uiService.Screens)
                {
                    if (screen.Renderer != null && screen.IsEnabled && screen.IsVisible)
                    {
                        desiredCursor = screen.Renderer.GetCursor(null);
                        if (desiredCursor != null)
                            break;
                    }
                }
            }

            return (Cursor) desiredCursor;
        }



        public void Update(GameTime gameTime)
        {
            var newCursor = GetCursor();

            if (newCursor != null &&
                newCursor != _cursor)
            {
                SetCursor(newCursor);
                _cursor = newCursor;
            }
        }




        
        public void Draw(SpriteBatch spriteBatch, UIScreen uiScreen)
        {
            #if MACOS
            //_uiService.InputService.MousePosition
            var mousePosition = _uiService.InputService.MousePositionRaw;
            //var bounds = new RectangleF(0, 0, (int)uiScreen.Width, (int)uiScreen.Height);

            if (_cursorSprite != null && _game.Window.Bounds.Contains(new PointF(mousePosition.X, mousePosition.Y)))
            {
                spriteBatch.Draw(_cursorSprite, mousePosition.ToXna() - _cursorSpriteOffset, Microsoft.Xna.Framework.Color.White);
            }
            #endif
        }


        public class CursorInfo
        {
            public string Name
            {
                get;
                set;
            }

            public Vector2 Offset
            {
                get;
                set;
            }

            public object Cursor
            {
                get;
                set;
            }
        }
    }
}