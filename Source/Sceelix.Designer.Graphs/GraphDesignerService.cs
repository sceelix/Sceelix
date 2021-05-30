using System;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Game.UI;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Core.Environments;
using Sceelix.Core.Graphs;
using Sceelix.Designer.Annotations;
using Sceelix.Designer.Graphs.Entities;
using Sceelix.Designer.Graphs.Environments;
using Sceelix.Designer.Graphs.GUI;
using Sceelix.Designer.Graphs.Handlers;
using Sceelix.Designer.Graphs.Tools;
using Sceelix.Designer.GUI.MenuHandling;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Managers;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.Plugins;
using Sceelix.Designer.ProjectExplorer.GUI;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Designer.Services;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Utils;
using Sceelix.Helpers;

namespace Sceelix.Designer.Graphs
{
    [DesignerService]
    public class GraphDesignerService : IServiceable
    {
        private IServiceLocator _services;

        private BarMenuEntry _currentResaveMenu;



        public void Initialize(IServiceLocator services)
        {
            _services = services;

            ProcedureItemLoader.Initialize();

            //services.Get<SettingsManager>().Register("Editors/Graph", new GraphEditorSettings());
            //services.Get<MessageManager>().Register<ProjectItemMoved>(OnFileRenamed);

            services.Get<MessageManager>().Register<ProjectLoaded>((projectLoaded) => UpdateProject(services, projectLoaded));

        #if DEBUG
            services.Get<MessageManager>().Register<ProjectLoaded>((projectLoaded) => RegisterResaveMenu(services, projectLoaded));
            #endif
        }



        



        private void RegisterResaveMenu(IServiceLocator services, ProjectLoaded projectLoaded)
        {
            if(_currentResaveMenu != null)
                services.Get<BarMenuManager>().UnregisterMenuEntry(_currentResaveMenu);

            //services.Get<MessageManager>().Publish(new LogMessageSent("Hello"));
            services.Get<BarMenuManager>().RegisterMenuEntry(_currentResaveMenu = new BarMenuEntry("Graphs/Resave All", () => PerformUpdateProject(projectLoaded.Project)));
        }



        /*private void UpdateGraph(IServiceLocator services, ProjectLoaded projectLoaded)
        {
            var progressWindow = new ProgressWindow(_services, (handler) =>
            {
                GraphCreator creator = new GraphCreator();
                ProcedureEnvironment loadEnvironment = new ProcedureEnvironment(new DesignerResourceManager(projectLoaded.Project, _services));

                foreach (var fileItem in projectLoaded.Project.BaseFolder.GetFileItemTree().Where(x => x.Extension == creator.Extension))
                {
                    if (handler.ShouldCancel)
                        break;

                    handler.SetText("Resaving " + fileItem.ProjectRelativePath);

                    Graph graph = GraphLoad.LoadFromPath(fileItem.FullPath, loadEnvironment);

                    //perform the refactoring, but only save the file if actual references to the folder were found.
                    graph.SaveXML(fileItem.FullPath);
                }
            })
            { LargeIcon = EmbeddedResources.Load<Texture2D>("Resources/Wizard_48x48.png"), Title = "Resaving Files..." };

            progressWindow.Show();
        }*/

        private void UpdateProject(IServiceLocator services, ProjectLoaded projectLoaded)
        {
            if (projectLoaded.Project.Version == "1.0.0.0" || new Version(projectLoaded.Project.Version) < SceelixApplicationInfo.CurrentVersion)
            {
                PerformUpdateProject(projectLoaded.Project);
            }
        }


        private void PerformUpdateProject(Project project)
        {
            var progressWindow = new ProgressWindow((handler) =>
                {
                    GraphCreator creator = new GraphCreator();
                    ProcedureEnvironment loadEnvironment = new ProcedureEnvironment(new DesignerResourceManager(project, _services));
                    
                    foreach (var fileItem in project.BaseFolder.GetFileItemTree().Where(x => x.Extension == creator.Extension))
                    {
                        if (handler.ShouldCancel)
                            break;

                        handler.SetText("Updating " + fileItem.ProjectRelativePath);

                        Graph graph = GraphLoad.LoadFromPath(fileItem.FullPath, loadEnvironment);

                        graph.SaveXML(fileItem.FullPath);
                    }
                })
                { LargeIcon = EmbeddedResources.Load<Texture2D>("Resources/Wizard_48x48.png"), Title = "Updating Project..." };

            progressWindow.Show(_services.Get<WindowAnimator>());
        }


        private void OnFileRenamed(ProjectItemMoved obj)
        {
            var progressWindow = new ProgressWindow((handler) =>
            {
                //determine the project relative paths of both origin and destination
                String originRelativePath = PathHelper.ToUniversalPath(obj.Origin.Substring(obj.Item.Project.BaseFolder.FullPath.Length + 1));
                String destinationRelativePath = PathHelper.ToUniversalPath(obj.Destination.Substring(obj.Item.Project.BaseFolder.FullPath.Length + 1));

                GraphCreator creator = new GraphCreator();
                ProcedureEnvironment loadEnvironment = new ProcedureEnvironment(new DesignerResourceManager(obj.Item.Project, _services));

                //if a folder was moved, we need to change references to folder and to files
                if (obj.Item is FolderItem)
                {
                    foreach (var fileItem in obj.Item.Project.BaseFolder.GetFileItemTree().Where(x => x.Extension == creator.Extension))
                    {
                        handler.SetText("Refactoring " + fileItem.ProjectRelativePath);

                        Graph graph = GraphLoad.LoadFromPath(fileItem.FullPath, loadEnvironment);

                        //perform the refactoring, but only save the file if actual references to the folder were found.
                        if(graph.RefactorReferencedFolder(loadEnvironment, originRelativePath, destinationRelativePath) > 0)
                            graph.SaveXML(fileItem.FullPath);
                    }
                }
                //if a file was moved, we need to change references to files
                else if (obj.Item is FileItem)
                {
                    foreach (var fileItem in obj.Item.Project.BaseFolder.GetFileItemTree().Where(x => x.Extension == creator.Extension))
                    {
                        handler.SetText("Refactoring " + fileItem.ProjectRelativePath);

                        Graph graph = GraphLoad.LoadFromPath(fileItem.FullPath, loadEnvironment);

                        //perform the refactoring, but only save the file if actual references to the file were found.
                        if (graph.RefactorReferencedFile(loadEnvironment, originRelativePath, destinationRelativePath) > 0)
                            graph.SaveXML(fileItem.FullPath);
                    }
                }

            }) {LargeIcon = EmbeddedResources.Load<Texture2D>("Resources/Wizard_48x48.png"), Title = "Refactoring References..."};

            progressWindow.Show(_services.Get<WindowAnimator>());



            //RenameReferencesInGraphs(obj.Item.Project.BaseFolder, originRelativePath, destinationRelativePath);
        }



        /// of the given paths.
        /// Recursively go through the folder tree and performs the appropriate refactoring 

        /// <summary>
        /// </summary>
        /// <param name="baseFolder"></param>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /*private void RenameReferencesInGraphs(FolderItem baseFolder,String origin, String destination)
        {
            GraphCreator creator = new GraphCreator();
            Environment loadEnvironment = new DesignerLoadEnvironment(baseFolder.Project,_services);
            
            foreach (var projectItem in baseFolder.SubItems)
            {
                if(projectItem is FolderItem)
                    RenameReferencesInGraphs((FolderItem)projectItem, origin, destination);
                else if (projectItem is FileItem && ((FileItem) projectItem).Extension == creator.Extension)
                {
                    Graph graph = GraphLoad.LoadFromPath(projectItem.FullPath, loadEnvironment);

                    graph.RefactorReferencedPaths(loadEnvironment,origin, destination);

                    graph.SaveXML(projectItem.FullPath);
                }
            }
        }*/
    }
}