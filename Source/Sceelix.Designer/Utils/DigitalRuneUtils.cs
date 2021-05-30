using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;

namespace Sceelix.Designer.Utils
{
    public class DigitalRuneUtils
    {
        public static readonly Matrix33F ZUpToYUpRotationMatrix = Matrix33F.CreateRotationX(-MathHelper.PiOver2);
        public static readonly Matrix33F YUpToZUpRotationMatrix = Matrix33F.CreateRotationX(MathHelper.PiOver2);
    }
}