using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.ProjectExplorer.FileHandlers;
using Sceelix.Designer.ProjectExplorer.FileHandlers.Text;
using Sceelix.Designer.ProjectExplorer.GUI;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Meshes.Handlers
{
    [FileHandler]
    public class ObjMtlHandler : DefaultFileHandler
    {
        public override string ItemName
        {
            get { return "OBJ Material"; }
        }



        public override IEnumerable<string> Extensions
        {
            get { yield return ".mtl"; }
        }



        public override Texture2D Icon16x16
        {
            get { return EmbeddedResources.Load<Texture2D>("Resources/Attach_16x16.png", GetType().Assembly); }
        }



        public override DocumentControl DocumentControl
        {
            get { return new TextEditorDocumentControl(); }
        }



        public override Guid? GetGuid(FileItem fileItem)
        {
            return null;
        }
    }
}