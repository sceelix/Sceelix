using System.Collections.Generic;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;

namespace Sceelix.Designer.Managers
{
    public class SceelixFocusManager : FocusManager
    {
        public SceelixFocusManager(UIScreen screen)
            : base(screen)
        {
        }



        protected override UIControl OnMoveFocus(bool moveLeft, bool moveRight, bool moveUp, bool moveDown, List<UIControl> focusableControls)
        {
            //do not allow it to move with keys
            return FocusedControl;
        }
    }
}