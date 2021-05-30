using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Input;

namespace Sceelix.Designer.GUI.Controls
{
    public class PopupWindow : ContentControl
    {
        public PopupWindow()
            : this(new ContentControl() {Height = 100, Width = 100})
        {
        }



        public PopupWindow(UIControl control)
        {
            Style = "MenuItemContent";
            //IsFocusScope = true;
            Focusable = true;
            Content = control;
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



        protected override void OnHandleInput(InputContext context)
        {
            base.OnHandleInput(context);

            if (IsOpen)
            {
                if (IsMouseOver)
                    InputService.IsMouseOrTouchHandled = true;


                if ((InputService.IsPressed(MouseButtons.Left, false) ||
                     InputService.IsPressed(MouseButtons.Middle, false) ||
                     InputService.IsPressed(MouseButtons.Right, false)))
                {
                    if (!IsMouseOver)
                    {
                        Close();
                    }
                }

                if (InputService != null && !InputService.IsKeyboardHandled)
                {
                    if (InputService.IsPressed(Keys.Escape, false))
                    {
                        InputService.IsKeyboardHandled = true;

                        Close();
                    }
                }
            }
        }



        public virtual void Close()
        {
            Screen.Children.Remove(this);
        }



        /*protected override void OnLoad()
        {
            base.OnLoad();

            StackPanel panel = new StackPanel();

            for (int i = 0; i < 10; i++)
            {
                panel.Children.Add(new TextBlock() { Text = "Text " + i,HorizontalAlignment = HorizontalAlignment.Stretch,Margin = new Vector4F(5)});
            }

            Content = panel;
        }*/



        /// <summary>
        /// Opens the popup window at the indicated position.
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="screenMousePosition"></param>
        public virtual void Open(UIScreen screen, Vector2F screenMousePosition)
        {
            if (IsOpen)
                return;

            X = screenMousePosition.X;
            Y = screenMousePosition.Y;

            screen.Children.Add(this);

            //screen.FocusManager.ClearFocus();
            screen.FocusManager.Focus(this);
        }



        /// <summary>
        /// Opens the popup window directly below the passed uicontrol.
        /// </summary>
        /// <param name="uiControl"></param>
        public void Open(UIControl uiControl)
        {
            if (IsOpen)
                return;

            X = uiControl.ActualX;
            Y = uiControl.ActualY + uiControl.ActualHeight;

            uiControl.Screen.Children.Add(this);

            uiControl.Screen.FocusManager.Focus(this);
        }
    }
}