using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;

namespace Sceelix.Designer.GUI.MenuControls
{
    public class SeparatorControl : UIControl
    {
        public SeparatorControl()
        {
            Style = "MenuItemContent";
            Height = 3;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            Margin = new Vector4F(25, Margin.Y, Margin.Z, Margin.W);
        }
    }
}