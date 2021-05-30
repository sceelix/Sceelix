using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Linq;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Annotations;
using Sceelix.Designer.Annotations;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.ProjectExplorer.GUI;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.ProjectExplorer.FileHandlers
{
    [DesignerService]
    public class FileHandlerManager : IServiceable
    {
        private List<IFileHandler> _fileHandlers = new List<IFileHandler>();
        private List<IFileCreator> _fileCreators = new List<IFileCreator>();

        private IServiceLocator _services;


        public void Initialize(IServiceLocator services)
        {
            _services = services;

            _fileHandlers = AttributeReader.OfAttribute<FileHandlerAttribute>().GetInstancesOfType<IFileHandler>();
            _fileHandlers.OfType<IServiceable>().ForEach(x => x.Initialize(services));

            _fileCreators = AttributeReader.OfAttribute<FileCreatorAttribute>().GetInstancesOfType<IFileCreator>();
        }


        /// <summary>
        /// Provides the file extensions that can be loaded, viewed or manipulated.
        /// </summary>
        /// <returns></returns>
        public String[] GetSupportedFileExtensions()
        {
            return _fileHandlers.SelectMany(val => val.Extensions).ToArray();
        }



        /// <summary>
        /// Gets the file handler of the given file.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public IFileHandler GetFileHandler(FileItem item)
        {
            return _fileHandlers.FirstOrDefault(val => val.Extensions.Contains(item.Extension));
        }



        /// <summary>
        /// Gets the file editor of the given file.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public DocumentControl GetDocumentControlForFile(FileItem item, DocumentControl defaultControl = null)
        {
            IFileHandler fileHandler = GetFileHandler(item);
            var documentControl = fileHandler == null ? defaultControl : fileHandler.DocumentControl;
            if (documentControl != null)
            {
                documentControl.FileItem = item;
                
                if(documentControl is IServiceable)
                    ((IServiceable)documentControl).Initialize(_services);

                documentControl.ReviewFormName();
            }

            return documentControl;
        }



        /// <summary>
        /// Gets the icon to be displayed for this file in the file tree.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Texture2D GetIconForFile(FileItem item)
        {
            IFileHandler fileHandler = GetFileHandler(item);
            if (fileHandler != null)
                return fileHandler.Icon16x16;


            return EmbeddedResources.Load<Texture2D>("Resources/document new.png");
        }



        /// <summary>
        /// Checks if there is a file handler for the file.
        /// </summary>
        /// <param name="extension">Extension of the file, with the ".".</param>
        /// <returns></returns>
        public bool IsSupported(string extension)
        {
            return _fileHandlers.Any(val => val.Extensions.Contains(extension));
        }

        

        public IEnumerable<IFileCreator> FileCreators
        {
            get { return _fileCreators; }
        }



        public List<IFileHandler> FileHandlers
        {
            get { return _fileHandlers; }
        }
    }
}