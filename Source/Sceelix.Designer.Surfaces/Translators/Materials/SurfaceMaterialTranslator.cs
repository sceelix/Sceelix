using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Threading;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Actors.Materials;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Renderer3D.Loading;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;
using Sceelix.Helpers;
using Sceelix.Mathematics.Data;
using Sceelix.Surfaces.Data;
using Material = Sceelix.Actors.Data.Material;


namespace Sceelix.Designer.Surfaces.Translators.Materials
{
    public abstract class SurfaceMaterialTranslator<TR> : MaterialTranslator, IServiceable, ISurfaceMaterialTranslator where TR : struct, IVertexType
    {
        private static readonly int MaxVertexCount = BuildPlatform.IsWindows ? int.MaxValue : short.MaxValue;

        private IGraphicsService _graphicsService;
        

        public virtual void Initialize(IServiceLocator services)
        {
            _graphicsService = services.Get<IGraphicsService>();
        }



        private IEnumerable<Submesh> InitializeBuffers(SurfaceEntity surfaceEntity, Material sceelixMaterial)
        {
            int width = surfaceEntity.NumColumns;
            int height = surfaceEntity.NumRows;

            //int numTriangles = (width - 1) * (height - 1) * 2;
            //int numVertices = width * height;
            var interpolation = GetSurfaceInterpolation(surfaceEntity);
            Func<int, int, int, int, int[]> action = interpolation == SurfaceInterpolation.TopRight ?
                                        (tl, tr, ll, lr) => new[] { tl, tr, ll, tr, lr, ll } :
                                        (Func<int, int, int, int, int[]>)((tl, tr, ll, lr) => new[] { tl, tr, lr, tl, lr, ll });

            //int vertexStride = default(TR).VertexDeclaration.VertexStride;
            var prepareVertexFunc = GetPrepareVertexFunc(surfaceEntity, sceelixMaterial);

            int halfVertices = (int) Math.Sqrt(MaxVertexCount);
            int widthPieces = (int) (width/(double) halfVertices) + 1;
            int heightPieces = (int) (height/(double) halfVertices) + 1;

            for (int i = 0; i < widthPieces; i++)
            {
                int startingK = i*halfVertices - ((i > 0) ? 1 : 0);
                int endingK = Math.Min(startingK + halfVertices, width);
                int numColumns = endingK - startingK;

                for (int j = 0; j < heightPieces; j++)
                {
                    int startingL = j*halfVertices - ((j > 0) ? 1 : 0);
                    int endingL = Math.Min(startingL + halfVertices, height);
                    int numRows = endingL - startingL;

                    int numVertices = numColumns*numRows;
                    int numTriangles = (numColumns - 1)*(numRows - 1)*2;

                    bool[] visibleArray = new bool[numVertices];
                    TR[] vertexArray = new TR[numVertices];
                    int[] indicesArray = new int[numTriangles*3];



                    ParallelHelper.For(0,numColumns, (k) =>
                    {
                        for (int l = 0; l < numRows; l++)
                        {
                            var vertex = prepareVertexFunc(new Coordinate(startingK + k, startingL + l));//surfaceEntity, sceelixMaterial, 
                            if (vertex.HasValue)
                            {
                                var location = l*numColumns + k;

                                vertexArray[location] = vertex.Value;
                                visibleArray[location] = true;
                            }
                        }
                    });

                    ParallelHelper.For(0, numColumns - 1, (k) =>
                    {
                        int indexPosition = k*(numRows - 1)*6;

                        for (int l = 0; l < numRows - 1; l++)
                        {
                            int topLeft = l * numColumns + k;
                            int topRight = l * numColumns + (k + 1);
                            int lowerLeft = (l + 1) * numColumns + k;
                            int lowerRight = (l + 1) * numColumns + (k + 1);

                            

                            var indices = action(topLeft, topRight, lowerLeft, lowerRight);
                            if (visibleArray[indices[0]] && visibleArray[indices[1]] && visibleArray[indices[2]])
                            {
                                indicesArray[indexPosition++] = indices[0];
                                indicesArray[indexPosition++] = indices[1];
                                indicesArray[indexPosition++] = indices[2];
                            }

                            if (visibleArray[indices[3]] && visibleArray[indices[4]] && visibleArray[indices[5]])
                            {
                                indicesArray[indexPosition++] = indices[3];
                                indicesArray[indexPosition++] = indices[4];
                                indicesArray[indexPosition++] = indices[5];
                            }
                        }
                    });
                    
                    var submesh = new Submesh
                    {
                        PrimitiveType = PrimitiveType.TriangleList,
                        PrimitiveCount = numTriangles,
                        VertexCount = numVertices,
                    };

                    //now create the buffers
                    submesh.VertexBuffer = new VertexBuffer(_graphicsService.GraphicsDevice, default(TR).VertexDeclaration, numVertices, BufferUsage.None);
                    submesh.VertexBuffer.SetData(vertexArray);


                    if (BuildPlatform.IsWindows)
                    {
                        submesh.IndexBuffer = new IndexBuffer(_graphicsService.GraphicsDevice, typeof(int), numTriangles * 3, BufferUsage.None);
                        submesh.IndexBuffer.SetData(indicesArray);
                    }
                    else
                    {
                        submesh.IndexBuffer = new IndexBuffer(_graphicsService.GraphicsDevice, typeof(short), numTriangles * 3, BufferUsage.None);
                        submesh.IndexBuffer.SetData(indicesArray.Select(x => (short)x).ToArray());
                    }

                    yield return submesh;
                }
            }
        }



        protected virtual SurfaceInterpolation GetSurfaceInterpolation(SurfaceEntity surfaceEntity)
        {
            var heightLayer = surfaceEntity.GetLayer<HeightLayer>();
            if (heightLayer != null)
                return heightLayer.Interpolation;

            return SurfaceInterpolation.TopLeft;
        }
        



        public override SceneNode CreateSceneNode(ContentLoader contentLoader, Material material, object data)
        {
            SurfaceEntity surfaceEntity = (SurfaceEntity) data;

            Mesh mesh = new Mesh();

            var drMaterial = CreateMaterial(contentLoader, material);

            IEnumerable<Submesh> submeshes = InitializeBuffers(surfaceEntity, material);
            foreach (Submesh submesh in submeshes)
            {
                mesh.Submeshes.Add(submesh);
                submesh.SetMaterial(drMaterial);
            }

            return new MeshNode(mesh) {PoseWorld = Pose.Identity, IsStatic = true};
        }



        protected abstract DigitalRune.Graphics.Material CreateMaterial(ContentLoader contentLoader, Material sceelixMaterial);



        protected virtual TR? PrepareVertex(SurfaceEntity surfaceEntity, Material sceelixMaterial, int column, int row)
        {
            return default(TR);
        }


        
        protected virtual Func<Coordinate, TR?> GetPrepareVertexFunc(SurfaceEntity surfaceEntity, Material sceelixMaterial)
        {
            return (coordinate) => PrepareVertex(surfaceEntity, sceelixMaterial, coordinate.X, coordinate.Y);
        }
    }
}