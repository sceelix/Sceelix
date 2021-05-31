using System.Collections.Generic;
using Sceelix.Core.Annotations;
using Sceelix.Core.Attributes;
using Sceelix.Core.IO;
using Sceelix.Core.Procedures;
using Sceelix.Meshes.Data;
using Sceelix.Paths.Data;

namespace Sceelix.MyNewEngineLibrary
{
    /// <summary>
    /// This sample illustrates how to convert a mesh to a path.
    /// 
    /// The purpose of this sample is to give some insight on how to use Meshes and create new Paths.
    /// </summary>
    [Procedure("77d3af67-4d00-4db5-9bdf-43566ef5799a", Label = "Mesh to Path Example")]
    public class MeshToPathExampleProcedure : SystemProcedure
    {
        private readonly SingleInput<MeshEntity> _input = new SingleInput<MeshEntity>("Input");
        private readonly Output<PathEntity> _output = new Output<PathEntity>("Output");

        protected override void Run()
        {
            MeshEntity meshEntity = _input.Read();

            //first of all, we need to create a mapping between 
            Dictionary<Vertex, PathVertex> mapping = new Dictionary<Vertex, PathVertex>();
            foreach (Vertex vertex in meshEntity.FaceVerticesWithHoles)
                mapping.Add(vertex, new PathVertex(vertex.Position));

            List<PathEdge> pathEdges = new List<PathEdge>();

            //now, iterate over each edge of the mesh
            foreach (Face face in meshEntity)
            {
                foreach (Edge faceEdge in face.Edges)
                {
                    //create a corresponding pathedge, connecting the corresponding new PathVertices
                    PathEdge pathEdge = new PathEdge(mapping[faceEdge.V0], mapping[faceEdge.V1]);

                    //we could set custom attributes to our edges, derived from the face
                    //for the sake of example, we set the "MyWidth" as the result of the faceEdge length, divided by 5
                    pathEdge.Attributes.TrySet(new GlobalAttributeKey("MyWidth"), faceEdge.Length / 5f);

                    //in the end, we must call warn the AttachToVertices() in order
                    //to store the reference to this edge back in the vertices, too
                    pathEdges.Add(pathEdge);
                }
            }

            //finally, return the newly create pathentity
            _output.Write(new PathEntity(pathEdges));
        }
    }
}
