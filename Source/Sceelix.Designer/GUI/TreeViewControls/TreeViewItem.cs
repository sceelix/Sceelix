using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Linq;
using DigitalRune.Mathematics.Algebra;

namespace Sceelix.Designer.GUI.TreeViewControls
{
    // A single item/node of a tree view.
    public class TreeViewItem : UIControl, ITreeViewControl
    {
        public static readonly int IsExpandedPropertyId = CreateProperty(typeof(TreeViewItem), "IsExpanded", GamePropertyCategories.Default, null, true, UIPropertyOptions.AffectsMeasure);

        private UIControl _header;
        // An internal stack panel is used to layout the child items.
        private StackPanel _panel;



        public TreeViewItem()
        {
            Style = "TreeViewItem";

            Items = new NotifyingCollection<TreeViewItem>(false, false);
            Items.CollectionChanged += (s, e) => OnItemsChanged();

            var isExpandedProperty = Properties.Get<bool>(IsExpandedPropertyId);
            isExpandedProperty.Changed += OnExpandedChanged;

            InputProcessed += OnInputProcessed;

            //Vector3F color = RandomHelper.Random.NextVector3F(0, 1); 
            //Background = new Color(color.X, color.Y, color.Z, 0.4f);
        }



        // The TreeView control can be found by searching the visual tree.
        public TreeView TreeView
        {
            get { return UIHelper.GetAncestors(this).OfType<TreeView>().FirstOrDefault(); }
        }



        public int Level
        {
            get { return UIHelper.GetAncestors(this).IndexOf(val => val is TreeView); }
        }



        /// <summary>
        /// Returns the total padding, ASSUMING that the padding 
        /// </summary>
        public float TotalPadding
        {
            get { return Level*Padding.X; }
        }



        public ITreeViewControl Parent
        {
            get
            {
                //var treeViewControls = UIHelper.GetAncestors(this).OfType<ITreeViewControl>();
                return UIHelper.GetAncestors(this).OfType<ITreeViewControl>().FirstOrDefault();
            }
        }



        public bool IsExpanded
        {
            get { return GetValue<bool>(IsExpandedPropertyId); }
            set
            {
                SetValue(IsExpandedPropertyId, value);

                foreach (var item in Items)
                    item.IsVisible = value;
            }
        }



        public override string VisualState
        {
            get
            {
                if (Items.Count == 0)
                    return "Default";

                if (IsExpanded)
                    return "Expanded";

                return "Collapsed";
            }
        }



        // The "label" of this item.
        public UIControl Header
        {
            get { return _header; }
            set
            {
                if (_header == value)
                    return;

                _header = value;

                OnItemsChanged();
            }
        }



        /// <summary>
        /// Indicates if the item or any its subitems should be updated.
        /// </summary>
        internal bool IsViable
        {
            get;
            set;
        }



        // The child items.
        public NotifyingCollection<TreeViewItem> Items
        {
            get;
            private set;
        }



        private void OnInputProcessed(object sender, InputEventArgs inputEventArgs)
        {
            if (InputService == null)
                return;
            


            if (!InputService.IsMouseOrTouchHandled && InputService.IsPressed(MouseButtons.Left, false))
            {
                // The mouse is not handled and the left button is pressed.

                var headerHeight = Header != null ? Header.ActualHeight : 0;

                if (inputEventArgs.Context.MousePosition.X > ActualX && inputEventArgs.Context.MousePosition.X < ActualX + Padding.X
                    && inputEventArgs.Context.MousePosition.Y > ActualY && inputEventArgs.Context.MousePosition.Y < ActualY + headerHeight)
                {
                    // The area left of the label was clicked. This is the area where a Expand/Collapse
                    // icon is drawn. --> Switch between expanded and collapsed state.
                    IsExpanded = !IsExpanded;

                    InputService.IsMouseOrTouchHandled = true;
                }
                /*else
                {
                    // If the mouse was over the Header, this control should be selected.
                    if (Header != null && Header.IsMouseOver && TreeView != null)
                        TreeView.SelectItem(this);
                }*/
            }
        }



        private void OnItemsChanged()
        {
            // When Items is modified, rebuild the content of the stack panel.

            if (_panel == null)
                return;

            _panel.Children.Clear();

            _panel.Children.Add(Header);
            foreach (var item in Items)
            {
                //the item will only be visible if expanded.
                item.IsVisible = this.IsExpanded;

                _panel.Children.Add(item);
            }


            InvalidateMeasure();
        }



        protected override void OnLoad()
        {
            base.OnLoad();

            if (_panel == null)
            {
                // Create the internal stack panel.
                // The padding of this tree view item is used as the margin of the panel.
                _panel = new StackPanel
                {
                    Margin = Padding,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                // Whenever the padding is changed, the panel margin should be updated.
                var panelMargin = _panel.Properties.Get<Vector4F>(MarginPropertyId);
                var padding = this.Properties.Get<Vector4F>(PaddingPropertyId);
                padding.Changed += panelMargin.Change;

                // Add Items to the panel.
                _panel.Children.Add(Header);
                foreach (var item in Items)
                    _panel.Children.Add(item);

                // The panel is the (only) visual child of the TreeView control.
                VisualChildren.Add(_panel);
            }
        }



        // Called when this control should handle input.
        /*protected override void OnHandleInput(InputContext context)
        {
            // Call base class. This will automatically handle the input of the visual children.
            base.OnHandleInput(context);

            
        }*/



        private void OnExpandedChanged(object sender, GamePropertyEventArgs<bool> eventArgs)
        {
            // Toggle the visibility of all child Items.
            foreach (var item in Items)
                item.IsVisible = eventArgs.NewValue;
        }



        public TreeViewItem GetTreeViewItemByUserData(object userData)
        {
            if (UserData == userData)
                return this;

            foreach (TreeViewItem treeViewItem in Items)
            {
                TreeViewItem subitem = treeViewItem.GetTreeViewItemByUserData(userData);
                if (subitem != null)
                    return subitem;
            }

            return null;
        }



        public TreeViewItem GetMouseHoveredItem()
        {
            if (!IsMouseOver)
                return null;

            if (Header.IsMouseOver)
                return this;

            foreach (var treeViewItem in Items)
            {
                var mouseHoveredItem = treeViewItem.GetMouseHoveredItem();
                if (mouseHoveredItem != null)
                    return mouseHoveredItem;
            }

            //if (IsMouseOver)
            //    return this;

            return null;

            //return Items.Select(treeViewItem => treeViewItem.GetMouseHoveredItem()).FirstOrDefault(mouseHoveredItem => mouseHoveredItem != null);
        }



        public bool GetTreeItemsBetween(TreeViewItem item1, TreeViewItem item2, List<TreeViewItem> items)
        {
            if (this == item1)
            {
                //if this matches item1 and we already had other items in the list,
                //it means item2 came first, so add the item and finish
                if (items.Count > 0)
                {
                    items.Add(this);
                    return false;
                }
                else
                {
                    //otherwise, add this item and let's keep going
                    items.Add(this);
                }
            }
            else if (this == item2)
            {
                //if this matches item2 and we already had other items in the list,
                //it means item1 came first, so add the item and finish
                if (items.Count > 0)
                {
                    items.Add(this);
                    return false;
                }
                else
                {
                    //otherwise, add this item and let's keep going
                    items.Add(this);
                }
            }
            else if (items.Count > 0)
            {
                //if it's something in the middle, add it
                items.Add(this);
            }

            if (IsExpanded)
            {
                foreach (var treeViewItem in Items)
                {
                    if (!treeViewItem.GetTreeItemsBetween(item1, item2, items))
                        return false;
                }
            }

            return true;
        }
    }
}