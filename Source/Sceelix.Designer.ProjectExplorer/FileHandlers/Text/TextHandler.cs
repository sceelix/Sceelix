using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.ProjectExplorer.GUI;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.ProjectExplorer.FileHandlers.Text
{
    [FileCreator]
    public class TextHandler : DefaultFileHandler, IFileCreator
    {
        public override string ItemName
        {
            get { return "Text"; }
        }



        public override IEnumerable<String> Extensions
        {
            get
            {
                yield return ".txt";
                yield return ".xml";
                yield return ".json";
                yield return ".ini";
                yield return ".csv";
            }
        }



        public override Texture2D Icon16x16
        {
            get { return EmbeddedResources.Load<Texture2D>("Resources/TextLarge16x16.png", GetType().Assembly); }
        }



        public override DocumentControl DocumentControl
        {
            get { return new TextEditorDocumentControl(); }
        }



        public string Extension
        {
            get { return ".txt"; }
        }



        public string Description
        {
            get { return "A simple text file."; }
        }



        public Texture2D Icon48X48
        {
            get { return EmbeddedResources.Load<Texture2D>("Resources/TextLarge48x48.png", GetType().Assembly); }
        }



        public string Category
        {
            get { return "Essentials"; }
        }



        public void CreatePhysicalFile(FileItem fileItem)
        {
            //create an empty graph and save it
            File.WriteAllText(fileItem.FullPath, "");
        }



        public override Guid? GetGuid(FileItem fileItem)
        {
            return null;
        }
    }
}