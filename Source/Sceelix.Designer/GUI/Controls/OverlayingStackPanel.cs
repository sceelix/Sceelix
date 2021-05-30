using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;

namespace Sceelix.Designer.GUI.Controls
{
    public class OverlayingStackPanel : StackPanel
    {



        protected override void OnArrange(Vector2F position, Vector2F size)
        {
            //all on top of each other
            foreach (UIControl uiControl in Children)
                uiControl.Arrange(new Vector2F(position.X, position.Y), new Vector2F(size.X, size.Y));
        }
    }
}