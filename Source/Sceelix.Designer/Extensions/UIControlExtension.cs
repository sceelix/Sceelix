using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;

namespace Sceelix.Designer.Extensions
{
    public static class UIControlExtension
    {
        public static Vector2F GetMouseRelativePosition(this UIControl control, Vector2F mouseScreenPosition)
        {
            return new Vector2F(mouseScreenPosition.X - control.ActualX, mouseScreenPosition.Y - control.ActualY);
        }



        /*public static void AddChangedHandler<T>(this UIControl control, String name, EventHandler<GamePropertyEventArgs<T>> eventHandler)
        {
            var gameProperty = control.Properties.Get<T>(name);
            gameProperty.Changed += eventHandler;
        }*/
    }
}