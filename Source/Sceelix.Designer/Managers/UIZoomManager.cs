using System;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Managers
{
    public class UIZoomManager
    {
        private readonly UIScreen _uiScreen;
        private readonly DesignerSettings _designerSettings;



        public UIZoomManager(UIScreen uiScreen, DesignerSettings designerSettings, GameWindow gameWindow)
        {
            _uiScreen = uiScreen;
            _designerSettings = designerSettings;
            uiScreen.InputProcessed += UIScreenOnInputProcessed;

            gameWindow.TextInput += WindowOnTextInput;
        }



        /// <summary>
        /// Deactivated for now (was moved to DesignerWindow), but might be important again.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="inputEventArgs"></param>
        private void UIScreenOnInputProcessed(object sender, InputEventArgs inputEventArgs)
        {
            if (BuildPlatform.IsWindows || BuildPlatform.IsLinux)
            {
                if (_uiScreen.InputService.ModifierKeys.HasFlag(ModifierKeys.Control))
                {
                    if (_uiScreen.InputService.IsPressed(Keys.Add, false) || _uiScreen.InputService.IsPressed(Keys.OemPlus, false))
                        Zoom(+1);
                    else if (_uiScreen.InputService.IsPressed(Keys.Subtract, false) || _uiScreen.InputService.IsPressed(Keys.OemMinus, false))
                        Zoom(-1);
                }
            }
        }

        
        private void WindowOnTextInput(object sender, TextInputEventArgs textInputEventArgs)
        {
            var character = textInputEventArgs.Character;

            if (_uiScreen.InputService.IsDown(Keys.LeftWindows) || _uiScreen.InputService.IsDown(Keys.RightWindows))
            {
                if (character == '+')
                    Zoom(+1);
                else if (character == '-')
                    Zoom(-1);
            }
        }



        private void Zoom(int indexIncrement)
        {
            _designerSettings.UIScale.Value = _designerSettings.UIScale.Choices[Math.Min(Math.Max(0, _designerSettings.UIScale.Index + indexIncrement), _designerSettings.UIScale.Choices.Length - 1)];
        }

    }
}
