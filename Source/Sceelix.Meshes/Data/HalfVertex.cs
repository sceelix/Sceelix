using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Attributes;
using Sceelix.Core.Data;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Helpers;

namespace Sceelix.Meshes.Data
{
    public class HalfVertex : Entity
    {
        private static readonly FieldKey NormalKey = new FieldKey("Normal");
        private static readonly FieldKey ColorKey = new FieldKey("Color");
        private static readonly FieldKey UV0Key = new FieldKey("UV0");
        private static readonly FieldKey TangentKey = new FieldKey("Tangent");
        private static readonly FieldKey BinormalKey = new FieldKey("Binormal");



        public HalfVertex(Face face, int faceIndex)
        {
            Face = face;
            FaceIndex = faceIndex;
        }



        public float Angle
        {
            get
            {
                float angleTo = MathHelper.ToDegrees((Previous.Position - Vertex.Position).AngleTo(Vertex.Position - Next.Position));
                return angleTo;
            }
        }



        public Vector3D Binormal
        {
            get
            {
                var binormal = Attributes.TryGet(BinormalKey) as Vector3D?;
                if (binormal != null)
                    return binormal.Value;

                return Vector3D.Zero;
            }
            set { Attributes.TrySet(BinormalKey, value, true); }
        }



        public Color Color
        {
            get
            {
                var geometryNormal = Attributes.TryGet(ColorKey) as Color?;
                if (geometryNormal.HasValue)
                    return geometryNormal.Value;

                Color? vertexColor = Vertex.Color;

                return vertexColor ?? Color.White;
            }
            set { Attributes.TrySet(ColorKey, value, true); }
        }



        public Face Face
        {
            get;
        }


        internal int FaceIndex
        {
            get;
            set;
        }


        public bool IsBoundary => !OtherFaces.Any();


        public Vertex Next => Face[FaceIndex + 1];



        public Vector3D Normal
        {
            get
            {
                var geometryNormal = Attributes.TryGet(NormalKey) as Vector3D?;
                if (geometryNormal.HasValue)
                    return geometryNormal.Value;

                Vector3D? vertexNormal = Vertex.Normal;

                return vertexNormal ?? Face.Normal;
            }
            set { Attributes.TrySet(NormalKey, value, true); }
        }



        public IEnumerable<Face> OtherFaces
        {
            get
            {
                var currentVertex = Vertex;

                return Next.HalfVertices.Where(x => x.Next == currentVertex).Select(x => x.Face);
            }
        }



        public IEnumerable<HalfVertex> OtherHalfVertices
        {
            get
            {
                var currentVertex = Vertex;

                return Next.HalfVertices.Where(x => x.Next == currentVertex);
            }
        }



        public Vertex Previous => Face[FaceIndex - 1];



        public Vector3D Tangent
        {
            get
            {
                var tangent = Attributes.TryGet(TangentKey) as Vector3D?;
                if (tangent != null)
                    return tangent.Value;

                return Vector3D.Zero;
            }
            set { Attributes.TrySet(TangentKey, value, true); }
        }



        public Vector2D UV0
        {
            get
            {
                var geometryNormal = Attributes.TryGet(UV0Key) as Vector2D?;
                if (geometryNormal.HasValue)
                    return geometryNormal.Value;

                return Vector2D.Zero;
            }
            set { Attributes.TrySet(UV0Key, value, true); }
        }



        public HalfVertexUVCollection UVs => new HalfVertexUVCollection(this);


        public Vertex Vertex => Face[FaceIndex];



        /// <summary>
        /// Get edge that starts at this vertex
        /// </summary>
        /// <returns></returns>
        public Edge GetEmanatingEdge()
        {
            return new Edge(Vertex, Next);
        }



        public class HalfVertexUVCollection
        {
            public HalfVertexUVCollection(HalfVertex halfVertex)
            {
                HalfVertex = halfVertex;
            }



            private HalfVertex HalfVertex
            {
                get;
            }



            public Vector2D? this[int index]
            {
                get
                {
                    var uvCoordinate = HalfVertex.Attributes.TryGet(new FieldKey("UV" + index)) as Vector2D?;
                    if (uvCoordinate.HasValue)
                        return uvCoordinate.Value;

                    Vector2D? uv = HalfVertex.Vertex.UVs[index];

                    return uv ?? Vector2D.Zero;
                }
                set { HalfVertex.Attributes.TrySet(new FieldKey("UV" + index), value, true); }
            }
        }
    }
}