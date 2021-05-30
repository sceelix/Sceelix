using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sceelix.Designer.Actors.VertexTypes
{
    public struct VertexPositionNormalColor2Texture : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Color Color;
        public Color Color1;
        public Vector2 TextureCoordinate;



        public VertexPositionNormalColor2Texture(Vector3 position)
            : this()
        {
            Position = position;
        }



        private readonly static VertexDeclaration StaticVertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float)*3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float)*6, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 6 + sizeof(byte)*4, VertexElementFormat.Color, VertexElementUsage.Color, 1),
            new VertexElement(sizeof(float) * 6 + sizeof(byte) * 8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        );



        public VertexDeclaration VertexDeclaration
        {
            get { return StaticVertexDeclaration; }
        }
    }
}