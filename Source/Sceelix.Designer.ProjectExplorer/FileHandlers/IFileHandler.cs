using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.ProjectExplorer.GUI;
using Sceelix.Designer.ProjectExplorer.Management;

namespace Sceelix.Designer.ProjectExplorer.FileHandlers
{
    public class FileHandlerAttribute : Attribute
    {
        
    }

    public interface IFileHandler
    {
        String ItemName
        {
            get;
        }



        IEnumerable<string> Extensions
        {
            get;
        }



        Texture2D Icon16x16
        {
            get;
        }



        DocumentControl DocumentControl
        {
            get;
        }



        Guid? GetGuid(FileItem fileItem);



        void Duplicate(FileItem fileItem, FileItem duplicateFileItem);
    }
}