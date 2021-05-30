using System.Collections.Generic;
using Sceelix.Core.IO;
using Sceelix.Mathematics.Data;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Procedures;
using Sceelix.Surfaces.Data;

namespace Sceelix.Surfaces.Parameters
{
    /// <summary>
    /// Creates a mesh from a given surface.
    /// </summary>
    /// <seealso cref="Sceelix.Meshes.Procedures.MeshCreateProcedure.PrimitiveMeshParameter" />
    public class FromSurfaceMeshParameter : MeshCreateProcedure.PrimitiveMeshParameter
    {
        /// <summary>
        /// Surface to be transformed to a mesh.
        /// </summary>
        private readonly SingleInput<SurfaceEntity> _input = new SingleInput<SurfaceEntity>("Input");



        public FromSurfaceMeshParameter()
            : base("From Surface")
        {
        }



        protected override MeshEntity CreateMesh()
        {
            var surfaceEntity = _input.Read();

            List<Face> faces = new List<Face>();


            var heightLayer = surfaceEntity.GetLayer<HeightLayer>();

            //now, let's create an array of vertices
            Vertex[,] vertexMatrix = new Vertex[surfaceEntity.NumRows, surfaceEntity.NumColumns];
            for (int x = 0; x < surfaceEntity.NumColumns; x++)
            for (int y = 0; y < surfaceEntity.NumRows; y++)
                vertexMatrix[x, y] = new Vertex(heightLayer.GetPosition(new Coordinate(x, y)));

            //now turn this array of vertices into a heightmap
            for (int x = 0; x < surfaceEntity.NumColumns - 1; x++)
            for (int y = 0; y < surfaceEntity.NumRows - 1; y++)
                faces.Add(new Face(vertexMatrix[x, y], vertexMatrix[x + 1, y], vertexMatrix[x + 1, y + 1], vertexMatrix[x, y + 1]));

            //now that that's finished, let's calculate the right normals
            for (int i = 0; i < surfaceEntity.NumColumns; i++)
            for (int j = 0; j < surfaceEntity.NumRows; j++)
            {
                Vector3D normal = new Vector3D();

                foreach (var halfVertex in vertexMatrix[i, j].HalfVertices)
                    normal += halfVertex.Normal;

                vertexMatrix[i, j].Normal = normal.Normalize();

                foreach (var halfVertex in vertexMatrix[i, j].HalfVertices)
                    halfVertex.UV0 = new Vector2D(i * 0.2f, j * 0.2f);
            }

            MeshEntity meshEntity = new MeshEntity(faces);

            //pass the attributes of the surface to the mesh
            surfaceEntity.Attributes.ComplementAttributesTo(meshEntity.Attributes);

            return meshEntity;
        }
    }
}