using System.Linq;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using Microsoft.Xna.Framework;
using Sceelix.Core.Parameters;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Designer.Graphs.Inspector.Graphs;

namespace Sceelix.Designer.Graphs.ParameterEditors
{
    public class ParameterEditorTreeViewItem : EditorTreeViewItem
    {
        private readonly GraphParameterEditorWindow _editorWindow;
        private readonly ParameterInfo _parameterInfo;


        //for dragging and dropping items on lists
        //private readonly Image _dragPointerIcon;
        private EditorTreeViewItem _previouslyHoveredControl;



        public ParameterEditorTreeViewItem(ParameterInfo parameterInfo, GraphParameterEditorWindow editorWindow)
            : base(parameterInfo.Label + " (" + Parameter.GetIdentifier(parameterInfo.Label) + ")",
                parameterInfo.CanHaveRecursiveItem)
        {
            _parameterInfo = parameterInfo;
            _editorWindow = editorWindow;
            //Text = parameterInfo.Label;
        }



        public ParameterInfo ParameterInfo
        {
            get { return _parameterInfo; }
        }



        public GraphParameterEditorWindow EditorWindow
        {
            get { return _editorWindow; }
        }



        public override bool CanBeDragged
        {
            get { return true; }
        }



        public void Delete()
        {
            Parent.Items.Remove(this);
        }



        protected override void OnHandleInput(InputContext context)
        {
            base.OnHandleInput(context);

            var draggedItem = TreeView.DraggedData as EditorTreeViewItem;
            if (draggedItem == null)
            {
                if (CanBeDragged && Header.IsMouseOver && InputService.IsPressed(MouseButtons.Left, false))
                {
                    TreeView.DraggedData = this;
                    Foreground = new Color(Foreground, 0.5f);
                }
            }
            else if (draggedItem == this)
            {
                if (_previouslyHoveredControl != null)
                    _previouslyHoveredControl.SetDropHighlights(null);


                //the item must be an ArgumentTreeViewItem, not be the dragged item itself and cannot be a child 
                EditorTreeViewItem hoveredItem = TreeView.GetMouseHoveredItem() as EditorTreeViewItem;
                if (hoveredItem != null && hoveredItem != draggedItem && !hoveredItem.GetAncestors().Contains(draggedItem))
                {
                    var relativePositionY = InputService.MousePosition.Y - hoveredItem.ActualY;
                    var relativePercentage = relativePositionY/hoveredItem.Header.ActualHeight;

                    var parent = hoveredItem.Parent as EditorTreeViewItem;
                    if (parent != null && CanDragBIntoA(parent, draggedItem) && CanDragBIntoA(hoveredItem, draggedItem))
                    {
                        //item can be put before, after and inside
                        if (InputService.IsDown(MouseButtons.Left))
                        {
                            if (relativePercentage < 0.3f)
                                hoveredItem.SetDropHighlights("Top");
                            else if (relativePercentage > 0.7f)
                                hoveredItem.SetDropHighlights("Bottom");
                            else
                                hoveredItem.SetDropHighlights("Middle");
                        }
                        else if (InputService.IsReleased(MouseButtons.Left))
                        {
                            var indexOf = parent.Items.IndexOf(hoveredItem);

                            if (relativePercentage < 0.3f)
                                parent.DragInto(draggedItem, indexOf); //place it above
                            else if (relativePercentage > 0.7f)
                                parent.DragInto(draggedItem, indexOf + 1); //place it below
                            else
                                hoveredItem.DragInto(draggedItem, hoveredItem.Items.OfType<ArgumentTreeViewItem>().Count()); //place it inside

                            TreeView.DraggedData = null;
                            draggedItem.Background = Color.Transparent;
                        }
                    }
                    //item can be put inside
                    else if (CanDragBIntoA(hoveredItem, draggedItem))
                    {
                        if (InputService.IsDown(MouseButtons.Left))
                        {
                            hoveredItem.SetDropHighlights("Middle");
                        }
                        else if (InputService.IsReleased(MouseButtons.Left))
                        {
                            hoveredItem.DragInto(draggedItem, hoveredItem.Items.OfType<ArgumentTreeViewItem>().Count()); //place it inside

                            TreeView.DraggedData = null;
                            draggedItem.Background = Color.Transparent;
                        }
                    }
                    //item can be put before or after, not inside
                    else if (parent != null && CanDragBIntoA(parent, draggedItem))
                    {
                        if (InputService.IsDown(MouseButtons.Left))
                        {
                            if (relativePercentage < 0.5f)
                                hoveredItem.SetDropHighlights("Top");
                            else if (relativePercentage > 0.5f)
                                hoveredItem.SetDropHighlights("Bottom");
                        }
                        else if (InputService.IsReleased(MouseButtons.Left))
                        {
                            var indexOf = parent.Items.IndexOf(hoveredItem);

                            if (relativePercentage < 0.5f)
                                parent.DragInto(draggedItem, indexOf); //place it above
                            else if (relativePercentage > 0.5f)
                                parent.DragInto(draggedItem, indexOf + 1); //place it below

                            TreeView.DraggedData = null;
                            draggedItem.Background = Color.Transparent;
                        }
                    }
                    else
                    {
                        hoveredItem.SetDropHighlights(null);
                    }

                    _previouslyHoveredControl = hoveredItem;
                }
                else
                {
                    //Parent.Items.Remove(HighlightItem);
                }

                if (InputService.IsReleased(MouseButtons.Left))
                {
                    _previouslyHoveredControl = null;
                    TreeView.DraggedData = null;
                    draggedItem.Foreground = new Color(Foreground, 1f);
                }
            }
        }



        private static bool CanDragBIntoA(EditorTreeViewItem itemA, EditorTreeViewItem itemB)
        {
            if (!itemA.CanHaveRecursiveItem &&
                itemB is ParameterEditorTreeViewItem
                && ((ParameterEditorTreeViewItem) itemB).ParameterInfo is RecursiveParameterInfo)
                return false;


            return itemA.AcceptsDrag(itemB);
        }
    }
}