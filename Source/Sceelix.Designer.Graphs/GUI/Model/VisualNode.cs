using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DigitalRune.Linq;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Core.Graphs;
using Sceelix.Core.Handles;
using Sceelix.Core.Messages;
using Sceelix.Designer.Extensions;
using Sceelix.Designer.Graphs.Extensions;
using Sceelix.Designer.Graphs.GUI.Basic;
using Sceelix.Designer.Graphs.GUI.Basic.Animations;
using Sceelix.Designer.Graphs.GUI.Model.Drawing;
using Sceelix.Designer.Graphs.Inspector.Nodes;
using Sceelix.Designer.Graphs.Messages;
using Sceelix.Designer.GUI;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.Messages;
using Point = Sceelix.Core.Graphs.Point;

namespace Sceelix.Designer.Graphs.GUI.Model
{
    public enum NodeType
    {
        Procedure,
        Input,
        Output
    }

    public enum VisualUnitStatus
    {
        Normal,
        Selected,
        Deleting,
        Deleted
    }

    public class VisualNode : ISelectableUnit, IHoverableUnit
    {
        //constants
        public const float PortSpacing = 20;
        public const float PortStartingY = 10;
        public const float PortStartingX = 10;

        //model reference
        private readonly Node _node;
        private readonly NodeLabel _nodeLabel;

        //drawing and animation
        private readonly NodeRectangle _nodeRectangle;
        private readonly VisualGraph _visualGraph;
        private RectangleAnimation _animation;

        //visual references
        private List<VisualPort> _inputVisualPorts = new List<VisualPort>();
        private bool _isHovered;
        private readonly MessageManager _messageManager;
        private Color _nodeColor;

        //status references
        private VisualUnitStatus _nodeStatus = VisualUnitStatus.Normal;
        private List<VisualPort> _outputVisualPorts = new List<VisualPort>();
        private RectangleF _realRectangle;
        private Color _selectedNodeColor;

        //keeping the animation and real rectangle sizes
        private RectangleF _transitionRectangle;
        //private Texture2D _disabledSign;

        private readonly List<VisualHandle> _visualHandles = new List<VisualHandle>();



        public VisualNode(VisualGraph graph, Node node, bool animate)
        {
            _visualGraph = graph;
            _node = node;


            foreach (Port port in _node.InputPorts)
                _inputVisualPorts.Add(new VisualPort(this, port));

            /*if (node.HasImpulsePort)
                _inputVisualPorts.Add(new VisualPort(this, _node.ImpulsePort));*/

            foreach (Port port in _node.OutputPorts)
                _outputVisualPorts.Add(new VisualPort(this, port));

            _realRectangle.Min = new Vector2((float) node.Position.X, (float) node.Position.Y);

            _nodeColor = ParseHexColor(node.ProcedureAttribute.HexColor);
            _selectedNodeColor = _nodeColor.Adjust(0.3f);
            //_selectedNodeColor = Color.Wheat;
            //var rand = new Random(_node.DefaultLabel.GetHashCode());

            //var color = ColorExtension.HsvToRgb(rand.NextFloat(0,1) * 360, 0.7f, 0.7f);
            //_nodeColor = color;

            _nodeLabel = new NodeLabel(node, Content, _nodeColor.Extreme());
            _nodeRectangle = new NodeRectangle(Content, _nodeColor, node.ShapeName); // {BorderColor = color };


            CalculateRectangleSize();

            _animation = new GrowAnimation(animate ? 500 : 1);

            _messageManager = _visualGraph.Control.Services.Get<MessageManager>();

            _messageManager.Register<AddVisualHandle>(OnDrawVisualGuide);
            _messageManager.Register<ClearVisualHandles>(OnClearVisualGuides);
            _messageManager.Register<MarkNode>(OnMarkNode);
            _messageManager.Register<MarkNodes>(OnMarkNodes);

            //_disabledSign = Content.Load<Texture2D>("Graphs/DisabledSign");
        }



        public List<VisualHandle> VisualHandles
        {
            get { return _visualHandles; }
        }



        /// <summary>
        /// Indicates if a visual node has enough ports to be integrated in an edge connection.
        /// </summary>
        public bool IsIntegrable
        {
            get { return _inputVisualPorts.Count > 0 && _outputVisualPorts.Count > 0; }
        }



        public void Select()
        {
            _nodeStatus = VisualUnitStatus.Selected;
            _nodeRectangle.FillColor = _selectedNodeColor;

            //VisualGraph.Control.Services.Get<MessageManager>().Publish(new NodeClicked(this.Node));
        }



        public void Deselect()
        {
            _nodeStatus = VisualUnitStatus.Normal;
            _nodeRectangle.FillColor = _nodeColor;

            //VisualGraph.Control.Services.Get<MessageManager>().Publish(new ClearVisualGuides());
        }



        private void OnClearVisualGuides(ClearVisualHandles obj)
        {
            _visualHandles.Clear();
        }



        private void OnDrawVisualGuide(AddVisualHandle obj)
        {
            if(obj.Node.IsStructurallyEqual(_node))
                _visualHandles.Add(obj.VisualHandle);
        }



        private void OnMarkNode(MarkNode obj)
        {
            if (obj.Node == null)
            {
                _nodeColor = ParseHexColor(_node.ProcedureAttribute.HexColor);
                _selectedNodeColor = _nodeColor.Adjust(0.3f);

                _nodeLabel.TextColor = _nodeColor.Extreme();
                _nodeRectangle.FillColor = _nodeColor;
            }
            else if (Node.IsStructurallyEqual(obj.Node))
            {
                _nodeColor = obj.Color;
                _selectedNodeColor = _nodeColor.Adjust(0.3f);

                _nodeLabel.TextColor = _nodeColor.Extreme();
                _nodeRectangle.FillColor = _nodeColor;
            }
        }



        private void OnMarkNodes(MarkNodes obj)
        {
            if (obj.Nodes.Any(x => x.IsStructurallyEqual(this.Node)))
            {
                _nodeColor = obj.Color;
                _selectedNodeColor = _nodeColor.Adjust(0.3f);

                _nodeLabel.TextColor = _nodeColor.Extreme();
                _nodeRectangle.FillColor = _nodeColor;
            }
            else
            {
                _nodeColor = ParseHexColor(_node.ProcedureAttribute.HexColor);
                _selectedNodeColor = _nodeColor.Adjust(0.3f);

                _nodeLabel.TextColor = _nodeColor.Extreme();
                _nodeRectangle.FillColor = _nodeColor;
            }
        }



        private Color ParseHexColor(string hexColor)
        {
            int red = int.Parse((hexColor).Substring(0, 2), NumberStyles.HexNumber);
            int green = int.Parse((hexColor).Substring(2, 2), NumberStyles.HexNumber);
            int blue = int.Parse((hexColor).Substring(4, 2), NumberStyles.HexNumber);

            return new Color(red, green, blue);
        }



        public void RefreshVisualPortList(List<Port> ports, List<VisualPort> visualPorts)
        {
            var oldVisualPorts = visualPorts.ToList();
            visualPorts.Clear();

            //first, create new visualports or readd the old ones (with the old connections), in the new specified order
            foreach (var visualPort in ports)
            {
                var newVisualPort = oldVisualPorts.FirstOrDefault(x => x.Port == visualPort) ?? new VisualPort(this, visualPort);
                visualPorts.Add(newVisualPort);
            }

            for (int i = 0; i < oldVisualPorts.Count; i++)
            {
                //if the port is not included in the new ports, it will be deleted, so try to move its edges
                if (!visualPorts.Contains(oldVisualPorts[i]))
                {
                    foreach (var visualEdge in oldVisualPorts[i].Edges.ToList())
                    {
                        for (int j = i; j >= 0; j--)
                        {
                            if (j >= visualPorts.Count)
                                continue;

                            if (visualEdge.AttemptVisualPortChange(oldVisualPorts[i], visualPorts[j]))
                                break;
                        }
                    }

                    oldVisualPorts[i].Delete();
                }
            }
        }



        public void RefreshParameterVisualPorts()
        {
            RefreshVisualPortList(Node.InputPorts, _inputVisualPorts);
            RefreshVisualPortList(Node.OutputPorts, _outputVisualPorts);
            /*var oldInputVisualPorts = _inputVisualPorts.ToList();
            _inputVisualPorts.Clear();

            foreach (var inputPort in Node.InputPorts)
            {
                var visualPort = oldInputVisualPorts.FirstOrDefault(x => x.Port == inputPort) ?? new VisualPort(this,inputPort);
                _inputVisualPorts.Add(visualPort);

                //oldInputVisualPorts.Remove(visualPort);
            }

            for (int i = 0; i < oldInputVisualPorts.Count; i++)
            {
                var oldVisualPort = oldInputVisualPorts[i];

                //if the port is not included in the new ports, it will be deleted, so try to move its edges
                if (!_inputVisualPorts.Contains(oldVisualPort))
                {
                    foreach (var visualEdge in oldVisualPort.Edges.ToList())
                    {
                        for (int j = i; j >= 0; j--)
                        {
                            if (j >= _inputVisualPorts.Count)
                                continue;

                            if (visualEdge.AttemptVisualPortChange(oldVisualPort, _inputVisualPorts[j]))
                                break;
                        }
                    }
                    
                    oldVisualPort.Delete();
                }
            }

            


            //oldInputVisualPorts.Except(_inputVisualPorts).ForEach(x => x.Delete());
            //if there are any items on this list, it means that previous visual input ports haven't been deleted
            //oldInputVisualPorts.ForEach(x => x.Delete());

            var oldOuputVisualPorts = _outputVisualPorts.ToList();
            _outputVisualPorts.Clear();

            foreach (var outputPort in Node.OutputPorts)
            {
                var visualPort = oldOuputVisualPorts.FirstOrDefault(x => x.Port == outputPort) ?? new VisualPort(this, outputPort);
                _outputVisualPorts.Add(visualPort);
                oldOuputVisualPorts.Remove(visualPort);
            }

            //if there are any items on this list, it means that previous visual input ports haven't been deleted


            //oldOuputVisualPorts.Except(_outputVisualPorts).ForEach(x => x.Delete());
            oldOuputVisualPorts.ForEach(x => x.Delete());*/

            CalculateRectangleSize();

            VisualGraph.ShouldRender = true;
        }



        public void CalculateRectangleSize()
        {
            int maxPortNumber = Math.Max(_inputVisualPorts.Count, _outputVisualPorts.Count);
            //float maxHeight = Math.Max(_nodeLabel.MinimumSize.Y, maxPortNumber*PortSpacing + PortStartingY*2);
            float maxWidth = Math.Max(_nodeLabel.MinimumSize.X, maxPortNumber*PortSpacing + PortStartingX*2);

            //_realRectangle.Max = _realRectangle.Min + new Vector2(_nodeLabel.MinimumSize.X, maxHeight);
            _realRectangle.Max = _realRectangle.Min + new Vector2(maxWidth, _nodeLabel.MinimumSize.Y);

            _realRectangle.Expand(25);
            _realRectangle.Translate(25, 25);
            _nodeRectangle.UpdateSize((int) _realRectangle.Width, (int) _realRectangle.Height);
        }



        public void Update(TimeSpan deltaTime)
        {
            VisualGraph.ShouldRender |= _nodeRectangle.UpdateColors(deltaTime);
            VisualGraph.ShouldRender |= _nodeLabel.UpdateColors(deltaTime);

            //if there's an animation going, we should render
            if (!_animation.IsFinished)
                VisualGraph.ShouldRender = true;

            _transitionRectangle = _animation.UpdateAnimation(deltaTime, _realRectangle);

            for (int i = 0; i < _inputVisualPorts.Count; i++)
                _inputVisualPorts[i].Update(deltaTime, _transitionRectangle, i, _inputVisualPorts.Count);

            for (int i = 0; i < _outputVisualPorts.Count; i++)
                _outputVisualPorts[i].Update(deltaTime, _transitionRectangle, i, _outputVisualPorts.Count);
        }



        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsDeleted)
                return;

            //spriteBatch.Draw(_texture2D, _containerRectangle.Min, Color.White);
            _nodeRectangle.Draw(spriteBatch, _transitionRectangle);//

            if (IsActive)
            {
                _nodeLabel.Draw(spriteBatch, _transitionRectangle, Node.ActuallyDisabledInSubgraph ? 0.6f : 1f);
                foreach (VisualPort visualPort in Ports)
                    visualPort.DrawBorderFill(spriteBatch);

                //if (!_node.IsEnabled)
                //    spriteBatch.Draw(_disabledSign, _transitionRectangle.Max, Color.White);
            }
        }



        public void Translate(Vector2 offset)
        {
            _realRectangle.Min += offset;
            _realRectangle.Max += offset;

            _node.Position = new Point(_node.Position.X + offset.X, _node.Position.Y + offset.Y);
        }



        public IHoverableUnit UpdateUnitHovers(Vector2 mouseModelLocation, RectangleF? selectionRectangle, IHoverableUnit hoveredUnit)
        {
            if (selectionRectangle.HasValue)
                IsHovered = selectionRectangle.Value.Intersects(_transitionRectangle);
            else
                IsHovered = hoveredUnit == null && _transitionRectangle.ContainsPoint(mouseModelLocation);

            UpdatePortSectionHovers(mouseModelLocation, hoveredUnit);


            //var portSectionRectangle = new BoundingRectangle(VisualNode.RealRectangle.Left, _position.Y, VisualNode.RealRectangle.Width, _portShape.Height);
            //IsSectionHovered = portSectionRectangle.ContainsPoint(mouseModelLocation)

            return IsHovered ? this : hoveredUnit;
        }



        private void UpdatePortSectionHovers(Vector2 mouseModelLocation, IHoverableUnit hoveredUnit)
        {
            var height = 25;

            foreach (VisualPort inputVisualPort in InputVisualPorts)
            {
                var portSectionRectangle = new RectangleF(RealRectangle.Left, RealRectangle.Top, RealRectangle.Width, height);
                inputVisualPort.IsSectionHovered = portSectionRectangle.ContainsPoint(mouseModelLocation) && hoveredUnit == null; // && hoveredUnit == this
            }

            IsTopHovered = InputVisualPorts.Any(x => x.IsSectionHovered);

            foreach (VisualPort outputVisualPort in OutputVisualPorts)
            {
                var portSectionRectangle = new RectangleF(RealRectangle.Left, RealRectangle.Bottom - height, RealRectangle.Width, height);
                outputVisualPort.IsSectionHovered = portSectionRectangle.ContainsPoint(mouseModelLocation) && hoveredUnit == null; // && hoveredUnit == this
            }

            IsBottomHovered = OutputVisualPorts.Any(x => x.IsSectionHovered);
        }



        public void ShowProperties()
        {
            _messageManager.Publish(new ShowPropertiesRequest(new NodeInspectorControl(VisualGraph.Control.Services, this, VisualGraph.FileItem), VisualGraph.Control.GraphDocumentControl));
            _messageManager.Publish(new ShowVisualHandles(this));


            //VisualGraph.Control.GraphEditorForm.ShowNodePanel(this);
            //VisualGraph.Control.InteractionHandler.ShowNodeProperties(this);
        }



        /// <summary>
        /// Checks if the current node is parent of the indicated node.
        /// This functions checks only for parenthood - if the nodes are the same, it returns false.
        /// </summary>
        /// <param name="possibleChildNode"></param>
        /// <returns>True if it is a parent, false otherwise.</returns>
        public bool IsParentOf(VisualNode possibleChildNode)
        {
            if (this == possibleChildNode)
                return false;

            foreach (var childrenNode in ChildrenNodes)
            {
                //attention: this is not the same as returning immediately the boolean value
                if (childrenNode == possibleChildNode)
                    return true;

                if (childrenNode.IsParentOf(possibleChildNode))
                    return true;
            }

            return false;
        }



        public void Delete()
        {
            //deselect the node
            Deselect();

            //mark it for deletion
            _nodeStatus = VisualUnitStatus.Deleting;

            //and start the corresponding animation
            _animation = new ShrinkAnimation();
            _animation.AnimationFinished += OnShrinkAnimationFinished;

            //delete all visual edges from the model
            IEnumerable<VisualEdge> visualEdges = Ports.SelectMany(val => val.Edges).ToList();
            foreach (VisualEdge visualEdge in visualEdges)
                visualEdge.Delete();

            //delete the node from the model
            _node.Delete();

            //_node.Graph.RefreshNodeIds();
        }



        private void OnShrinkAnimationFinished(object sender, EventArgs eventArgs)
        {
            _nodeStatus = VisualUnitStatus.Deleted;
            VisualGraph.ShouldRender = true;
        }



        public void ResetPortOrder()
        {
            List<VisualPort> newIns = new List<VisualPort>();
            List<VisualPort> newOuts = new List<VisualPort>();

            foreach (Port port in _node.InputPorts)
                newIns.Add(_inputVisualPorts.Find(val => val.Port == port));

            foreach (Port port in _node.OutputPorts)
                newOuts.Add(_outputVisualPorts.Find(val => val.Port == port));

            _inputVisualPorts = newIns;
            _outputVisualPorts = newOuts;
        }



        public void Disconnect()
        {
            foreach (VisualEdge visualEdge in OutgoingEdges.Union(IngoingEdges).ToList())
            {
                visualEdge.Delete();
            }
        }



        public void SetImpulsePort(bool hasImpulsePort)
        {
            if (hasImpulsePort)
            {
                _node.HasImpulsePort = true;
                _inputVisualPorts.Add(new VisualPort(this, _node.ImpulsePort));
            }
            else
            {
                IEnumerable<VisualPort> visualPorts = _inputVisualPorts.Where(val => val.Port == _node.ImpulsePort);
                foreach (VisualPort visualPort in visualPorts.ToList())
                    visualPort.Delete();

                _node.HasImpulsePort = false;
            }

            VisualGraph.ShouldRender = true;
        }

        #region Properties

        public Object Tooltip
        {
            get
            {
                if (Node.ProcedureAttribute.ObsoleteAttribute != null)
                    return new ToolTipControl(Node.DefaultLabel, Node.ProcedureAttribute.Description, "Obsolete: " + Node.ProcedureAttribute.ObsoleteAttribute.Message);

                return new ToolTipControl(Node.DefaultLabel, Node.ProcedureAttribute.Description);
            }
        }



        public bool IsHovered
        {
            get { return _isHovered && IsActive; }
            set
            {
                if (_isHovered != value && !IsSelected)
                {
                    _nodeRectangle.FillColor = value ? _selectedNodeColor : _nodeColor;

                    if (value)
                        VisualGraph.ShowToolTip(this, _realRectangle.Center);
                    //_nodeLabel.TextColor = value ? Color.White : Color.Black;
                }

                _isHovered = value;
            }
        }



        public bool IsBottomHovered
        {
            get;
            private set;
        }



        public bool IsTopHovered
        {
            get;
            private set;
        }



        protected bool IsActive
        {
            get
            {
                return _nodeStatus == VisualUnitStatus.Normal ||
                       _nodeStatus == VisualUnitStatus.Selected;
            }
        }



        public bool IsSelected
        {
            get { return _nodeStatus == VisualUnitStatus.Selected; }
        }



        public RectangleF RealRectangle
        {
            get { return _realRectangle; }
        }



        public Vector2 Position
        {
            get { return _realRectangle.Min; }
        }



        public String Label
        {
            get { return _nodeLabel.Text; }
            set
            {
                _nodeLabel.Text = value;
                _node.Label = value;
                CalculateRectangleSize();
                VisualGraph.ShouldRender = true;
            }
        }



        public Node Node
        {
            get { return _node; }
        }



        /*public NodeType Type
        {
            get { return _type; }
        }*/



        public List<VisualPort> InputVisualPorts
        {
            get { return _inputVisualPorts; }
        }



        public List<VisualPort> OutputVisualPorts
        {
            get { return _outputVisualPorts; }
        }



        public IEnumerable<VisualPort> Ports
        {
            get { return InputVisualPorts.Union(OutputVisualPorts); }
        }



        public IEnumerable<VisualNode> ChildrenNodes
        {
            get { return OutgoingEdges.Select(x => x.ToVisualPort.VisualNode); }
        }



        public VisualGraph VisualGraph
        {
            get { return _visualGraph; }
        }



        public IEnumerable<VisualEdge> IngoingEdges
        {
            get
            {
                foreach (VisualPort port in InputVisualPorts)
                    foreach (var edge in port.Edges)
                        yield return edge;
            }
        }



        public IEnumerable<VisualEdge> OutgoingEdges
        {
            get
            {
                foreach (VisualPort port in OutputVisualPorts)
                    foreach (var edge in port.Edges)
                        yield return edge;
            }
        }



        public ContentManager Content
        {
            get { return _visualGraph.Content; }
        }



        public bool IsDeleted
        {
            get { return _nodeStatus == VisualUnitStatus.Deleted; }
        }



        /// <summary>
        /// A node is isolated if it has no incoming or outcoming edges
        /// </summary>
        public bool IsIsolated
        {
            get { return _node.IsIsolated; }
        }



        /*public int Id
        {
            get { return _id; }
        }*/

        #endregion
    }
}