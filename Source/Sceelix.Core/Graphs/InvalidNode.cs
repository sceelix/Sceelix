using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Core.Environments;
using Sceelix.Core.IO;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;

namespace Sceelix.Core.Graphs
{
    public class InvalidNode : SystemNode
    {
        private readonly string _description;

        private readonly string _label;
        private Guid _procedureGuid;



        public InvalidNode(string guid, Point position, XmlElement xmlNode, string nodeType)
        {
            Position = position;
            XMLNode = xmlNode;
            NodeType = nodeType;
            _procedureGuid = new Guid(guid);

            int inputPortCount = xmlNode["InputPorts"].GetAttributeOrDefault<int>("Count");
            int outputPortCount = xmlNode["OutputPorts"].GetAttributeOrDefault<int>("Count");

            for (int i = 0; i < inputPortCount; i++)
                InputPorts.Add(new InputPort("Input " + i, typeof(IEntity), this, InputNature.Single));

            for (int i = 0; i < outputPortCount; i++)
                OutputPorts.Add(new OutputPort("Output " + i, typeof(IEntity), this));

            if (nodeType == "SystemNode")
            {
                ShapeName = "Rectangle01";
                var typeName = xmlNode.GetAttributeOrDefault("ProcedureType", string.Empty);
                if (!string.IsNullOrWhiteSpace(typeName))
                {
                    var simpleClassName = typeName.Split(',').First();

                    _label = simpleClassName.Split('.').Last();
                    _description = "Could not find type '" + simpleClassName + "'.\nThe procedure may have been moved, deleted or simply not loaded.";
                }
            }
            else
            {
                ShapeName = "Rectangle03";
                var typeName = xmlNode.GetAttributeOrDefault("RelativePath", string.Empty);
                if (!string.IsNullOrWhiteSpace(typeName))
                {
                    _label = typeName.Split('\\').Last();
                    _description = "Could not find graph '" + _label + "'.\nThe graph may have been moved, deleted or simply not loaded.";
                }
            }
        }



        public override string DefaultLabel => _label + "?";


        public string NodeType
        {
            get;
        }


        public override ProcedureAttribute ProcedureAttribute => new ProcedureAttribute(_procedureGuid.ToString()) {HexColor = "a50b00", Label = "Invalid!", Description = _description};


        public override Type ProcedureType => typeof(InvalidProcedure);


        public override string ShapeName
        {
            get;
        }


        public XmlElement XMLNode
        {
            get;
        }



        public override Procedure CreateProcedure(IProcedureEnvironment loadEnvironment)
        {
            return new InvalidProcedure(_description);
        }



        public void FixComponentNodePath(string path)
        {
            XMLNode.Attributes["RelativePath"].InnerText = path;
        }



        public override IEnumerable<Assembly> GetReferencedAssemblies(IProcedureEnvironment loadEnvironment)
        {
            yield break;
        }



        public override IEnumerable<string> GetReferencedPaths(IProcedureEnvironment environment)
        {
            yield break;
        }
    }
}