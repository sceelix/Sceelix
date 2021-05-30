using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Actors.Materials;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Renderer3D.Loading;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Operations;
using Material = Sceelix.Actors.Data.Material;
using DrMaterial = DigitalRune.Graphics.Material;

namespace Sceelix.Designer.Meshes.Translators.Materials
{
    public abstract class MeshMaterialTranslator<T> : MaterialTranslator, IServiceable, IMeshMaterialTranslator where T : struct, IVertexType
    {
        private IGraphicsService _graphicsService;


        private static readonly int PrimitiveLimit = BuildPlatform.IsWindows ? 1048575 : 65535;
        private static readonly int BufferSizeLimit = 67108863;

        

        public virtual void Initialize(IServiceLocator services)
        {
            _graphicsService = services.Get<IGraphicsService>();
        }

        //public static Dictionary<Material, DrMaterial> MaterialCache = new Dictionary<Material, DrMaterial>();


        public override SceneNode CreateSceneNode(ContentLoader contentLoader, Material sceelixMaterial, object data)
        {
            var faces = (List<Face>) data;

            Mesh mesh = new Mesh();

            DrMaterial drMaterial = contentLoader.LoadAsset(sceelixMaterial, () => CreateMaterial(contentLoader, sceelixMaterial));

            /*DrMaterial drMaterial;
            if(!MaterialCache.TryGetValue(sceelixMaterial, out drMaterial))
                MaterialCache.Add(sceelixMaterial,drMaterial = CreateMaterial(contentLoader, sceelixMaterial));*/

            IEnumerable<Submesh> submeshes = InitializeBuffersPerformance(faces, sceelixMaterial);
            foreach (Submesh submesh in submeshes)
            {
                mesh.Submeshes.Add(submesh);
                submesh.SetMaterial(drMaterial);
            }

            return new MeshNode(mesh) {PoseWorld = Pose.Identity, IsStatic = true};
        }



        private IEnumerable<Submesh> InitializeBuffersPerformance(IEnumerable<Face> faceSet, Material sceelixMaterial)
        {
            List<T> customVertexList = new List<T>();
            int bufferSize = 0;

            int vertexStride = default(T).VertexDeclaration.VertexStride;

            //Load the vertices and triangles from each face
            foreach (Face face in faceSet)
            {
                List<FaceTriangle> faceTriangles = face.Triangulate();

                foreach (FaceTriangle faceTriangle in faceTriangles)
                {
                    //partition where the mesh exceeds the limits
                    if (customVertexList.Count/3 == PrimitiveLimit)
                    {
                        yield return CreateSubmesh(_graphicsService.GraphicsDevice, customVertexList);

                        customVertexList = new List<T>();
                        bufferSize = 0;
                    }

                    foreach (Vertex vertex in faceTriangle.Vertices)
                    {
                        //partition where the mesh exceeds the limits
                        if (bufferSize + vertexStride > BufferSizeLimit)
                        {
                            yield return CreateSubmesh(_graphicsService.GraphicsDevice, customVertexList);

                            customVertexList = new List<T>();
                            bufferSize = 0;
                        }

                        T createdVertex = PrepareVertex(sceelixMaterial, face, vertex); //faceTriangle

                        customVertexList.Add(createdVertex);

                        bufferSize += vertexStride;
                    }
                }
            }

            if (customVertexList.Count > 0)
                yield return CreateSubmesh(_graphicsService.GraphicsDevice, customVertexList);
        }



        private Submesh CreateSubmesh(GraphicsDevice device, List<T> customVertexList)
        {
            //now create the buffers
            VertexDeclaration declaration = customVertexList[0].VertexDeclaration;

            var submesh = new Submesh
            {
                PrimitiveType = PrimitiveType.TriangleList,
                PrimitiveCount = customVertexList.Count/3,
                VertexCount = customVertexList.Count,
                VertexBuffer = new VertexBuffer(device, declaration, customVertexList.Count, BufferUsage.None),
            };


            submesh.VertexBuffer.SetData(customVertexList.ToArray());
            
            if (BuildPlatform.IsWindows)
            {
                submesh.IndexBuffer = new IndexBuffer(device, typeof(int), customVertexList.Count, BufferUsage.None);
                submesh.IndexBuffer.SetData(Enumerable.Range(0, customVertexList.Count).ToArray());
            }
            else
            {
                submesh.IndexBuffer = new IndexBuffer(device, typeof(short), customVertexList.Count, BufferUsage.None);
                submesh.IndexBuffer.SetData(Enumerable.Range(0, customVertexList.Count).Select(x => (short)x).ToArray());
            }

            return submesh;
        }



        private Submesh InitializeBuffersMemory(IEnumerable<Face> faceSet, Material sceelixMaterial)
        {
            Dictionary<T, int> customVertexIndices = new Dictionary<T, int>();

            List<int> indicesList = new List<int>();
            int indexerValue = 0;

            //Load the vertices and triangles from each face
            foreach (Face face in faceSet)
            {
                List<FaceTriangle> faceTriangles = face.Triangulate();

                foreach (FaceTriangle faceTriangle in faceTriangles)
                {
                    foreach (Vertex vertex in faceTriangle.Vertices)
                    {
                        T createdVertex = PrepareVertex(sceelixMaterial, face, vertex);

                        if (customVertexIndices.ContainsKey(createdVertex))
                            indicesList.Add(customVertexIndices[createdVertex]);
                        else
                        {
                            customVertexIndices.Add(createdVertex, indexerValue);
                            indicesList.Add(indexerValue++);
                        }
                    }
                }
            }

            //now create the buffers
            VertexDeclaration declaration = customVertexIndices.First().Key.VertexDeclaration;

            var submesh = new Submesh
            {
                PrimitiveType = PrimitiveType.TriangleList,
                PrimitiveCount = indicesList.Count/3,
                VertexCount = customVertexIndices.Keys.Count,
                VertexBuffer = new VertexBuffer(_graphicsService.GraphicsDevice, declaration, customVertexIndices.Keys.Count, BufferUsage.WriteOnly),
            };

            submesh.VertexBuffer.SetData(customVertexIndices.Keys.ToArray());

            submesh.IndexBuffer = new IndexBuffer(_graphicsService.GraphicsDevice, typeof(int), indicesList.Count, BufferUsage.WriteOnly);
            submesh.IndexBuffer.SetData(indicesList.ToArray());

            return submesh;
        }



        /// <summary>
        /// Converts the given Sceelix vertex to a vertex of type TR.
        /// 
        /// To be overriden in the subclasses.
        /// </summary>
        /// <param name="sceelixMaterial"></param>
        /// <param name="face"></param>
        /// <param name="vertex"></param>
        /// <returns></returns>
        protected abstract T PrepareVertex(Material sceelixMaterial, Face face, Vertex vertex);



        /// <summary>
        /// Converts the given Sceelix material to a Digitalrune material
        /// To be overriden in the subclasses.
        /// </summary>
        /// <param name="contentLoader"></param>
        /// <param name="sceelixMaterial"></param>
        /// <returns></returns>
        protected abstract DigitalRune.Graphics.Material CreateMaterial(ContentLoader contentLoader, Material sceelixMaterial);
    }
}