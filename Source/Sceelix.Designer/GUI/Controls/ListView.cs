using System;
using DigitalRune.Collections;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using Orientation = DigitalRune.Game.UI.Orientation;

namespace Sceelix.Designer.GUI.Controls
{
    public class ListView : ContentControl
    {
        private readonly StackPanel _panel;



        public ListView()
        {
            Content = _panel = new StackPanel {Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch};

            Items = new NotifyingCollection<ListViewItem>(false, false);
            Items.CollectionChanged += OnItemsChanged;
        }



        public NotifyingCollection<ListViewItem> Items
        {
            get;
            private set;
        }



        public event EventHandler<EventArgs> ItemSelectionChanged = delegate { };



        private void OnItemsChanged(object sender, CollectionChangedEventArgs<ListViewItem> e)
        {
            foreach (var treeViewItem in e.NewItems)
                treeViewItem.ListView = this;

            _panel.Children.Clear();

            foreach (var item in Items)
                _panel.Children.Add(item);

            InvalidateMeasure();
        }



        public void DeselectAll()
        {
            foreach (var listViewItem in Items)
            {
                listViewItem.IsSelected = false;
            }
        }



        public void Select(int index)
        {
            if (index >= 0 && index < Items.Count)
            {
                Select(Items[index]);
            }
        }



        public void Select(ListViewItem listViewItem)
        {
            if (!listViewItem.IsSelected)
            {
                DeselectAll();
                listViewItem.IsSelected = true;
                ItemSelectionChanged(listViewItem, EventArgs.Empty);
            }
        }
    }

    public class ListViewItem : ContentControl
    {
        private bool _isSelected;



        public ListViewItem()
        {
            Style = "StandardTreeViewItem";
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
        }



        public override string VisualState
        {
            get
            {
                if (_isSelected)
                    return "Selected";

                return "Default";
            }
        }



        public bool IsSelected
        {
            get { return _isSelected; }
            internal set
            {
                _isSelected = value;
                InvalidateVisual();
            }
        }



        internal ListView ListView
        {
            get;
            set;
        }



        public event EventHandler<EventArgs> Click = delegate { };
        public event EventHandler<EventArgs> RightClick = delegate { };
        public event EventHandler<EventArgs> DoubleClick = delegate { };



        protected override void OnHandleInput(InputContext context)
        {
            base.OnHandleInput(context);

            if (!InputService.IsMouseOrTouchHandled && Content.IsMouseOver)
            {
                if (InputService.IsDoubleClick(MouseButtons.Left))
                {
                    ListView.Select(this);
                    DoubleClick(this, EventArgs.Empty);
                }
                else if (InputService.IsPressed(MouseButtons.Right, false))
                {
                    ListView.Select(this);
                    RightClick(this, EventArgs.Empty);
                }
                else if (InputService.IsPressed(MouseButtons.Left, false))
                {
                    ListView.Select(this);
                    Click(this, EventArgs.Empty);
                }

                InputService.IsMouseOrTouchHandled = true;
            }
        }
    }
}