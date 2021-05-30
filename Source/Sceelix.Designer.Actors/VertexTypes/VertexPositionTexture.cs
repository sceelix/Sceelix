using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sceelix.Designer.Actors.VertexTypes
{
    public struct VertexPositionTexture : IVertexType
    {
        public Vector3 Position;
        public Vector2 Texture;



        public VertexPositionTexture(Vector3 position)
            : this()
        {
            Position = position;
        }



        private readonly static VertexDeclaration StaticVertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float)*3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        );



        public VertexDeclaration VertexDeclaration
        {
            get { return StaticVertexDeclaration; }
        }
    }
}