using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Sceelix.Core.Annotations;
using Sceelix.Core.Code;
using Sceelix.Core.Data;
using Sceelix.Core.Environments;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Core.Resources;
using Sceelix.Extensions;
using ParameterInfo = Sceelix.Core.Parameters.Infos.ParameterInfo;

namespace Sceelix.Core.Graphs
{
    internal enum NodeState
    {
        Enabled,
        DisabledForSubgraphs,
        Disabled
    }

    public abstract class Node
    {
        //private bool _isSourceNode;
        //private List<AttributeLink> _variableLinks = new List<AttributeLink>();



        protected Node()
        {
        }



        /*protected Node(XmlNode xmlNode, Environment environment)
        {
            _label = xmlNode.GetAttributeOrDefault<String>("Label");//.Attributes["Label"].InnerText;
            _procedureGuid = new Guid(xmlNode.Attributes["NodeTypeGUID"].InnerText);//xmlNode.GetAttributeOrDefault<Guid>("NodeTypeGUID", Guid());

            _position = Point.Parse(xmlNode.Attributes["Location"].InnerText, CultureInfo.InvariantCulture);
        }*/



        //By definition, when you create a reference through its procedure, only values are assigned
        protected Node(Procedure procedure, Point position)
        {
            Position = position;

            Parameters = procedure.GetDefaultArguments().ToList();
            //_variableLinks = procedure.GetDefaultVariableLinks().ToList();

            foreach (Input input in procedure.Inputs.Select(x => x.Input))
                InputPorts.Add(new InputPort(input.Label, input.EntityType, this, input.InputNature) {Description = input.Description, IsOptional = input.IsOptional}); //or UniqueLabel?  input.ToInputPort()   

            foreach (Output output in procedure.Outputs.Select(x => x.Output))
                OutputPorts.Add(new OutputPort(output.Label, output.EntityType, this) {Description = output.Description}); //or UniqueLabel?

            RefreshParameterPorts();

            /*if (_inputPorts.Count == 0)
                _isSourceNode = true;*/

            //LoadProcedureSignature(procedure);
        }



        public bool CanBeDisabled => InputPorts.Count == 0 || HasMatchingPorts();


        public Guid Guid
        {
            get;
            set;
        } = Guid.NewGuid();


        public bool IsEnabled
        {
            get;
            set;
        } = true;


        public bool UseCache
        {
            get;
            set;
        }



        public virtual void Analyse(IProcedureEnvironment loadEnvironment)
        {
        }



        /// <summary>
        /// Creates a fully parameterized procedure to which this node refers to.
        /// </summary>
        /// <param name="procedureEnvironment"></param>
        /// <param name="masterProcedure"></param>
        /// <returns></returns>
        public virtual Procedure CreateParameterizedProcedure(IProcedureEnvironment procedureEnvironment, Procedure masterProcedure)
        {
            Procedure procedure = CreateProcedure(procedureEnvironment);
            //procedure.Environment = procedureEnvironment;
            procedure.Parent = masterProcedure;
            procedure.UseCache = UseCache;

            if (HasImpulsePort)
                procedure.SetupImpulsePorts(ImpulsePort.Label);

            foreach (ParameterInfo argument in Parameters)
                procedure.Parameters[argument.Label].Set(argument, masterProcedure, procedure);

            procedure.UpdateParameterPorts();

            /*foreach (var inputPort in _inputPorts.Where(x => x.IsParameterPort).OfType<InputPort>())
            {
                var abstractInput = inputPort.ToInput();
                abstractInput.Owner = procedure;
                procedure.Inputs.Add(abstractInput);
            }

            foreach (var outputPort in _outputPorts.Where(x => x.IsParameterPort).OfType<OutputPort>())
            {
                var output = outputPort.ToOutput();
                output.Owner = procedure;
                procedure.Outputs.Add(output);
            }*/

            //_inputPorts.AddRange(_arguments.SelectMany(x => x.GetInputPorts()));

            //foreach (AttributeLink variableLink in _variableLinks.Where(variableLink => variableLink.HasLink()))
            //    procedure.Attributes.Link(variableLink.Label, dataBlock.Attributes[variableLink.TargetVariable]);

            return procedure;
        }



        /// <summary>
        /// Creates an instance of the procedure to which this node refers to.
        /// </summary>
        /// <param name="loadEnvironment"></param>
        /// <returns></returns>
        public abstract Procedure CreateProcedure(IProcedureEnvironment loadEnvironment);



        /// <summary>
        /// Creates a disconnected (no edge connections are cloned) clone of the node.
        /// </summary>
        /// <returns></returns>
        public virtual Node DeepClone()
        {
            var clone = (Node) MemberwiseClone();

            clone.Graph = null;
            clone.Id = -1;

            clone.Position = Position;
            clone.Guid = Guid.NewGuid();
            clone.Label = Label;
            clone.IsEnabled = IsEnabled;
            clone.DisableInSubgraphs = DisableInSubgraphs;

            if (ImpulsePort != null)
            {
                clone.ImpulsePort = (Port) ImpulsePort.Clone();
                clone.ImpulsePort.Node = this;
            }

            clone.InputPorts = InputPorts.Select(x => (Port) x.Clone()).ToList();
            clone.OutputPorts = OutputPorts.Select(x => (Port) x.Clone()).ToList();
            clone.Parameters = Parameters.Select(x => (ParameterInfo) x.Clone()).ToList();

            clone.InputPorts.ForEach(x => x.Node = clone);
            clone.OutputPorts.ForEach(x => x.Node = clone);

            return clone;
        }



        public void Delete()
        {
            //remove all edges that are connected to this node
            foreach (Edge connectedEdge in ConnectedEdges.ToList())
                connectedEdge.Delete();

            //remove self
            Graph.Nodes.Remove(this);
        }



        /// <summary>
        /// Clones the node interface
        /// </summary>
        /// <returns></returns>
        /*public Node SimpleClone()
        {
            Node clone = (Node)MemberwiseClone();

            //id, position, _label, isSourceNode are copied naturally, because they are type values
            clone._arguments = _arguments.Select(val => val.DeepClone()).ToList();

            clone._inputPorts = _inputPorts.Select(val => val.DeepClone()).ToList();


            clone._variableLinks = new List<Argument>();

                    private int _id;
        private Point _position;
        private String _label;
        private Graph _graph;
        
        private List<Argument> _arguments = new List<Argument>();
        private List<AttributeLink> _variableLinks = new List<AttributeLink>();

        private readonly List<Port> _inputPorts = new List<Port>();
        private readonly List<Port> _outputPorts = new List<Port>();

        private bool _isSourceNode;
        private Port _impulsePort;

        }*/
        public string GenerateCSharpCallCode()
        {
            CodeBuilder codeBuilder = new CodeBuilder();

            var varName = GetSpecificCSharpCallCode(codeBuilder);
            foreach (var parameter in Parameters) parameter.CreateCSharpCode(codeBuilder, varName);

            foreach (var port in InputPorts.Where(x => !x.IsParameterPort))
            {
                var portEntityVariable = Parameter.GetIdentifier(port.ObjectType.Name).FirstLetterToLower();
                codeBuilder.AppendLine($"{varName}.Inputs[{port.Label.Quote()}].Enqueue({portEntityVariable});");
            }

            codeBuilder.AppendLine($"{varName}.Execute();");

            foreach (var port in OutputPorts.Where(x => !x.IsParameterPort))
            {
                var identifier = Parameter.GetIdentifier(port.Label).FirstLetterToLower();
                codeBuilder.AppendLine($"var {identifier}Data = {varName}.Outputs[{port.Label.Quote()}].Dequeue();");
            }

            return codeBuilder.ToString();
        }



        public abstract IEnumerable<Assembly> GetReferencedAssemblies(IProcedureEnvironment loadEnvironment);

        public abstract IEnumerable<string> GetReferencedPaths(IProcedureEnvironment environment);


        public abstract string GetSpecificCSharpCallCode(CodeBuilder codeBuilder);



        private bool HasMatchingPorts()
        {
            if (InputPorts.Count != OutputPorts.Count)
                return false;

            for (int i = 0; i < InputPorts.Count; i++)
                if (InputPorts[i].ObjectType != OutputPorts[i].ObjectType)
                    return false;

            return true;
        }



        /// <summary>
        /// Checks if the node has any references to the given parameter.
        /// </summary>
        /// <param name="label">Label of the parameter.</param>
        /// <returns>True it has any reference or false otherwise.</returns>
        public virtual bool HasReferenceToParameter(string label)
        {
            return Parameters.Any(argument => argument.HasReferenceToParameter(label));
        }



        /// <summary>
        /// Loads the default arguments, links and ports from the procedure signature (or interface).
        /// </summary>
        /// <param name="procedure"></param>
        /*protected void LoadProcedureSignature(Procedure procedure)
        {
            _arguments = procedure.GetDefaultArguments().ToList();
            _variableLinks = procedure.GetDefaultVariableLinks().ToList();

            foreach (AbstractInput input in procedure.GetSubInputs())
                _inputPorts.Add(new InputPort(input.Label, input.Type, this, input.InputNature));

            foreach (Output output in procedure.GetSubOutputs())
                _outputPorts.Add(new OutputPort(output.Label, output.Type, this));
        }*/
        internal void Initialize(Graph graph, int id)
        {
            Graph = graph;
            Id = id;

            Label = string.IsNullOrEmpty(Label) ? ProcedureAttribute.Label : Label;
        }



        /// <summary>
        /// Verifies if this node is the parent (directly or indirectly) of the given node. 
        /// If the nodes are the same, it returns false.
        /// </summary>
        /// <param name="otherNode"></param>
        /// <returns>True if there is a direct or indirect connection from this node to the given one, or false otherwise (this includes if they are the same node).</returns>
        public bool IsParentOf(Node otherNode)
        {
            //if the othernode is a source node, it means it has no parents, obviously
            if (otherNode.IsSourceNode)
                return false;

            HashSet<Guid> checkedNodes = new HashSet<Guid> {Guid};
            Queue<Node> uncheckedNodes = new Queue<Node>(OutgoingEdges.Select(x => x.ToPort.Node));

            while (uncheckedNodes.Count > 0)
            {
                var subNode = uncheckedNodes.Dequeue();

                //avoid loops by checking if a node was included already
                if (!checkedNodes.Contains(subNode.Guid))
                {
                    if (subNode.Guid == otherNode.Guid)
                        return true;

                    foreach (Node childNode in subNode.OutgoingEdges.Select(x => x.ToPort.Node))
                        uncheckedNodes.Enqueue(childNode);

                    checkedNodes.Add(subNode.Guid);
                }
            }

            return false;
        }



        /// <summary>
        /// Verifies if this node has the same guid as the given one.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool IsStructurallyEqual(Node node)
        {
            return Guid == node.Guid;
        }



        /// <summary>
        /// Refactors a parameter label from the node.
        /// </summary>
        /// <param name="oldLabel">Old label of the parameter.</param>
        /// <param name="newLabel">New label of the parameter.</param>
        public virtual void RefactorParameterReferences(string oldLabel, string newLabel)
        {
            foreach (ParameterInfo argument in Parameters)
                argument.RefactorGlobalParameters(oldLabel, newLabel);
        }



        /// <summary>
        /// Renames node and parameter references to the indicated, new filepaths.
        /// </summary>
        /// <param name="resourceManager"></param>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        public virtual void RefactorReferencedPaths(ResourceManager resourceManager, string origin, string destination)
        {
            //Arguments.ForEach(x => x.RefactorReferencedFolder(procedureEnvironment,origin, destination));

            /*foreach (ParameterInfo info in Arguments.SelectMany(x => x.GetThisAndSubtree(false)))
            {
                if (info is FolderParameterInfo || info is FileParameterInfo)
                {
                    PrimitiveParameterInfo<String> parameterInfo = info as PrimitiveParameterInfo<String>;

                    if (parameterInfo.FixedValue.Contains(origin))
                        parameterInfo.FixedValue = parameterInfo.FixedValue.Replace(origin, destination);
                }
            }*/
        }



        public void RefreshParameterPorts()
        {
            //remove all ports from the actual port list and update it afterwards by reading from the parameters
            InputPorts.RemoveAll(x => x.IsParameterPort);
            var newinputPorts = Parameters.SelectMany(x => x.SubInputPortTree).ToList();
            newinputPorts.ForEach(x => x.Node = this);
            InputPorts.AddRange(newinputPorts);

            //same goes for output ports
            OutputPorts.RemoveAll(x => x.IsParameterPort);
            var newoutputPorts = Parameters.SelectMany(x => x.SubOutputPortTree).ToList();
            newoutputPorts.ForEach(x => x.Node = this);
            OutputPorts.AddRange(newoutputPorts);

            /*for (int index = 0; index < _inputPorts.Count; index++)
                _inputPorts[index].UniqueLabel = _inputPorts[index].Label + " #" + index;

            for (int index = 0; index < _outputPorts.Count; index++)
                _outputPorts[index].UniqueLabel = _outputPorts[index].Label + " #" + index;*/
        }



        public abstract void WriteSpecificXML(XmlWriter writer);

        #region Properties

        public List<Port> InputPorts
        {
            get;
            private set;
        } = new List<Port>();


        public List<Port> OutputPorts
        {
            get;
            private set;
        } = new List<Port>();


        public List<ParameterInfo> Parameters
        {
            get;
            private set;
        } = new List<ParameterInfo>();


        public abstract Type ProcedureType
        {
            get;
        }


        /*public List<AttributeLink> VariableLinks
        {
            get { return _variableLinks; }
        }*/


        public abstract string ShapeName
        {
            get;
        }


        public abstract ProcedureAttribute ProcedureAttribute
        {
            get;
        }


        public Point Position
        {
            get;
            set;
        }


        public string Label
        {
            get;
            set;
        }


        public virtual string DefaultLabel => string.Empty;


        public IEnumerable<Port> Ports => InputPorts.Union(OutputPorts);


        public Graph Graph
        {
            get;
            private set;
        }



        public IEnumerable<Edge> IngoingEdges
        {
            get { return InputPorts.SelectMany(port => port.Edges); }
        }



        public IEnumerable<Edge> OutgoingEdges
        {
            get { return OutputPorts.SelectMany(port => port.Edges); }
        }



        public IEnumerable<Edge> ConnectedEdges => OutgoingEdges.Union(IngoingEdges);


        public int Id
        {
            get;
            set;
        }



        /// <summary>
        /// A node is isolated if it has no incoming or outcoming edges
        /// </summary>
        public bool IsIsolated
        {
            get { return Ports.All(val => val.Edges.Count == 0); }
        }



        /// <summary>
        /// A node is called a "TransferNode" if it has exactly one input and one output, both being of the same type
        /// </summary>
        public bool IsTransferNode => InputPorts.Count == 1 && OutputPorts.Count == 1 && InputPorts[0].ObjectType == OutputPorts[0].ObjectType;


        public Type TransferNodeType => InputPorts[0].ObjectType;


        /// <summary>
        /// Indicates if the node does not have native input ports. 
        /// Activated impulse ports do not count - even if it is activated, this will return true. 
        /// </summary>
        public bool IsSourceNode => InputPorts.Count == 0 || HasImpulsePort;
        //private set { _isSourceNode = value; }


        /// <summary>
        /// Indicates if the node has currently any input ports (impulse ports included).
        /// </summary>
        public bool HasInputPorts => InputPorts.Count > 0;


        public virtual bool IsObsolete => false;



        /// <summary>
        /// Indicates if this node has an impulse port.
        /// If set to true, a new impulse port will be assigned (and made available through the ImpulsePort property) add added to the list of input ports.
        /// If set to false, the ImpulsePort property will be made null and the port removed from the list of inputs.
        /// </summary>
        public bool HasImpulsePort
        {
            get { return ImpulsePort != null; }
            set
            {
                if (value)
                {
                    List<string> portNames = InputPorts.Select(val => val.Label).ToList();
                    string impulsePortName = StringExtension.FindNonConflict(portNames.Contains, val => "Impulse Port " + val);
                    ImpulsePort = new InputPort(impulsePortName, typeof(IEntity), this, InputNature.Single, true);
                    InputPorts.Add(ImpulsePort);
                }
                else
                {
                    InputPorts.Remove(ImpulsePort);
                    ImpulsePort = null;
                }
            }
        }



        /// <summary>
        /// Returns the created impulse port, of null, if it has not been set.
        /// </summary>
        public Port ImpulsePort
        {
            get;
            private set;
        }
        /*set { 
                _impulsePort = value;

                if (_impulsePort != null)
                    _inputPorts.Add(_impulsePort);
                else
                    
            }*/


        public virtual string NodeTypeName => GetType().Name;


        /// <summary>
        /// Indicates if the node should not be executed in the parent graph is being used as a subgraph.
        /// In practice, only available for source nodes at this point.
        /// </summary>
        public bool DisableInSubgraphs
        {
            get;
            set;
        } = true;


        /// <summary>
        /// Result of combinining the user intent through the DisableInSubgraph property
        /// and the necessary conditions for it to be disabled (i.e. must be a source node with no impulse ports).
        /// </summary>
        public bool ActuallyDisabledInSubgraph => DisableInSubgraphs && !HasInputPorts;


        /*public bool IsSource
        {
            get { return InputPorts.Count == 0; }
        }*/

        #endregion
    }
}