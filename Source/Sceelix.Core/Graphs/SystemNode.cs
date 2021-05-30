using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Sceelix.Core.Annotations;
using Sceelix.Core.Code;
using Sceelix.Core.Environments;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;

namespace Sceelix.Core.Graphs
{
    public class SystemNode : Node
    {
        private string _nativeTags;
        private Type _procedureType;



        public SystemNode()
        {
        }



        /*public SystemNode(XmlNode xmlNode, Environment environment)
            : base(xmlNode, environment)
        {
            String s = xmlNode.Attributes["ProcedureType"].InnerText;
            _procedureType = Type.GetType(s);

            Procedure procedure = ((Procedure) Activator.CreateInstance(_procedureType));
            procedure.Environment = environment;
            
            LoadProcedureSignature(procedure);
            LoadProcedureXMLDetails(xmlNode);
        }*/



        public SystemNode(SystemProcedure procedure, Point position)
            : base(procedure, position)
        {
            _procedureType = procedure.GetType();
            _nativeTags = string.Join(", ", procedure.Tags);
        }



        public SystemNode(Type procedureType, string nativeTags)

        {
            _procedureType = procedureType;
            _nativeTags = nativeTags;
        }



        public override string DefaultLabel => ProcedureAttribute.Label;


        public override bool IsObsolete => ProcedureAttribute.ObsoleteAttribute != null;


        public override ProcedureAttribute ProcedureAttribute => ProcedureAttribute.GetAttributeForProcedure(_procedureType);


        public override Type ProcedureType => _procedureType;


        public override string ShapeName => "Rectangle01";


        /// <summary>
        /// Gets the tags of the node. Tries getting the ones defined in the procedure attribute. If it can't it fetches the dynamically defined ones.
        /// </summary>
        public string Tags => string.IsNullOrEmpty(ProcedureAttribute.Tags) ? _nativeTags : ProcedureAttribute.Tags;



        public override Procedure CreateProcedure(IProcedureEnvironment loadEnvironment)

        {
            var procedure = (Procedure) Activator.CreateInstance(_procedureType);
            procedure.Environment = loadEnvironment;
            return procedure;
        }



        public override Node DeepClone()
        {
            var systemNode = (SystemNode) base.DeepClone();

            systemNode._procedureType = _procedureType;
            systemNode._nativeTags = _nativeTags;

            return systemNode;
        }



        public override IEnumerable<Assembly> GetReferencedAssemblies(IProcedureEnvironment loadEnvironment)
        {
            yield return _procedureType.Assembly;
        }



        public override IEnumerable<string> GetReferencedPaths(IProcedureEnvironment environment)
        {
            return Parameters.SelectMany(val => val.GetReferencedPaths());
        }



        public override string GetSpecificCSharpCallCode(CodeBuilder codeBuilder)
        {
            var procedureFullType = _procedureType.FullName;
            var varName = _procedureType.Name.FirstLetterToLower() + Id;

            codeBuilder.AppendLine($"var {varName} = new {procedureFullType}();");
            return varName;
        }



        public override void WriteSpecificXML(XmlWriter writer)
        {
            writer.WriteAttributeString("ProcedureType", _procedureType.Name);
        }
    }
}