using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Sceelix.Core.Graphs;

namespace Sceelix.Designer.Graphs.Messages
{
    public class MarkPort
    {
        private readonly Color _color;
        private readonly int _count;
        private readonly Port _port;



        public MarkPort(Port port)
        {
            _port = port;
        }



        public MarkPort(Port port, Color color, int count)
        {
            _port = port;
            _color = color;
            _count = count;
        }



        public Port Port
        {
            get { return _port; }
        }



        public Color Color
        {
            get { return _color; }
        }



        public int Count
        {
            get { return _count; }
        }
    }

    public class MarkPorts
    {
        private readonly Color _color;
        private readonly List<Port> _ports;



        public MarkPorts(List<Port> ports, Color color)
        {
            _ports = ports;
            _color = color;
        }



        public List<Port> Ports
        {
            get { return _ports; }
        }



        public Color Color
        {
            get { return _color; }
        }
    }
}