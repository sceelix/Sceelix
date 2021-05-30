using System;
using System.Linq;
using System.Text.RegularExpressions;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Linq;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sceelix.Core.Graphs;
using Sceelix.Core.Graphs.Functions;
using Sceelix.Core.Parameters;
using Sceelix.Designer.Graphs.GUI.Toolbox;
using Sceelix.Designer.Graphs.ParameterEditors.Controls;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.MenuControls;
using Sceelix.Designer.Utils;
using Sceelix.Extensions;

namespace Sceelix.Designer.Graphs.ParameterEditors.Windows
{
    public class ExpressionPopupWindow : ContentControl
    {
        private readonly ExpressionTextBox _expressionTextbox;
        private readonly Synchronizer _synchronizer = new Synchronizer();
        //private TextBox _textbox;
        private readonly StackPanel _listPanel;
        private GameProperty<string> _property;
        private bool _thisWasTheMouseTouchHandled;



        public ExpressionPopupWindow(Graph graph, ExpressionTextBox expressionTextbox)
            //: base(services)
        {
            _expressionTextbox = expressionTextbox;
            Width = 500;
            Height = 200;

            Style = "MenuItemContent";

            Width = 300;
            Height = 250;
            //Focusable = true;

            _property = _expressionTextbox.Properties.Get<String>("Text");
            _property.Changed += TextOnChanged;

            FlexibleStackPanel basePanel = new FlexibleStackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };


            ScrollViewer viewer = new VerticalScrollViewer()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };

            _listPanel = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            var parameterItems = graph.ParameterInfos.OrderBy(x => x.Label).ToList();
            if (parameterItems.Any())
            {
                _listPanel.Children.Add(new TextBlock() {Text = "Parameters", Margin = new Vector4F(3, 5, 0, 0)});

                foreach (var parameterInfo in parameterItems)
                    _listPanel.Children.Add(new ExpressionWindowItem(Parameter.GetIdentifier(parameterInfo.Label), EmbeddedResources.Load<Texture2D>("Resources/P.png")));
            }

            //add the names of attributes
            var attributeItems = graph.GetReferencedAttributes().OrderBy(x => x).Distinct().ToList();
            if (attributeItems.Any())
            {
                if (_expressionTextbox.IsEntityEvaluation)
                {
                    _listPanel.Children.Add(new TextBlock() { Text = "Subattributes", Margin = new Vector4F(3, 5, 0, 0) });
                    foreach (var name in attributeItems)
                        _listPanel.Children.Add(new ExpressionWindowItem("@@" + name, EmbeddedResources.Load<Texture2D>("Resources/A.png")));
                }

                _listPanel.Children.Add(new TextBlock() {Text = "Attributes", Margin = new Vector4F(3, 5, 0, 0)});
                foreach (var name in attributeItems)
                    _listPanel.Children.Add(new ExpressionWindowItem("@" + name, EmbeddedResources.Load<Texture2D>("Resources/A.png")));
            }


            //add the names of functions
            var functionItems = FunctionManager.GetRegisteredFunctionNames().OrderBy(x => x).ToList();
            if (functionItems.Any())
            {
                _listPanel.Children.Add(new TextBlock() {Text = "Functions", Margin = new Vector4F(3, 5, 0, 0)});

                foreach (var name in functionItems)
                    _listPanel.Children.Add(new ExpressionWindowItem(name, EmbeddedResources.Load<Texture2D>("Resources/F.png")));
                //foreach (var name in functionItems)
                //    _listPanel.Children.Add(new Button() { Content = new TextBlock() { Text = name },HorizontalAlignment = HorizontalAlignment.Stretch});
            }


            EnumerableExtension.ForEach(_listPanel.Children.OfType<ExpressionWindowItem>(), x => x.Click += XOnClick);
            EnumerableExtension.ForEach(_listPanel.Children.OfType<ExpressionWindowItem>(), x => x.DoubleClick += XOnDblClick);

            var firstItem = _listPanel.Children.OfType<ExpressionWindowItem>().FirstOrDefault();
            if (firstItem != null)
                firstItem.IsSelected = true;

            viewer.Content = _listPanel;

            basePanel.Children.Add(viewer);

            Content = basePanel;

            //Content = new TextBlock() { Margin = new Vector4F(5), HorizontalAlignment = HorizontalAlignment.Stretch ,Text = "Hello World"};

            ParseText();
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



        private void XOnDblClick(object sender, EventArgs e)
        {
            ClearSelection();

            var item = (ExpressionWindowItem) sender;
            item.IsSelected = true;

            AcceptSelection();
        }



        private void XOnClick(object sender, EventArgs eventArgs)
        {
            ClearSelection();

            var item = (ExpressionWindowItem) sender;
            item.IsSelected = true;

            //AcceptSelection();

            //_expressionTextbox.Focus();
        }



        private void TextOnChanged(object sender, GamePropertyEventArgs<string> gamePropertyEventArgs)
        {
            //because the caret index is only updated afterwards, we have to postpone the execution of this function for a later moment
            _synchronizer.Enqueue(ParseText);
        }



        private void ParseText()
        {
            if (!String.IsNullOrWhiteSpace(_expressionTextbox.SelectedText))
                ProcessExpressionText(_expressionTextbox.SelectedText);
            else
                ProcessExpressionText(_expressionTextbox.Text.Substring(0, _expressionTextbox.CaretIndex));
        }



        private void ProcessExpressionText(string newValue)
        {
            string[] strings = GetParsedStrings(newValue);

            var lastString = strings.Last().ToLower();

            ClearSelection();

            if (!String.IsNullOrWhiteSpace(lastString))
            {
                var expressionWindowItem = _listPanel.Children.OfType<ExpressionWindowItem>().FirstOrDefault(x => x.ItemName.ToLower().StartsWith(lastString));
                if (expressionWindowItem != null)
                {
                    _synchronizer.Enqueue(expressionWindowItem.BringIntoView);
                    expressionWindowItem.IsSelected = true;
                }
            }
        }



        private String[] GetParsedStrings(String input)
        {
            return Regex.Split(input, @"(\s|\+|\-|\\|\*|\(|\)|,|%)");
        }



        private bool IsNonWord(String input)
        {
            return Regex.IsMatch(input, @"[\s\+\-\\\*\(\),%]");
        }



        private void ClearSelection()
        {
            foreach (var item in _listPanel.Children.OfType<ExpressionWindowItem>())
                item.IsSelected = false;
        }



        private void AcceptSelection()
        {
            var expressionWindowItem = _listPanel.Children.OfType<ExpressionWindowItem>().FirstOrDefault(x => x.IsSelected);
            if (expressionWindowItem != null)
            {
                //if there's text selected on the textbox, replace it completely
                if (!String.IsNullOrWhiteSpace(_expressionTextbox.SelectedText))
                {
                    int index = _expressionTextbox.Text.IndexOf(_expressionTextbox.SelectedText, _expressionTextbox.CaretIndex - _expressionTextbox.SelectedText.Length, StringComparison.Ordinal);

                    var firstSection = _expressionTextbox.Text.Substring(0, index);
                    var secondSection = _expressionTextbox.Text.Substring(index + _expressionTextbox.SelectedText.Length);

                    _expressionTextbox.Text = firstSection + expressionWindowItem.ItemName + secondSection;
                }
                else
                {
                    int caretIndex = _expressionTextbox.CaretIndex;

                    _expressionTextbox.Text = ReplaceString(_expressionTextbox.Text, expressionWindowItem.ItemName, ref caretIndex);

                    _expressionTextbox.CaretIndex = caretIndex;
                    //otherwise, insert the text at that location, replacing the starting text, if applicable
                    /*String currentText = _expressionTextbox.Text.Substring(0, _expressionTextbox.CaretIndex);

                    string[] strings = GetParsedStrings(currentText);

                    var firstSection = currentText.Substring(0, currentText.Length - strings.Last().Length);

                    var secondSection = _expressionTextbox.Text.Substring(_expressionTextbox.CaretIndex);

                    _expressionTextbox.Text = firstSection + expressionWindowItem.ItemName + secondSection;

                    _expressionTextbox.CaretIndex += expressionWindowItem.ItemName.Length;*/
                }
            }

            Close();
        }



        private String ReplaceString(string text, string replacement, ref int caretIndex)
        {
            string[] parsedStrings = GetParsedStrings(text);
            string joinedString = String.Empty;
            bool foundLocation = false;

            foreach (string str in parsedStrings)
            {
                if (joinedString.Length + str.Length >= caretIndex && !foundLocation)
                {
                    //if we are talking about white spaces, commas, operators...leave them
                    if (IsNonWord(str))
                        joinedString += str;

                    //otherwise, replace the string
                    joinedString += replacement;

                    //update the caret location, because we want to keep working
                    caretIndex = joinedString.Length;

                    //the rest of the process will be just adding the remaining strings
                    foundLocation = true;
                }
                else
                {
                    joinedString += str;
                }
            }

            return joinedString;
        }



        private void ControlOnInputProcessing(object sender, InputEventArgs inputEventArgs)
        {
            var mouseOver = ContentBounds.Contains(inputEventArgs.Context.MousePosition);

            if (IsOpen)
            {
                if (mouseOver)
                {
                    InputService.IsMouseOrTouchHandled = true;
                    _thisWasTheMouseTouchHandled = true;
                }

                if ((InputService.IsPressed(MouseButtons.Left, false) ||
                     InputService.IsPressed(MouseButtons.Middle, false) ||
                     InputService.IsPressed(MouseButtons.Right, false)))
                {
                    if (!mouseOver)
                    {
                        Close();
                    }
                }

                if (InputService != null && !InputService.IsKeyboardHandled)
                {
                    if (InputService.IsPressed(Keys.Enter, false))
                    {
                        InputService.IsKeyboardHandled = true;

                        AcceptSelection();
                    }
                    else if (InputService.IsPressed(Keys.Escape, false))
                    {
                        InputService.IsKeyboardHandled = true;

                        Close();
                    }
                    else if (InputService.IsPressed(Keys.Down, true))
                    {
                        InputService.IsKeyboardHandled = true;

                        MoveSelection(+1);
                    }
                    else if (InputService.IsPressed(Keys.Up, true))
                    {
                        InputService.IsKeyboardHandled = true;

                        MoveSelection(-1);
                    }
                }
            }
        }



        protected override void OnHandleInput(InputContext context)
        {
            context.IsMouseOver = ContentBounds.Contains(context.MousePosition);

            if (_thisWasTheMouseTouchHandled)
                InputService.IsMouseOrTouchHandled = false;

            base.OnHandleInput(context);

            if (_thisWasTheMouseTouchHandled)
                InputService.IsMouseOrTouchHandled = true;


            if (IsOpen)
            {
                if (InputService.IsPressed(Keys.Left, true))
                {
                    UpdateLocation(this.Screen);
                    ParseText();
                }
                else if (InputService.IsPressed(Keys.Right, true))
                {
                    UpdateLocation(this.Screen);
                    ParseText();
                }
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="increment">For this current case, this value is always +1 or -1.</param>
        private void MoveSelection(int increment)
        {
            var itemList = _listPanel.Children.OfType<ExpressionWindowItem>().ToList();
            var index = itemList.IndexOf(x => x.IsSelected);
            if (index >= 0)
            {
                //if these is a selected item, deselect it
                itemList[index].IsSelected = false;

                //increment the value and circle around the list size to select the new one
                index += increment;
                if (index < 0)
                {
                    index = itemList.Count - 1;
                }
                else if (index >= itemList.Count)
                {
                    index = 0;
                }

                itemList[index].IsSelected = true;
                itemList[index].BringIntoView();
            }
            else
            {
                //if no item is select at this point...
                if (increment > 0)
                    index = 0;
                else
                    index = itemList.Count - 1;

                itemList[index].IsSelected = true;
                itemList[index].BringIntoView();
            }
        }



        protected override void OnUpdate(TimeSpan deltaTime)
        {
            base.OnUpdate(deltaTime);

            _synchronizer.Update();
        }



        public void Open()
        {
            if (IsOpen)
                return;

            //_uiControl = textbox;
            //this.Width = textbox.ActualWidth;

            var screen = _expressionTextbox.Screen;
            _synchronizer.Enqueue(() => UpdateLocation(screen));

            //X = textbox.ActualX;
            //Y = textbox.ActualY + textbox.ActualHeight;

            _expressionTextbox.Screen.Children.Add(this);

            //_expressionTextbox.Screen.FocusManager.Focus(this);

            //_expressionTextbox.InputProcessed += ControlOnInputProcessed;
            _expressionTextbox.InputProcessing += ControlOnInputProcessing;
        }



        private void UpdateLocation(UIScreen screen)
        {
            var suggestedX = _expressionTextbox.VisualCaret.X;
            var suggestedY = _expressionTextbox.ActualY + _expressionTextbox.ActualHeight;

            X = suggestedX + this.Width > screen.ActualWidth ? suggestedX - this.Width : suggestedX;
            Y = suggestedY + this.Height > screen.Screen.ActualHeight ? _expressionTextbox.ActualY - this.Height : suggestedY;
        }



        public void Close()
        {
            var screen = _expressionTextbox.Screen;

            _synchronizer.Enqueue(delegate
            {
                screen.Children.Remove(this);
                _expressionTextbox.InputProcessing -= ControlOnInputProcessing;
                _property.Changed -= TextOnChanged;
            });
        }

        #region ExpressionWindowItem

        public class ExpressionWindowItem : ContentControl
        {
            public static readonly int IsSelectedPropertyId = CreateProperty(typeof(ToolboxItemControl), "IsSelected", GamePropertyCategories.Style, null, false, UIPropertyOptions.AffectsRender);

            private readonly string _itemName;



            public ExpressionWindowItem(String itemName, Texture2D icon)
            {
                _itemName = itemName;
                Content = new FlexibleStackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new Image()
                        {
                            Texture = icon,
                            Width = 16,
                            Height = 16,
                            Margin = new Vector4F(0, 1, 5, 0)
                        },
                        new TextBlock()
                        {
                            Text = itemName,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            Foreground = Color.Black,
                            Margin = new Vector4F(0, 2, 0, 0)
                        },
                    }
                };

                HorizontalAlignment = HorizontalAlignment.Stretch;
                Style = "ToolBoxItem";
            }



            public string ItemName
            {
                get { return _itemName; }
            }



            public override string VisualState
            {
                get
                {
                    if (IsSelected)
                        return "Selected";

                    return "Default";
                }
            }



            public bool IsSelected
            {
                get { return GetValue<bool>(IsSelectedPropertyId); }
                set
                {
                    SetValue(IsSelectedPropertyId, value);
                    InvalidateVisual();
                }
            }



            public event EventHandler<EventArgs> Click = delegate { };
            public event EventHandler<EventArgs> DoubleClick = delegate { };



            protected override void OnHandleInput(InputContext context)
            {
                base.OnHandleInput(context);

                //if (ActualBounds.Contains(InputService.MousePosition))
                if (IsMouseOver)
                {
                    if (InputService.IsPressed(MouseButtons.Left, false))
                    {
                        InputService.IsMouseOrTouchHandled = true;
                        Click(this, EventArgs.Empty);
                    }
                    if (InputService.IsDoubleClick(MouseButtons.Left))
                    {
                        InputService.IsMouseOrTouchHandled = true;
                        DoubleClick(this, EventArgs.Empty);
                    }
                }
            }
        }

        #endregion
    }
}