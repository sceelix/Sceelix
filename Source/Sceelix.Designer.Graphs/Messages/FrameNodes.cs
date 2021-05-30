using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Graphs;

namespace Sceelix.Designer.Graphs.Messages
{
    class FrameNodes
    {
        private readonly List<Node> _nodes;



        public FrameNodes(List<Node> nodes)
        {
            _nodes = nodes;
        }



        public List<Node> Nodes
        {
            get { return _nodes; }
        }



        protected bool Equals(FrameNodes other)
        {
            return _nodes.SequenceEqual(other._nodes);
        }



        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FrameNodes) obj);
        }



        public override int GetHashCode()
        {
            return (_nodes != null ? _nodes.GetHashCode() : 0);
        }



        public static bool operator ==(FrameNodes left, FrameNodes right)
        {
            return Equals(left, right);
        }



        public static bool operator !=(FrameNodes left, FrameNodes right)
        {
            return !Equals(left, right);
        }
    }
}
