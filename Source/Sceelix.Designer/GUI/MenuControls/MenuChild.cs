using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DigitalRune.Collections;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.GUI.MenuControls
{
    public class MenuChild : ContentControl, IMenuControl //ButtonBase
    {
        //private bool _previouslyMouseOver;

        private readonly Image _image;

        private readonly NotifyingCollection<MenuChild> _menuChildren = new NotifyingCollection<MenuChild>();

        private readonly StackPanel _stackContent = new StackPanel() {Orientation = Orientation.Vertical, Style = "MenuItemContent"};
        private readonly TextBlock _textBlock;

        private Orientation _orientation = Orientation.Horizontal;



        public MenuChild()
        {
            OverrideDefaultValue(typeof(MenuItem), FocusWhenMouseOverPropertyId, true);
            var gameProperty = Properties.Get<bool>(IsMouseOverPropertyId);
            gameProperty.Changed += GamePropertyOnChanged;


            _menuChildren.CollectionChanged += MenuChildrenOnCollectionChanged;

            Style = "MenuItemControl";
            IsFocusScope = true;
            Focusable = true;

            var buttonStackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Vector4F(2, 2, 20, 2)
            };

            buttonStackPanel.Children.Add(_image = new Image()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Vector4F(2),
                Height = 16,
                Width = 16
            });

            buttonStackPanel.Children.Add(_textBlock = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Vector4F(3)
            });

            Content = buttonStackPanel;
        }



        public MenuChild(Action<MenuChild> clickAction)
            : this()
        {
            Click += clickAction;

            /*Click += delegate(object sender, EventArgs args)
            {
                //if (IsCheckItem)
                    //IsChecked = !IsChecked;

                clickAction(this);
            };*/
        }



        public bool HasSubmenuOpen
        {
            get { return Screen != null && Screen.Children.Contains(_stackContent); }
        }



        public Orientation Orientation
        {
            get { return _orientation; }
            set { _orientation = value; }
        }



        public override string VisualState
        {
            get
            {
                String style = IsMouseOver ? "MouseOver" : "Default";

                if (MenuChildren.Count > 0)
                    style += "HasChildren";

                return style;
            }
        }



        /// <summary>
        /// This is set when the item is added as a child of another MenuItemControl.
        /// </summary>
        internal IMenuControl MenuParent
        {
            get;
            set;
        }



        public IEnumerable<MenuChild> Siblings
        {
            get { return MenuParent.MenuChildren.Where(val => val != this); }
        }



        public bool MouseOverOrOnChild
        {
            get { return IsMouseOver || MenuChildren.Any(val => val.MouseOverOrOnChild); }
        }



        public bool HasChildren
        {
            get { return MenuChildren.Count > 0; }
        }



        public bool BeginGroup
        {
            get;
            set;
        }



        public String Text
        {
            get { return _textBlock.Text; }
            set { _textBlock.Text = value; }
        }



        public Texture2D Icon
        {
            get { return _image.Texture; }
            set { _image.Texture = value; }
        }



        public bool HasAction
        {
            get { return Click != null; }
        }



        public NotifyingCollection<MenuChild> MenuChildren
        {
            get { return _menuChildren; }
        }



        //internal event Action<MenuChild> MouseClick = delegate { };
        internal event Action<MenuChild> Focused = delegate { };

        public event Action<MenuChild> Click;

        internal event Action<MenuChild> NativeClick;



        private void GamePropertyOnChanged(object sender, GamePropertyEventArgs<bool> gamePropertyEventArgs)
        {
            if (gamePropertyEventArgs.NewValue)
                Focused.Invoke(this);
        }



        private void MenuChildrenOnCollectionChanged(object sender, CollectionChangedEventArgs<MenuChild> collectionChangedEventArgs)
        {
            if (collectionChangedEventArgs.Action == CollectionChangedAction.Add)
            {
                foreach (MenuChild menuItemControl in collectionChangedEventArgs.NewItems)
                {
                    menuItemControl.MenuParent = this;
                }
            }

            RefreshItems();
        }



        private void RefreshItems()
        {
            _stackContent.Children.Clear();

            foreach (var menuItemControl in MenuChildren)
            {
                if (menuItemControl.BeginGroup)
                    _stackContent.Children.Add(new SeparatorControl());

                menuItemControl.HorizontalAlignment = HorizontalAlignment.Stretch;

                _stackContent.Children.Add(menuItemControl);
            }
        }



        protected override void OnHandleInput(InputContext context)
        {
            var inputService = InputService;

            //handle the children first
            base.OnHandleInput(context);

            if (!inputService.IsMouseOrTouchHandled)
            {
                /*if (!_previouslyMouseOver && IsMouseOver)
                {
                    inputService.IsMouseOrTouchHandled = true;
                    MouseEntered(this);
                    InvalidateVisual();
                }*/
                if (ActualBounds.Contains(inputService.MousePosition))
                    //if (IsMouseOver)
                {
                    inputService.IsMouseOrTouchHandled = true;

                    if (inputService.IsPressed(MouseButtons.Left, false))
                    {
                        if (Click != null)
                            Click(this);

                        if (NativeClick != null)
                            NativeClick(this);
                    }
                }

                /*if (IsMouseOver && inputService.IsPressed(MouseButtons.Left, false))
                {
                    //inputService.IsMouseOrTouchHandled = true;
                    //MouseClick.Invoke(this);
                    
                }*/
            }

            if (!inputService.IsKeyboardHandled)
            {
                if (inputService.IsPressed(Keys.Escape, false) && HasSubmenuOpen)
                {
                    inputService.IsKeyboardHandled = true;
                    CloseSubMenu();
                }
            }

            //_previouslyMouseOver = IsMouseOver;
        }



        protected override void OnUpdate(TimeSpan deltaTime)
        {
            base.OnUpdate(deltaTime);

            //we need to do this, otherwise focus problems can make this window stay behind
            //other windows
            if (HasSubmenuOpen)
                Screen.BringToFront(_stackContent);
        }



        internal void OpenSubMenu()
        {
            //if any sibling had they menus open, close them
            foreach (MenuChild menuItemControl in Siblings)
                menuItemControl.CloseSubMenu();

            //if we don't have any children or if our menu is already open, there's nothing else to do here
            if (!HasChildren || HasSubmenuOpen)
                return;

            if (_orientation == Orientation.Vertical)
            {
                _stackContent.X = ActualX;

                if (ActualY + ActualHeight + _stackContent.ActualHeight > Screen.ActualHeight)
                    _stackContent.Y = ActualY - _stackContent.ActualHeight;
                else
                    _stackContent.Y = ActualY + ActualHeight;

                _stackContent.MinWidth = this.ActualWidth;
            }
            else
            {
                if (ActualX + ActualWidth > Screen.ActualWidth)
                    _stackContent.X = ActualX - ActualWidth;
                else
                    _stackContent.X = ActualX + ActualWidth;

                _stackContent.Y = ActualY;
            }

            Screen.Children.Add(_stackContent);


            Screen.FocusManager.Focus(_stackContent);
        }



        internal void CloseSubMenu()
        {
            foreach (var menuItemControl in MenuChildren)
                menuItemControl.CloseSubMenu();

            if (Screen != null)
                Screen.Children.Remove(_stackContent);
        }



        internal bool CloseLeaf()
        {
            //check if any child can close
            if (!MenuChildren.Any(val => val.CloseLeaf()))
            {
                //if not, check if we can close this one
                if (HasSubmenuOpen)
                {
                    CloseSubMenu();
                    return true;
                }

                return false;
            }

            return true;
        }
    }
}