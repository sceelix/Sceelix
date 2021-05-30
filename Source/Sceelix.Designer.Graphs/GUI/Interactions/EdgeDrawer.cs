using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Core.Graphs;
using Sceelix.Designer.Graphs.GUI.Model;
using Sceelix.Designer.Graphs.GUI.Model.Drawing;

namespace Sceelix.Designer.Graphs.GUI.Interactions
{
    /// <summary>
    /// The temporary class that is used when an edge is being drawn between 2 ports
    /// </summary>
    public class EdgeDrawer
    {
        private readonly EdgeLine _line;
        private readonly VisualPort _startPort;



        public EdgeDrawer(VisualPort startPort)
        {
            _startPort = startPort;
            _line = new EdgeLine(startPort.Content);
        }



        private VisualGraph VisualGraph
        {
            get { return _startPort.VisualGraph; }
        }



        public VisualPort StartPort
        {
            get { return _startPort; }
        }



        public bool IsValidTargetPort
        {
            get;
            private set;
        }



        public void Update(TimeSpan deltaTime, Vector2 positionEnd)
        {
            VisualPort hoveredPort = VisualGraph.FindHoveredPort();
            if (hoveredPort != null)
            {
                IsValidTargetPort = CheckIfValidTargetPort(_startPort, hoveredPort);
                if (IsValidTargetPort)
                    _line.Color = DoubleCheckOnTypeCompatibility(VisualGraph, _startPort, hoveredPort) ? Color.DarkGreen : Color.Orange;
                else
                    _line.Color = Color.Red;
            }
            else
                _line.Color = VisualGraph.Control.EdgeColor;

            if(_line.Update(deltaTime, _startPort.CenterPoint, positionEnd))
                VisualGraph.ShouldRender = true;
        }



        public void Draw(SpriteBatch spriteBatch)
        {
            _line.DrawLine(spriteBatch, false);
        }



        public static bool CheckIfValidTargetPort(VisualPort startPort, VisualPort candidateTargetPort)
        {
            return candidateTargetPort != startPort && //must not be the same port
                   candidateTargetPort.VisualNode != startPort.VisualNode && //must be on different nodes
                   candidateTargetPort.Port.GetType() != startPort.Port.GetType() && //one must be output and the other an input port
                   !startPort.VisualGraph.ContainsConnection(startPort, candidateTargetPort) && //connection musn't yet exist
                   (candidateTargetPort.Port.ObjectType.IsAssignableFrom(startPort.Port.ObjectType) ||
                    startPort.Port.ObjectType.IsAssignableFrom(candidateTargetPort.Port.ObjectType)); //targetport must be of supertype or type of the start port e.g. shape -> sceelix entity
            //!candidateTargetPort.VisualNode.IsParentOf(_startPort.VisualNode);        //target port should not come before the starting port (to avoid cycles)
        }



        public static bool DoubleCheckOnTypeCompatibility(VisualGraph visualGraph, VisualPort startPort, VisualPort candidateTargetPort)
        {
            return candidateTargetPort.Port.ObjectType.IsAssignableFrom(startPort.Port.ObjectType); //start port must be of supertype or type of the targetport e.g. sceelix entity -> shape
        }



        public void CreateEdge(VisualPort targetPort)
        {
            //for coherency, create the edge always from the output port of the first to the input port of the second
            /*VisualPort firstPort = _startPort.Port is OutputPort ? _startPort : targetPort;
            VisualPort secondPort = _startPort.Port is InputPort ? _startPort : targetPort;

            Edge edge = new Edge(firstPort.Port, secondPort.Port);
            VisualGraph.Graph.AddEdge(edge);

            VisualEdge visualEdge = new VisualEdge(edge, firstPort, secondPort);
            VisualGraph.VisualEdges.Add(visualEdge);
            */
            AddEdge(_startPort, targetPort);
        }



        public static void AddEdge(VisualPort startPort, VisualPort targetPort)
        {
            //for coherency, create the edge always from the output port of the first to the input port of the second
            VisualPort firstPort = startPort.Port is OutputPort ? startPort : targetPort;
            VisualPort secondPort = startPort.Port is InputPort ? startPort : targetPort;

            Edge edge = new Edge(firstPort.Port, secondPort.Port);
            startPort.VisualGraph.Graph.AddEdge(edge);

            VisualEdge visualEdge = new VisualEdge(edge, firstPort, secondPort);
            startPort.VisualGraph.VisualEdges.Add(visualEdge);

            visualEdge.VisualGraph.AlertForFileChange(true);
        }
    }
}