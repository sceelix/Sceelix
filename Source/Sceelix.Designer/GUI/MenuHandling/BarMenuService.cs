using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.GUI.MenuControls;

namespace Sceelix.Designer.GUI.MenuHandling
{
    public class BarMenuService
    {
        private readonly BarMenu _barMenu;


        public BarMenuService(BarMenu barMenu)
        {
            _barMenu = barMenu;
        }


        public MenuEntry RegisterMenuEntry(string path, Action action, Texture2D icon = null, bool beginGroup = false)
        {
            var nameQueue = new Queue<String>(path.Split('/'));
            var currentMenuChildren = _barMenu.MenuChildren;

            while (nameQueue.Count > 0)
            {
                var name = nameQueue.Dequeue();

                if (nameQueue.Count == 0)
                {
                    if (currentMenuChildren.Any(x => x.Text == name))
                        throw new InvalidOperationException(String.Format("Could not add menu item '{0}'. An item with the same name already exists.", path));

                    var menuChild = new MenuChild(child => action()) { Text = name, Icon = icon, BeginGroup = beginGroup };
                    currentMenuChildren.Add(menuChild);

                    return new MenuEntry(menuChild);
                }
                else
                {
                    MenuChild parentMenu = currentMenuChildren.FirstOrDefault(x => x.Text == name);
                    if(parentMenu == null)
                        currentMenuChildren.Add(parentMenu = new MenuChild() { Text = name });

                    currentMenuChildren = parentMenu.MenuChildren;
                }
            }

            return null;
        }


        public void UnregisterMenuEntry(MenuEntry menuEntry)
        {
            var menuChildMenuParent = menuEntry.MenuChild.MenuParent;
            menuChildMenuParent.MenuChildren.Remove(menuEntry.MenuChild);
            RemoveEmpty(menuChildMenuParent);
        }


        private void RemoveEmpty(IMenuControl menuControl)
        {
            if (menuControl.MenuChildren.Count == 0 && menuControl is MenuChild)
            {
                var menuChild = (MenuChild) menuControl;
                menuChild.MenuParent.MenuChildren.Remove(menuChild);
                RemoveEmpty(menuChild.MenuParent);
            }
        }
    }
}
