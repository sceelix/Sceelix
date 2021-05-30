using System.Collections.Generic;
using Sceelix.Actors.Data;
using Sceelix.Mathematics.Data;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Operations;

namespace Sceelix.Unity.Data
{
    public class UnityMesh
    {
        public List<List<int>> SubmeshTriangles = new List<List<int>>();



        public UnityMesh()
        {
            Positions = new List<Vector3D>();
            Normals = new List<Vector3D>();
            Uvs = new List<Vector2D>();
            Colors = new List<Color>();
            Tangents = new List<Vector4D>();
        }



        public List<Color> Colors
        {
            get;
            set;
        }


        //public int Id { get; set; }
        public MeshEntity MeshEntity
        {
            get;
            set;
        }


        public List<Vector3D> Normals
        {
            get;
            set;
        }


        public List<Vector3D> Positions
        {
            get;
            set;
        }


        public List<Vector4D> Tangents
        {
            get;
            set;
        }


        public List<Vector2D> Uvs
        {
            get;
            set;
        }
    }


    public class UnityMeshWithMaterials
    {
        public UnityMeshWithMaterials(MeshEntity meshEntity)
        {
            Dictionary<Material, int> materialToMaterialData = new Dictionary<Material, int>();

            int indexerValue = 0;
            UnityMesh = new UnityMesh();
            Materials = new List<Material>();

            //Mesh meshData = new Mesh();
            //Mesh.Id = meshEntity.GetHashCode();
            UnityMesh.MeshEntity = meshEntity;

            foreach (Face face in meshEntity)
            {
                int index;

                if (!materialToMaterialData.TryGetValue(face.Material, out index))
                {
                    index = materialToMaterialData.Count;

                    materialToMaterialData.Add(face.Material, index);

                    Materials.Add(face.Material);
                    UnityMesh.SubmeshTriangles.Add(new List<int>());
                }

                List<FaceTriangle> faceTriangles = face.Triangulate();

                foreach (FaceTriangle faceTriangle in faceTriangles)
                    //faceTriangle.Vertices.Reverse();

                foreach (Vertex vertex in faceTriangle.Vertices)
                {
                    var normal = vertex[face].Normal;
                    var tangent = vertex[face].Tangent;
                    var binormal = vertex[face].Binormal;

                    UnityMesh.Positions.Add(vertex.Position.FlipYZ());
                    UnityMesh.Normals.Add(normal.FlipYZ());
                    UnityMesh.Colors.Add(vertex[face].Color);
                    UnityMesh.Tangents.Add(new Vector4D(tangent, tangent.Cross(normal).Dot(binormal) > 0 ? 1f : -1f));
                    UnityMesh.Uvs.Add(vertex[face].UV0 * new Vector2D(1, -1));

                    UnityMesh.SubmeshTriangles[index].Add(indexerValue++);
                }
            }

            //meshData.SubmeshTriangles = submeshTriangles.Select(x => x.ToArray()).ToList();
        }



        public List<Material> Materials
        {
            get;
        }


        public UnityMesh UnityMesh
        {
            get;
            set;
        }
    }
}