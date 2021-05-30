namespace Sceelix.Designer.Helpers
{
    public static class WindowHelper
    {

#if LINUX || MACOS
        public static OpenTK.NativeWindow GetForm(this Microsoft.Xna.Framework.GameWindow gameWindow)
        {
            System.Reflection.FieldInfo field = gameWindow.GetType().GetField("window", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                return field.GetValue(gameWindow) as OpenTK.NativeWindow;
            return null;
        }
#endif
    }
}
