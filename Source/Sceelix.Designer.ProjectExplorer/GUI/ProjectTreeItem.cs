using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI.Controls;
using Microsoft.Xna.Framework;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.ProjectExplorer.Management;

namespace Sceelix.Designer.ProjectExplorer.GUI
{
    public class ProjectTreeItem : SelectableTreeViewItem
    {
        private bool _canBeDragged;



        public ProjectTreeItem()
        {
            base.IsMultiSelectable = true;
        }



        /*protected override void OnLoad()
        {
            base.OnLoad();

            //Cursor = Screen.Renderer.GetCursor("SizeAll");
        }

        protected override void OnHandleInput(InputContext context)
        {
            base.OnHandleInput(context);

            var draggedItem = TreeView.DraggedData as ProjectTreeItem[];
            if (draggedItem == null)
            {
                if (IsSelected && InputService.IsPressed(MouseButtons.Left, false))
                {
                    TreeView.DraggedData = new[] { this };
                    this.Background = Color.DimGray;
                }
            }
        }*/



        public bool CanBeDragged
        {
            get { return _canBeDragged; }
            set { _canBeDragged = value; }
        }
    }
}