using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;
using Sceelix.Logging;
using Sceelix.Paths.Data;
using Sceelix.Paths.Parameters;

namespace Sceelix.Paths.Procedures
{
    /// <summary>
    /// Divides paths into subpaths according to specific criteria.
    /// </summary>
    [Procedure("5d95c0d1-140a-4735-90f2-97e8a34d7a7a", Label = "Path Divide", Category = "Path")]
    public class PathDivideProcedure : SystemProcedure
    {
        /// <summary>
        /// The path to be divided.
        /// </summary>
        private readonly SingleInput<PathEntity> _input = new SingleInput<PathEntity>("Input");

        /// <summary>
        /// The divided paths, according to the defined groups.
        /// </summary>
        private readonly Output<PathEntity> _output = new Output<PathEntity>("Output");


        /// <summary>
        /// Criteria by which the path should be divided. If none is indicated, the whole set of edges will be considered.
        /// </summary>
        private readonly ListParameter<PathDivideParameter> _parameterDivideGroups = new ListParameter<PathDivideParameter>("Groups");

        /// <summary>
        /// If true, each one of the edges of the path will be placed into a separate path entity.
        /// </summary>
        private readonly OptionalListParameter<PathSeparateParameter> _parameterSeparate = new OptionalListParameter<PathSeparateParameter>("Separate");



        protected override void Run()
        {
            var originalPathEntity = _input.Read();

            List<IEnumerable<PathEdge>> edgeGroups = new List<IEnumerable<PathEdge>>();
            edgeGroups.Add(originalPathEntity.Edges);

            //each parameter will do its grouping and return enumerables of groups, which will be added to the list of enumerables
            foreach (var pathDivideParameter in _parameterDivideGroups.Items)
                edgeGroups = edgeGroups.SelectMany(edgeGroup => pathDivideParameter.PerformGroupBy(edgeGroup)).ToList();

            foreach (var edgeGroup in edgeGroups)
                if (_parameterSeparate.HasValue)
                {
                    foreach (var edge in edgeGroup) _output.Write(_parameterSeparate.Value.Process(originalPathEntity, edge));
                }
                else
                {
                    //the deepclone is important
                    //otherwise we cannot perform clean properly
                    //because despite we are separating the edges, the vertices are still shared
                    //so they need to be cloned
                    PathEntity derivedPath = (PathEntity) new PathEntity(edgeGroup).DeepClone();
                    derivedPath.AdjustScope(originalPathEntity.BoxScope);
                    derivedPath.CleanConnections();
                    originalPathEntity.Attributes.SetAttributesTo(derivedPath.Attributes);

                    _output.Write(derivedPath);
                }
        }



        #region Abstract Parameter

        public abstract class PathDivideParameter : CompoundParameter
        {
            protected PathDivideParameter(string label)
                : base(label)
            {
            }



            protected internal abstract IEnumerable<IEnumerable<PathEdge>> PerformGroupBy(IEnumerable<PathEdge> edges);
        }

        #endregion

        #region Attribute

        /// <summary>
        /// Divides the paths by attribute value, i.e. building sets of edges that share the same value.
        /// </summary>
        /// <seealso cref="PathDivideParameter" />
        public class PathDivideAttributeSetParameter : PathDivideParameter
        {
            /// <summary>
            /// Value to divide the paths by.
            /// </summary>
            private readonly ObjectParameter _parameterValue = new ObjectParameter("Value") {EntityEvaluation = true};



            public PathDivideAttributeSetParameter()
                : base("Attribute")
            {
            }



            protected internal override IEnumerable<IEnumerable<PathEdge>> PerformGroupBy(IEnumerable<PathEdge> edges)
            {
                return edges.GroupBy(x => _parameterValue.Get(x));
            }
        }

        #endregion

        #region Direction

        /// <summary>
        /// Divides the paths into sets of edges that share the same (approximate) normal direction.
        /// </summary>
        /// <seealso cref="PathDivideParameter" />
        public class PathDivideDirectionSetParameter : PathDivideParameter
        {
            public PathDivideDirectionSetParameter()
                : base("Direction")
            {
            }



            protected internal override IEnumerable<IEnumerable<PathEdge>> PerformGroupBy(IEnumerable<PathEdge> edges)
            {
                return edges.GroupBy(x => x.Direction.Round());
            }
        }

        #endregion

        #region Adjacency

        /// <summary>
        /// Divides the paths by their vertex connections.
        /// </summary>
        /// <seealso cref="PathDivideParameter" />
        public class PathDivideAdjacencySetParameter : PathDivideParameter
        {
            public PathDivideAdjacencySetParameter()
                : base("Adjacency")
            {
            }



            protected internal override IEnumerable<IEnumerable<PathEdge>> PerformGroupBy(IEnumerable<PathEdge> edges)
            {
                HashSet<PathEdge> startingEdges = new HashSet<PathEdge>(edges);
                List<HashSet<PathEdge>> groups = new List<HashSet<PathEdge>>();

                while (startingEdges.Count > 0)
                {
                    PathEdge extractedEdge = startingEdges.First();
                    //startingEdges.Remove(extractedEdge);

                    HashSet<PathEdge> groupSet = new HashSet<PathEdge>();
                    Queue<PathEdge> queue = new Queue<PathEdge>();
                    queue.Enqueue(extractedEdge);

                    while (!queue.IsEmpty())
                    {
                        var currentEdge = queue.Dequeue();
                        foreach (PathVertex vertex in currentEdge.Vertices)
                        foreach (PathEdge otherEdge in vertex.Edges)
                            //if the edge is still in the starting edges, it means
                            //we still haven't analyzed it
                            if (startingEdges.Contains(otherEdge))
                            {
                                groupSet.Add(otherEdge);
                                queue.Enqueue(otherEdge);
                                startingEdges.Remove(otherEdge);
                            }
                    }

                    groups.Add(groupSet);
                }

                return groups;
            }
        }

        #endregion

        #region Size

        /// <summary>
        /// Divides the paths by number of edges or vertices, so that they don't exceed the requested size.
        /// </summary>
        /// <seealso cref="PathDivideParameter" />
        public class PathDivideSizeParameter : PathDivideParameter
        {
            /// <summary>
            /// Type of element to divide by:<br/>
            /// <b>Edges</b> means that the paths will not exceed the indicated edge count.<br/>
            /// <b>Vertices</b> means that the paths will not exceed the indicated vertex count.<br/>
            /// </summary>
            private readonly ChoiceParameter _parameterType = new ChoiceParameter("Type", "Edges", "Edges", "Vertices");

            /// <summary>
            /// The maximum allowed count of edges or vertices to keep in each resulting path.
            /// </summary>
            private readonly IntParameter _parameterCount = new IntParameter("Count", 1000);



            public PathDivideSizeParameter()
                : base("Size")
            {
            }



            protected internal override IEnumerable<IEnumerable<PathEdge>> PerformGroupBy(IEnumerable<PathEdge> edges)
            {
                switch (_parameterType.Value)
                {
                    case "Edges":
                        return SplitList(edges.ToList(), _parameterCount.Value);

                    case "Vertices":
                    {
                        int maxCount = _parameterCount.Value;

                        int edgesExceeedingVertexCount = 0;
                        List<List<PathEdge>> totalList = new List<List<PathEdge>>();

                        List<PathEdge> currentList = new List<PathEdge>();
                        int currentCount = 0;

                        //iterate over the edges and add them to the list until the max value
                        //is achieved, then add it to the list of lists
                        foreach (PathEdge edge in edges)
                        {
                            var vertexCount = edge.Vertices.Count();
                            if (vertexCount > maxCount)
                            {
                                edgesExceeedingVertexCount++;
                            }
                            else
                            {
                                if (currentCount + vertexCount > maxCount)
                                {
                                    totalList.Add(currentList);

                                    currentList = new List<PathEdge> {edge};
                                    currentCount = vertexCount;
                                }
                                else
                                {
                                    currentList.Add(edge);
                                    currentCount += vertexCount;
                                }
                            }
                        }

                        //if the last list we were dealing with has items
                        //and has not been added to the list, add it now
                        if (currentList.Count > 0 && (!totalList.Any() || totalList.Last() != currentList))
                            totalList.Add(currentList);


                        //if any of the edges had its vertex count larger than the allowed value, warn the user
                        if (edgesExceeedingVertexCount > 0)
                        {
                            var message = edgesExceeedingVertexCount == 1
                                ? "There was 1 edge that exceeded the maximum count value alone. This has not been included."
                                : string.Format("There were {0} edges that exceeded the maximum count value alone. These have not been included.", edgesExceeedingVertexCount);

                            ProcedureEnvironment.GetService<ILogger>().Log(message, LogType.Warning);
                        }

                        return totalList;
                    }
                }


                return new IEnumerable<PathEdge>[0];
            }



            public static List<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
            {
                var list = new List<List<T>>();

                for (int i = 0; i < locations.Count; i += nSize) list.Add(locations.GetRange(i, Math.Min(nSize, locations.Count - i)));

                return list;
            }
        }

        #endregion
    }
}