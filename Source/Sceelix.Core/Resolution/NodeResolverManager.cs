using System.Collections.Generic;
using Sceelix.Annotations;
using Sceelix.Core.Graphs;

namespace Sceelix.Core.Resolution
{
    public class NodeResolverManager
    {
        private static readonly Dictionary<string, NodeResolver> NodeResolvers = AttributeReader.OfStringKeyAttribute<NodeResolverAttribute>().GetInstancesOfType<NodeResolver>();



        public static void Resolve(Graph graph, Node node)
        {
            var guidString = node.ProcedureAttribute.Guid;

            NodeResolver resolver;
            if (NodeResolvers.TryGetValue(guidString, out resolver)) resolver.Resolve(graph, node);
        }
    }
}