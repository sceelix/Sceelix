using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.ProjectExplorer.GUI;
using Sceelix.Designer.ProjectExplorer.Management;

namespace Sceelix.Designer.ProjectExplorer.FileHandlers
{
    public abstract class DefaultFileHandler : IFileHandler
    {

        public abstract string ItemName
        {
            get;
        }



        public abstract IEnumerable<string> Extensions
        {
            get;
        }



        public abstract Texture2D Icon16x16
        {
            get;
        }



        public abstract DocumentControl DocumentControl
        {
            get;
        }


        public abstract Guid? GetGuid(FileItem fileItem);



        public virtual void Duplicate(FileItem fileItem, FileItem duplicateFileItem)
        {
            File.Copy(fileItem.FullPath, duplicateFileItem.FullPath);
        }
    }
}