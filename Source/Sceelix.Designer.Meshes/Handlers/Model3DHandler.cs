using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Meshes.Controls;
using Sceelix.Designer.ProjectExplorer.FileHandlers;
using Sceelix.Designer.ProjectExplorer.FileHandlers.Text;
using Sceelix.Designer.ProjectExplorer.GUI;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Meshes.Handlers
{
    [FileHandler]
    public class Model3DHandler : DefaultFileHandler
    {
        public override string ItemName
        {
            get { return "3D Model"; }
        }



        public override IEnumerable<string> Extensions
        {
            get
            {
                yield return ".3ds";
                yield return ".ase";
                yield return ".dxf";
                yield return ".fbx";
                yield return ".blend";
                yield return ".dae";
                yield return ".lwo";
                yield return ".ms3d";
                yield return ".lxo";
                yield return ".stl";
                yield return ".obj";
                yield return ".x";
            }
        }



        public override Texture2D Icon16x16
        {
            get { return EmbeddedResources.Load<Texture2D>("Resources/Cube_16x16.png", GetType().Assembly); }
        }



        public override DocumentControl DocumentControl
        {
            get { return new ModelViewerDocumentControl(); }
        }



        public override Guid? GetGuid(FileItem fileItem)
        {
            return null;
        }
    }
}