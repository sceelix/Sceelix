using DigitalRune.Collections;

namespace Sceelix.Designer.GUI.MenuControls
{
    public interface IMenuControl
    {
        NotifyingCollection<MenuChild> MenuChildren
        {
            get;
        }
    }
}