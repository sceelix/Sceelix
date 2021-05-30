using System;
using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using HorizontalAlignment = DigitalRune.Game.UI.HorizontalAlignment;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using MouseButtons = DigitalRune.Game.Input.MouseButtons;
using Orientation = DigitalRune.Game.UI.Orientation;

namespace Sceelix.Designer.GUI.MenuControls
{
    public class MultiContextMenu : StackPanel, IMenuControl
    {
        private readonly UIControl _control;
        private readonly NotifyingCollection<MenuChild> _menuChildren = new NotifyingCollection<MenuChild>();



        public MultiContextMenu()
        {
            Style = "MenuItemContent";

            Orientation = Orientation.Vertical;
            IsFocusScope = true;

            _menuChildren.CollectionChanged += MenuChildrenOnCollectionChanged;
        }



        public MultiContextMenu(UIControl control) : this()
        {
            _control = control;

            control.InputProcessing += ControlOnInputProcessing;
        }



        public bool IsOpen
        {
            get
            {
                if (Screen == null)
                    return false;

                return Screen.Children.Contains(this);
            }
        }



        public NotifyingCollection<MenuChild> MenuChildren
        {
            get { return _menuChildren; }
        }



        private void ControlOnInputProcessing(object sender, InputEventArgs inputEventArgs)
        {
            if (_control.IsMouseOver && _control.InputService.IsPressed(MouseButtons.Right, false))
                Open(_control.Screen, inputEventArgs.Context.ScreenMousePosition);
        }



        /// <summary>
        /// For the items on the main strip.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="collectionChangedEventArgs"></param>
        protected void MenuChildrenOnCollectionChanged(object sender, CollectionChangedEventArgs<MenuChild> collectionChangedEventArgs)
        {
            if (collectionChangedEventArgs.Action == CollectionChangedAction.Add)
            {
                foreach (MenuChild menuItemControl in collectionChangedEventArgs.NewItems)
                {
                    menuItemControl.NativeClick += AnyMenuItemControlMouseClick;

                    menuItemControl.Focused += OnMainMenuItemEnter;
                    menuItemControl.Orientation = Orientation.Horizontal;
                    menuItemControl.HorizontalAlignment = HorizontalAlignment.Stretch;
                    menuItemControl.MenuParent = this;

                    menuItemControl.MenuChildren.CollectionChanged += SubMenuChildrenOnCollectionChanged;
                }
            }
        }



        /// <summary>
        /// For the subitems.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="collectionChangedEventArgs"></param>
        private void SubMenuChildrenOnCollectionChanged(object sender, CollectionChangedEventArgs<MenuChild> collectionChangedEventArgs)
        {
            if (collectionChangedEventArgs.Action == CollectionChangedAction.Add)
            {
                foreach (MenuChild menuItemControl in collectionChangedEventArgs.NewItems)
                {
                    menuItemControl.NativeClick += AnyMenuItemControlMouseClick;
                    menuItemControl.Focused += OnMainMenuItemEnter;
                    menuItemControl.Orientation = Orientation.Horizontal;

                    menuItemControl.MenuChildren.CollectionChanged += SubMenuChildrenOnCollectionChanged;
                }
            }
        }



        public void Open(UIScreen screen, Vector2F screenMousePosition)
        {
            if (IsOpen)
                return;

            X = screenMousePosition.X;
            Y = screenMousePosition.Y;

            screen.Children.Insert(0, this);

            screen.FocusManager.Focus(this);
        }



        protected override Vector2F OnMeasure(Vector2F availableSize)
        {
            var measure = base.OnMeasure(availableSize);

            if (X + measure.X > Screen.ActualWidth)
                X = X - measure.X;
            if (Y + measure.Y > Screen.ActualHeight)
                Y = Y - measure.Y;

            return measure;
        }



        protected override void OnHandleInput(InputContext context)
        {
            base.OnHandleInput(context);

            if (IsOpen)
            {
                //we need to decide if we close the menu or not
                //if (!InputService.IsMouseOrTouchHandled)
                {
                    if ((InputService.IsPressed(MouseButtons.Left, false) ||
                         InputService.IsPressed(MouseButtons.Middle, false) ||
                         InputService.IsPressed(MouseButtons.Right, false)))
                    {
                        //InputService.IsMouseOrTouchHandled = true;
                        //if none of the children are selected
                        if (_menuChildren.All(val => !val.MouseOverOrOnChild))
                        {
                            CloseAll();
                        }
                    }
                }

                if (InputService != null && !InputService.IsKeyboardHandled)
                {
                    if (InputService.IsPressed(Keys.Escape, false) && IsOpen)
                    {
                        InputService.IsKeyboardHandled = true;

                        CloseAll();

                        //CloseLeaf();
                    }
                }

                //if (IsMouseOver)
                //    InputService.IsMouseOrTouchHandled = true;
            }
        }



        private void AnyMenuItemControlMouseClick(MenuChild menuChild)
        {
            if (!menuChild.HasChildren)
            {
                CloseAll();
            }
        }



        /*private void AnyMenuItemControlMouseClick(object sender, EventArgs eventArgs)
        {
            if (!((MenuChild)sender).HasChildren)
            {
                CloseAll();
            }
        }*/



        private void OnMainMenuItemEnter(MenuChild obj)
        {
            //if (IsOpen)
            obj.OpenSubMenu();
        }



        protected override void OnLoad()
        {
            base.OnLoad();

            Children.Clear();

            foreach (var menuItemControl in MenuChildren)
            {
                if (menuItemControl.BeginGroup)
                    Children.Add(new SeparatorControl());

                Children.Add(menuItemControl);
            }
        }



        internal virtual void CloseLeaf()
        {
            if (!MenuChildren.Any(val => val.CloseLeaf()))
            {
                Screen.Children.Remove(this);
            }
        }



        internal virtual void CloseAll()
        {
            foreach (var menuItemControl in MenuChildren)
                menuItemControl.CloseSubMenu();

            Screen.Children.Remove(this);
        }



        protected override void OnUpdate(TimeSpan deltaTime)
        {
            base.OnUpdate(deltaTime);

            //we need to do this, otherwise focus problems can make this window stay behind
            //other windows
            if (IsOpen)
                Screen.BringToFront(this);
        }
    }
}