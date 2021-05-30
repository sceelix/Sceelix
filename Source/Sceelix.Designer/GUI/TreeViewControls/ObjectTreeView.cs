using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.MenuControls;
using Sceelix.Designer.Utils;
using Sceelix.Extensions;

namespace Sceelix.Designer.GUI.TreeViewControls
{
    public partial class ObjectTreeView : ContentControl
    {
        public delegate void ItemClick(Object item);

        //public delegate void CellClick(Object item, String column);

        public delegate void PropertyChange(String propertyName, Object oldValue, Object newValue);

        public enum ListViewType
        {
            List,
            Tree
        }

        public enum SelectionViewType
        {
            None,
            Line,
            //Cell
        }

        

        private const float DefaultInset = 20;
        private const float DefaultHeight = 18;
        private const float DefaultBranchPadding = 13;


        private readonly List<Column> _columns = new List<Column>();

        private readonly Synchronizer _synchronizer = new Synchronizer();
        private FlexibleStackPanel _columnStackPanel;
        private bool _hasCellToolTip = false;
        private bool _hasItemToolTip = false;
        
        private float _itemHeight = DefaultHeight;
        private Vector4F _itemMargin;
        private Vector4F _itemPadding = new Vector4F(DefaultBranchPadding, 0, 0, 0);
        private Color _selectionColor = new Color(200, 200, 200, 200);

        //the actual items are inside the itemHolders
        private ItemHolder _rootHolder;
        //private List<ItemHolder> _itemHolders = new List<ItemHolder>();

        private StackPanel _itemStackPanel;

        private float _itemTreeInset = DefaultInset;

        //holds the last saved height, so that we can check for changes in the control height
        private double _lastHeight;
        private bool _orderedDescending = false;

        private Column _ordereredColumn = null;
        private ScrollBar _scrollBar;


        private ItemControl _selectedItemControl;
        //private int _selectedItemControlIndex = -1;
        private SelectionViewType _selectionType;
        private bool _showColumnHeaders = true;
        private int _totalItemCount;

        private ListViewType _viewType = ListViewType.Tree;
        public Func<Column, Object, UIControl> GetCellControl = (column, obj) => new TextBlock() {Text = obj.SafeToString()};
        public Func<Object, Column, Object> GetCellTooltip = (obj, column) => null;
        public Func<Object, IEnumerable<object>> GetChildren = delegate { return new object[0]; };
        public Func<Object, Column, Object> GetColumnValue = (o, i) => o;
        public Func<Object, Column, Color> GetForeground;
        public Func<Object, Color> GetSelectionColor;
        public Func<Object, Column, MultiContextMenu> GetContextMenu = (o,c) => null;


        public Func<Object, Object> GetItemTooltip = (o) => null;
        public Func<Object, bool> HasChildren = delegate { return false; };
        public Func<Object, bool> IsInitiallyExpanded = delegate { return false; };
        



        public ObjectTreeView()
        {
            GetForeground = (o, i) => this.Foreground;
            GetSelectionColor = delegate(object o) { return _selectionColor; };

            _rootHolder = new ItemHolder(this,new List<ItemHolder>());
        }



        public IEnumerable<object> Items
        {
            get
            {
                return _rootHolder.SubItemHolders.Select(x => x.Item);
            }
            set
            {
                Clear();

                _rootHolder= new ItemHolder(this,value.Select(x => new ItemHolder(x, this)).ToList());

                //do a first calculation
                _totalItemCount = _rootHolder.VisibleItemCount; //_itemControls

                _synchronizer.Enqueue(UpdateScrollbarSize);
            }
        }



        public List<Column> Columns
        {
            get { return _columns; }
        }



        public float ItemTreeInset
        {
            get { return _itemTreeInset; }
            set { _itemTreeInset = value; }
        }



        public float ItemHeight
        {
            get { return _itemHeight; }
            set { _itemHeight = value; }
        }



        public bool HasItemToolTip
        {
            get { return _hasItemToolTip; }
            set { _hasItemToolTip = value; }
        }



        public bool HasCellToolTip
        {
            get { return _hasCellToolTip; }
            set { _hasCellToolTip = value; }
        }



        public ListViewType ViewType
        {
            get { return _viewType; }
            set { _viewType = value; }
        }



        public SelectionViewType SelectionType
        {
            get { return _selectionType; }
            set { _selectionType = value; }
        }



        public Vector4F ItemMargin
        {
            get { return _itemMargin; }
            set { _itemMargin = value; }
        }



        public Vector4F ItemPadding
        {
            get { return _itemPadding; }
            set { _itemPadding = value; }
        }



        public bool ShowColumnHeaders
        {
            get { return _showColumnHeaders; }
            set { _showColumnHeaders = value; }
        }



        public Color SelectionColor
        {
            get { return _selectionColor; }
            set { _selectionColor = value; }
        }



        protected override void OnLoad()
        {
            this.ClipContent = true;

            var verticalStackPanel = new FlexibleStackPanel()
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            if (_showColumnHeaders && Columns.Any())
            {
                _columnStackPanel = new FlexibleStackPanel()
                {
                    //Test = 1,
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    ProportionalFlexibility = true
                };

                for (int index = 0; index < _columns.Count; index++)
                {
                    Column column = _columns[index];

                    var actualColumnWidth = index == 0 ? column.Width - _itemPadding.X : column.Width;
                    var alignment = column.Flexible ? HorizontalAlignment.Stretch : HorizontalAlignment.Left;

                    var button = new Button()
                    {
                        HorizontalAlignment = alignment,
                        Content = new TextBlock()
                        {
                            Text = column.Name,
                            //Width = actualColumnWidth,
                            HorizontalAlignment = HorizontalAlignment.Center
                        },
                        Width = actualColumnWidth,
                        UserData = column
                    };
                    button.Click += delegate { OrderByColumn(column, column == _ordereredColumn && !_orderedDescending); };

                    _columnStackPanel.Children.Add(button);
                }

                verticalStackPanel.Children.Add(_columnStackPanel);
            }

            //OnPropertyChanged.Invoke(this,new GamePropertyEventArgs<Object>(){});
            verticalStackPanel.Children.Add(_itemStackPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
                //Background = Color.Red
            });

            var horizontalScroll = new ScrollViewer()
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, //ScrollBarVisibility.Auto
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Content = verticalStackPanel
            };

            _scrollBar = new ScrollBar()
            {
                Orientation = Orientation.Vertical,
                Style = "ScrollBarVertical",
                SmallChange = _itemHeight,
                LargeChange = _itemHeight*10,
                Value = 0,
                ViewportSize = 0.3f,
                Margin = new Vector4F(0),
                //HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            var valueProperty = _scrollBar.Properties.Get<float>("Value");
            valueProperty.Changed += delegate { UpdateVisibleItems(); };

            Content = new FlexibleStackPanel()
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Children = {horizontalScroll, _scrollBar}
            };

            InputProcessed += OnInputProcessed;

            base.OnLoad();
        }



        /*public float BranchPadding
        {
            get { return ViewType == ListViewType.Tree ? DefaultBranchPadding : 0; }
        }*/
        



        public void OrderByColumn(Column column, bool descending)
        {
            if (column.AllowSorting)
            {
                _ordereredColumn = column;
                _orderedDescending = descending;

                _rootHolder.OrderBy(column,descending);

                UpdateVisibleItems();

                UpdateColumnTitles();
            }
        }



        private void UpdateColumnTitles()
        {
            foreach (var columnButton in _columnStackPanel.Children.OfType<Button>())
            {
                var columnTextBlock = (TextBlock) columnButton.Content;

                var column = (Column) columnButton.UserData;
                if (column == _ordereredColumn)
                    columnTextBlock.Text = (_orderedDescending ? "> " : "< ") + column.Name;
                else
                    columnTextBlock.Text = column.Name;
            }
        }



        private void OnInputProcessed(object sender, InputEventArgs inputEventArgs)
        {
            if (_itemStackPanel.ActualBounds.Contains(InputService.MousePosition) && Math.Abs(InputService.MouseWheelDelta) > Single.Epsilon)
            {
                _scrollBar.Value -= (InputService.MouseWheelDelta/120)*_itemHeight;
            }
        }



        // Called when this control should handle input.
        /*protected override void OnHandleInput(InputContext context)
        {
            // Call base class. This will automatically handle the input of the visual children.
            base.OnHandleInput(context);

            if (_itemStackPanel.ActualBounds.Contains(InputService.MousePosition) && Math.Abs(InputService.MouseWheelDelta) > float.Epsilon)
            {
                _scrollBar.Value -= (InputService.MouseWheelDelta / 120) * _itemHeight;
            }
        }*/



        protected override void OnUpdate(TimeSpan deltaTime)
        {
            base.OnUpdate(deltaTime);

            //Check if the height has changed
            if (Math.Abs(_itemStackPanel.ActualHeight - _lastHeight) > Single.Epsilon)
            {
                _lastHeight = _itemStackPanel.ActualHeight;

                UpdateScrollbarSize();
            }

            _synchronizer.Update();
        }



        private void UpdateScrollbarSize()
        {
            //var newScrollbarVisibility = _scrollBar.IsVisible;
            //var totalItemCount = //

            //if we have items to be shown
            if (_totalItemCount > 0)
            {
                //determine how much space they would occupy
                var itemsTotalHeight = _totalItemCount*_itemHeight;

                //the size of the scroll bar
                var ratio = _itemStackPanel.ActualHeight/itemsTotalHeight;

                //if we have more space than items, the scrollbar will stay invisible
                _scrollBar.ViewportSize = Math.Min(ratio, 1);
                _scrollBar.IsVisible = ratio < 1;
            }
            else
            {
                //if it is empty, we do not need to show the scrollbar
                _scrollBar.IsVisible = false;
            }

            //_totalItemCount * _itemHeight - _itemStackPanel.ActualHeight + _itemHeight
            //_scrollBar.Maximum = _scrollBar.IsVisible ? _totalItemCount * _itemHeight - _itemStackPanel.ActualHeight + _itemHeight : _itemStackPanel.ActualHeight;

            //the minimum and maximum value of our scrollbar should match the item number
            _scrollBar.Minimum = 0;
            //_scrollBar.Maximum = Math.Max(_itemStackPanel.ActualHeight, _totalItemCount * _itemHeight - _itemStackPanel.ActualHeight + _itemHeight);
            _scrollBar.Maximum = _totalItemCount*_itemHeight - _itemStackPanel.ActualHeight + _itemHeight;
            _scrollBar.Value = Math.Max(0, Math.Min(_scrollBar.Value, _scrollBar.Maximum));

            UpdateVisibleItems();
        }



        



        private void UpdateVisibleItems()
        {
            _itemStackPanel.Children.Clear();

            int startingIndex = 0;
            int endIndex = _totalItemCount;

            //determine the starting and end index of the items we're showing
            if (_scrollBar.IsVisible)
            {
                startingIndex = (int) (_scrollBar.Value/_itemHeight);
                endIndex = (int) (_scrollBar.Value/_itemHeight + _itemStackPanel.ActualHeight/_itemHeight); // + 1;

                //small correction: Add 1 so as to compensate for the fraction, which could leave an empty space on the bottom
                //endIndex = Math.Min(_totalItemCount, endIndex + 1);
            }

            int currentIndex = 0;

            //now, go down the tree only the items we want
            UpdateVisibleItems(_rootHolder.SubItemHolders, ref currentIndex, startingIndex, endIndex);

            InvalidateMeasure();
        }



        private bool UpdateVisibleItems(List<ItemHolder> itemHolders,ref int currentIndex, int startingIndex, int endIndex)
        {
            for (int i = 0; i < itemHolders.Count; i++)
            {
                var currentItemHolder = itemHolders[i];
                var currentItem = currentItemHolder.Item;
                
                //if we have passed the starting index, start adding items
                if (currentIndex >= startingIndex)
                {
                    //if the item hasn't been created, do it now
                    if (currentItemHolder.ItemControl == null)
                        currentItemHolder.ItemControl = new ItemControl(currentItemHolder);

                    //and add it to the stackpanel of visible controls
                    _itemStackPanel.Children.Add(currentItemHolder.ItemControl);
                }

                currentIndex++;

                //terminate if we reached the end index, stop adding items and terminate the recursive procedure
                if (currentIndex >= endIndex)
                    return true;

                //if the item has children, we have to create the control to go deep in its children
                if (currentItemHolder.ItemControl == null && currentItemHolder.HasChildren && currentItemHolder.IsExpanded)
                    currentItemHolder.ItemControl = new ItemControl(currentItemHolder);

                if (currentItemHolder.ItemControl != null && currentItemHolder.ItemControl.IsExpanded)
                {
                    if (UpdateVisibleItems(currentItemHolder.SubItemHolders, ref currentIndex, startingIndex, endIndex))
                        return true;
                }
            }

            return false;
        }



        private void ExplorerItemControlOnExpanded(object sender, EventArgs eventArgs)
        {
            var control = (ItemControl) sender;

            var itemHolder = control.ItemHolder;
            var controlItemCount = itemHolder.VisibleItemCount;

            //var controlItemCount = control.VisibleItemCount();

            //add or subtract the number of items under that item, depending if expanded or collapsed
            _totalItemCount += control.IsExpanded ? controlItemCount : -controlItemCount;

            UpdateScrollbarSize();
        }


        /// <summary>
        /// Clears the item list and the contents of the treeview.
        /// </summary>
        public void Clear()
        {
            //removes items
            //_items.Clear();
            //_itemControls.Clear();
            _rootHolder = new ItemHolder(this, new List<ItemHolder>());
            _totalItemCount = 0;

            //updates scrollbar visibility and all
            _synchronizer.Enqueue(() => UpdateScrollbarSize());
        }

        

        public void Select(Object item, bool invokeEvent = false)
        {
            int index = 0;
            var select = Select(item, _rootHolder.SubItemHolders, index);
            if(select >= 0)
                FocusOnIndex(select);

            if(invokeEvent)
                ItemSelected.Invoke(item);
        }


        


        private int Select(Object itemToSelect, List<ItemHolder> itemHolders, int currentIndex)
        {
            for (int i = 0; i < itemHolders.Count; i++)
            {
                var currentItemHolder = itemHolders[i];
                var currentItem = currentItemHolder.Item;
                //ItemControl currentControl = currentItemHolder.ItemControl;
                

                //if this is the item we are looking for, we can end
                if (currentItem.Equals(itemToSelect))
                {
                    if (currentItemHolder.ItemControl == null)
                        currentItemHolder.ItemControl = new ItemControl(currentItemHolder);

                    /*if (_selectedItemControl != null)
                        _selectedItemControl.IsSelected = false;

                    _selectedItemControl = currentItemHolder.ItemControl;*/
                    currentItemHolder.ItemControl.IsSelected = true;

                    return currentIndex;
                }
                    

                currentIndex++;

                //if this item has children, we will look inside
                if (currentItemHolder.HasChildren)
                {
                    //goes recursively
                    var foundIndex = Select(itemToSelect, currentItemHolder.SubItemHolders, currentIndex);
                    if (foundIndex >= 0)
                    {
                        if(currentItemHolder.ItemControl == null)
                            currentItemHolder.ItemControl = new ItemControl(currentItemHolder);

                        currentItemHolder.ItemControl.IsExpanded = true;

                        return foundIndex;
                    }

                    //if the item was not found inside this item, add the count of subitems to the index
                    if(currentItemHolder.IsExpanded)
                        currentIndex += currentItemHolder.VisibleItemCount;
                }

                //check if we already have the corresponding itemControl created
                //if so, 
               /* if (currentItemHolder.ItemControl != null && currentItemHolder.ItemControl.HasChildren)
                {
                    var foundIndex = Select(itemToSelect, currentItemHolder.SubItemHolders, currentIndex);
                    if (foundIndex >= 0)
                    {
                        currentItemHolder.ItemControl.IsExpanded = true;
                        return foundIndex;
                    }
                    
                    //if the control was expanded, we still need to count all its children as opened items for the index
                    if (currentItemHolder.ItemControl.IsExpanded)
                    {
                        currentIndex += currentItemHolder.SubItemHolders.Count;
                    }
                }
                else if (HasChildren(currentItem))
                {
                    //get the children only once
                    //var children = GetChildren(currentItem).ToList();

                    //create the container for the controls alread, so that it can be updated in the sub function calls
                    //var newSubItemControls = new Dictionary<Object, ItemControl>();

                    //go recursively - if we have found the item, we can already create the itemcontrols, as EXPANDED
                    var foundIndex = Select(itemToSelect, currentItemHolder.SubItemHolders, currentIndex);
                    if (foundIndex >= 0)
                    {
                        currentItemHolder.ItemControl = new ItemControl(currentItemHolder);

                        return foundIndex;
                    }
                    //if the control was expanded, we still need to count all its children as opened items for the index
                    else if (IsInitiallyExpanded(currentItem))
                    {
                        currentIndex += currentItemHolder.SubItemHolders.Count;
                    }
                }*/
            }

            //we haven't found the item
            return -1;
        }



        public void FocusOnIndex(int index)
        {
            _scrollBar.Value = Math.Max(0, Math.Min(index*_itemHeight, _scrollBar.Maximum));
            UpdateVisibleItems();
        }



        public void DeselectAll()
        {
            //if indeed there is something is deselect
            if (_selectedItemControl != null)
            {
                _selectedItemControl.IsSelected = false;
                _selectedItemControl = null;
            }
        }



        public void Deselect(Object objToDeselect)
        {
            //if indeed there is something is deselect
            if (_selectedItemControl != null && _selectedItemControl.Item == objToDeselect)
            {
                _selectedItemControl.IsSelected = false;
                _selectedItemControl = null;
            }
        }



        public void SelectFirst(bool invokeEvent = false)
        {
            Select(_rootHolder.SubItemHolders.Select(x=>x.Item).FirstOrDefault(), invokeEvent);
        }
    }
}