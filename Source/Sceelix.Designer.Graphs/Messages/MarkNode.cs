using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Sceelix.Core.Graphs;

namespace Sceelix.Designer.Graphs.Messages
{
    public class MarkNode
    {
        private readonly Color _color;
        private readonly Node _node;



        public MarkNode(Node node)
        {
            _node = node;
        }



        public MarkNode(Node node, Color color)
        {
            _node = node;
            _color = color;
        }



        public Node Node
        {
            get { return _node; }
        }



        public Color Color
        {
            get { return _color; }
        }
    }

    public enum MarkNodeMode
    {
        Inclusive, //all the nodes in the list are colored in the indicated color, otherwise clear
        Exclusive //all the nodes in the list are colored in the indicated color, otherwise leave as it is
        //Clear all the nodes in the list
        //nodesinlist -> clear/set color
        //nopdes not in list -> clear/set color
    }

    public class MarkNodes
    {
        private readonly Color _color;
        private readonly List<Node> _nodes;



        public MarkNodes(List<Node> nodes, Color color)
        {
            _nodes = nodes;
            _color = color;
        }



        public List<Node> Nodes
        {
            get { return _nodes; }
        }



        public Color Color
        {
            get { return _color; }
        }
    }
}