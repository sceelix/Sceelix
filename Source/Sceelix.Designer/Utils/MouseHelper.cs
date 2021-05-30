using Microsoft.Xna.Framework.Input;

namespace Sceelix.Designer.Utils
{
    /// <summary>
    /// This class is a temporary patch to a bigger problem, which is the GUI scaling. It should be used instead of the Mouse.SetPosition directly.
    /// </summary>
    public class MouseHelper
    {
        private static float _inputRenderScale = 1;



        public static float InputRenderScale
        {
            set { _inputRenderScale = value; }
        }



        public static void SetPosition(int x, int y)
        {
            var newX = (int) (x/_inputRenderScale);
            var newY = (int) (y/_inputRenderScale);

            Mouse.SetPosition(newX, newY);
        }
    }
}