using System;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.GUI.MenuControls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Graphs.ParameterEditors
{
    public class EditorTreeViewItem : SelectableTreeViewItem
    {
        private readonly bool _canHaveRecursiveItem;
        private readonly MultiContextMenu _multiContextMenu;


        /*public Action<EditorTreeViewItem, int> ChildAdded { get; set; }
        public Action<EditorTreeViewItem> ChildRemoved { get; set; }*/


        public Func<EditorTreeViewItem, bool> AcceptsDrag = delegate { return false; };
        //public Action<EditorTreeViewItem, int> DragInto = delegate { };

        public EditorTreeViewItem(string text, bool canHaveRecursiveItem = false)
        {
            _canHaveRecursiveItem = canHaveRecursiveItem;
            Text = text;

            this.RightClick += OnRightClick;

            _multiContextMenu = new MultiContextMenu();
        }



        public MultiContextMenu MultiContextMenu
        {
            get { return _multiContextMenu; }
        }



        public bool CanHaveRecursiveItem
        {
            get { return _canHaveRecursiveItem; }
        }



        public virtual bool CanBeDragged
        {
            get { return false; }
        }



        private void OnRightClick(object sender, EventArgs eventArgs)
        {
            _multiContextMenu.Open(this.Screen, InputService.MousePosition);
        }



        /// <summary>
        /// Set (or removes) the highlight icon for Drag & Drop.
        /// </summary>
        /// <param name="icon">Can be either null, "Top", "Middle" or "Bottom"</param>
        internal void SetDropHighlights(String icon)
        {
            Texture = icon == null ? null : EmbeddedResources.Load<Texture2D>("Resources/ArrowDrop" + icon + ".png");
        }



        public void DragInto(EditorTreeViewItem item, int index)
        {
            if (item.Parent == this && index >= item.Parent.Items.IndexOf(item))
                index--;
            /*if (item.Parent == this)
                index--;*/

            item.Parent.Items.Remove(item);

            Items.Insert(index, item);

            /*argumentTreeViewItem.Items.Insert(i, item);
            item.Argument.Parent = listArgument;
            listArgument.Items.Insert(i, item.Argument);

            UpdatePorts(argumentTreeViewItem);*/
        }
    }
}