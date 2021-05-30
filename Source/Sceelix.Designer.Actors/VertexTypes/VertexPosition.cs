using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sceelix.Designer.Actors.VertexTypes
{
    public struct VertexPosition : IVertexType
    {
        public Vector3 Position;



        public VertexPosition(Vector3 position)
            : this()
        {
            Position = position;
        }



        private readonly static VertexDeclaration StaticVertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0)
        );



        public VertexDeclaration VertexDeclaration
        {
            get { return StaticVertexDeclaration; }
        }
    }
}