using System;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace Sceelix.Designer.GUI.TreeViewControls
{
    public class SelectableTreeViewItem : TreeViewItem
    {
        private readonly SelectableTreeViewItemContent _itemContent;
        private ContentControl _contentControl;



        public SelectableTreeViewItem()
        {
            Header = _itemContent = new SelectableTreeViewItemContent();
            Header.InputProcessed += HeaderOnInputProcessed;
        }



        public Texture2D Texture
        {
            get { return _itemContent.Texture; }
            set { _itemContent.Texture = value; }
        }



        public string Text
        {
            get { return _itemContent.Text; }
            set { _itemContent.Text = value; }
        }



        public bool IsSelected
        {
            get { return _itemContent.IsSelected; }
            set
            {
                _itemContent.IsSelected = value;

                if (value)
                    TreeView.SelectedItem = this;

                Header.InvalidateVisual();
            }
        }



        public Color TextColor
        {
            get { return _itemContent.TextColor; }
            set { _itemContent.TextColor = value; }
        }



        public bool IsMultiSelectable
        {
            get;
            set;
        }



        public event EventHandler<EventArgs> Click = delegate { };
        public event EventHandler<EventArgs> RightClick = delegate { };
        public event EventHandler<EventArgs> DoubleClick = delegate { };



        private void HeaderOnInputProcessed(object sender, InputEventArgs inputEventArgs)
        {
            if (!InputService.IsMouseOrTouchHandled && Header.IsMouseOver)
            {
                //On double click, launch the event and select the item
                if (InputService.IsDoubleClick(MouseButtons.Left))
                {
                    InputService.IsMouseOrTouchHandled = true;

                    DeselectAllItems();
                    IsSelected = true;
                    DoubleClick(this, EventArgs.Empty);
                }
                else if (InputService.IsPressed(MouseButtons.Right, false))
                {
                    InputService.IsMouseOrTouchHandled = true;

                    if (IsMultiSelectable)
                    {
                        if (IsSelected)
                            RightClick(this, EventArgs.Empty);
                        else
                        {
                            DeselectAllItems();
                            IsSelected = true;
                            RightClick(this, EventArgs.Empty);
                        }
                    }
                    else
                    {
                        DeselectAllItems();
                        IsSelected = true;
                        RightClick(this, EventArgs.Empty);
                    }
                }
                else if (InputService.IsPressed(MouseButtons.Left, false)
                         || InputService.Gestures.Any(x => x.GestureType.HasFlag(GestureType.Tap) || x.GestureType == GestureType.Tap))
                {
                    InputService.IsMouseOrTouchHandled = true;

                    if (IsMultiSelectable)
                    {
                        if (InputService.ModifierKeys.HasFlag(ModifierKeys.Control))
                        {
                            IsSelected = !IsSelected;
                        }
                        else if (InputService.ModifierKeys.HasFlag(ModifierKeys.Shift))
                        {
                            DeselectAllItems(false);

                            foreach (SelectableTreeViewItem item in TreeView.GetTreeItemsBetween(TreeView.SelectedItem, this).OfType<SelectableTreeViewItem>())
                                item._itemContent.IsSelected = true;
                        }
                        else
                        {
                            if (!IsSelected)
                                DeselectAllItems();

                            IsSelected = true;
                        }
                    }
                    else
                    {
                        DeselectAllItems();
                        IsSelected = true;
                    }

                    Click(this, EventArgs.Empty);
                }
                else if (InputService.IsReleased(MouseButtons.Left))
                {
                    InputService.IsMouseOrTouchHandled = true;

                    if (IsMultiSelectable)
                    {
                        if (!InputService.ModifierKeys.HasFlag(ModifierKeys.Control) && !InputService.ModifierKeys.HasFlag(ModifierKeys.Shift))
                        {
                            var previouslySelected = IsSelected;

                            DeselectAllItems();

                            IsSelected = previouslySelected;
                        }
                    }
                }
            }
        }



        protected override void OnLoad()
        {
            base.OnLoad();

            _contentControl = UIHelper.GetAncestors(this).OfType<ContentControl>().FirstOrDefault();
        }



        /*protected override void OnHandleInput(InputContext context)
        {
            base.OnHandleInput(context);
            
            if (!InputService.IsMouseOrTouchHandled && Header.IsMouseOver)
            {
                //On double click, launch the event and select the item
                if (InputService.IsDoubleClick(MouseButtons.Left))
                {
                    InputService.IsMouseOrTouchHandled = true;

                    DeselectAllItems();
                    IsSelected = true;
                    DoubleClick(this, EventArgs.Empty);
                }
                else if (InputService.IsPressed(MouseButtons.Right,false))
                {
                    InputService.IsMouseOrTouchHandled = true;

                    if (IsMultiSelectable)
                    {
                        if (IsSelected)
                            RightClick(this, EventArgs.Empty);
                        else
                        {
                            DeselectAllItems();
                            IsSelected = true;
                            RightClick(this, EventArgs.Empty);
                        }
                    }
                    else
                    {
                        DeselectAllItems();
                        IsSelected = true;
                        RightClick(this, EventArgs.Empty);
                    }
                }
                else if (InputService.IsPressed(MouseButtons.Left,false))
                {
                    InputService.IsMouseOrTouchHandled = true;

                    if (IsMultiSelectable)
                    {
                        if (InputService.ModifierKeys.HasFlag(ModifierKeys.Control))
                        {
                            IsSelected = !IsSelected;
                        }
                        else if (InputService.ModifierKeys.HasFlag(ModifierKeys.Shift))
                        {
                            DeselectAllItems(false);
                            
                            foreach (SelectableTreeViewItem item in TreeView.GetTreeItemsBetween(TreeView.SelectedItem, this).OfType<SelectableTreeViewItem>())
                                item._itemContent.IsSelected = true;
                        }
                        else
                        {
                            if(!IsSelected)
                                DeselectAllItems();

                            IsSelected = true;
                        }   
                    }
                    else
                    {
                        DeselectAllItems();
                        IsSelected = true;
                    }

                    Click(this, EventArgs.Empty);
                }
                else if (InputService.IsReleased(MouseButtons.Left))
                {
                    InputService.IsMouseOrTouchHandled = true;

                    if (IsMultiSelectable)
                    {
                        if (!InputService.ModifierKeys.HasFlag(ModifierKeys.Control) && !InputService.ModifierKeys.HasFlag(ModifierKeys.Shift))
                        {
                            var previouslySelected = IsSelected;

                            DeselectAllItems();

                            IsSelected = previouslySelected;
                        }
                    }
                }
            }

            
        }*/



        protected override void OnRender(UIRenderContext context)
        {
            //this heavily improves drawing performance for large trees
            if (!_contentControl.ClipContent || _contentControl.ActualBounds.Intersects(this.ActualBounds))
                base.OnRender(context);
        }



        protected void DeselectAllItems(bool resetTreeViewItem = true)
        {
            foreach (var item in UIHelper.GetDescendants(TreeView).OfType<SelectableTreeViewItem>())
            {
                item.IsSelected = false;
            }

            if (resetTreeViewItem)
                TreeView.SelectedItem = null;
        }
    }
}