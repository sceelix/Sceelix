using System.Collections.Generic;
using System.Linq;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;
using Sceelix.Designer.Actors.Utils;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Operations;
using Triangle = DigitalRune.Geometry.Shapes.Triangle;

namespace Sceelix.Designer.Meshes.Utils
{
    public class MeshTranslationHelper
    {
        public static TriangleMeshShape CalculateTriangleMesh(IEnumerable<Face> faces)
        {
            IEnumerable<FaceTriangle> triangles = faces.SelectMany(val => val.Triangulate());

            //change winding order
            TriangleMesh triangleMesh = new TriangleMesh();

            foreach (FaceTriangle triangle in triangles)
                //triangleMesh.Add(new Triangle(triangle.V0.Position.ToVector3F(), triangle.V2.Position.ToVector3F(), triangle.V1.Position.ToVector3F()));
                triangleMesh.Add(new Triangle(triangle.V0.Position.ToVector3F(), triangle.V2.Position.ToVector3F(), triangle.V1.Position.ToVector3F()));

            // Create a shape for the triangle mesh.
            var triMeshShape = new TriangleMeshShape(triangleMesh);

            // Assign a spatial partition to the mesh.
            triMeshShape.Partition = new AabbTree<int> { BottomUpBuildThreshold = 0, };

            // Build spatial partition. (It is also a good idea to measure how long this take.)
            triMeshShape.Partition.Update(false);

            return triMeshShape;
        }
    }
}
