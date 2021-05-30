using System.IO;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Extensions;
using Sceelix.Designer.ProjectExplorer.GUI;

namespace Sceelix.Designer.ProjectExplorer.FileHandlers.Images
{
    public class ImageDocumentControl : DocumentControl
    {


        protected override void OnFirstLoad()
        {
            var graphicsDevice = Screen.Renderer.GraphicsDevice;

            using (var fileStream = new FileStream(FileItem.FullPath, FileMode.Open))
            {
                Texture2D texture2D = Texture2D.FromStream(graphicsDevice, fileStream);

                var data = new EditableTexture2D(texture2D);
                var image = new Image
                {
                    Texture = texture2D,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Foreground = Color.White
                    //Height = 300,
                    //Width = 300
                };

                var scrollViewer = new ScrollViewer
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Content = image
                };

                Content = scrollViewer;
            }
        }



        private void TextBoxOnPropertyChanged(object sender, GamePropertyEventArgs gamePropertyEventArgs)
        {
            //if(gamePropertyEventArgs.Property.Name == "Text")
            //    AlertFileChange();
        }
    }
}