using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Core.Environments;
using Sceelix.Core.Graphs;
using Sceelix.Designer.Graphs.Environments;
using Sceelix.Designer.Graphs.GUI;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.ProjectExplorer.FileHandlers;
using Sceelix.Designer.ProjectExplorer.GUI;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Graphs.Handlers
{
    [FileCreator]
    public class GraphCreator : DefaultFileHandler, IFileCreator, IServiceable
    {
        private IServiceLocator _services;


        public void Initialize(IServiceLocator services)
        {
            _services = services;
        }


        public override Guid? GetGuid(FileItem fileItem)
        {
            return GraphLoad.GetGuid(fileItem.FullPath);
        }


        public void CreatePhysicalFile(FileItem fileItem)
        {
            //create an empty graph and save it
            Graph graph = new Graph();
            graph.SaveXML(fileItem.FullPath);
        }


        public override void Duplicate(FileItem fileItem, FileItem duplicateFileItem)
        {
            var procedureEnvironment = new ProcedureEnvironment(new DesignerResourceManager(fileItem.Project, _services));

            var graph = GraphLoad.LoadFromPath(fileItem.FullPath, procedureEnvironment);
            graph.Guid = Guid.NewGuid();
            graph.ResetNodeGuids();

            graph.SaveXML(duplicateFileItem.FullPath);
        }


        public override string ItemName
        {
            get { return "Graph"; }
        }


        public override IEnumerable<string> Extensions
        {
            get { yield return ".slxg"; }
        }


        public string Extension
        {
            get { return ".slxg"; }
        }


        public override Texture2D Icon16x16
        {
            get { return EmbeddedResources.Load<Texture2D>("Resources/Graph16x16.png", GetType().Assembly); }
        }


        public override DocumentControl DocumentControl
        {
            get { return new GraphDocumentControl(); }
        }


        public string Description
        {
            get { return "A graph-based editor for procedural modeling."; }
        }


        public Texture2D Icon48X48
        {
            get { return EmbeddedResources.Load<Texture2D>("Resources/Graph48x48.png", GetType().Assembly); }
        }


        public string Category
        {
            get { return "Graphs"; }
        }
    }
}