using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Game.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sceelix.Core.Graphs;
using Sceelix.Designer.Graphs.ParameterEditors.Windows;
using Sceelix.Designer.GUI.Controls;

namespace Sceelix.Designer.Graphs.ParameterEditors.Controls
{
    public class ExpressionTextBox : SyntaxHighlightingTextBox
    {
        public static readonly Color StandardEvaluationColor = new Color(54,86,175);
        public static readonly Color EntityEvaluationColor = new Color(44, 119, 37);


        public static readonly Color FunctionColor = Color.LightBlue;
        public static readonly Color AttributeColor = Color.Pink;
        public static readonly Color ParameterColor = Color.LimeGreen;
        public static readonly Color NumericColor = Color.Yellow;
        public static readonly Color BoolColor = Color.YellowGreen;
        public static readonly Color StringColor = Color.Orange;

        private ExpressionPopupWindow _expressionPopupWindow;
        private readonly Graph _graph;
        private bool _isEntityEvaluation;



        public ExpressionTextBox(Graph graph)
        {
            _graph = graph;

            BracketMatchColor = Color.Green;
            BracketMissingColor = Color.Red;

            

            this.SyntaxHighlightingStyles.AddRange(SceelixExpressionStyles);

            this.BracketStyles.AddRange(SceelixBracketStyles);

            //when the user uses the CTRL+Space combination, open the autocomplete popup
            InputProcessing += delegate
            {
                if (IsFocused)
                {
                    var pressedKeys = InputService.PressedKeys.Select(x => UIService.KeyMap[x, InputService.ModifierKeys]);
                    var quickSuggestionPressed = InputService.IsPressed(Keys.Space, false) && InputService.ModifierKeys.HasFlag(ModifierKeys.Control);
                    

                    if (quickSuggestionPressed)
                    {
                        //InputService.IsMouseOrTouchHandled = true;
                        //InputService.IsKeyboardHandled = true;

                        if (_expressionPopupWindow != null)
                            _expressionPopupWindow.Close();

                        _expressionPopupWindow = new ExpressionPopupWindow(graph, this); //new ExpressionWindow()
                        _expressionPopupWindow.Open();
                        
                        InputService.IsKeyboardHandled = true;
                    }
                    else if ((_expressionPopupWindow == null || !_expressionPopupWindow.IsOpen) && pressedKeys.Any(Char.IsLetterOrDigit))
                    {
                        _expressionPopupWindow = new ExpressionPopupWindow(graph, this); //new ExpressionWindow()
                        _expressionPopupWindow.Open();
                    }

                    if (!InputService.IsMouseOrTouchHandled && InputService.IsDoubleClick(MouseButtons.Left) && IsMouseOver)
                    {
                        var text = Text;
                        if (CaretIndex > 0 && CaretIndex < text.Length)
                        {
                            var startIndex = CaretIndex;
                            var endIndex = CaretIndex;

                            while (startIndex > 0 && char.IsLetterOrDigit(text[startIndex-1]))
                                startIndex--;

                            while (endIndex < text.Length && char.IsLetterOrDigit(text[endIndex]))
                                endIndex++;

                            Select(startIndex, endIndex-startIndex);
                        }
                    }
                }
            };
            var focusedProperty = Properties.Get<bool>("IsFocused");
            focusedProperty.Changed += delegate
            {
                if (!IsFocused && _expressionPopupWindow != null && _expressionPopupWindow.IsOpen)
                    _expressionPopupWindow.Close();
            };

        }



        public bool IsEntityEvaluation
        {
            get { return _isEntityEvaluation; }
            set
            {
                _isEntityEvaluation = value;
                Style = _isEntityEvaluation ? "EntityExpressionTextbox" : "ExpressionTextbox";
            }
        }



        public static IEnumerable<SyntaxHighlightingStyle> SceelixExpressionStyles
        {
            get
            {
                yield return new SyntaxHighlightingStyle("String", "\\\"[^\\\"]*\\\"", StringColor);
                yield return new SyntaxHighlightingStyle("Numeric", "[0-9]+(\\.[0-9]+)?(d|f|l)?", NumericColor);
                yield return new SyntaxHighlightingStyle("Bool", "(true)|(false)", BoolColor);
                yield return new SyntaxHighlightingStyle("Function", "[a-zA-Z]+[0-9a-zA-Z]*(?=\\()", FunctionColor);
                yield return new SyntaxHighlightingStyle("Attribute", "@?@[a-zA-Z]+[0-9a-zA-Z]*", AttributeColor);
                yield return new SyntaxHighlightingStyle("Parameter", "[a-zA-Z]+[0-9a-zA-Z]*", ParameterColor);
            }
        }



        public static IEnumerable<BracketStyle> SceelixBracketStyles
        {
            get
            {
                yield return new BracketStyle('(', ')');
                yield return new BracketStyle('[', ']');
            }
        }
    }
}
