using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Sceelix.Core.Graphs;
using Sceelix.Designer.Graphs.GUI.Model;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Graphs.GUI.Interactions
{
    public class GraphDropHandler
    {
        private readonly GraphControl _graphControl;
        private FileItem _fileItem;
        private MessageManager _messageManager;


        public GraphDropHandler(IServiceLocator services, GraphControl graphControl)
        {
            _graphControl = graphControl;
            _fileItem = graphControl.FileItem;

            _messageManager = services.Get<MessageManager>();
            _messageManager.Register<ProjectItemsDropped>(OnProjectItemsDropped);
        }


        private void OnProjectItemsDropped(ProjectItemsDropped obj)
        {
            if (_graphControl.ActualIsVisible && _graphControl.ActualBounds.Contains(_graphControl.InputService.MousePosition))
            {
                var modelPosition = _graphControl.Camera.ToModelPosition(_graphControl.InputService.MousePosition.ToXna() - new Vector2(_graphControl.ActualX, _graphControl.ActualY));

                int count = 0;
                List<Node> addedNodes = new List<Node>();
                foreach (FileItem fileItem in obj.ProjectTreeItems.OfType<FileItem>())
                {
                    //do not allow the current file to be added
                    if (fileItem != _fileItem)
                    {
                        try
                        {
                            var visualNode = VisualGraph.AddNode(fileItem, new Core.Graphs.Point((int)modelPosition.X + 100 * count, (int)modelPosition.Y + 100 * count));
                            addedNodes.Add(visualNode.Node);
                        }
                        catch (Exception exception)
                        {
                            _messageManager.Publish(new ExceptionThrown(new Exception("Error when loading graph: " + exception.Message, exception)));
                        }
                    }
                }

                //if there were indeed added nodes, alert that the file has been changed
                //only re-execute if there are source nodes
                if (addedNodes.Any())
                    VisualGraph.AlertForFileChange(addedNodes.Any(x => x.IsSourceNode));
            }
        }


        public VisualGraph VisualGraph
        {
            get { return _graphControl.VisualGraph; }
        }
    }
}