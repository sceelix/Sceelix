using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sceelix.Designer.GUI.MenuControls;

namespace Sceelix.Designer.GUI.MenuHandling
{
    public class MenuEntry
    {
        private MenuChild _menuChild;


        public MenuEntry(MenuChild menuChild)
        {
            _menuChild = menuChild;
        }


        internal MenuChild MenuChild
        {
            get { return _menuChild; }
        }
    }
}
