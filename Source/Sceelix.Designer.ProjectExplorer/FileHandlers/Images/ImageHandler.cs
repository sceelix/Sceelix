using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.ProjectExplorer.GUI;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.ProjectExplorer.FileHandlers.Images
{
    [FileHandler]
    public class ImageHandler : DefaultFileHandler
    {
        public override string ItemName
        {
            get { return "Image"; }
        }



        public override IEnumerable<string> Extensions
        {
            get
            {
                yield return ".bmp";
                yield return ".jpg";
                yield return ".jpeg";
                yield return ".png";
            }
        }



        public override Texture2D Icon16x16
        {
            get { return EmbeddedResources.Load<Texture2D>("Resources/Picture16x16.png", GetType().Assembly); }
        }



        public override DocumentControl DocumentControl
        {
            get { return new ImageDocumentControl(); }
        }



        public override Guid? GetGuid(FileItem fileItem)
        {
            return null;
        }
    }
}