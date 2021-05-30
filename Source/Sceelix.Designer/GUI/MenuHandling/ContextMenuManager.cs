using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.GUI.MenuControls;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.GUI.MenuHandling
{
    public class ContextMenuManager
    {
        private readonly List<BarMenuEntry> _menuEntries = new List<BarMenuEntry>();

        List<ContextMenuEntry> _contextMenuEntries = new List<ContextMenuEntry>();

        public void Initialize(MultiContextMenu multiContextMenu, Object content)
        {
            foreach (var contextMenuEntry in _contextMenuEntries.OrderBy(x => x.Priority))
            {
                ApplyMenuEntry(multiContextMenu, contextMenuEntry, content);
            }   
        }


        private void ApplyMenuEntry(MultiContextMenu multiContextMenu, ContextMenuEntry entry, object content)
        {
            if (entry.VisibilityCheck != null && !entry.VisibilityCheck(content))
                return;

            var nameQueue = new Queue<String>(entry.Path.Split('/'));
            var currentMenuChildren = multiContextMenu.MenuChildren;

            while (nameQueue.Count > 0)
            {
                var name = nameQueue.Dequeue();

                if (nameQueue.Count == 0)
                {
                    if (currentMenuChildren.Any(x => x.Text == name))
                        throw new InvalidOperationException(String.Format("Could not add menu item '{0}'. An item with the same name already exists.", entry.Path));

                    var menuChild = new MenuChild(child => entry.Action?.Invoke(content)) { Text = name, Icon = entry.Icon, BeginGroup = entry.BeginGroup};

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
            }
        }


        public ContextMenuEntry RegisterEntry(ContextMenuEntry contextMenuEntry)
        {
            _contextMenuEntries.Add(contextMenuEntry);

            return contextMenuEntry;
        }


        public void RemoveEntry(ContextMenuEntry contextMenuEntry)
        {
            _contextMenuEntries.Remove(contextMenuEntry);
        }
    }


    public class ContextMenuEntry
    {
        private readonly string _path;
        
        private Texture2D _icon;
        private readonly Action<object> _action;
        private readonly Func<object, bool> _visibilityCheck;


        public ContextMenuEntry(string path, Action<object> action = null, Func<object, bool> visibilityCheck = null)
        {
            _path = path;
            _action = action;
            _visibilityCheck = visibilityCheck;
        }



        public bool BeginGroup
        {
            get;
            set;
        }



        public string Path => _path;

        public Texture2D Icon
        {
            get { return _icon; }
            set { _icon = value; }
        }



        public Action<object> Action => _action;

        public Func<object, bool> VisibilityCheck => _visibilityCheck;


        public int Priority
        {
            get;
            set;
        }
    }
}
