using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI.Controls;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sceelix.Designer.GUI.MenuControls;
using Sceelix.Designer.Helpers;
using Sceelix.Designer.Utils;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using MouseButtons = DigitalRune.Game.Input.MouseButtons;
using TextBox = DigitalRune.Game.UI.Controls.TextBox;

namespace Sceelix.Designer.GUI.Controls
{
    //In the future, we should replace with a differential, instead of keeping the whole string
    //which is a waste of memory
    //this approach seems simple: http://people.cs.ksu.edu/~rhowell/DataStructures/stacks-queues/undo.html
    internal class HistoryState
    {
        private readonly String _content;
        private readonly int _caretIndex;

        public HistoryState(string content, int caretIndex)
        {
            _content = content;
            _caretIndex = caretIndex;
        }

        public string Content
        {
            get { return _content; }
        }

        public int CaretIndex
        {
            get { return _caretIndex; }
        }
    }

    public class ExtendedTextBox : TextBox
    {
        private readonly Stack<HistoryState> _redoStates = new Stack<HistoryState>();
        private readonly Stack<HistoryState> _undoStates = new Stack<HistoryState>();
        private bool _isInternalChange;

        public ExtendedTextBox()
        {
            InputProcessing += OnInputProcessing;


        }

        protected override void OnLoad()
        {
            base.OnLoad();

            var property = Properties.Get<String>("Text");
            property.Changed += OnTextChanged;
        }

        private void OnTextChanged(object sender, GamePropertyEventArgs<string> e)
        {
            if (_isInternalChange)
                return;

            _redoStates.Clear();

            _undoStates.Push(new HistoryState(e.OldValue,CaretIndex));
        }


        private void OnInputProcessing(object sender, InputEventArgs inputEventArgs)
        {
            if (!InputService.IsMouseOrTouchHandled && InputService.IsDoubleClick(MouseButtons.Left) && IsMouseOver)
            {
                var text = Text;

                var startIndex = CaretIndex;
                var endIndex = CaretIndex;

                while (startIndex > 0 && char.IsLetterOrDigit(text[startIndex - 1]))
                    startIndex--;

                while (endIndex < text.Length && char.IsLetterOrDigit(text[endIndex]))
                    endIndex++;

                Select(startIndex, endIndex - startIndex);
            }
            if (!InputService.IsKeyboardHandled)
            {
                var isModifierKey = BuildPlatform.IsMacOS
                    ? InputService.IsDown(Keys.LeftWindows) || InputService.IsDown(Keys.RightWindows)
                    : InputService.ModifierKeys.HasFlag(ModifierKeys.Control);


                if (isModifierKey && InputService.IsPressed(Keys.Z, false))
                {
                    InputService.IsKeyboardHandled = true;
                    Undo();
                }
                else if (isModifierKey && InputService.IsPressed(Keys.Y, false))
                {
                    InputService.IsKeyboardHandled = true;
                    Redo();
                }

                if (BuildPlatform.IsMacOS)
                {
                    if (isModifierKey && InputService.IsPressed(Keys.C, false))
                    {
                        InputService.IsKeyboardHandled = true;
                        ClipboardHelper.Copy(SelectedText);
                    }
                    else if (isModifierKey && InputService.IsPressed(Keys.V, false))
                    {
                        InputService.IsKeyboardHandled = true;
                        Text = Text.Insert(CaretIndex, ClipboardHelper.Paste());
                    }
                }
            }

            if (!InputService.IsMouseOrTouchHandled)
            {
                if (InputService.IsPressed(MouseButtons.Right, false) && IsMouseOver)
                {
                    MultiContextMenu multiContext = new MultiContextMenu();
                    multiContext.MenuChildren.Add(new MenuChild((obj) => Cut()) { Text = "Cut", Icon = EmbeddedResources.Load<Texture2D>("Resources/ClipboardCut_16x16.png") });
                    multiContext.MenuChildren.Add(new MenuChild((obj) => Copy()) { Text = "Copy", Icon = EmbeddedResources.Load<Texture2D>("Resources/ClipboardCopy_16x16.png") });
                    multiContext.MenuChildren.Add(new MenuChild((obj) => Paste()) { Text = "Paste", Icon = EmbeddedResources.Load<Texture2D>("Resources/ClipboardPaste_16x16.png") });
                    multiContext.Open(this.Screen, InputService.MousePosition);

                    InputService.IsMouseOrTouchHandled = true;
                }
            }
        }



        private void CutOperation(MenuChild obj)
        {
            Cut();
        }

        private void CopyOperation(MenuChild obj)
        {
            Copy();
        }

        private void PasteOperation(MenuChild obj)
        {
            Paste();
        }

        private void Redo()
        {
            if (!_redoStates.Any())
                return;

            var redoState = _redoStates.Pop();
            _undoStates.Push(new HistoryState(Text,CaretIndex));

            RestoreState(redoState);
        }


        private void Undo()
        {
            if (!_undoStates.Any())
                return;
            
            var undoState = _undoStates.Pop();

            _redoStates.Push(new HistoryState(Text,CaretIndex));

            RestoreState(undoState);
        }


        private void RestoreState(HistoryState state)
        {
            _isInternalChange = true;

            Text = state.Content;
            CaretIndex = state.CaretIndex;

            _isInternalChange = false;
        }
    }
}
