using Microsoft.Xna.Framework;
using Sceelix.Core.Graphs;

namespace Sceelix.Designer.Graphs.Messages
{
    public class MarkEdge
    {
        private readonly Color _color;
        private readonly Edge _edge;



        public MarkEdge(Edge edge)
        {
            _edge = edge;
        }



        public MarkEdge(Edge edge, Color color)
        {
            _edge = edge;
            _color = color;
        }



        public Edge Edge
        {
            get { return _edge; }
        }



        public Color Color
        {
            get { return _color; }
        }
    }
}