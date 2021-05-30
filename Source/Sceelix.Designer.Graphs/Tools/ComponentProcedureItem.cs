using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core;
using Sceelix.Core.Annotations;
using Sceelix.Core.Environments;
using Sceelix.Core.Graphs;
using Sceelix.Core.Procedures;
using Sceelix.Designer.ProjectExplorer.Management;

namespace Sceelix.Designer.Graphs.Tools
{
    public class ComponentProcedureItem : IProcedureItem
    {
        private readonly string _category;
        private readonly FileItem _componentFileItem;
        private readonly string _description;
        private readonly string _tags;



        public ComponentProcedureItem(FileItem componentFileItem, ProcedureEnvironment visualGraphEnvironment)
        {
            _componentFileItem = componentFileItem;

            var procedureAttribute = GraphLoad.LoadHeader(_componentFileItem.ProjectRelativePath, visualGraphEnvironment);

            _tags = procedureAttribute.Tags;
            _category = !String.IsNullOrWhiteSpace(procedureAttribute.Category) ? procedureAttribute.Category : "Components";
            _description = !String.IsNullOrWhiteSpace(procedureAttribute.Description) ? procedureAttribute.Description : "No description available.";
            //let's override the tags for now

            // + String.Join(", ", GetAutomaticTags(procedure));
        }



        public string Label
        {
            get { return _componentFileItem.Name; }
        }



        public String Tags
        {
            get { return _tags; }
        }



        public string Category
        {
            get { return _category; }
        }



        public string Description
        {
            get { return _description; }
        }



        /// <summary>
        /// Generates a node from the signature (including ports and definitions) and adds it to the graph
        /// </summary>
        /// <param name="position"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        public Node GenerateNode(Point position, IProcedureEnvironment environment)
        {
            ComponentNode componentNode;


            /*GraphProcedure graphProcedure = environment.LoadGraphProcedure(_componentFileItem.ProjectRelativePath);
            //graphProcedure.ProcedureAttribute.Guid = componentFileItem.Guid.ToString();
            graphProcedure.Name = _componentFileItem.Name;

            if (graphProcedure is AggregativeGraphProcedure)
            {
                componentNode = new AggregativeComponentNode((AggregativeGraphProcedure)graphProcedure, position, _componentFileItem.ProjectRelativePath);

                AggregativeComponentNode aggregativeComponentNode = (AggregativeComponentNode)componentNode;
                aggregativeComponentNode.AddFilter(aggregativeComponentNode.PossibleOutputFilters.First(val => val.Label == "All"));
            }
            else*/
            //componentNode = new ComponentNode(graphProcedure, position, _componentFileItem.ProjectRelativePath);
            componentNode = new ComponentNode(_componentFileItem.ProjectRelativePath, GraphLoad.LoadFromPath(_componentFileItem.ProjectRelativePath, environment), position);


            return componentNode;
        }



        private IEnumerable<String> GetAutomaticTags(Procedure procedure)
        {
            return procedure.Inputs.Select(val => val.EntityType).Union(procedure.Outputs.Select(val => val.EntityType)).Select(val => val.Name).Distinct();
        }
    }
}