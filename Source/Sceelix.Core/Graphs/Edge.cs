using System;

namespace Sceelix.Core.Graphs
{
    public class Edge
    {
        private Port _fromPort;
        private Port _toPort;



        public Edge()
        {
        }



        public Edge(Port fromPort, Port toPort)
        {
            _fromPort = fromPort;
            _toPort = toPort;

            fromPort.Edges.Add(this);
            toPort.Edges.Add(this);
        }



        public void Delete()
        {
            _fromPort.Edges.Remove(this);
            _toPort.Edges.Remove(this);

            Graph.Edges.Remove(this);
        }



        internal void EnsurePortConnection()
        {
            _fromPort.Edges.Add(this);
            _toPort.Edges.Add(this);
        }



        /// <summary>
        /// Verifies if this edge has the same connections as the given edge, i.e.
        /// if they are connecting the same port indices between nodes of equal
        /// guids. Useful to make comparisons between edges of cloned graphs.
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public bool IsStructurallyEqual(Edge edge)
        {
            return FromPort.Node.Guid == edge.FromPort.Node.Guid && ToPort.Node.Guid == edge.ToPort.Node.Guid
                                                                 && FromPort.Index == edge.FromPort.Index && ToPort.Index == edge.ToPort.Index;
        }



        public Port OtherEnd(Port port)
        {
            if (port == _fromPort)
                return _toPort;

            if (port == _toPort)
                return _fromPort;

            return null;
        }



        #region Properties

        public Port FromPort
        {
            get { return _fromPort; }
            set
            {
                _fromPort.Edges.Remove(this);
                _fromPort = value;
                _fromPort.Edges.Add(this);
            }
        }



        public Port ToPort
        {
            get { return _toPort; }
            set
            {
                _toPort.Edges.Remove(this);
                _toPort = value;
                _toPort.Edges.Add(this);
            }
        }



        public Graph Graph => FromPort.Graph;


        public bool Enabled
        {
            get;
            set;
        } = true;


        public Type ObjectType => _fromPort.ObjectType;

        #endregion
    }
}