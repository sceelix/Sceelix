using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;

namespace Sceelix.Designer.GUI.TreeViewControls
{
    public class TreeView : UIControl, ITreeViewControl
    {
        // An internal stack panel is used to layout the Items.
        private StackPanel _panel;
        private bool _updateInProgress;



        public TreeView()
        {
            Style = "TreeView";

            Items = new NotifyingCollection<TreeViewItem>(false, false);
            Items.CollectionChanged += (s, e) => OnItemsChanged();
        }



        public TreeViewItem SelectedItem
        {
            get;
            internal set;
        }



        public Object DraggedData
        {
            get;
            set;
        }



        public NotifyingCollection<TreeViewItem> Items
        {
            get;
            private set;
        }



        public void BeginUpdate()
        {
            _updateInProgress = true;
        }



        public void EndUpdate()
        {
            _updateInProgress = false;
            OnItemsChanged();
        }



        private void OnItemsChanged()
        {
            if (_updateInProgress)
                return;

            // When Items is modified, rebuild the content of the stack panel.
            if (_panel == null)
                return;

            _panel.Children.Clear();

            foreach (var item in Items)
                _panel.Children.Add(item);

            InvalidateMeasure();
        }



        /*protected override void OnHandleInput(InputContext context)
        {
            //if(IsMouseOver)
                base.OnHandleInput(context);
        }*/



        protected override void OnLoad()
        {
            base.OnLoad();

            if (_panel == null)
            {
                // Create and initialize the internal stack panel.
                _panel = new StackPanel() {HorizontalAlignment = HorizontalAlignment.Stretch};

                foreach (var item in Items)
                    _panel.Children.Add(item);

                // The panel is the (only) visual child of the TreeView control.
                VisualChildren.Add(_panel);
            }
        }



        /*public class Columns
        {
            public float DesiredSize { get; set; }
            public int Level{get; set;}
        }

        protected override Vector2F OnMeasure(Vector2F availableSize)
        {
            base.OnMeasure(availableSize);

            List<Columns> _columns = new List<Columns>();

            foreach (var treeViewItem in Items.OfType<IColumnTreeViewItem>())
                DoStuff(treeViewItem, _columns, 0);*/

        /*List<float> maxSizes = new List<float>();

            foreach (var treeViewItem in Items.OfType<IColumnTreeViewItem>())
                Do(treeViewItem, 1, maxSizes);*/

        /*return base.OnMeasure(availableSize);
        }

        private void DoStuff(IColumnTreeViewItem treeViewItem, List<Columns> columns, int level)
        {
            foreach (var uiControl in treeViewItem.Columns)
            {
                columns.Add(new Columns() { DesiredSize = uiControl.DesiredWidth, Level = level });
            }

            foreach (IColumnTreeViewItem columnTreeViewItem in treeViewItem.Items.OfType<IColumnTreeViewItem>())
            {
                DoStuff(columnTreeViewItem, columns, level + 1);
            }
        }

        private void Do(IColumnTreeViewItem treeViewItem, int level, List<float> maxSizes)
        {
            for (int index = 0; index < treeViewItem.Columns.Count; index++)
            {
                var column = treeViewItem.Columns[index];

                if (index >= maxSizes.Count)
                    maxSizes.Add(0);

                float desiredWidth = column.DesiredWidth;
                if (index == 0)
                    desiredWidth += level*Padding.X;
                    
                maxSizes[index] = Math.Max(desiredWidth, maxSizes[index]);
            }

            foreach (IColumnTreeViewItem columnViewItem in treeViewItem.Items.OfType<IColumnTreeViewItem>())
            {
                Do(columnViewItem, level + 1, maxSizes);
            }
        }*/


        /*public void SelectItem(TreeViewItem item)
        {
            SelectedItem = item;

            // Update IsSelected flags of all TreeViewItems. 
            foreach (var descendant in UIHelper.GetDescendants(this).OfType<TreeViewItem>())
                descendant.IsSelected = (descendant == item);
        }*/



        public IEnumerable<TreeViewItem> GetLeafTreeItems()
        {
            return Items.SelectMany(GetLeafTreeItems);
        }



        private static IEnumerable<TreeViewItem> GetLeafTreeItems(TreeViewItem parentItem)
        {
            if (!parentItem.Items.Any())
                yield return parentItem;

            foreach (TreeViewItem treeViewItem in parentItem.Items)
            {
                foreach (var subItem in GetLeafTreeItems(treeViewItem))
                    yield return subItem;
            }
        }



        public IEnumerable<TreeViewItem> GetFlatTreeItems()
        {
            return Items.SelectMany(GetFlatTreeItems);
        }



        private static IEnumerable<TreeViewItem> GetFlatTreeItems(TreeViewItem parentItem)
        {
            yield return parentItem;

            foreach (TreeViewItem treeViewItem in parentItem.Items)
            {
                yield return treeViewItem;

                foreach (var subItem in GetFlatTreeItems(treeViewItem))
                    yield return subItem;
            }
        }



        public TreeViewItem GetTreeViewItemByUserData(object userData)
        {
            foreach (TreeViewItem treeViewItem in Items)
            {
                TreeViewItem subitem = treeViewItem.GetTreeViewItemByUserData(userData);
                if (subitem != null)
                    return subitem;
            }

            return null;
        }



        public void ExpandParents(TreeViewItem treeViewItem)
        {
            foreach (var result in UIHelper.GetAncestors(treeViewItem).OfType<TreeViewItem>())
            {
                result.IsExpanded = true;
            }
        }



        public TreeViewItem GetMouseHoveredItem()
        {
            if (!IsMouseOver)
                return null;

            foreach (var treeViewItem in Items)
            {
                var mouseHoveredItem = treeViewItem.GetMouseHoveredItem();
                if (mouseHoveredItem != null)
                    return mouseHoveredItem;
            }

            return null;
        }



        public IEnumerable<TreeViewItem> GetTreeItemsBetween(TreeViewItem item1, TreeViewItem item2)
        {
            List<TreeViewItem> items = new List<TreeViewItem>();

            foreach (var treeViewItem in Items)
            {
                if (!treeViewItem.GetTreeItemsBetween(item1, item2, items))
                    break;
            }

            return items;
        }

        public IEnumerable<TreeViewItem> GetParentItems(TreeViewItem item)
        {
            var currentItem = item.Parent as TreeViewItem;
            
            while(currentItem != null)
            {
                yield return currentItem;

                currentItem = currentItem.Parent as TreeViewItem;
            }
        }
    }
}