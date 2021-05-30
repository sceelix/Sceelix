using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ionic.Zip;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.ProjectExplorer.FileHandlers;
using Sceelix.Designer.ProjectExplorer.GUI;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Unity3D.GUI
{
    #if DEBUG
    [FileCreator]
    #endif
    public class UnityExportConfigurationFile : DefaultFileHandler, IFileCreator
    {
        public override string ItemName
        {
            get { return "Unity Export Configuration"; }
        }

        public override IEnumerable<string> Extensions
        {
            get { yield return ".slxu"; }
        }

        public string Extension
        {
            get { return ".slxu"; }
        }



        public override Texture2D Icon16x16
        {
            get { return EmbeddedResources.Load<Texture2D>("Resources/Game_16x16.png", this.GetType().Assembly); }
        }



        public override DocumentControl DocumentControl
        {
            get { return new UnityExportConfigurationDocumentControl(); }
        }



        public string Description
        {
            get { return "A file to configure an export to Unity."; }
        }



        public Texture2D Icon48X48
        {
            get { return EmbeddedResources.Load<Texture2D>("Resources/Game_48x48.png", this.GetType().Assembly); }
        }



        public string Category
        {
            get { return "Game Engines"; }
        }



        public override Guid? GetGuid(FileItem fileItem)
        {
            return null;
        }



        public void CreatePhysicalFile(FileItem fileItem)
        {
            File.Create(fileItem.FullPath);
        }
    }
}