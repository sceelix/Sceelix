using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.IO;

namespace Sceelix.Core.Graphs
{
    public class OutputPort : Port
    {
        public OutputPort(string label, Type objectType, Node node)
            : base(label, objectType, node)
        {
        }



        public OutputPort(Output output)
            : base(output.Label, output.EntityType, null)
        {
            IsParameterPort = true;
        }



        public override List<Port> FellowPorts => Node.OutputPorts;



        public override string FullLabel
        {
            get
            {
                if (IsParameterPort) return Node.Parameters.SelectMany(x => x.GetSubtree(true)).First(param => param.OutputPorts.Contains(this)).FullName + "." + Label;

                return Label;
            }
        }



        public bool IsClosed
        {
            get;
            set;
        }


        public override string Nature => "Output";


        /// <inheritdoc />
        public override string Shape => "Circle";



        public override object Clone()
        {
            return new OutputPort(Label, ObjectType, Node) {IsParameterPort = IsParameterPort};
        }



        public Output GenerateOutput()
        {
            return new Output(GateLabel, ObjectType);
        }



        /*public Output ToOutput()
        {
            return new Output(Label, ObjectType);
        }*/
    }
}