using System;
using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Sceelix.Designer.GUI.MenuControls
{
    public class BarMenu : ContentControl, IMenuControl
    {
        private readonly NotifyingCollection<MenuChild> _menuChildren = new NotifyingCollection<MenuChild>();

        private readonly StackPanel _stackPanel = new StackPanel {Orientation = Orientation.Horizontal};

        private bool _isOpen;



        public BarMenu()
        {
            Style = "MenuItemContent";

            //this will be located at the top of a window, streching by default
            HorizontalAlignment = HorizontalAlignment.Stretch;

            ScrollViewer viewer = new ScrollViewer
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = _stackPanel
            };

            _menuChildren.CollectionChanged += MenuChildrenOnCollectionChanged;

            Height = 28;
            //_stackPanel.Children.Add(new Button(){Content = new TextBlock(){Text = "Dsda"}});

            Content = viewer;
        }



        public NotifyingCollection<MenuChild> MenuChildren
        {
            get { return _menuChildren; }
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
                    menuItemControl.Focusable = false;
                    menuItemControl.NativeClick += MainMenuItemControlMouseClick;
                    menuItemControl.NativeClick += AnyMenuItemControlMouseClick;

                    menuItemControl.Focused += OnMainMenuItemEnter;
                    menuItemControl.Orientation = Orientation.Vertical;
                    menuItemControl.MenuParent = this;

                    menuItemControl.MenuChildren.CollectionChanged += SubMenuChildrenOnCollectionChanged;
                }
            }

            Reload();
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
            if (_isOpen)
                obj.OpenSubMenu();
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
                    menuItemControl.Focused += OnSubmenuItemEnter;
                    menuItemControl.Orientation = Orientation.Horizontal;

                    menuItemControl.MenuChildren.CollectionChanged += SubMenuChildrenOnCollectionChanged;
                }
            }
        }



        private void OnSubmenuItemEnter(MenuChild obj)
        {
            //opens the submenu
            obj.OpenSubMenu();
        }



        private void MainMenuItemControlMouseClick(MenuChild menuChild)
        {
            if (_isOpen)
            {
                CloseAll();
            }
            else
            {
                _isOpen = true;

                menuChild.OpenSubMenu();
            }
        }



        /*private void MainMenuItemControlMouseClick(object sender, EventArgs eventArgs)
        {
            if (_isOpen)
            {
                CloseAll();
            }
            else
            {
                _isOpen = true;

                ((MenuChild)sender).OpenSubMenu();
            }
        }*/



        protected override void OnLoad()
        {
            base.OnLoad();

            Reload();
        }



        private void Reload()
        {
            _stackPanel.Children.Clear();

            foreach (var menuItemControl in MenuChildren)
            {
                if (menuItemControl.BeginGroup)
                    _stackPanel.Children.Add(new SeparatorControl());

                _stackPanel.Children.Add(menuItemControl);
            }
        }



        protected override void OnHandleInput(InputContext context)
        {
            base.OnHandleInput(context);

            //we need to decide if we close the menu or not
            //if (!InputService.IsMouseOrTouchHandled)
            {
                if ((InputService.IsPressed(MouseButtons.Left, false) ||
                     InputService.IsPressed(MouseButtons.Middle, false) ||
                     InputService.IsPressed(MouseButtons.Right, false)) && _isOpen)
                {
                    //InputService.IsMouseOrTouchHandled = true;
                    //if none of the children are selected
                    if (_menuChildren.All(val => !val.MouseOverOrOnChild))
                    {
                        CloseAll();
                    }
                }
            }

            /*if (InputService != null && !InputService.IsKeyboardHandled)
            {
                if (InputService.IsPressed(Keys.Escape, false) && )
                {
                    InputService.IsKeyboardHandled = true;

                    CloseAll();
                }
            }*/
        }



        internal virtual void CloseLeaf()
        {
            MenuChildren.Any(val => val.CloseLeaf());
        }



        internal virtual void CloseAll()
        {
            foreach (var menuItemControl in MenuChildren)
                menuItemControl.CloseSubMenu();

            _isOpen = false;
        }
    }
}