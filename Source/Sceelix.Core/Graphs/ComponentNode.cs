using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Sceelix.Core.Annotations;
using Sceelix.Core.Code;
using Sceelix.Core.Environments;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;
using Sceelix.Helpers;
using ParameterInfo = Sceelix.Core.Parameters.Infos.ParameterInfo;

namespace Sceelix.Core.Graphs
{
    public class ComponentNode : Node
    {
        protected DateTime _lastReadTime;
        protected ProcedureAttribute _procedureAttribute;
        protected string _projectRelativePath;



        protected ComponentNode()
        {
        }



        public ComponentNode(GraphProcedure procedure, Point position, string projectRelativePath)
            : base(procedure, position)
        {
            _projectRelativePath = projectRelativePath;
            _lastReadTime = DateTime.Now;

            //unlike system nodes, the procedure attribute is saved within the graph file, so it is not an attribute of the type that can be read at any time
            _procedureAttribute = procedure.ProcedureAttribute;


            //_outputTags = procedure.OutputTags;
            //procedure.OutputTags
        }



        public ComponentNode(string projectRelativePath, Graph graph, Point position)
        {
            _projectRelativePath = projectRelativePath;
            _lastReadTime = DateTime.Now;
            _procedureAttribute = graph.ProcedureReference;
            _procedureAttribute.Label = Path.GetFileNameWithoutExtension(projectRelativePath);


            Position = position;

            foreach (ParameterInfo parameterInfo in graph.ParameterInfos)
                Parameters.Add((ParameterInfo) parameterInfo.Clone());

            //foreach (AttributeInfo variableInfo in graph.VariableInfos)
            //    VariableLinks.Add(new AttributeLink(variableInfo.Label,variableInfo.Type,variableInfo.IsPublic,variableInfo.Forward));

            foreach (Node node in graph.Nodes)
            {
                foreach (InputPort port in node.InputPorts.OfType<InputPort>().Where(val => val.PortState == PortState.Gate))
                    if (InputPorts.All(x => x.Label != port.GateLabel))
                        InputPorts.Add(new InputPort(port.GateLabel, port.ObjectType, this, port.InputNature) {IsOptional = port.IsOptional});

                foreach (OutputPort port in node.OutputPorts.OfType<OutputPort>().Where(val => val.PortState == PortState.Gate))
                    if (OutputPorts.All(x => x.Label != port.GateLabel))
                        OutputPorts.Add(new OutputPort(port.GateLabel, port.ObjectType, this));
            }
        }



        /*public ComponentNode(XmlNode xmlNode, Environment environment)
            : base(xmlNode, environment)
        {
            _projectRelativePath = xmlNode.Attributes["RelativePath"].InnerText;

            GraphProcedure graphProcedure = environment.LoadGraphProcedure(_projectRelativePath, _procedureGuid.ToString());

            //GraphProcedure graphProcedure = GraphProcedure.Load(_projectRelativePath, environment);
            LoadProcedureSignature(graphProcedure);

            Argument.LoadArguments(xmlNode, Arguments);
            AttributeLink.LoadVariableLinks(xmlNode, VariableLinks);

            _procedureAttribute = graphProcedure.ProcedureAttribute;
        }*/


        public override string DefaultLabel => Path.GetFileNameWithoutExtension(_projectRelativePath);


        public override bool IsObsolete => _procedureAttribute.ObsoleteAttribute != null;



        public override void Analyse(IProcedureEnvironment loadEnvironment)
        {
            /*Graph graph = GraphIO.LoadFromPath(Path.Combine(loadEnvironment.projectFolder, _projectRelativePath), loadEnvironment);

            base.Analyse(loadEnvironment);

            _procedureAttribute = graph.ProcedureReference;*/
        }



        public override Procedure CreateProcedure(IProcedureEnvironment loadEnvironment)
        {
            return GraphProcedure.FromPath(loadEnvironment, _projectRelativePath);
        }



        public override IEnumerable<Assembly> GetReferencedAssemblies(IProcedureEnvironment loadEnvironment)
        {
            Graph graph = GraphLoad.LoadFromPath(_projectRelativePath, loadEnvironment); //loadEnvironment.Resources.GetFullPath()

            return graph.GetReferencedAssemblies(loadEnvironment);
        }



        public override IEnumerable<string> GetReferencedPaths(IProcedureEnvironment environment)
        {
            Graph graph = GraphLoad.LoadFromPath(_projectRelativePath, environment); //environment.Resources.GetFullPath()

            HashSet<string> referencedFiles = new HashSet<string>(graph.GetReferencedPaths(environment));

            //add the file itself
            referencedFiles.Add(_projectRelativePath);

            //add the argument references as well
            referencedFiles.UnionWith(Parameters.SelectMany(val => val.GetReferencedPaths()));

            return referencedFiles;
        }



        public override string GetSpecificCSharpCallCode(CodeBuilder codeBuilder)
        {
            var varName = "componentNode" + Id;

            codeBuilder.AppendLine($"var {varName} = GraphProcedure.FromPath(procedureEnvironment, {_projectRelativePath.Quote()});");
            return varName;
        }



        public override void WriteSpecificXML(XmlWriter writer)
        {
            writer.WriteAttributeString("RelativePath", PathHelper.ToUniversalPath(_projectRelativePath));
        }



        /// <summary>
        /// Renames node and parameter references to the indicated, new filepaths.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /*NO NEED FOR THIS AS THE GRAPH LOADER DOES THIS AUTOMATICALLY
         * 
         * public override void RefactorReferencedPaths(string origin, string destination)
        {
            base.RefactorReferencedPaths(origin, destination);

            if(_projectRelativePath.Contains(origin))
                _projectRelativePath = _projectRelativePath.Replace(origin, destination);
        }*/

        #region Properties

        public override Type ProcedureType
        {
            get { return typeof(GraphProcedure); }
        }



        public override string ShapeName => "Rectangle03";



        public string ProjectRelativePath
        {
            get { return _projectRelativePath; }
            set { _projectRelativePath = value; }
        }



        public DateTime LastReadTime
        {
            get { return _lastReadTime; }
            set { _lastReadTime = value; }
        }



        public override ProcedureAttribute ProcedureAttribute => _procedureAttribute;

        #endregion
    }
}