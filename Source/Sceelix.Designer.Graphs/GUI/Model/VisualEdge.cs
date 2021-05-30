using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Core.Data;
using Sceelix.Core.Graphs;
using Sceelix.Designer.Graphs.GUI.Basic;
using Sceelix.Designer.Graphs.GUI.Interactions;
using Sceelix.Designer.Graphs.GUI.Model.Drawing;
using Sceelix.Designer.Graphs.Messages;
using Sceelix.Designer.GUI;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.Utils;
using Color = Microsoft.Xna.Framework.Color;
using RectangleF = Sceelix.Designer.Graphs.GUI.Basic.RectangleF;

namespace Sceelix.Designer.Graphs.GUI.Model
{
    public class VisualEdge : ISelectableUnit, IHoverableUnit
    {
        private readonly Texture2D _disabledSign;
        //public static readonly Color EdgeDefaultColor = new Color(114, 114, 114);

        private readonly Edge _edge;

        private readonly EdgeLine _line;

        private VisualUnitStatus _edgeStatus = VisualUnitStatus.Normal;

        private VisualPort _fromVisualPort;
        private bool _isDragHovered;
        private bool _isHovered;

        private readonly MessageManager _messageManager;
        private VisualPort _toVisualPort;



        public VisualEdge(Edge edge, Dictionary<Port, VisualPort> portMappings)
            : this(edge, portMappings[edge.FromPort], portMappings[edge.ToPort])
        {
        }



        public VisualEdge(Edge edge, VisualPort fromVisualPort, VisualPort toVisualPort)
        {
            _edge = edge;

            _fromVisualPort = fromVisualPort;
            _toVisualPort = toVisualPort;

            fromVisualPort.Edges.Add(this);
            toVisualPort.Edges.Add(this);

            _disabledSign = EmbeddedResources.Load<Texture2D>("Resources/Graphs/DisabledSign.png");
            _line = new EdgeLine(Content) {TransitionSpeed = 300, Color = toVisualPort.VisualGraph.Control.EdgeColor};

            _messageManager = VisualGraph.Control.Services.Get<MessageManager>();

            _messageManager.Register<MarkEdge>(OnMarkEdge);
            _messageManager.Register<MarkPorts>(OnMarkPorts);
        }



        public void Select()
        {
            _edgeStatus = VisualUnitStatus.Selected;
            VisualGraph.ShouldRender = true;
            //_line.Color = Color.Wheat;
        }



        public void Deselect()
        {
            _edgeStatus = VisualUnitStatus.Normal;
            //_line.Color = VisualGraph.Control.EdgeColor;
        }



        private void OnMarkEdge(MarkEdge obj)
        {
            if (obj.Edge == null)
            {
                _line.Color = VisualGraph.Control.EdgeColor;
            }
            else if (obj.Edge.IsStructurallyEqual(this.Edge))
            {
                _line.Color = obj.Color;
            }
        }



        private void OnMarkPorts(MarkPorts obj)
        {
            if (obj.Ports.Any(x => x.IsStructurallyEqual(this.Edge.FromPort)) && obj.Ports.Any(x => x.IsStructurallyEqual(this.Edge.ToPort)))
            {
                _line.Color = obj.Color;
            }
            else
            {
                _line.Color = VisualGraph.Control.EdgeColor;
            }
        }



        public void Update(TimeSpan deltaTime)
        {
            if (_line.Update(deltaTime, _fromVisualPort.CenterPoint, _toVisualPort.CenterPoint))
                VisualGraph.ShouldRender = true; 

            //if (_edgeStatus == VisualUnitStatus.Deleting)
            //Console.WriteLine(_line.Color);

            if (_line.Color == Color.Transparent)
                _edgeStatus = VisualUnitStatus.Deleted;
        }



        public void Draw(SpriteBatch spritebatch)
        {
            _line.DrawLine(spritebatch, IsHovered || IsSelected, !Enabled);

            //if(!Enabled)
            //    spritebatch.Draw(_disabledSign, _line.MidPoint - new Vector2(_disabledSign.Width / 2f, _disabledSign.Height / 2f),Color.White);
        }



        public void Delete()
        {
            //mark it for deletion
            _edgeStatus = VisualUnitStatus.Deleting;
            _line.Color = Color.Transparent;

            //delete from the underlying model
            _edge.Delete();

            //also, remove from the visual ports
            FromVisualPort.Edges.Remove(this);
            ToVisualPort.Edges.Remove(this);

            _line.TransitionSpeed = 400;
        }



        public IHoverableUnit UpdateUnitHovers(Vector2 mouseModelLocation, RectangleF? selectionRectangle, IHoverableUnit hoveredUnit)
        {
            if (!IsActive)
                return hoveredUnit;

            IsHovered = _line.ContainsPoint(mouseModelLocation) && hoveredUnit == null;

            return IsHovered ? this : hoveredUnit;
        }



        public VisualEdge UpdateNodeDragOnEdge(Vector2 mouseModelLocation, VisualEdge hoveredEdge, VisualNode draggedNode)
        {
            if (IsActive)
            {
                bool value = _line.ContainsPoint(mouseModelLocation) && hoveredEdge == null;

                if (_isDragHovered != value)
                {
                    if (value)
                    {
                        if (EdgeDrawer.CheckIfValidTargetPort(_fromVisualPort, draggedNode.InputVisualPorts[0])
                            && EdgeDrawer.CheckIfValidTargetPort(draggedNode.OutputVisualPorts[0], _toVisualPort))
                        {
                            _line.Color = Color.Green;
                        }
                        else
                        {
                            _line.Color = Color.Red;
                        }
                    }
                    else
                    {
                        _line.Color = VisualGraph.Control.EdgeColor;
                    }

                    _isDragHovered = value;
                }
            }

            return _isDragHovered ? this : hoveredEdge;
        }



        public void ShowProperties()
        {
        }



        public VisualPort OtherEnd(VisualPort visualPort)
        {
            if (visualPort == _fromVisualPort)
                return _toVisualPort;

            if (visualPort == _toVisualPort)
                return _fromVisualPort;

            return null;
        }



        public bool AttemptVisualPortChange(VisualPort originalPort, VisualPort newPort)
        {
            var otherEnd = OtherEnd(originalPort);

            if (EdgeDrawer.CheckIfValidTargetPort(newPort, otherEnd))
            {
                if (originalPort == _fromVisualPort)
                {
                    FromVisualPort = newPort;
                }
                else
                {
                    ToVisualPort = newPort;
                }

                return true;
            }

            return false;
        }

        #region Properties

        public VisualPort FromVisualPort
        {
            get { return _fromVisualPort; }
            set
            {
                _edge.FromPort = value.Port;

                _fromVisualPort.Edges.Remove(this);
                _fromVisualPort = value;
                _fromVisualPort.Edges.Add(this);
            }
        }



        public VisualPort ToVisualPort
        {
            get { return _toVisualPort; }
            set
            {
                _edge.ToPort = value.Port;

                _toVisualPort.Edges.Remove(this);
                _toVisualPort = value;
                _toVisualPort.Edges.Add(this);
            }
        }



        public VisualGraph VisualGraph
        {
            get { return FromVisualPort.VisualGraph; }
        }



        public ContentManager Content
        {
            get { return VisualGraph.Content; }
        }



        public Object Tooltip
        {
            get { return new ToolTipControl("Edge", string.Format("{0} ({1}) --> {2} ({3})", _edge.FromPort.Label, Entity.GetDisplayName(_edge.FromPort.ObjectType), _edge.ToPort.Label, Entity.GetDisplayName(_edge.ToPort.ObjectType))); }
        }



        public bool IsHovered
        {
            get { return _isHovered && IsActive; }
            set
            {
                if (_isHovered != value && !IsSelected)
                {
                    VisualGraph.ShouldRender = true;
                    //_line.Color = value ? Color.Wheat : VisualGraph.Control.EdgeColor;

                    if (value)
                        VisualGraph.ShowToolTip(this, _fromVisualPort.CenterPoint + (_toVisualPort.CenterPoint - _fromVisualPort.CenterPoint)/2f);
                }


                _isHovered = value;
            }
        }



        public bool IsDragHovered
        {
            get { return _isDragHovered && IsActive; }
        }



        public bool IsSelected
        {
            get { return _edgeStatus == VisualUnitStatus.Selected; }
        }



        protected bool IsActive
        {
            get
            {
                return _edgeStatus == VisualUnitStatus.Normal ||
                       _edgeStatus == VisualUnitStatus.Selected;
            }
        }



        public bool IsDeleted
        {
            get { return _edgeStatus == VisualUnitStatus.Deleted; }
        }



        public bool Enabled
        {
            get { return _edge.Enabled; }
            set { _edge.Enabled = value; }
        }



        public Type ObjectType
        {
            get { return _edge.ObjectType; }
        }



        public EdgeLine EdgeLine
        {
            get { return _line; }
        }



        public Edge Edge
        {
            get { return _edge; }
        }

        #endregion
    }
}