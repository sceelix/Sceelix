using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DigitalRune.Game.UI.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Core.Caching;
using Sceelix.Core.Environments;
using Sceelix.Core.Graphs;
using Sceelix.Core.Handles;
using Sceelix.Core.Procedures;
using Sceelix.Designer.Extensions;
using Sceelix.Designer.Graphs.Environments;
using Sceelix.Designer.Graphs.GUI.Basic;
using Sceelix.Designer.Graphs.GUI.Interactions;
using Sceelix.Designer.Graphs.GUI.Model.Drawing;
using Sceelix.Designer.Graphs.GUI.Navigation;
using Sceelix.Designer.Graphs.Logging;
using Sceelix.Designer.Graphs.Messages;
using Sceelix.Designer.Graphs.Messaging;
using Sceelix.Designer.Graphs.Tools;
using Sceelix.Designer.Helpers;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;
using Sceelix.Extensions;
using HorizontalAlignment = DigitalRune.Game.UI.HorizontalAlignment;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Orientation = DigitalRune.Game.UI.Orientation;
using Point = Sceelix.Core.Graphs.Point;
using RectangleF = Sceelix.Designer.Graphs.GUI.Basic.RectangleF;

namespace Sceelix.Designer.Graphs.GUI.Model
{
    public class VisualGraph
    {
        //private readonly List<VisualGroup> _visualGroups = new List<VisualGroup>();

        //content management
        private readonly GraphControl _control;
        private readonly ContentManager _manager;
        private readonly List<VisualEdge> _visualEdges = new List<VisualEdge>();

        //visual references
        private readonly List<VisualNode> _visualNodes = new List<VisualNode>();

        //private ToolTipController _controller = new ToolTipController(){AllowHtmlText = true};

        private readonly ProcedureEnvironment _environment;
        //model references
        private Graph _graph;
        private readonly Stack<string> _redoStates = new Stack<string>();
        private readonly Stack<string> _undoStates = new Stack<string>();



        public VisualGraph(IServiceLocator services, GraphControl control, ContentManager manager, ICacheManager cacheManager)
        {
            _control = control;
            _manager = manager;
            _environment = new ProcedureEnvironment();

            _environment.Services.Add(new GraphDesignerLogger(services));
            _environment.Services.Add(new GraphDesignerMessenger(services));
            _environment.Services.Add(new DesignerResourceManager(control.FileItem.Project, services));
            _environment.Services.Add(new SimpleVisualHandleManager());
            Environment.Services.Add(cacheManager);
            

            //load the graph from the file
            try
            {
                //_graph = GraphBinaryIO.Load(control.FileItem.FullPath, new Environment(control.FileItem.Project.FolderPath));

                //GraphLoad.ClearCache();
                _graph = GraphLoad.LoadFromPath(control.FileItem.FullPath, _environment); //new Environment(control.FileItem.Project.FolderPath)
            }
            catch (Exception)
            {
                _graph = new Graph();

                //Messenger.MessageManager.Publish(new CorruptFileContent(control.FileItem));
                //Messenger.MessageManager.Publish(new GraphContentChanged(control.FileItem,true));
            }

            //Graph graph2 = Graph.LoadFromPath(control.FileItem.FullPath + "x", new Environment(control.FileItem.Project.FolderPath));
            //_graph = Graph.LoadFromPath(control.FileItem.FullPath + "x", new Environment(control.FileItem.Project.FolderPath));

            //the initial state is stored
            _undoStates.Push(_graph.GetXML());

            RefreshVisualGraph(true);

            control.Services.Get<MessageManager>().Register<GraphContentChanged>(OnGraphContentChanged, x => x.Item == FileItem);
            control.Services.Get<MessageManager>().Register<FrameNodes>(OnFrameNodes);
        }



        private void OnFrameNodes(FrameNodes obj)
        {
            if (!Control.IsLoaded)
                return;

            //this could fail for older graphs which did not have any guids set
            //otherwise it works fine
            var visualNodes = VisualNodes.Where(x => obj.Nodes.Any(y => y.IsStructurallyEqual(x.Node)));
            _control.Camera.FrameNodes(visualNodes.ToList(),true);
        }



        private void OnGraphContentChanged(GraphContentChanged obj)
        {
            //if a change gets done, add the new state to the undo list
            _undoStates.Push(_graph.GetXML());

            //also, if a change gets done, clear the redo list
            _redoStates.Clear();
        }



        public void Undo()
        {
            if (_undoStates.Count <= 1)
                return;

            //the first state is the current state, so take it and put it in the redo stack
            _redoStates.Push(_undoStates.Pop());

            //get the next one
            var graphState = _undoStates.Peek();

            LoadHistoryGraph(graphState);

            //_redoStates.Push(graphState);
        }



        public void Redo()
        {
            if (_redoStates.Count == 0)
                return;

            var graphState = _redoStates.Pop();

            _undoStates.Push(graphState);

            LoadHistoryGraph(graphState);
        }



        private void LoadHistoryGraph(string graphState)
        {
            //GraphLoad.ClearCache();
            _graph = GraphLoad.LoadFromXML(graphState, _environment);
            RefreshVisualGraph(false);
        }



        public void RefreshVisualGraph(bool animate)
        {
            _visualNodes.Clear();
            _visualEdges.Clear();

            foreach (Node node in _graph.Nodes)
                _visualNodes.Add(new VisualNode(this, node, animate));

            Dictionary<Port, VisualPort> portMappings = GetPortMappings(_visualNodes);

            foreach (Edge edge in _graph.Edges)
                _visualEdges.Add(new VisualEdge(edge, portMappings));
        }



        /// <summary>
        /// Processes a mapping between ports and visualPorts
        /// </summary>
        /// <param name="visualNodes"></param>
        /// <returns></returns>
        private Dictionary<Port, VisualPort> GetPortMappings(IEnumerable<VisualNode> visualNodes)
        {
            Dictionary<Port, VisualPort> portMappings = new Dictionary<Port, VisualPort>();

            foreach (VisualNode visualNode in visualNodes)
                foreach (VisualPort visualPort in visualNode.Ports)
                    portMappings.Add(visualPort.Port, visualPort);

            return portMappings;
        }



        public IHoverableUnit UpdateUnitHovers(Vector2 mouseModelLocation, RectangleF? selectionRectangle)
        {
            IHoverableUnit hoveredUnit = null;

            foreach (VisualNode visualNode in VisualNodes.Reverse<VisualNode>())
            {
                foreach (VisualPort visualPort in visualNode.Ports)
                    hoveredUnit = visualPort.UpdateUnitHovers(mouseModelLocation, hoveredUnit);

                hoveredUnit = visualNode.UpdateUnitHovers(mouseModelLocation, selectionRectangle, hoveredUnit);
            }

            foreach (VisualEdge visualEdge in VisualEdges.Reverse<VisualEdge>())
                hoveredUnit = visualEdge.UpdateUnitHovers(mouseModelLocation, selectionRectangle, hoveredUnit);


            //if (!(hoveredUnit is VisualPort))
            //    _controller.HideHint();

            return hoveredUnit;
        }



        public void UpdatePortEmphasis(IHoverableUnit hoveredUnit)
        {
            List<VisualPort> allVisualPorts = VisualNodes.SelectMany(val => val.Ports).ToList();

            if (!(hoveredUnit is VisualPort))
            {
                foreach (VisualPort visualPort in allVisualPorts)
                    visualPort.Emphasis = PortEmphasis.None;
            }
            else
            {
                VisualPort port = (VisualPort) hoveredUnit;

                allVisualPorts.Remove(port);

                foreach (VisualPort visualPort in allVisualPorts)
                {
                    VisualPort startPort = port.Port is OutputPort ? port : visualPort;
                    VisualPort endPort = port.Port is InputPort ? port : visualPort;

                    //the effect on t
                    if (EdgeDrawer.CheckIfValidTargetPort(startPort, endPort))
                        visualPort.Emphasis = EdgeDrawer.DoubleCheckOnTypeCompatibility(this, startPort, endPort) ? PortEmphasis.Positive : PortEmphasis.Unsure;
                    else
                        visualPort.Emphasis = PortEmphasis.Negative;
                }
            }
        }



        public void Update(TimeSpan deltaTime)
        {
            //_control.ToolTip = null;

            //if(VisualNodes.Any(x => x.IsDeleted))
            VisualNodes.RemoveAll(x => x.IsDeleted);
            VisualEdges.RemoveAll(x => x.IsDeleted);

            foreach (VisualNode visualNode in VisualNodes)
                visualNode.Update(deltaTime);

            foreach (VisualEdge visualEdge in VisualEdges)
                visualEdge.Update(deltaTime);

            if (!HasHoveredItems && _control.IsMouseOver)
            {
                _control.ToolTip = null;
                _control.Screen.ToolTipManager.CloseToolTip();
            }
            /*var toolTip = _control.ToolTip as ToolTipControl;
            if (toolTip != null && !toolTip.IsAlive)
            {
                _control.ToolTip = null;
                _control.Screen.ToolTipManager.CloseToolTip();
                
                _environment.Log("I went null");
            }*/
        }



        public void Draw(SpriteBatch spriteBatch, Matrix matrix)
        {
            /*Vector3 scale, translation;
            Quaternion quaternion;
            matrix.Decompose(out scale, out quaternion, out translation);

            //var orderedEdges = VisualEdges.OrderBy(x => x.IsHovered ? 1 : 0).ToList();
            var totalBoundingRectangle = BoundingRectangle.Merge(VisualEdges.Select(x => x.BoundingRectangle));
            totalBoundingRectangle.Expand(EdgeLine.PenSize * 2);
            
            Bitmap bitmap;
            using (var graphics = Graphics.FromImage(bitmap = new Bitmap((int)(totalBoundingRectangle.Width * scale.X), (int) (totalBoundingRectangle.Height * scale.Y))))
            {
                graphics.ScaleTransform(scale.X, scale.Y);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;

                foreach (VisualEdge visualEdge in VisualEdges)
                {
                    visualEdge.Draw(graphics, totalBoundingRectangle);
                }
            }
                

            spriteBatch.Draw(NewToTexture2D(bitmap,_control.GraphicsDevice), totalBoundingRectangle.Min);
            bitmap.Dispose();*/

            //this sorting could be optimized in the future...
            foreach (VisualEdge visualEdge in VisualEdges.OrderBy(x => x.IsHovered ? 1 : 0))
                visualEdge.Draw(spriteBatch);

            foreach (VisualNode node in VisualNodes)
                node.Draw(spriteBatch);
        }



        private Texture2D NewToTexture2D(Bitmap bitmap, GraphicsDevice graphicsDevice)
        {
            MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);

            // Finally create the Texture2D from stream
            return Texture2D.FromStream(graphicsDevice, stream);
        }



        public void TranslateVisualNodes(Vector2 offSet, Vector2 newMouseScreenPosition)
        {
            List<VisualNode> selectedNodes = VisualNodes.Where(val => val.IsSelected).ToList();

            foreach (VisualNode selectedNode in selectedNodes)
                selectedNode.Translate(offSet);

            //if we are dragging exactly one node, we can try to incorporate it into the flow
            if (selectedNodes.Count == 1 && selectedNodes[0].IsIsolated)
            {
                VisualEdge hoveredEdge = null;
                VisualNode draggedNode = selectedNodes[0];

                foreach (VisualEdge visualEdge in VisualEdges.Reverse<VisualEdge>())
                    hoveredEdge = (VisualEdge) visualEdge.UpdateUnitHovers(newMouseScreenPosition, null, hoveredEdge); //draggedNode
            }
        }



        public bool TryIsolatedNodeIntegration()
        {
            List<VisualNode> selectedNodes = VisualNodes.Where(val => val.IsSelected).ToList();

            //if we are dragging exactly one node, we can try to incorporate it into the flow
            if (selectedNodes.Count == 1 && selectedNodes[0].IsIsolated && selectedNodes[0].IsIntegrable)
            {
                VisualNode draggedNode = selectedNodes[0];

                VisualEdge hoveredEdge = VisualEdges.FirstOrDefault(val => val.IsHovered);

                if (hoveredEdge != null && EdgeDrawer.CheckIfValidTargetPort(hoveredEdge.FromVisualPort, draggedNode.InputVisualPorts[0])
                    && EdgeDrawer.CheckIfValidTargetPort(draggedNode.OutputVisualPorts[0], hoveredEdge.ToVisualPort))
                {
                    EdgeDrawer drawer = new EdgeDrawer(draggedNode.OutputVisualPorts[0]);
                    drawer.CreateEdge(hoveredEdge.ToVisualPort);

                    hoveredEdge.ToVisualPort = draggedNode.InputVisualPorts[0];
                }

                return true;

                //foreach (VisualEdge visualEdge in VisualEdges.Reverse<VisualEdge>())
                //    hoveredEdge = (VisualEdge) visualEdge.UpdateUnitHovers(newMouseScreenPosition, null, hoveredEdge);
            }

            return false;
        }



        public void DeleteSelectedUnits()
        {
            if (VisualEdges.Where(x => x.IsSelected).Count() == 1 && !HasSelectedNodes)
            {
                VisualEdges[VisualEdges.FindIndex(x => x.IsSelected)].Delete();
            }
            else if (ShowDeletionDialog())
            {
                foreach (VisualEdge visualEdge in VisualEdges.Where(x => x.IsSelected))
                    visualEdge.Delete();

                foreach (VisualNode visualNode in VisualNodes.Where(x => x.IsSelected))
                    visualNode.Delete();

                //clear the inspector window
                AlertForFileChange(true);

                _control.Services.Get<MessageManager>().Publish(new ShowPropertiesRequest(null, _control));
            }
        }



        private bool ShowDeletionDialog()
        {
            String message = "Are you sure you want to delete the selected ";

            if (HasSelectedNodes)
            {
                message += "nodes";
                if (HasSelectedEdges)
                    message += " and edges";
            }
            else
                message += "edges";

            //DialogResult dialogResult = XtraMessageBox.Show(message + "?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            //return dialogResult == DialogResult.Yes;
            return true;
        }



        public VisualNode AddNode(IProcedureItem nodeItem, Vector2 modelPosition)
        {
            Node node = nodeItem.GenerateNode(new Point((int) modelPosition.X, (int) modelPosition.Y), Environment);
            _graph.AddNode(node);

            VisualNode visualNode = new VisualNode(this, node, true);
            _visualNodes.Add(visualNode);

            AlertForFileChange(visualNode.Node.IsSourceNode);

            return visualNode;
        }



        public void AlertForFileChange(bool worthReexecutingGraph)
        {
            _control.ShouldRender = true;

            _control.Services.Get<MessageManager>().Publish(new GraphContentChanged(FileItem, worthReexecutingGraph));
        }



        public VisualNode AddNode(FileItem componentFileItem, Point modelPosition)
        {
            var componentNode = new ComponentNode(componentFileItem.ProjectRelativePath, GraphLoad.LoadFromPath(componentFileItem.ProjectRelativePath, _environment), modelPosition);


            //Graph componentGraph = GraphLoad.LoadFromPath(componentFileItem.FullPath, _environment);
            /*ComponentNode componentNode;

            GraphProcedure graphProcedure = GraphProcedure.FromPath(_environment, componentFileItem.ProjectRelativePath);
            graphProcedure.Name = componentFileItem.Name;
            componentNode = new ComponentNode(graphProcedure, modelPosition, componentFileItem.ProjectRelativePath);*/


            //ComponentNode componentNode = new ComponentNode(graphProcedure, modelPosition, componentFileItem.ProjectRelativePath);
            _graph.AddNode(componentNode);

            VisualNode componentVisualNode;
            _visualNodes.Add(componentVisualNode = new VisualNode(this, componentNode, true));

            //Messenger.MessageManager.Publish(new GraphContentChanged(FileItem,true));

            return componentVisualNode;
        }



        public VisualPort FindHoveredPort()
        {
            return VisualPorts.FirstOrDefault(x => x.IsHovered);
        }



        public VisualEdge FindHoveredEdge()
        {
            return VisualEdges.FirstOrDefault(x => x.IsHovered);
        }



        /// <summary>
        /// Checks if there is any edge between the two indicated ports.
        /// </summary>
        /// <param name="startPort"></param>
        /// <param name="targetPort"></param>
        /// <returns></returns>
        public bool ContainsConnection(VisualPort startPort, VisualPort targetPort)
        {
            return VisualEdges.Any(x => (x.FromVisualPort == startPort && x.ToVisualPort == targetPort)
                                        || (x.FromVisualPort == targetPort && x.ToVisualPort == startPort));
        }



        public void DeselectAll()
        {
            foreach (VisualNode visualNode in VisualNodes.Where(x => x.IsSelected))
                visualNode.Deselect();

            foreach (VisualEdge visualEdge in VisualEdges.Where(x => x.IsSelected))
                visualEdge.Deselect();

            foreach (VisualPort visualPort in VisualPorts.Where(x => x.IsSelected))
                visualPort.Deselect();
        }



        private void SelectHoveredNodes(bool leftButton)
        {
            //get a list of all hovered nodes by the rectangle
            List<VisualNode> hoveredVisualNodes = VisualNodes.Where(val => val.IsHovered).ToList();

            //remove from the list of nodes, and put it back in the end (so that they will always be drawn on top)
            _visualNodes.RemoveAll(hoveredVisualNodes.Contains);
            _visualNodes.AddRange(hoveredVisualNodes);

            //select all of them
            foreach (VisualNode visualNode in hoveredVisualNodes)
                visualNode.Select();

            //if indeed any was selected, show the properties of the first
            if (hoveredVisualNodes.Count > 0 && leftButton)
                hoveredVisualNodes.First().ShowProperties();
        }



        private void SelectHoveredEdges()
        {
            //get a list of all hovered edges
            List<VisualEdge> hoveredVisualEdges = VisualEdges.Where(val => val.IsHovered).ToList();

            //remove from the list of edges, and put it back in the end (so that they will always be drawn on top)
            _visualEdges.RemoveAll(hoveredVisualEdges.Contains);
            _visualEdges.AddRange(hoveredVisualEdges);

            //select all of them
            foreach (VisualEdge visualEdge in hoveredVisualEdges)
                visualEdge.Select();

            //if indeed any was selected, show the properties of the first
            if (hoveredVisualEdges.Count > 0)
                hoveredVisualEdges.First().ShowProperties();
        }



        public void SelectHoveredUnits(bool leftButton)
        {
            //SelectHoveredPorts();
            SelectHoveredNodes(leftButton);
            SelectHoveredEdges();
        }



        private void SelectHoveredPorts()
        {
            //get a list of all hovered edges
            List<VisualPort> hoveredVisualPorts = VisualNodes.SelectMany(val => val.Ports).Where(val => val.IsHovered).ToList();

            //select all of them
            foreach (VisualPort visualPort in hoveredVisualPorts)
                visualPort.Select();

            //if indeed any was selected, show the properties of the first
            if (hoveredVisualPorts.Count > 0)
                hoveredVisualPorts.First().ShowProperties();
        }



        public void SelectFirstSelectedUnit()
        {
            //get a list of all hovered nodes by the rectangle
            List<VisualNode> visualNodes = VisualNodes.Where(val => val.IsSelected).ToList();
            if (visualNodes.Count > 0)
            {
                visualNodes.First().ShowProperties();

                return;
            }

            List<VisualEdge> visualEdges = VisualEdges.Where(val => val.IsSelected).ToList();
            if (visualEdges.Count > 0)
                visualEdges.First().ShowProperties();
        }



        public ISelectableUnit GetHoveredSelectedUnit()
        {
            VisualNode hoveredSelectedNode = _visualNodes.FirstOrDefault(val => val.IsHovered && val.IsSelected);
            if (hoveredSelectedNode != null)
                return hoveredSelectedNode;

            return _visualEdges.FirstOrDefault(val => val.IsHovered && val.IsSelected);
        }



        public void ShowToolTip(IHoverableUnit unit, Vector2 position)
        {
            _control.ToolTip = unit.Tooltip;


            //ToolTipControlInfo info = unit.TooltipText;

            //_controller.ShowHint(info.Text, info.Title, ToolTipLocation.BottomRight, Control.Camera.ToApplicationPosition(position));
        }



        public void DisableSelectedEdges()
        {
            foreach (VisualEdge visualEdge in VisualEdges)
            {
                if (visualEdge.IsSelected)
                {
                    visualEdge.Enabled = false;
                }
            }

            ShouldRender = true;
        }



        public void EnableSelectedEdges()
        {
            foreach (VisualEdge visualEdge in VisualEdges)
            {
                if (visualEdge.IsSelected)
                    visualEdge.Enabled = true;
            }

            ShouldRender = true;
        }



        public void EncapsulateSelection(string chosenName)
        {
            //make sure all ids start from 0
            Graph.RefreshNodeIds();

            //now create a list of Ids of the selected nodes. These will be the links between graphs
            List<int> selectedNodeIds = SelectedUnits.OfType<VisualNode>().Select(val => val.Node.Id).ToList();

            BuildGates(Graph, selectedNodeIds);


            //Step 1: CREATE THE GRAPH FILE
            FolderItem parentFolder = FileItem.ParentFolder;


            //make a copy of the graph through XML serialization
            string xml = Graph.GetXML();
            Graph loadedGraph = GraphLoad.LoadFromXML(xml, _environment);
            loadedGraph.Guid = Guid.NewGuid();


            //Remove all the nodes whose IDs does not match our selection
            foreach (Node node in loadedGraph.Nodes.Where(x => !x.Id.IsIn(selectedNodeIds)).ToList())
                node.Delete();


            //clean up aspects that aren't referenced
            loadedGraph.CleanupUpParameters();

            //set all parameters as public
            loadedGraph.ParameterInfos.ForEach(x => x.IsPublic = true);


            //create the fileitem and add it to the project tree
            FileItem fileItem = new FileItem(chosenName, parentFolder, loadedGraph.Guid);
            parentFolder.AddItem(fileItem);
            loadedGraph.SaveXML(fileItem.FullPath);

            _control.Services.Get<MessageManager>().Publish(new FileCreated(fileItem));

            //Step 2: INTRODUCE THE NEW NODE IN THE PRESENT GRAPH
            int min = selectedNodeIds.Min();
            Point position = Graph.Nodes.First(val => val.Id == min).Position;

            VisualNode componentVisualNode = AddNode(fileItem, position);

            List<VisualPort> componentNodePorts = componentVisualNode.Ports.ToList();


            //remove the nodes of the graph

            foreach (VisualNode visualNode in VisualNodes.ToList().Where(x => x.Node.Id.IsIn(selectedNodeIds)).ToList())
            {
                foreach (VisualPort visualPort in visualNode.Ports)
                {
                    foreach (VisualEdge visualEdge in visualPort.Edges)
                    {
                        VisualPort otherPort = visualEdge.OtherEnd(visualPort);

                        //the connection must be outside this selection of nodes
                        if (!selectedNodeIds.Contains(visualEdge.OtherEnd(visualPort).VisualNode.Node.Id))
                        {
                            VisualPort componentNodePort = componentNodePorts.FirstOrDefault(val => val.Label == visualPort.Port.GateLabel);
                            if (componentNodePort != null)
                            {
                                EdgeDrawer.AddEdge(otherPort, componentNodePort);
                            }
                        }
                    }
                }

                visualNode.Delete();
            }
        }



        private void BuildGates(Graph loadedGraph, List<int> selectedNodeIds)
        {
            HashSet<String> inputNames = new HashSet<String>();
            HashSet<String> outputNames = new HashSet<String>();

            foreach (Node node in loadedGraph.Nodes.Where(x => x.Id.IsIn(selectedNodeIds)))
            {
                CreateGates(node.InputPorts, selectedNodeIds, inputNames);
                CreateGates(node.OutputPorts, selectedNodeIds, outputNames);
            }
        }



        private void CreateGates(IEnumerable<Port> ports, List<int> selectedNodeIds, HashSet<String> takenNames)
        {
            foreach (var port in ports)
            {
                //if the port is empty or has any connection to a node outside the selection
                if (port.Edges.Count == 0 || port.Edges.Any(x => !x.OtherEnd(port).Node.Id.IsIn(selectedNodeIds)))
                {
                    if (port.PortState == PortState.Normal)
                    {
                        port.PortState = PortState.Gate;

                        String gateLabel = port.Label;
                        while (takenNames.Contains(gateLabel))
                            gateLabel = gateLabel + "*";

                        takenNames.Add(gateLabel);

                        port.GateLabel = gateLabel;
                    }
                }
            }
        }



        private void MakeGate(int nodeId, string inputType, List<Port> inputPorts)
        {
            for (int index = 0; index < inputPorts.Count; index++)
            {
                Port input = inputPorts[index];

                if (input.PortState == PortState.Normal)
                    input.PortState = PortState.Gate;

                input.GateLabel = input.Label; //inputType + "/" + nodeId + "/" + index;
            }
        }



        public void CutToClipBoard()
        {
            CopyToClipBoard();

            foreach (VisualNode visualNode in SelectedUnits.OfType<VisualNode>())
            {
                visualNode.Delete();
            }
        }



        public void CopyToClipBoard()
        {
            Graph.RefreshNodeIds();

            string xml = Graph.GetXML();

            Graph loadedGraph = GraphLoad.LoadFromXML(xml, _environment);

            List<int> idList = SelectedUnits.OfType<VisualNode>().Select(val => val.Node.Id).ToList();

            foreach (Node node in loadedGraph.Nodes.ToList())
            {
                if (!idList.Contains(node.Id))
                    node.Delete();
            }

            loadedGraph.CleanupUpParameters();


            ClipboardHelper.Copy(loadedGraph.GetXML());
        }



        public void PasteFromClipBoard(Vector2 mouseScreenPosition)
        {
            //gets the text from clipboard and tries to parse it into a graph
            //if it is invalid, an exception will be thrown (ignore for now)
            //TODO: Find a better solution for handling it
            try
            {
                Graph loadedGraph = GraphLoad.LoadFromXML(ClipboardHelper.Paste(), Environment);
                loadedGraph.ResetNodeGuids();

                Vector2 modelPosition = Control.Camera.ToModelPosition(mouseScreenPosition);

                Graph.Incorporate(loadedGraph, new Point(modelPosition.X, modelPosition.Y));

                RefreshVisualGraph(false);
            }
            catch
            {
            }
        }

        #region Properties

        public ProcedureEnvironment Environment
        {
            get { return _environment; }
        }



        public GraphEditorSettings Settings
        {
            get { return _control.Settings; }
        }


        public FileItem FileItem
        {
            get { return _control.FileItem; }
        }



        public Graph Graph
        {
            get { return _graph; }
            set { _graph = value; }
        }



        public List<VisualNode> VisualNodes
        {
            get { return _visualNodes; }
        }



        public ContentManager Content
        {
            get { return _manager; }
        }



        public List<VisualEdge> VisualEdges
        {
            get { return _visualEdges; }
        }



        public GraphControl Control
        {
            get { return _control; }
        }



        public List<VisualPort> VisualPorts
        {
            get { return _visualNodes.SelectMany(x => x.Ports).ToList(); }
        }



        public bool HasSelectedUnits
        {
            get { return _visualNodes.Any(val => val.IsSelected) || _visualEdges.Any(val => val.IsSelected); }
        }



        public bool HasSelectedNodes
        {
            get { return _visualNodes.Any(val => val.IsSelected); }
        }



        public bool HasSelectedConnectedNodes
        {
            get { return _visualNodes.Any(val => val.IsSelected && !val.IsIsolated); }
        }



        public bool HasSelectedGraphNodes
        {
            get { return _visualNodes.Any(val => val.IsSelected && val.Node is ComponentNode); }
        }



        public bool HasSelectedEdges
        {
            get { return _visualEdges.Any(val => val.IsSelected); }
        }



        public bool HasHoveredNodes
        {
            get { return _visualNodes.Any(val => val.IsHovered); }
        }



        public bool HasHoveredUnits
        {
            get { return _visualNodes.Any(val => val.IsHovered) || _visualEdges.Any(val => val.IsHovered); }
        }



        /// <summary>
        /// This includes nodes, edges and ports
        /// </summary>
        public bool HasHoveredItems
        {
            get { return _visualNodes.Any(val => val.IsHovered) || _visualEdges.Any(val => val.IsHovered) || VisualPorts.Any(val => val.IsHovered); }
        }



        public bool HasHoveredSelectedUnits
        {
            get { return _visualNodes.Any(val => val.IsHovered && val.IsSelected) || _visualEdges.Any(val => val.IsHovered && val.IsSelected); }
        }



        public IEnumerable<ISelectableUnit> SelectedUnits
        {
            get { return _visualNodes.Where(val => val.IsSelected).Union<ISelectableUnit>(_visualEdges.Where(val => val.IsSelected)); }
        }



        public bool ShowPortLabels
        {
            get;
            set;
        }



        public bool HasOnlyEnabledEdgesSelected
        {
            get
            {
                IEnumerable<ISelectableUnit> selectableUnits = SelectedUnits.ToList();

                return selectableUnits.Any() && selectableUnits.All(val => val is VisualEdge && ((VisualEdge) val).Enabled);
                //return SelectedUnits.All(val => val is VisualEdge && ((VisualEdge)val).Enabled);
            }
        }



        public bool HasOnlyDisabledEdgesSelected
        {
            get
            {
                IEnumerable<ISelectableUnit> selectableUnits = SelectedUnits.ToList();

                return selectableUnits.Any() && selectableUnits.All(val => val is VisualEdge && !((VisualEdge) val).Enabled);
            }
        }



        public bool ShouldRender
        {
            get { return _control.ShouldRender; }
            set { _control.ShouldRender = value; }
        }

        #endregion

        public void Dispose()
        {
            Control.Services.Get<MessageManager>().Unregister(this);
        }
    }
}