using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.ProjectExplorer.Management;

namespace Sceelix.Designer.ProjectExplorer.FileHandlers
{
    public class FileCreatorAttribute : FileHandlerAttribute
    {
    }

    public interface IFileCreator
    {
        String Description
        {
            get;
        }


        String ItemName
        {
            get;
        }


        Texture2D Icon48X48
        {
            get;
        }



        String Category
        {
            get;
        }


        String Extension
        {
            get;
        }


        void CreatePhysicalFile(FileItem fileItem);


        Guid? GetGuid(FileItem fileItem);
    }
}