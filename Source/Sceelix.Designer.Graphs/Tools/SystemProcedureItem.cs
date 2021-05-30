using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.Environments;
using Sceelix.Core.Graphs;
using Sceelix.Core.Procedures;

namespace Sceelix.Designer.Graphs.Tools
{
    public class SystemProcedureItem : IProcedureItem
    {
        private readonly ProcedureAttribute _procedureAttribute;
        private readonly Type _type;
        private readonly string _tags;



        public SystemProcedureItem(Type type)
        {
            _type = type;

            _procedureAttribute = ProcedureAttribute.GetAttributeForProcedure(type);
            _tags = _procedureAttribute.Tags;

            //let's override the tags for now
            if (String.IsNullOrEmpty(_tags))
            {
                SystemProcedure procedure = (SystemProcedure) Activator.CreateInstance(_type);
                _tags = String.Join(", ", procedure.Tags);
            }
        }



        public string Guid
        {
            get { return _procedureAttribute.Guid; }
        }



        public string Author
        {
            get { return _procedureAttribute.Author; }
        }



        public string HexColor
        {
            get { return _procedureAttribute.HexColor; }
        }



        public Type ProcedureType
        {
            get { return _type; }
        }



        public bool Obsolete
        {
            get { return _procedureAttribute.ObsoleteAttribute != null; }
        }



        public string Label
        {
            get
            {
                string label = String.IsNullOrEmpty(_procedureAttribute.Label) ? _type.Name : _procedureAttribute.Label;

                return _procedureAttribute.ObsoleteAttribute != null ? label + " [Deprecated]" : label;
            }
        }



        public string Category
        {
            get { return _procedureAttribute.Category; }
        }



        public string Description
        {
            get { return _procedureAttribute.Description; }
        }



        /*public string[] Category
        {
            get { return _category; }
        }*/



        public String Tags
        {
            get { return _tags; } // String.Join(", ", _category); }
        }



        /// <summary>
        /// Generates a node from the signature (including ports and definitions) and adds it to the graph
        /// </summary>
        /// <param name="position"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        public Node GenerateNode(Point position, IProcedureEnvironment environment)
        {
            SystemProcedure procedure = (SystemProcedure) Activator.CreateInstance(_type);

            return new SystemNode(procedure, position);
        }



        private IEnumerable<String> GetAutomaticTags(Procedure procedure)
        {
            return procedure.Inputs.Select(val => val.EntityType).Union(procedure.Outputs.Select(val => val.EntityType)).Select(val => val.Name).Distinct();
        }



        /*public void GenerateHtml()
        {
            var folderPath = "E:\\My .NET Projects\\Sceelix Support\\Docs\\Node Reference";
            if(!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileLocation = Path.Combine(folderPath, ProcedureType.FullName.Replace(".", "-") + "|" + _procedureAttribute.Guid  + ".html");

            SystemProcedure procedure = (SystemProcedure)Activator.CreateInstance(_type);
            var systemNode = new SystemNode(procedure, new Point());
            
            File.WriteAllText(fileLocation, systemNode.GetHtmlDocCode());
        }*/
    }
}