using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.IO;

namespace Sceelix.Core.Graphs
{
    public class InputPort : Port
    {
        public InputPort(string label, Type objectType, Node node, InputNature inputNature, bool isImpulse = false)
            : base(label, objectType, node)
        {
            InputNature = inputNature;
            IsImpulse = isImpulse;
        }



        public InputPort(Input input, InputNature inputNature)
            : base(input.Label, input.EntityType, null)
        {
            IsParameterPort = true;
            InputNature = inputNature;
        }



        public override List<Port> FellowPorts => Node.InputPorts;



        public override string FullLabel
        {
            get
            {
                if (IsParameterPort) return Node.Parameters.SelectMany(x => x.GetSubtree(true)).First(param => param.InputPorts.Contains(this)).FullName + "." + Label;

                return Label;
            }
        }



        public InputNature InputNature
        {
            get;
        }


        public bool IsImpulse
        {
            get;
        }


        public bool IsOptional
        {
            get;
            set;
        }


        public override string Nature => InputNature.ToString();


        /// <inheritdoc />
        public override string Shape => InputNature == InputNature.Collective ? "Square" : "Circle";


        public override string ToolTipTitle => Label + " (" + InputNature + ")";



        public override object Clone()
        {
            return new InputPort(Label, ObjectType, Node, InputNature) {IsParameterPort = IsParameterPort, Description = Description, IsOptional = IsOptional};
        }



        public Input ToGateInput()
        {
            if (InputNature == InputNature.Single)
                return new SingleInput(GateLabel, ObjectType) {IsOptional = IsOptional};

            return new CollectiveInput(GateLabel, ObjectType) {IsOptional = IsOptional};
        }



        public Input ToInput()
        {
            if (InputNature == InputNature.Single)
                return new SingleInput(Label, ObjectType);

            return new CollectiveInput(Label, ObjectType);
        }
    }
}