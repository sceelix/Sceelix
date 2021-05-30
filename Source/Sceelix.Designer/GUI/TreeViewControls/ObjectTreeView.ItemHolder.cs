using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sceelix.Designer.GUI.TreeViewControls
{
    public partial class ObjectTreeView 
    {
        public class ItemHolder
        {
            private readonly ObjectTreeView _objectTreeView;
            private readonly int _level;

            private Object _item;
            private ItemControl _itemControl;

            private List<ItemHolder> _subItemHolders;

            private bool? _hasChildren;
            private bool? _isExpanded;



            /// <summary>
            /// Constructor for the root element.
            /// </summary>
            /// <param name="item">The item.</param>
            /// <param name="objectTreeView">The object TreeView.</param>
            /// <param name="itemHolders">The item holders.</param>
            public ItemHolder(ObjectTreeView objectTreeView, List<ItemHolder> itemHolders)
            {
                _objectTreeView = objectTreeView;
                _level = -1;
                _hasChildren = true;
                _isExpanded = true;
                _subItemHolders = itemHolders;
            }


            public ItemHolder(object item, ObjectTreeView objectTreeView, int level = 0)
            {
                _item = item;
                _objectTreeView = objectTreeView;
                _level = level;
            }


            public object Item
            {
                get { return _item; }
            }



            public ItemControl ItemControl
            {
                get { return _itemControl; }
                set { _itemControl = value; }
            }



            public ObjectTreeView ObjectTreeView
            {
                get { return _objectTreeView; }
            }



            public void OrderBy(Column column, bool descending)
            {
                _subItemHolders = descending ? 
                    SubItemHolders.OrderByDescending(x => _objectTreeView.GetColumnValue(x.Item, column)).ToList() 
                    : SubItemHolders.OrderBy(x => _objectTreeView.GetColumnValue(x.Item, column)).ToList();

                foreach (ItemHolder itemHolder in SubItemHolders.Where(x => x.IsExpanded))
                    itemHolder.OrderBy(column, descending);
            }


            /// <summary>
            /// Gets the number of items down the tree that are visible (meaning that the parents have children and are expanded).
            /// </summary>
            /// <value>
            /// The visible item count.
            /// </value>
            public int VisibleItemCount
            {
                get { return SubItemHolders.Count + SubItemHolders.Where(x => x.HasVisibleSubItems).Sum(x => x.VisibleItemCount); }
            }



            public bool HasChildren
            {
                get
                {
                    if (!_hasChildren.HasValue)
                        _hasChildren = _objectTreeView.HasChildren(_item);

                    return _hasChildren.Value;
                }
            }

            public bool IsExpanded
            {
                get
                {
                    if (!_isExpanded.HasValue)
                        _isExpanded = _objectTreeView.IsInitiallyExpanded(_item);

                    return _isExpanded.Value;
                }
                set
                {
                    if (_isExpanded != value)
                    {
                        _isExpanded = value;

                        _objectTreeView.ItemPropertyChanged("IsExpanded", !value, value);
                    }
                }
            }


            public bool HasVisibleSubItems
            {
                get { return HasChildren && IsExpanded; }
            }


            public List<ItemHolder> SubItemHolders
            {
                get
                {
                    if (_subItemHolders == null)
                        _subItemHolders = _objectTreeView.GetChildren(_item).Select(x => new ItemHolder(x,_objectTreeView, _level + 1)).ToList();

                    return _subItemHolders;
                }
            }



            public int Level
            {
                get { return _level; }
            }
        }
    }
}
