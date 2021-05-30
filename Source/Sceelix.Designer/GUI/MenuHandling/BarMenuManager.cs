using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalRune.Collections;
using DigitalRune.Game.UI;
using Sceelix.Designer.GUI.MenuControls;

namespace Sceelix.Designer.GUI.MenuHandling
{
    public class BarMenuManager
    {
        //private BarMenu _mainBarMenu;

        private readonly Dictionary<String, BarMenu> _barMenus = new Dictionary<string, BarMenu>();
        //private Dictionary<String,List<BarMenuEntry>> _barMenuEntries = new Dictionary<string, List<BarMenuEntry>>();
        private readonly List<BarMenuEntry> _menuEntries = new List<BarMenuEntry>();


        
        internal void Initialize(BarMenu mainBarMenu)
        {
            _barMenus[""] = mainBarMenu;

            foreach (BarMenuEntry barMenuEntry in _menuEntries.Where(x => x.Menu == String.Empty))
            {
                ApplyMenuEntry(mainBarMenu, barMenuEntry.Path, barMenuEntry.Action);
            }
        }


        public void RegisterMenu(BarMenu menu, String name)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Menu name cannot be empty.");

            _barMenus[name] = menu;

            //check all the waiting entries
            foreach (BarMenuEntry barMenuEntry in _menuEntries.Where(x => x.Menu == name))
            {
                ApplyMenuEntry(menu, barMenuEntry.Path, barMenuEntry.Action);
            }
        }



        public void UnregisterMenu(BarMenu menu)
        {
            //make sure that a user cannot unregister a menu unless he owns it
            foreach (string key in _barMenus.Where(x => x.Value == menu).Select(x => x.Key).ToList())
            {
                _barMenus.Remove(key);
            }
        }



        public void RegisterMenuEntry(BarMenuEntry menuEntry)
        {
            if (_menuEntries.Contains(menuEntry))
                throw new ArgumentException("The same menu entry has already been added.");

            _menuEntries.Add(menuEntry);

            if (_barMenus.ContainsKey(menuEntry.Menu))
            {
                ApplyMenuEntry(_barMenus[menuEntry.Menu], menuEntry.Path, menuEntry.Action);
            }
        }



        public void UnregisterMenuEntry(BarMenuEntry menuEntry)
        {
            _menuEntries.Remove(menuEntry);

            if (_barMenus.ContainsKey(menuEntry.Menu))
            {
                RemoveMenuEntry(_barMenus[menuEntry.Menu], menuEntry);
            }
        }



        private void ApplyMenuEntry(BarMenu barMenu, string path, Action action)
        {
            var nameQueue = new Queue<String>(path.Split('/'));
            var currentMenuChildren = barMenu.MenuChildren;

            while (nameQueue.Count > 0)
            {
                var name = nameQueue.Dequeue();

                if (nameQueue.Count == 0)
                {
                    if (currentMenuChildren.Any(x => x.Text == name))
                        throw new InvalidOperationException(String.Format("Could not add menu item '{0}'. An item with the same name already exists.", path));

                    var menuChild = new MenuChild(child => action()) {Text = name};

                    currentMenuChildren.Add(menuChild);
                }
                else
                {
                    MenuChild firstOrDefault = currentMenuChildren.FirstOrDefault(x => x.Text == name);
                    if (firstOrDefault != null)
                    {
                        currentMenuChildren = firstOrDefault.MenuChildren;
                    }
                    else
                    {
                        var newMiddleMenu = new MenuChild() {Text = name};
                        currentMenuChildren.Add(newMiddleMenu);

                        currentMenuChildren = newMiddleMenu.MenuChildren;
                    }
                }
            }
        }



        private void RemoveMenuEntry(BarMenu barMenu, BarMenuEntry menuEntry)
        {
            /*foreach (MenuChild menuChild in UIHelper.GetSubtree(barMenu).OfType<MenuChild>().Where(x => x.UserData == menuEntry).ToList())
            {
                menuChild.MenuParent.MenuChildren.Remove(menuChild);
            }*/


            var nameQueue = new Queue<String>(menuEntry.Path.Split('/'));
            var currentMenuChildren = barMenu.MenuChildren;

            RemoveMenuEntryRecursive(currentMenuChildren, nameQueue);

            /*while (nameQueue.Count > 0)
            {
                var name = nameQueue.Dequeue();

                if (nameQueue.Count == 0)
                {
                    var menuChild = new MenuChild(child => action()) { Text = name };

                    currentMenuChildren.Add(menuChild);
                }
                else
                {
                    MenuChild firstOrDefault = currentMenuChildren.FirstOrDefault(x => x.Text == name);
                    if (firstOrDefault != null)
                    {
                        currentMenuChildren = firstOrDefault.MenuChildren;
                    }
                    else
                    {
                        var newMiddleMenu = new MenuChild() { Text = name };
                        currentMenuChildren.Add(newMiddleMenu);

                        currentMenuChildren = newMiddleMenu.MenuChildren;
                    }

                }
            }*/
        }



        private void RemoveMenuEntryRecursive(NotifyingCollection<MenuChild> currentMenuChildren, Queue<string> nameQueue)
        {
            var name = nameQueue.Dequeue();

            if (nameQueue.Count == 0)
            {
                var item = currentMenuChildren.FirstOrDefault(x => x.Text == name);
                if (item != null)
                    currentMenuChildren.Remove(item);
                else
                    throw new InvalidOperationException("Could not remove menu entry.");
            }
            else
            {
                MenuChild firstOrDefault = currentMenuChildren.FirstOrDefault(x => x.Text == name);
                if (firstOrDefault != null)
                {
                    RemoveMenuEntryRecursive(firstOrDefault.MenuChildren, nameQueue);

                    //if the parent menu is empty and has no action, we can delete it
                    if (!firstOrDefault.HasChildren && !firstOrDefault.HasAction)
                        currentMenuChildren.Remove(firstOrDefault);
                }
                else
                {
                    throw new InvalidOperationException("Could not remove menu entry.");
                }
            }
        }
    }
}