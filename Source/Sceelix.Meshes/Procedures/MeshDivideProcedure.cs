using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;
using Sceelix.Logging;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Parameters;

namespace Sceelix.Meshes.Procedures
{
    /// <summary>
    /// Divides meshes into submeshes according to specific criteria.
    /// </summary>
    [Procedure("c08e468c-4d16-45c0-91f9-7d93320b7c89", Label = "Mesh Divide", Category = "Mesh")]
    public class MeshDivideProcedure : SystemProcedure
    {
        /// <summary>
        /// The mesh to be divided.
        /// </summary>
        private readonly SingleInput<MeshEntity> _input = new SingleInput<MeshEntity>("Input");

        /// <summary>
        /// The divided meshes, according to the defined groups.
        /// </summary>
        private readonly Output<MeshEntity> _output = new Output<MeshEntity>("Output");


        /// <summary>
        /// Criteria by which the mesh should be divided. If none is indicated, the whole set of faces will be considered.
        /// </summary>
        private readonly ListParameter<MeshDivideParameter> _parameterMeshDivideGroups = new ListParameter<MeshDivideParameter>("Groups");

        /// <summary>
        /// if true, each one of the faces of the mesh will be placed into a separate mesh entity.
        /// </summary>
        private readonly OptionalListParameter<MeshSeparateParameter> _parameterSeparateMesh = new OptionalListParameter<MeshSeparateParameter>("Separate");



        protected override void Run()
        {
            var originalMeshEntity = _input.Read();

            List<IEnumerable<Face>> faceGroups = new List<IEnumerable<Face>>();
            faceGroups.Add(originalMeshEntity.Faces);

            //each parameter will do its grouping and return enumerables of groups, which will be added to the list of enumerables
            foreach (var meshDivideParameter in _parameterMeshDivideGroups.Items)
                faceGroups = faceGroups.SelectMany(faceGroup => meshDivideParameter.PerformGroupBy(faceGroup)).ToList();

            foreach (var faceGroup in faceGroups)
                if (_parameterSeparateMesh.HasValue)
                {
                    foreach (var face in faceGroup) _output.Write(_parameterSeparateMesh.Value.Process(originalMeshEntity, face));
                }
                else
                {
                    //the deepclone is important
                    //otherwise we cannot perform clean properly
                    //because despite we are separating the faces, the vertices are still shared
                    //so they need to be cloned
                    MeshEntity derivedMesh = (MeshEntity) originalMeshEntity.CreateDerived(faceGroup).DeepClone();
                    derivedMesh.CleanFaceConnections();
                    _output.Write(derivedMesh);
                }
        }



        /*public class FaceGroup
        {
            
        }*/

        #region Abstract Parameter

        public abstract class MeshDivideParameter : CompoundParameter
        {
            protected MeshDivideParameter(string label)
                : base(label)
            {
            }



            protected internal abstract IEnumerable<IEnumerable<Face>> PerformGroupBy(IEnumerable<Face> faces);
        }

        #endregion

        #region Attribute

        /// <summary>
        /// Divides the meshes by attribute value, i.e. building sets of faces that share the same value.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshDivideProcedure.MeshDivideParameter" />
        public class MeshDivideAttributeSetParameter : MeshDivideParameter
        {
            /// <summary>
            /// Value to divide the meshes by.
            /// </summary>
            private readonly ObjectParameter _parameterValue = new ObjectParameter("Value") {EntityEvaluation = true};



            /*
            /// <summary>
            /// Value key that actually distinguishes the group.
            /// </summary>
            private readonly AttributeParameter<Object> _attributeKey = new AttributeParameter<object>("Group Key",AttributeAccess.Write);

            */
            public MeshDivideAttributeSetParameter()
                : base("Attribute")
            {
            }



            protected internal override IEnumerable<IEnumerable<Face>> PerformGroupBy(IEnumerable<Face> faces)
            {
                /*foreach (IGrouping<object, Face> grouping in faces.GroupBy(x => _parameterValue.Get(x));)
                {
                    
                }*/

                return faces.GroupBy(x => _parameterValue.Get(x));
            }
        }

        #endregion

        #region Direction

        /// <summary>
        /// Divides the meshes into sets of faces that share the same (approximate) normal direction.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshDivideProcedure.MeshDivideParameter" />
        public class MeshDivideDirectionSetParameter : MeshDivideParameter
        {
            public MeshDivideDirectionSetParameter()
                : base("Direction")
            {
            }



            protected internal override IEnumerable<IEnumerable<Face>> PerformGroupBy(IEnumerable<Face> faces)
            {
                return faces.GroupBy(x => x.Normal.Round());
            }
        }

        #endregion

        #region Adjacency

        /// <summary>
        /// Divides the meshes by their vertex connections.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshDivideProcedure.MeshDivideParameter" />
        public class MeshDivideAdjacencySetParameter : MeshDivideParameter
        {
            public MeshDivideAdjacencySetParameter()
                : base("Adjacency")
            {
            }



            protected internal override IEnumerable<IEnumerable<Face>> PerformGroupBy(IEnumerable<Face> faces)
            {
                HashSet<Face> startingFaces = new HashSet<Face>(faces);
                List<HashSet<Face>> groups = new List<HashSet<Face>>();

                while (startingFaces.Count > 0)
                {
                    Face extractedFace = startingFaces.First();
                    //startingEdges.Remove(extractedEdge);

                    HashSet<Face> groupSet = new HashSet<Face>();
                    Queue<Face> queue = new Queue<Face>();
                    queue.Enqueue(extractedFace);

                    while (!queue.IsEmpty())
                    {
                        var currentFace = queue.Dequeue();
                        foreach (Vertex vertex in currentFace.Vertices)
                        foreach (Face otherFace in vertex.AdjacentFaces)
                            //if the edge is still in the starting edges, it means
                            //we still haven't analyzed it
                            if (startingFaces.Contains(otherFace))
                            {
                                groupSet.Add(otherFace);
                                queue.Enqueue(otherFace);
                                startingFaces.Remove(otherFace);
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
        /// Divides the meshes by number of faces of vertices, so that they don't exceed the requested size.
        /// </summary>
        /// <seealso cref="Sceelix.Meshes.Procedures.MeshDivideProcedure.MeshDivideParameter" />
        public class MeshDivideSizeParameter : MeshDivideParameter
        {
            /// <summary>
            /// Type of element to divide by:<br/>
            /// <b>Faces</b> means that the meshes will not exceed the indicated face count.<br/>
            /// <b>Vertices</b> means that the meshes will not exceed the indicated vertex count.<br/>
            /// </summary>
            private readonly ChoiceParameter _parameterType = new ChoiceParameter("Type", "Faces", "Faces", "Vertices");

            /// <summary>
            /// The maximum allowed count of faces or vertices to keep in each resulting mesh.
            /// </summary>
            private readonly IntParameter _parameterCount = new IntParameter("Count", 1000);



            public MeshDivideSizeParameter()
                : base("Size")
            {
            }



            protected internal override IEnumerable<IEnumerable<Face>> PerformGroupBy(IEnumerable<Face> faces)
            {
                switch (_parameterType.Value)
                {
                    case "Faces":
                        return SplitList(faces.ToList(), _parameterCount.Value);

                    case "Vertices":
                    {
                        int maxCount = _parameterCount.Value;

                        int facesExceeedingVertexCount = 0;
                        List<List<Face>> totalList = new List<List<Face>>();

                        List<Face> currentList = new List<Face>();
                        int currentCount = 0;

                        //iterate over the faces and add them to the list until the max value
                        //is achieved, then add it to the list of lists
                        foreach (Face face in faces)
                        {
                            var vertexCount = face.AllVertices.Count();
                            if (vertexCount > maxCount)
                            {
                                facesExceeedingVertexCount++;
                            }
                            else
                            {
                                if (currentCount + vertexCount > maxCount)
                                {
                                    totalList.Add(currentList);

                                    currentList = new List<Face> {face};
                                    currentCount = vertexCount;
                                }
                                else
                                {
                                    currentList.Add(face);
                                    currentCount += vertexCount;
                                }
                            }
                        }

                        //if the last list we were dealing with has items
                        //and has not been added to the list, add it now
                        if (currentList.Count > 0 && (!totalList.Any() || totalList.Last() != currentList))
                            totalList.Add(currentList);


                        //if any of the faces had its vertex count larger than the allowed value, warn the user
                        if (facesExceeedingVertexCount > 0)
                        {
                            var message = facesExceeedingVertexCount == 1
                                ? "There was 1 face that exceeded the maximum count value alone. This has not been included."
                                : string.Format("There were {0} faces that exceeded the maximum count value alone. These have not been included.", facesExceeedingVertexCount);

                            ProcedureEnvironment.GetService<ILogger>().Log(message, LogType.Warning);
                        }

                        return totalList;
                    }
                }


                return new IEnumerable<Face>[0];
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