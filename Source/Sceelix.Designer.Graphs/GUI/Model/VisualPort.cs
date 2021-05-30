using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Core.Annotations;
using Sceelix.Core.Graphs;
using Sceelix.Designer.Graphs.GUI.Basic;
using Sceelix.Designer.Graphs.GUI.Basic.Animations;
using Sceelix.Designer.Graphs.GUI.Model.Drawing;
using Sceelix.Designer.Graphs.Inspector.Ports;
using Sceelix.Designer.Graphs.Messages;
using Sceelix.Designer.GUI;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Extensions;

namespace Sceelix.Designer.Graphs.GUI.Model
{
    public enum PortEmphasis
    {
        Positive,
        Negative,
        Unsure,
        None
    }

    public class VisualPort : IHoverableUnit, ISelectableUnit
    {
        public const int SelectedSize = 10;
        private readonly HashSet<VisualEdge> _edges = new HashSet<VisualEdge>();
        private readonly Port _port;
        private readonly PortLabel _portLabel;

        ///private int _index;
        private readonly PortShape _portShape;

        private readonly VisualNode _visualNode;
        private PortEmphasis _emphasis;
        private bool _isHovered;
        private bool _isSectionHovered;
        private readonly MessageManager _messageManager;

        private VisualUnitStatus _portStatus = VisualUnitStatus.Normal;

        //private BoundingRectangle _lastNodeRectangle;
        private Vector2 _position;
        private int? _lastObjectCount;



        public VisualPort(VisualNode visualNode, Port port)
        {
            _visualNode = visualNode;
            _port = port;

            _portShape = new PortShape(port, Content);
            _portLabel = new PortLabel(port, Content, _portShape.Width);

            _messageManager = VisualGraph.Control.Services.Get<MessageManager>();

            _messageManager.Register<MarkPort>(OnMarkPort);
            _messageManager.Register<MarkPorts>(OnMarkPorts);
        }



        protected List<VisualPort> FellowPorts
        {
            get { return _port is InputPort ? _visualNode.InputVisualPorts : _visualNode.OutputVisualPorts; }
        }



        public void Select()
        {
            _portStatus = VisualUnitStatus.Selected;
            //_portShape.FillColor = Color.Tan;
            _portShape.ExtraSize = SelectedSize;
        }



        public void Deselect()
        {
            _portStatus = VisualUnitStatus.Normal;
            //_portShape.FillColor = Color.White;
            _portShape.ExtraSize = 0;
        }



        private void OnMarkPort(MarkPort obj)
        {
            if (obj.Port == null)
            {
                _portShape.FillColor = Color.White;
                _lastObjectCount = null;
            }
            else if (obj.Port.IsStructurallyEqual(this.Port))
            {
                _portShape.FillColor = obj.Color;
                _lastObjectCount = obj.Count;
            }
        }



        private void OnMarkPorts(MarkPorts obj)
        {
            if (obj.Ports.Any(x => x.IsStructurallyEqual(this.Port)))
            {
                _portShape.FillColor = obj.Color;
            }
            else
            {
                _portShape.FillColor = Color.White;
            }
        }



        public void Update(TimeSpan deltaTime, RectangleF nodeRectangle, int index, int portNumber)
        {
            //to spare some calculations...
            //if (!_lastNodeRectangle.Equals(nodeRectangle))
            _position = RecalculatePosition(nodeRectangle, index, portNumber);

            VisualGraph.ShouldRender |= _portShape.UpdateColors(deltaTime);

            //_lastNodeRectangle = nodeRectangle;
        }



        /// <summary>
        /// Draws the port.
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void DrawBorderFill(SpriteBatch spriteBatch)
        {
            _portShape.DrawBorderFill(spriteBatch, _position);

            if (VisualGraph.ShowPortLabels)
                _portLabel.Draw(spriteBatch, _position);

            if (VisualGraph.Settings.ViewEntityCount.Value && _lastObjectCount.HasValue)
            {
                _portLabel.Draw(spriteBatch, _position, _lastObjectCount.Value);
                
            }
        }



        /// <summary>
        /// Recalculates the position of the port based on the rectangle boundary of the containing node
        /// </summary>
        /// <param name="nodeRectangle"></param>
        /// <param name="index"></param>
        /// <param name="portNumber"></param>
        /// <returns></returns>
        private Vector2 RecalculatePosition(RectangleF nodeRectangle, int index, int portNumber)
        {
            /*float xPosition = _port is InputPort
                                  ? nodeRectangle.Min.X - _portShape.Width / 2f + 2         //the +2 concerns the width of the rectangle border
                                  : nodeRectangle.Max.X - _portShape.Width / 2f - 2;

            float ySpacing = (nodeRectangle.Height - VisualNode.PortStartingY * 2f) / portNumber;

            float yPosition = nodeRectangle.Min.Y + VisualNode.PortStartingY + index * ySpacing + ySpacing / 2f - _portShape.Height / 2f;

            return new Vector2(xPosition, yPosition);*/

            float yPosition = _port is InputPort
                ? nodeRectangle.Min.Y - _portShape.Height/2f + 2 //the +2 concerns the width of the rectangle border
                : nodeRectangle.Max.Y - _portShape.Height/2f - 2;

            float xSpacing = (nodeRectangle.Width - VisualNode.PortStartingX*2f)/portNumber;

            float xPosition = nodeRectangle.Min.X + VisualNode.PortStartingX + index*xSpacing + xSpacing/2f - _portShape.Width/2f;

            return new Vector2(xPosition, yPosition);
        }



        //protected virtual float XOffset {}


        public IHoverableUnit UpdateUnitHovers(Vector2 mouseModelLocation, IHoverableUnit hoveredUnit)
        {
            //hover the port only if the mouse is on it and nothing else is hovered
            IsHovered = _portShape.Rectangle.Contains((int) mouseModelLocation.X, (int) mouseModelLocation.Y) && (hoveredUnit == null);

            // && (hoveredUnit == VisualNode)

            return IsHovered ? this : hoveredUnit;
        }



        /*private bool Contains(Vector2 mouseModelLocation)
        {
            float radius = _portShape.Width / 2f;

            Vector2 distanceVector = CenterPoint - mouseModelLocation;

            return distanceVector.Length() <= radius;
        }*/



        public void Delete()
        {
            foreach (VisualEdge edge in _edges.ToList())
                edge.Delete();

            FellowPorts.Remove(this);
        }



        public void ShowProperties()
        {
            VisualGraph.Control.Services.Get<MessageManager>().Publish(new ShowPropertiesRequest(new PortInspectorControl(this, VisualGraph.FileItem), VisualGraph.Control));
        }



        public void CycleStateNext()
        {
            if (_port.PortState == PortState.Normal)
            {
                _port.PortState = PortState.Blocked;
                _visualNode.VisualGraph.AlertForFileChange(true);
            }
            else if (_port.PortState == PortState.Blocked)
            {
                _port.PortState = PortState.Gate;
                _visualNode.VisualGraph.AlertForFileChange(true);
            }
            else if (_port.PortState == PortState.Gate)
            {
                _port.PortState = PortState.Normal;
                _visualNode.VisualGraph.AlertForFileChange(false);
            }
        }



        public void CycleStatePrevious()
        {
            if (_port.PortState == PortState.Normal)
            {
                _port.PortState = PortState.Gate;

                _visualNode.VisualGraph.AlertForFileChange(true);
            }
            else if (_port.PortState == PortState.Blocked)
            {
                _port.PortState = PortState.Normal;
                _visualNode.VisualGraph.AlertForFileChange(true);
            }
            else if (_port.PortState == PortState.Gate)
            {
                _port.PortState = PortState.Blocked;
                _visualNode.VisualGraph.AlertForFileChange(false);
            }
        }

        #region Properties

        public string Label
        {
            get { return _port.Label; }
        }



        public VisualNode VisualNode
        {
            get { return _visualNode; }
        }



        public HashSet<VisualEdge> Edges
        {
            get { return _edges; }
        }



        public VisualGraph VisualGraph
        {
            get { return VisualNode.VisualGraph; }
        }



        public Port Port
        {
            get { return _port; }
        }



        public ContentManager Content
        {
            get { return _visualNode.Content; }
        }



        public Vector2 Position
        {
            get { return _position; }
        }



        public PortShape PortShape
        {
            get { return _portShape; }
        }



        public Vector2 CenterPoint
        {
            get { return _position + new Vector2(_portShape.Width/2f, _portShape.Width/2f); }
        }



        public Object Tooltip
        {
            get
            {
                var attribute = Port.ObjectType.GetCustomAttribute<EntityAttribute>();
                var portType = attribute != null ? attribute.Name : Port.ObjectType.Name;

                var description = String.IsNullOrWhiteSpace(Port.Description) ? String.Empty : "<br/>" + Port.Description;

                /*if (!String.IsNullOrWhiteSpace(Port.Description))
                    return new ToolTipControl(_port.Label, "Type: " + portType + "<br/>"+ Port.Description);
                else*/
                return new ToolTipControl(_port.Label, "Type: " + portType + description);
            } //_port.UniqueLabel
        }



        public bool IsHovered
        {
            get { return _isHovered; }
            set
            {
                if (_isHovered != value && !IsSelected)
                {
                    _portShape.ExtraSize = value ? SelectedSize : 0;
                    //_portShape.FillColor = value ? Color.Tan : Color.White;
                    if (value)
                        VisualGraph.ShowToolTip(this, CenterPoint);
                }

                _isHovered = value;
            }
        }



        public bool IsSectionHovered
        {
            get { return _isSectionHovered; }
            set
            {
                if (_isSectionHovered != value && !IsSelected && !IsHovered) //
                {
                    _portShape.ExtraSize = value ? SelectedSize : 0;
                    VisualGraph.ShouldRender = true;
                    //_portShape.FillColor = value ? Color.Tan : Color.White;
                    //if (value)
                    //    VisualGraph.ShowToolTip(this, CenterPoint);
                }

                _isSectionHovered = value;
            }
        }



        public bool IsSelected
        {
            get { return _portStatus == VisualUnitStatus.Selected; }
        }



        public PortEmphasis Emphasis
        {
            get { return _emphasis; }
            set
            {
                if (_emphasis != value)
                {
                    switch (value)
                    {
                        case PortEmphasis.Positive:
                            _portShape.Animation = new PortAnimation(1, 10, 30);
                            _portShape.BorderColor = Color.DarkGreen;
                            break;
                        case PortEmphasis.Negative:
                            _portShape.Animation = null;
                            _portShape.BorderColor = Color.Red;
                            break;
                        case PortEmphasis.Unsure:
                            _portShape.Animation = new PortAnimation(0.4f, 5, 15);
                            _portShape.BorderColor = Color.Orange;
                            break;
                        case PortEmphasis.None:
                            _portShape.Animation = null;
                            _portShape.BorderColor = Color.Black;
                            break;
                    }

                    _emphasis = value;
                }
            }
        }



        /*public bool IsEmphasized
        {
            get { return _isEmphasized; }
            set
            {
                if (_isEmphasized != value)
                    _portShape.BorderColor = value ? Color.Green : Color.Black;

                _isEmphasized = value;
                _portShape.Animate = value;
            }
        }*/

        #endregion
    }
}