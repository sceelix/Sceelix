using System;
using Microsoft.Xna.Framework.Graphics;

namespace Sceelix.Designer.GUI.MenuHandling
{
    public class BarMenuEntry
    {
        private readonly String _path;
        private readonly Action _action;
        private readonly String _menu;
        private readonly Texture2D _icon;


        /// <summary>
        /// Creates a menu entry to add to the main (top) menu.
        /// </summary>
        /// <param name="path">The menu path, defining its structure, e.g. Tools/MyTool/Hello.</param>
        /// <param name="action">The action to be performed on click.</param>
        public BarMenuEntry(string path, Action action)
        {
            _menu = "";
            _path = path;
            _action = action;
        }

        /// <summary>
        /// Creates a menu entry to add to the indicated menu.
        /// </summary>
        /// <param name="path">The menu path, defining its structure, e.g. Tools/MyTool/Hello.</param>
        /// <param name="action">The action to be performed on click.</param>
        public BarMenuEntry(string path, Action action, string menu)
        {
            _menu = menu;
            _path = path;
            _action = action;
        }


        public BarMenuEntry(string path, Action action, string menu, Texture2D icon)
        {
            _menu = menu;
            _path = path;
            _action = action;
            _icon = icon;
        }


        /// <summary>
        /// The menu where this entry will be placed.
        /// </summary>
        public string Menu
        {
            get { return _menu; }
        }



        public string Path
        {
            get { return _path; }
        }



        public Action Action
        {
            get { return _action; }
        }
    }
}