using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Sceelix.Core.Parameters;
using Sceelix.Extensions;

namespace Sceelix.Core.Graphs
{
    public enum PortState
    {
        Normal,
        Blocked,
        Gate
    }

    public abstract class Port : ICloneable
    {
        protected Port(string label, Type objectType, Node node)
        {
            Label = label;

            ObjectType = objectType;
            Node = node;

            Edges = new HashSet<Edge>();
        }



        public virtual string ToolTipTitle => Label;


        public abstract object Clone();



        public void Delete()
        {
            foreach (Edge edge in Edges.ToList())
                edge.Delete();

            FellowPorts.Remove(this);
        }



        /// <summary>
        /// Verifies if this port is located on the same node (with the same guid) and index as another node. 
        /// Useful to make comparisons between ports of cloned graphs.
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool IsStructurallyEqual(Port port)
        {
            return Node.Guid == port.Node.Guid && Index == port.Index && Nature == port.Nature;
        }



        /// <summary>
        /// I'm making a duplicate function for the other type of port, because they will soon have other distinct 
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <param name="ports"></param>
        public static void LoadPorts(XmlNode xmlNode, string nature, List<Port> ports)
        {
            XmlNodeList inputPortsList = xmlNode[nature].GetElementsByTagName("Port");

            foreach (XmlNode inputPortNode in inputPortsList)
            {
                int id = inputPortNode.GetAttributeOrDefault<int>("id");
                if (id < ports.Count)
                {
                    string stateString = inputPortNode.GetAttributeOrDefault<string>("state");

                    string gateLabel = inputPortNode.GetAttributeOrDefault<string>("GateLabel");
                    if (gateLabel != null)
                    {
                        ports[id].PortState = PortState.Gate;
                        ports[id].GateLabel = gateLabel;
                    }

                    if (stateString != null)
                        ports[id].PortState = (PortState) Enum.Parse(typeof(PortState), stateString);
                }
            }
        }



        #region Properties

        public string Label
        {
            get;
        }
        //_label;


        public string Identifier => Parameter.GetIdentifier(Label);


        /*public string UniqueLabel
        {
            get; set;
        }*/
        //_label;}

        /*public string UniqueLabel
        {
            get { return _augmentationIndex != -1 ? Augmentation.GetUniqueLabel(_label, _augmentationIndex) : _label; }
        }*/

        /*public int Index
        {
            get { return _index; }
            set { _index = value; }
        }*/

        /*public bool IsGate
        {
            get { return _isGate; }
            set { _isGate = value; }
        }*/


        public PortState PortState
        {
            get;
            set;
        } = PortState.Normal;


        public Node Node
        {
            get;
            internal set;
        }


        public HashSet<Edge> Edges
        {
            get;
        }


        public Graph Graph => Node.Graph;



        public int Index
        {
            get
            {
                List<Port> fellowPorts = FellowPorts;
                for (int index = 0; index < fellowPorts.Count; index++)
                {
                    var port = fellowPorts[index];
                    if (port == this)
                        return index;
                }

                return -1;
            }
        }



        public abstract List<Port> FellowPorts
        {
            get;
        }


        /// <summary>
        /// Indicates the shape of this port: "Square" (for collective nodes) and "Circle" (for single nodes)
        /// </summary>
        public abstract string Shape
        {
            get;
        }


        public Type ObjectType
        {
            get;
            set;
        }


        public string GateLabel
        {
            get;
            set;
        }


        public bool IsParameterPort
        {
            get;
            set;
        }


        /// <summary>
        /// For input ports, values are "Single" and "Collective".
        /// For output ports, values are only "Output"
        /// </summary>
        public abstract string Nature
        {
            get;
        }


        public string Description
        {
            get;
            set;
        }


        public abstract string FullLabel
        {
            get;
        }

        #endregion
    }
}