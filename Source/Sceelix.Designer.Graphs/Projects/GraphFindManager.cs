using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Storages;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Core.Environments;
using Sceelix.Core.Graphs;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Designer.Graphs.Environments;
using Sceelix.Designer.Graphs.Handlers;
using Sceelix.Designer.GUI.MenuHandling;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Managers;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.Annotations;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;
using Sceelix.Helpers;

namespace Sceelix.Designer.Graphs.Projects
{
#if DEBUG
    [ProjectExplorerService]
#endif
    public class GraphFindManager : IServiceable
    {
        private MessageManager _messageManager;
        private IServiceLocator _services;
        private GraphCreator _graphCreator;



        public void Initialize(IServiceLocator services)
        {
            _services = services;
            _messageManager = services.Get<MessageManager>();
            _graphCreator = new GraphCreator();

            var fileMenuManager = services.Get<ContextMenuManager>("FileProject");

            fileMenuManager.RegisterEntry(new ContextMenuEntry("Find") {Icon = EmbeddedResources.Load<Texture2D>("Resources/Search_16x16.png"), Priority = 100});
            fileMenuManager.RegisterEntry(new ContextMenuEntry("Find/References", FindFileReferences) {Icon = EmbeddedResources.Load<Texture2D>("Resources/Link_16x16.png"), Priority = 101});
            fileMenuManager.RegisterEntry(new ContextMenuEntry("Find/Dependencies", FindDependencies, IsGraphFile) {Icon = EmbeddedResources.Load<Texture2D>("Resources/Tree_16x16.png"), Priority = 101});

            var folderMenuManager = services.Get<ContextMenuManager>("FolderProject");
            folderMenuManager.RegisterEntry(new ContextMenuEntry("Find") {Icon = EmbeddedResources.Load<Texture2D>("Resources/Search_16x16.png"), Priority = 100});
            folderMenuManager.RegisterEntry(new ContextMenuEntry("Find/External References", FindFolderReferences) {Icon = EmbeddedResources.Load<Texture2D>("Resources/Link_16x16.png"), Priority = 101});
            folderMenuManager.RegisterEntry(new ContextMenuEntry("Find/External Dependencies", FindFolderDependencies) {Icon = EmbeddedResources.Load<Texture2D>("Resources/Tree_16x16.png"), Priority = 101});
            folderMenuManager.RegisterEntry(new ContextMenuEntry("Find/External Paths", FindExternalPaths) { Icon = EmbeddedResources.Load<Texture2D>("Resources/Tree_16x16.png"), Priority = 101 });
        }



        private void FindExternalPaths(object data)
        {
            var selectableTreeViewItem = (SelectableTreeViewItem)data;
            var folderItem = (FolderItem)selectableTreeViewItem.UserData;


            RunInProgressWindow("Looking for External Paths", (handler) =>
            {
                ProcedureEnvironment loadEnvironment = new ProcedureEnvironment(new DesignerResourceManager(folderItem.Project, _services));

                HashSet<NodeReference> absolutePaths = new HashSet<NodeReference>();
                foreach (var projectFileItem in folderItem.GetFileItemTree().Where(x => x.Extension == _graphCreator.Extension))
                {
                    handler.SetText("Searching " + projectFileItem.ProjectRelativePath);

                    foreach (var nodeReference in GetFileParameterPathsFromFile(projectFileItem, loadEnvironment))
                    {
                        if (Path.IsPathRooted(nodeReference.ItemPath))
                            absolutePaths.Add(nodeReference);
                    }
                }
                

                foreach (var nodeReference in absolutePaths.OrderBy(x => x.GraphPath))
                    _messageManager.Publish(new LogMessageSent(nodeReference.ItemPath + " (" + nodeReference.Location + ")"));
            });
        }



        private void FindFileReferences(Object data)
        {
            var selectableTreeViewItem = (SelectableTreeViewItem) data;
            var fileItem = (FileItem) selectableTreeViewItem.UserData;
            var project = fileItem.Project;

            RunInProgressWindow("Looking for File References", (handler) =>
            {
                ProcedureEnvironment loadEnvironment = new ProcedureEnvironment(new DesignerResourceManager(project, _services));

                foreach (var projectFileItem in project.BaseFolder.GetFileItemTree().Where(x => x.Extension == _graphCreator.Extension))
                {
                    if (handler.ShouldCancel)
                        break;

                    handler.SetText("Searching " + projectFileItem.ProjectRelativePath);

                    var fileParameterPathsFromFile = GetFileParameterPathsFromFile(projectFileItem, loadEnvironment).ToList();
                    if(fileParameterPathsFromFile.Any(x => x.ItemPath.Contains(fileItem.ProjectRelativePath)))
                        _messageManager.Publish(new LogMessageSent(projectFileItem.ProjectRelativePath));
                }
            });
        }



        private bool IsGraphFile(Object data)
        {
            var selectableTreeViewItem = (SelectableTreeViewItem)data;
            var fileItem = (FileItem)selectableTreeViewItem.UserData;

            return fileItem.Extension == _graphCreator.Extension;
        }



        private void FindDependencies(Object data)
        {
            var selectableTreeViewItem = (SelectableTreeViewItem)data;
            var fileItem = (FileItem)selectableTreeViewItem.UserData;
            var project = fileItem.Project;

            RunInProgressWindow("Looking for Dependencies", (handler) =>
            {
                ProcedureEnvironment loadEnvironment = new ProcedureEnvironment(new DesignerResourceManager(project, _services));

                foreach (var nodeReference in GetFileParameterPathsFromFile(fileItem, loadEnvironment))
                    _messageManager.Publish(new LogMessageSent(nodeReference.GraphPath));
            });
        }




        private void FindFolderReferences(Object data)
        {
            var selectableTreeViewItem = (SelectableTreeViewItem)data;
            var folderItem = (FolderItem)selectableTreeViewItem.UserData;

            RunInProgressWindow("Looking for External References", (handler) =>
            {
                ProcedureEnvironment loadEnvironment = new ProcedureEnvironment(new DesignerResourceManager(folderItem.Project, _services));

                HashSet<NodeReference> uniqueReferences = new HashSet<NodeReference>();
                foreach (var projectFileItem in folderItem.Project.BaseFolder.GetFileItemTree()
                    .Where(x => x.Extension == _graphCreator.Extension && 
                           !x.ProjectRelativePath.StartsWith(folderItem.ProjectRelativePath + "/")))
                {
                    handler.SetText("Searching " + projectFileItem.ProjectRelativePath);

                    foreach (var nodeReference in GetFileParameterPathsFromFile(projectFileItem, loadEnvironment))
                    {
                        if (nodeReference.GraphPath.StartsWith(folderItem.ProjectRelativePath + "/"))
                            uniqueReferences.Add(new NodeReference(nodeReference.Node, projectFileItem.ProjectRelativePath, nodeReference.GraphPath));
                    }
                }

                foreach (var nodeReference in uniqueReferences.OrderBy(x => x.GraphPath))
                    _messageManager.Publish(new LogMessageSent(nodeReference.Location));
            });
        }


        private void FindFolderDependencies(Object data)
        {
            var selectableTreeViewItem = (SelectableTreeViewItem)data;
            var folderItem = (FolderItem)selectableTreeViewItem.UserData;
            

            RunInProgressWindow("Looking for External Dependencies", (handler) =>
            {
                ProcedureEnvironment loadEnvironment = new ProcedureEnvironment(new DesignerResourceManager(folderItem.Project, _services));

                HashSet<NodeReference> uniqueReferences = new HashSet<NodeReference>();
                foreach (var projectFileItem in folderItem.GetFileItemTree().Where(x => x.Extension == _graphCreator.Extension))
                {
                    foreach (var nodeReference in GetFileParameterPathsFromFile(projectFileItem, loadEnvironment))
                    {
                        if(!nodeReference.GraphPath.StartsWith(folderItem.ProjectRelativePath))
                            uniqueReferences.Add(nodeReference);
                    }   
                }

                foreach (var nodeReference in uniqueReferences.OrderBy(x => x.GraphPath))
                    _messageManager.Publish(new LogMessageSent(nodeReference.Location));
            });

        }


        private IEnumerable<NodeReference> GetFileParameterPathsFromFile(ProjectItem projectFileItem, ProcedureEnvironment loadEnvironment)
        {
            Graph graph = GraphLoad.LoadFromPath(projectFileItem.FullPath, loadEnvironment);

            foreach (var nodeReference in GetFileParameterPathsFromParameters(null, projectFileItem.FullPath, graph.ParameterInfos))
                yield return nodeReference;

            foreach (var graphNode in graph.Nodes)
            {
                //look up into the file parameters
                foreach (var nodeReference in GetFileParameterPathsFromParameters(graphNode, projectFileItem.FullPath, graphNode.Parameters))
                    yield return nodeReference;
                
                //also, if the node is a component node, check if the graph behind it is the one we're looking for
                if (graphNode is ComponentNode)
                    yield return new NodeReference(graphNode, projectFileItem.FullPath,((ComponentNode) graphNode).ProjectRelativePath);
            }
        }



        private IEnumerable<NodeReference> GetFileParameterPathsFromParameters(Node graphNode, String graphPath, List<ParameterInfo> graphParameterInfos)
        {
            //TODO: Incomplete: still doesn't account for selectListParameters, which may have stored data in the non-selected options
            // meaning that this will still find paths below those parameters, and perhaps it shouldn't 
            foreach (var graphNodeParameter in graphParameterInfos.Where(x => !x.IsExpression))
            {
                foreach (var fileParameterInfo in graphNodeParameter.GetThisAndSubtree(true).OfType<FileParameterInfo>().Where(x => !x.IsExpression))
                {
                    if (!String.IsNullOrWhiteSpace(fileParameterInfo.FixedValue))
                        yield return new NodeReference(graphNode, graphPath, PathHelper.ToUniversalPath(fileParameterInfo.FixedValue));
                }
            }
        }



        private void RunInProgressWindow(String title, Action<ProgressWindow.ProgressHandler> action)
        {
            var progressWindow = new ProgressWindow(action)
            {
                LargeIcon = EmbeddedResources.Load<Texture2D>("Resources/Wizard_48x48.png"),
                Title = title
            };

            progressWindow.Show(_services.Get<WindowAnimator>());
        }



        public class NodeReference
        {
            public NodeReference(Node node, string graphPath, string itemPath)
            {
                Node = node;
                GraphPath = graphPath;
                ItemPath = itemPath;
            }

            public Node Node
            {
                get;
            }

            public String GraphPath
            {
                get;
            }

            public String ItemPath
            {
                get;
            }

            public string Location
            {
                get { return Node != null ? Node.Label + ", on " + GraphPath : GraphPath; }
            }



            protected bool Equals(NodeReference other)
            {
                return Equals(Node, other.Node) && GraphPath == other.GraphPath && ItemPath == other.ItemPath;
            }



            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((NodeReference) obj);
            }



            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (Node != null ? Node.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (GraphPath != null ? GraphPath.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (ItemPath != null ? ItemPath.GetHashCode() : 0);
                    return hashCode;
                }
            }



            public static bool operator ==(NodeReference left, NodeReference right)
            {
                return Equals(left, right);
            }



            public static bool operator !=(NodeReference left, NodeReference right)
            {
                return !Equals(left, right);
            }
        }
    }
}
