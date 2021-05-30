using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sceelix.Designer.Actors.VertexTypes
{
    public struct VertexPositionNormalTextureTangents : IVertexType
    {
        public Vector3 Position;
        public Vector2 TextureCoordinate;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector3 Binormal;



        public VertexPositionNormalTextureTangents(Vector3 position)
            : this()
        {
            Position = position;
        }



        private readonly static VertexDeclaration StaticVertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float)*3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float)*5, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float)*8, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
            new VertexElement(sizeof(float)*11, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0)
        );



        public VertexDeclaration VertexDeclaration
        {
            get { return StaticVertexDeclaration; }
        }
    }

    /*public struct VertexPositionNormalTextureTangents : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;
        public Vector3 Tangent;
        public Vector3 Binormal;

        public VertexPositionNormalTextureTangents(Vector3 position)
            : this()
        {
            Position = position;
        }

        private readonly static VertexDeclaration StaticVertexDeclaration = new VertexDeclaration
            (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 8, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
            new VertexElement(sizeof(float) * 11, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0)
            );

        public VertexDeclaration VertexDeclaration
        {
            get { return StaticVertexDeclaration; }
        }
    }*/
}