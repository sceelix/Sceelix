using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Sceelix.Designer.GUI.Controls;

namespace Sceelix.Designer.GUI.TreeViewControls
{
    public partial class ObjectTreeView
    {
        /// <summary>
        /// Event that indicates when an item property (for instance, IsExpanded) is changed
        /// </summary>
        public event PropertyChange ItemPropertyChanged = delegate { };



        /// <summary>
        /// Event that indicates that an item has been clicked or selected with the keyboard.
        /// </summary>
        public event ItemClick ItemSelected = delegate { };



        /// <summary>
        /// Event that indicates that an item has been double-clicked or pressed enter when selected.
        /// </summary>
        public event ItemClick ItemEntered = delegate { };

        /// <summary>
        /// Event that indicates that a cell has been clicked or selected with the keyboard.
        /// </summary>
        /*public event CellClick CellSelected = delegate { };



        /// <summary>
        /// Event that indicates that a cell has been double-clicked or pressed enter when selected.
        /// </summary>
        public event CellClick CellEntered = delegate { };*/



        public class ItemControl : ContentControl
        {
            public event EventHandler<EventArgs> Expanded = delegate { };

            private ItemHolder _itemHolder;
            private bool _isSelected;



            public ItemControl(ItemHolder itemHolder)
            {
                _itemHolder = itemHolder;

                Style = "TreeViewItem";

                Expanded += BaseView.ExplorerItemControlOnExpanded;
            }



            public ObjectTreeView BaseView
            {
                get { return _itemHolder.ObjectTreeView; }
            }



            public Object Item
            {
                get { return _itemHolder.Item; }
            }


            public int Level
            {
                get { return _itemHolder.Level; }
            }

            

            public bool IsExpanded
            {
                get { return _itemHolder.IsExpanded; }
                set
                {
                    _itemHolder.IsExpanded = value;
                }
            }



            public bool HasChildren
            {
                get { return _itemHolder.HasChildren; }
            }



            public bool IsSelected
            {
                get { return _isSelected; }
                set
                {
                    //only proceed if there is a change
                    if (_isSelected != value)
                    {
                        if (value)
                        {
                            //deselect the one before, if needed
                            if (BaseView._selectedItemControl != null)
                                BaseView._selectedItemControl.IsSelected = false;

                            //store this one as selected
                            BaseView._selectedItemControl = this;
                        }

                        _isSelected = value;

                        BaseView.ItemPropertyChanged("IsSelected", !value, value);

                        //update the background color
                        Background = _isSelected ? BaseView.GetSelectionColor(Item) : Color.Transparent;

                        //and warn whoever is interested
                        if (_isSelected)
                            BaseView.ItemSelected(Item);
                    }
                }
            }



            public override string VisualState
            {
                get
                {
                    if (!HasChildren)
                        return "Default";

                    if (IsExpanded)
                        return "Expanded";

                    return "Collapsed";
                }
            }


            



            protected override void OnLoad()
            {
                if (!BaseView.Columns.Any())
                {
                    //get the value
                    var value = BaseView.GetColumnValue(Item, null);

                    //get the type of control, by default is textual
                    var control = BaseView.GetCellControl(null, value);

                    //set the color
                    control.Foreground = BaseView.GetForeground(Item, null);

                    CreateMenuItem(control, null);
                    

                    Content = control;
                }
                else
                {
                    var panel = new FlexibleStackPanel() {Margin = new Vector4F(0, 0, 0, 0), Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Stretch, ProportionalFlexibility = true, Test = 1}; //, 
                    for (int index = 0; index < BaseView.Columns.Count; index++)
                    {
                        Column column = BaseView.Columns[index];

                        var alignment = column.Flexible ? HorizontalAlignment.Stretch : HorizontalAlignment.Left;
                        var width = index == 0 ? column.Width - BaseView.ItemTreeInset*Level - BaseView.ItemPadding.X : column.Width;

                        //get the value
                        var value = BaseView.GetColumnValue(Item, column);

                        //get the type of control, by default is textual
                        var control = BaseView.GetCellControl(column, value);

                        //set the color
                        control.Foreground = BaseView.GetForeground(Item, null);
                        control.HorizontalAlignment = HorizontalAlignment.Stretch;

                        CreateMenuItem(control, column);

                        control.Width = width;
                        control.HorizontalAlignment = alignment;

                        if (BaseView.HasCellToolTip)
                            control.ToolTip = BaseView.GetCellTooltip(Item, column);

                        panel.Children.Add(control);
                    }
                    panel.VerticalAlignment = VerticalAlignment.Stretch;
                    Content = panel;
                }

                Height = BaseView.ItemHeight;
                Margin = new Vector4F(BaseView.ItemTreeInset*Level, 0, 0, 0) + BaseView.ItemMargin;
                HorizontalAlignment = HorizontalAlignment.Stretch;
                Padding = BaseView.ItemPadding;

                if (BaseView.HasItemToolTip)
                    ToolTip = BaseView.GetItemTooltip(Item);

                base.OnLoad();
            }



            private void CreateMenuItem(UIControl control, Column column)
            {
                var menu = BaseView.GetContextMenu(Item, column);
                if (menu != null)
                {
                    control.InputProcessed += delegate
                    {
                        //if the right mouse button is clicked, select the item and open the menu
                        if (!InputService.IsMouseOrTouchHandled && InputService.IsPressed(MouseButtons.Right, false) && control.IsMouseOver)
                        {
                            menu.Open(BaseView.Screen, InputService.MousePosition);
                        }
                    };
                }
            }



            public ItemHolder ItemHolder
            {
                get { return _itemHolder; }
            }



            // Called when this control should handle input.
            protected override void OnHandleInput(InputContext context)
            {
                // Call base class. This will automatically handle the input of the visual children.
                base.OnHandleInput(context);

                if (!InputService.IsMouseOrTouchHandled && IsMouseOver)
                {
                    // The mouse is not handled and the left button is pressed.

                    if (InputService.IsPressed(MouseButtons.Left, false))
                    {
                        if (context.MousePosition.X > ActualX && context.MousePosition.X < ActualX + Padding.X
                            && context.MousePosition.Y > ActualY && context.MousePosition.Y < ActualY + BaseView.ItemHeight)
                        {
                            InputService.IsMouseOrTouchHandled = true;

                            // The area left of the label was clicked. This is the area where a Expand/Collapse
                            // icon is drawn. --> Switch between expanded and collapsed state.
                            IsExpanded = !IsExpanded;

                            InvalidateVisual();

                            Expanded(this, EventArgs.Empty);
                        }
                        else if (BaseView.SelectionType == SelectionViewType.Line)
                        {
                            IsSelected = true;


                            /*if (!_isSelected)
                            {
                                //first, deselect the other control that may be selected
                                if (BaseView._selectedItemControl != null)
                                    BaseView._selectedItemControl.IsSelected = false;

                                //store this one as selected
                                BaseView._selectedItemControl = this;

                                //and finally Select this item
                                IsSelected = true;

                                //and warn whoever is interested
                                BaseView.ItemSelected(Item);
                            }*/
                        }
                        /*else if (_baseView.SelectionType == SelectionViewType.Cell)
                        {
                            
                        }*/
                    }
                    else if (InputService.IsPressed(MouseButtons.Right, false))
                    {
                        IsSelected = true;
                    }
                    if (InputService.IsDoubleClick(MouseButtons.Left))
                    {
                        if (BaseView.SelectionType == SelectionViewType.Line)
                            BaseView.ItemEntered.Invoke(Item);
                    }
                    
                }
            }

        }
    }
}