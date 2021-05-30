using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Sceelix.Designer.GUI;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Utils;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using MouseButtons = DigitalRune.Game.Input.MouseButtons;

namespace Sceelix.Designer.Managers
{
    /// <summary>
    /// This input manager is meant to compensate for some issues with the default input manager, especially
    /// when a low framerate occurs.
    /// </summary>
    public class ExtendedInputManager : IInputService
    {
        private readonly GraphicsWindowManager _graphicsWindowManager;
        private readonly DesignerSettings _designerSettings;

        //to fix the keyboard mapping
        private readonly Queue<char> _chars = new Queue<char>();

        private readonly InputManager _inputManager;
        private UIManager _uiManager;

        private InputState _previousInputState = new InputState();
        private InputState _currentInputState = new InputState();

        private float _inputRenderScale;

        //to fix the issues when the window is not active
        private bool _previousInactive;

        class InputState
        {
            public readonly bool[] IsDoubleClicked = new bool[Enum.GetNames(typeof(MouseButtons)).Length];
            public readonly bool[] IsClicked = new bool[Enum.GetNames(typeof(MouseButtons)).Length];
        }


        public ExtendedInputManager(GraphicsWindowManager graphicsWindowManager, DesignerSettings designerSettings)
        {
            _graphicsWindowManager = graphicsWindowManager;
            _designerSettings = designerSettings;
            

            _inputRenderScale = MouseHelper.InputRenderScale = _designerSettings.ParseRenderScale(_designerSettings.UIScale.Value);
            _inputManager = new InputManager(false);

            #if WINDOWS
            var form = (Form) Control.FromHandle(_graphicsWindowManager.Window.Handle);
            form.MouseClick += delegate(object sender, MouseEventArgs args)
            {
                if (args.Clicks == 1)
                {
                    _previousInputState.IsClicked[(int) ToDigitalRuneButtons(args.Button)] = true;
                }
            };

            form.MouseDoubleClick += (sender, args) =>
            {
                _previousInputState.IsDoubleClicked[(int) ToDigitalRuneButtons(args.Button)] = true;

                //Console.WriteLine("Got double click from Form!");
            };
            #endif
            
            graphicsWindowManager.Window.TextInput += WindowOnTextInput;
            
            designerSettings.UIScale.Changed += UIScaleOnChanged;
        }
        
        
        private void UIScaleOnChanged(ApplicationField<string> field, string oldValue, string newValue)
        {
            var newPercentage = _designerSettings.ParseRenderScale(newValue);

            InputRenderScale = newPercentage;
        }



        public void Update(TimeSpan deltaTime)
        {

            if (_graphicsWindowManager.IsReallyActive)
            {
                UpdateAndScaleInput(deltaTime);
            }
            else
                _previousInactive = true;

            //if we were previously inactive and we're now active
            //we need to update the input a second time in this loop
            //to make sure that bugs, such as the mousedelta are 
            //eliminated.
            if (_previousInactive && _graphicsWindowManager.IsReallyActive)
            {
                _previousInactive = false;
                UpdateAndScaleInput(deltaTime);
            }
        }

        

        private void UpdateAndScaleInput(TimeSpan deltaTime)
        {
            _inputManager.Update(deltaTime);

            _currentInputState = _previousInputState;
            _previousInputState = new InputState();


            var newX = _inputManager.MousePosition.X * _inputRenderScale;
            var newY = _inputManager.MousePosition.Y * _inputRenderScale;

            _inputManager.MousePosition = new Vector2F(newX, newY);

            //#if !WINDOWS
            if (_uiManager != null)
                UpdateMap();
            //#endif
        }
        

        private void UpdateMap()
        {
            var keys = _inputManager.PressedKeys;

            foreach (var key in keys) //.Where(x => !IsModifierKey(x))
            {
                while (_chars.Any())
                {
                    var pressedChar = _chars.Dequeue();

                    //this should fix the issue where the shift would start writing stuff
                    if (IsModifierKey(key))
                        continue;

                    _uiManager.KeyMap[key, _inputManager.ModifierKeys] = pressedChar;
                    //_services.Get<MessageManager>().Publish(new LogMessageSent(""));
                    //DesignerProgram.Log.Debug("Matching keys [" + key + "," + _inputManager.ModifierKeys + "] to text '" + pressedChar + "'.");
                }
            }
        }



        private bool IsModifierKey(Keys key)
        {
            switch (key)
            {
                case Keys.LeftWindows:
                case Keys.RightWindows:
                case Keys.LeftControl:
                case Keys.RightControl:
                case Keys.LeftShift:
                case Keys.RightShift:
                case Keys.LeftAlt:
                case Keys.RightAlt:
                    return true;
                default:
                    return false;
            }
        }



        public bool IsDoubleClicked
        {
            get;
            private set;
        }



        public float InputRenderScale
        {
            get { return _inputRenderScale; }
            set
            {
                _inputRenderScale = value;
                MouseHelper.InputRenderScale = _inputRenderScale;
            }
        }



        public UIManager UIManager
        {
            get { return _uiManager; }
            set { _uiManager = value; }
        }



        public PlayerIndex? GetLogicalPlayer(LogicalPlayerIndex player)
        {
            return _inputManager.GetLogicalPlayer(player);
        }



        public void SetLogicalPlayer(LogicalPlayerIndex player, PlayerIndex? controller)
        {
            _inputManager.SetLogicalPlayer(player, controller);
        }



        public void SetGamePadHandled(LogicalPlayerIndex player, bool value)
        {
            _inputManager.SetGamePadHandled(player, value);
        }



        public void SetGamePadHandled(PlayerIndex controller, bool value)
        {
            _inputManager.SetGamePadHandled(controller, value);
        }



        public bool IsGamePadHandled(LogicalPlayerIndex player)
        {
            return _inputManager.IsGamePadHandled(player);
        }



        public bool IsGamePadHandled(PlayerIndex controller)
        {
            return _inputManager.IsGamePadHandled(controller);
        }



        public void SetAllHandled(bool value)
        {
            _inputManager.SetAllHandled(value);
        }



        public GamePadState GetGamePadState(LogicalPlayerIndex player)
        {
            return _inputManager.GetGamePadState(player);
        }



        public GamePadState GetGamePadState(PlayerIndex controller)
        {
            return _inputManager.GetGamePadState(controller);
        }



        public GamePadState GetPreviousGamePadState(LogicalPlayerIndex player)
        {
            return _inputManager.GetPreviousGamePadState(player);
        }



        public GamePadState GetPreviousGamePadState(PlayerIndex controller)
        {
            return _inputManager.GetPreviousGamePadState(controller);
        }



        public bool IsDown(Buttons button, LogicalPlayerIndex player)
        {
            return _inputManager.IsDown(button, player);
        }



        public bool IsDown(Buttons button, PlayerIndex controller)
        {
            return _inputManager.IsDown(button, controller);
        }



        public bool IsUp(Buttons button, LogicalPlayerIndex player)
        {
            return _inputManager.IsUp(button, player);
        }



        public bool IsUp(Buttons button, PlayerIndex controller)
        {
            return _inputManager.IsUp(button, controller);
        }



        public bool IsPressed(Buttons button, bool useButtonRepetition, LogicalPlayerIndex player)
        {
            return _inputManager.IsPressed(button, useButtonRepetition, player);
        }



        public bool IsPressed(Buttons button, bool useButtonRepetition, PlayerIndex controller)
        {
            return _inputManager.IsPressed(button, useButtonRepetition, controller);
        }



        public bool IsReleased(Buttons button, LogicalPlayerIndex player)
        {
            return _inputManager.IsReleased(button, player);
        }



        public bool IsReleased(Buttons button, PlayerIndex controller)
        {
            return _inputManager.IsReleased(button, controller);
        }



        public bool IsDoubleClick(Buttons button, LogicalPlayerIndex player)
        {
            return _inputManager.IsDoubleClick(button, player);
        }



        public bool IsDoubleClick(Buttons button, PlayerIndex controller)
        {
            return _inputManager.IsDoubleClick(button, controller);
        }



        public bool IsDown(Keys key)
        {
            return _inputManager.IsDown(key);
        }



        public bool IsUp(Keys key)
        {
            return _inputManager.IsUp(key);
        }



        public bool IsPressed(Keys key, bool useKeyRepetition)
        {
            return _inputManager.IsPressed(key, useKeyRepetition);
        }



        public bool IsReleased(Keys key)
        {
            return _inputManager.IsReleased(key);
        }



        public bool IsDoubleClick(Keys key)
        {
            return _inputManager.IsDoubleClick(key);
        }



        public bool IsDown(MouseButtons button)
        {
            return _inputManager.IsDown(button);
        }



        public bool IsUp(MouseButtons button)
        {
            return _inputManager.IsUp(button);
        }



        public bool IsPressed(MouseButtons button, bool useButtonRepetition)
        {
            //return _currentInputState.IsClicked[(int)button];

            return _inputManager.IsPressed(button, useButtonRepetition);
        }



        public bool IsReleased(MouseButtons button)
        {
            return _inputManager.IsReleased(button);
        }



        public bool IsDoubleClick(MouseButtons button)
        {
            return BuildPlatform.IsWindows ? 
                _currentInputState.IsDoubleClicked[(int) button]
                    : _inputManager.IsDoubleClick(button);
        }



        public InputSettings Settings
        {
            get { return _inputManager.Settings; }
            set { _inputManager.Settings = value; }
        }



        public int MaxNumberOfPlayers
        {
            get { return _inputManager.MaxNumberOfPlayers; }
        }



        public bool IsMouseOrTouchHandled
        {
            get { return _inputManager.IsMouseOrTouchHandled; }
            set { _inputManager.IsMouseOrTouchHandled = value; }
        }



        public bool IsKeyboardHandled
        {
            get { return _inputManager.IsKeyboardHandled; }
            set { _inputManager.IsKeyboardHandled = value; }
        }



        public bool IsAccelerometerHandled
        {
            get { return _inputManager.IsAccelerometerHandled; }
            set { _inputManager.IsAccelerometerHandled = value; }
        }



        public bool EnableMouseCentering
        {
            get { return _inputManager.EnableMouseCentering; }
            set { _inputManager.EnableMouseCentering = value; }
        }



        public MouseState MouseState
        {
            get { return _inputManager.MouseState; }
        }



        public MouseState PreviousMouseState
        {
            get { return _inputManager.PreviousMouseState; }
        }



        public float MouseWheelDelta
        {
            get { return _inputManager.MouseWheelDelta; }
        }



        public Vector2F MousePositionRaw
        {
            get { return _inputManager.MousePositionRaw; }
        }



        public Vector2F MousePositionDeltaRaw
        {
            get { return _inputManager.MousePositionDeltaRaw; }
        }



        public Vector2F MousePosition
        {
            get { return _inputManager.MousePosition; }
            set { _inputManager.MousePosition = value; }
        }



        public Vector2F MousePositionDelta
        {
            get { return _inputManager.MousePositionDelta; }
            set { _inputManager.MousePositionDelta = value; }
        }



        public KeyboardState KeyboardState
        {
            get { return _inputManager.KeyboardState; }
        }



        public KeyboardState PreviousKeyboardState
        {
            get { return _inputManager.PreviousKeyboardState; }
        }



        public ReadOnlyCollection<Keys> PressedKeys
        {
            get { return _inputManager.PressedKeys; }
        }



        public ModifierKeys ModifierKeys
        {
            get { return _inputManager.ModifierKeys; }
        }



        public TouchCollection TouchCollection
        {
            get { return _inputManager.TouchCollection; }
        }



        public List<GestureSample> Gestures
        {
            get { return _inputManager.Gestures; }
        }



        public InputCommandCollection Commands
        {
            get { return _inputManager.Commands; }
        }
        

        private void WindowOnTextInput(object sender, TextInputEventArgs textInputEventArgs)
        {
            _chars.Enqueue(textInputEventArgs.Character);

            //DesignerProgram.Log.Debug("Char is " + textInputEventArgs.Character);
        }


        private MouseButtons ToDigitalRuneButtons(System.Windows.Forms.MouseButtons button)
        {
            return (MouseButtons) Enum.Parse(typeof(MouseButtons), Enum.GetName(typeof(System.Windows.Forms.MouseButtons), button));
        }

    }

}