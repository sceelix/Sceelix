using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sceelix.Core.ExpressionParsing;
using Sceelix.Core.Graphs;
using Sceelix.Core.Graphs.Expressions;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Designer.Graphs.ExpressionParsing;
using Sceelix.Designer.Graphs.GUI.Model;
using Sceelix.Designer.Graphs.ParameterEditors.Controls;
using Sceelix.Designer.Graphs.ParameterEditors.Windows;
using Sceelix.Designer.GUI;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;


namespace Sceelix.Designer.Graphs.ParameterEditors
{
    public class ArgumentTreeViewItem : TreeViewItem, ILabeledTreeViewItem
    {
        //the actual argument being saved here
        private readonly ParameterInfo _argument;
        private readonly UIControl _control;

        //for dragging and dropping items on lists
        private readonly Image _dragPointerIcon;
        private readonly FileItem _fileItem;
        private readonly TextBlock _textBlock;

        private readonly ContentControl _variableControlArea;
        private readonly VisualNode _visualNode;

        private readonly Synchronizer _synchronizer = new Synchronizer();
        private FlexibleStackPanel _expressionStackPanel;


        private ExpressionTextBox _expressionTextBox;
        private bool _canBeDragged;

        private List<TreeViewItem> _fixedSubItems = new List<TreeViewItem>();


        //private readonly ContentControl _upControl, _downControl;
        private ArgumentTreeViewItem _previouslyHoveredControl;
        public Func<ArgumentTreeViewItem, bool> CanDragInto = delegate { return false; };
        public Action<ArgumentTreeViewItem, int> DragInto = delegate { };
        private readonly ContextMenu _parameterMenu;



        public ArgumentTreeViewItem(ParameterInfo argument, UIControl control, VisualNode visualNode, FileItem fileItem, string group)
        {
            this.Margin = new Vector4F(Margin.X, Margin.Y + 10, Margin.Z, Margin.W);
            _argument = argument;
            _control = control;
            _visualNode = visualNode;
            _fileItem = fileItem;

            Name = argument.Label;

            _parameterMenu = new ContextMenu();

            //set the alignment to stretch for the node and the control ifself as well
            HorizontalAlignment = HorizontalAlignment.Stretch;
            _control.HorizontalAlignment = HorizontalAlignment.Stretch;

            FlexibleStackPanel horizontalStackPanel = new FlexibleStackPanel() {Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Vector4F(0)};
            horizontalStackPanel.Children.Add(_dragPointerIcon = new Image());

            //define the textblock
            //if the parameter can be turned into an expression (for instance, for nodes, but not for graph parameters)
            //then allow it to be underlined and turned into an expression.
            _textBlock = new TextBlock {Text = argument.Label + ":", Margin = new Vector4F(3, 3, 10, 3)};
            
            horizontalStackPanel.Children.Add(new LineContainer(_textBlock) {CanShowLine = () => _parameterMenu.Items.Any()});
            horizontalStackPanel.Children.Add(_variableControlArea = new ContentControl {HorizontalAlignment = HorizontalAlignment.Stretch});

            //if we decide to change the padding, here would be the location
            //Padding = new Vector4F(15,0,0,0);

            //prepare the expression box
            if (SupportsExpression)
                SetupExpressionControls();

            if (!String.IsNullOrWhiteSpace(_argument.Description))
                TextBlock.ToolTip = new ToolTipControl(_argument.Label, _argument.Description + "<br/>(Click to toggle expression mode)");

            if (!String.IsNullOrWhiteSpace(group))
            {
                var groupPanel = new FlexibleStackPanel(){Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Stretch};
                groupPanel.Children.Add(new TextBlock(){Text = group, Margin = new Vector4F(3, 20, 3, 6), Font = "DefaultBold"});
                groupPanel.Children.Add(horizontalStackPanel);
                Header = groupPanel;
            }
            else
            {
                Header = horizontalStackPanel;
            }

            Margin = new Vector4F(0, 1, 0, 1);

            _synchronizer.Enqueue(UpdateSpecialControl);
        }



        public ParameterInfo Argument
        {
            get { return _argument; }
        }



        public IServiceLocator Services
        {
            get { return VisualNode.VisualGraph.Control.Services; }
        }



        public bool CanBeDragged
        {
            get { return _canBeDragged; }
            set { _canBeDragged = value; }
        }



        public bool IsExpression
        {
            get { return _argument.IsExpression; }
            set
            {
                if (value != _argument.IsExpression)
                {
                    //alert that the file was changed
                    _visualNode.VisualGraph.AlertForFileChange(false);

                    _argument.IsExpression = value;
                    UpdateSpecialControl();
                }
            }
        }



        private bool SupportsExpression
        {
            get { return _visualNode != null; }
        }



        public VisualNode VisualNode
        {
            get { return _visualNode; }
        }



        public Node Node
        {
            get { return _visualNode.Node; }
        }



        public ContextMenu ParameterMenu
        {
            get { return _parameterMenu; }
        }



        public TextBlock TextBlock
        {
            get { return _textBlock; }
        }



        public UIControl Control
        {
            get { return _control; }
        }



        public FileItem FileItem
        {
            get { return _fileItem; }
        }



        public Graph Graph
        {
            get { return _visualNode.VisualGraph.Graph; }
        }



        public TextBlock TextBlockLabel
        {
            get { return TextBlock; }
        }



        private void SetupExpressionControls()
        {
            MenuItem menuItem;

            //_textBlock.ContextMenu = new ContextMenu();
            _parameterMenu.Items.Add(menuItem = new MenuItem {Content = new TextBlock() {Text = "Fixed"}});
            menuItem.Click += (sender, args) => IsExpression = false; 

            _parameterMenu.Items.Add(menuItem = new MenuItem {Content = new TextBlock() {Text = "Expression"}});
            menuItem.Click += (sender, args) => IsExpression = true;

            _expressionStackPanel = new FlexibleStackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            _expressionTextBox = new ExpressionTextBox(Graph) //_visualNode.VisualGraph.Graph
            {
                //Style = argument.EntityEvaluation ? "ExpressionTextBoxGreen" : "ExpressionTextBox",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                IsEntityEvaluation = _argument.EntityEvaluation
            };

            _expressionTextBox.ToolTip = new ToolTipControl(_argument.Label + " (Expression)",
                _argument.Description + "<br/><br/>" + "Press CTRL+SPACE to open suggestions.\nALT+SPACE to open the advanced expression editor."); //,


            //as soon as the textbox loses focus, parse the expression
            var focusedProperty = _expressionTextBox.Properties.Get<bool>("IsFocused");
            focusedProperty.Changed += delegate
            {
                if (!_expressionTextBox.IsFocused)
                    _argument.ParsedExpression = ExpressionParser.Parse(_expressionTextBox.Text);
            };
            var textProperty = _expressionTextBox.Properties.Get<String>("Text");
            textProperty.Changed += delegate { _visualNode.VisualGraph.AlertForFileChange(false); };

            _expressionTextBox.InputProcessed += delegate(object sender, InputEventArgs args)
            {
                
                if (!InputService.IsKeyboardHandled && InputService.IsPressed(Keys.Space, false) && InputService.ModifierKeys.HasFlag(ModifierKeys.Alt))
                {
                    OpenAdvancedExpressionWindow();
                    InputService.IsKeyboardHandled = true;
                }
            };

            _expressionStackPanel.Children.Add(_expressionTextBox);

            var buttonAdvancedEditor = new Button() {Content = new TextBlock() {Text = "E"}, Width = 25, ToolTip = new ToolTipControl("Advanced Expression Editor\n(ALT+SPACE to open)")}; //{Text = "?",, Margin = new Vector4F(0), Padding = new Vector4F(0)};
            buttonAdvancedEditor.Click += delegate
            {
                OpenAdvancedExpressionWindow();
            };
            _expressionStackPanel.Children.Add(buttonAdvancedEditor);


            var buttonAddExpression = new Button() {Content = new TextBlock() {Text = "P"}, Width = 25, ToolTip = new ToolTipControl("Create and Reference Parameter")}; //{Text = "?",, Margin = new Vector4F(0), Padding = new Vector4F(0)};
            buttonAddExpression.Click += (sender, args) =>
            {
                ParameterCreateWindow parameterCreateWindow = new ParameterCreateWindow(Services, Graph, FileItem, _argument);
                parameterCreateWindow.Accepted += delegate
                {
                    //add the reference to the textbox
                    _expressionTextBox.InsertTextInCaret(parameterCreateWindow.CreatedParameterInfo.Identifier);
                    Focus();
                };
                parameterCreateWindow.Show(Screen);
            };
            _expressionStackPanel.Children.Add(buttonAddExpression);
        }

        private void OpenAdvancedExpressionWindow()
        {
            AdvancedExpressionWindow window = new AdvancedExpressionWindow(Services, _expressionTextBox.Text, _argument, FileItem, _visualNode.VisualGraph.Graph);
            window.Accepted += delegate
            {
                _expressionTextBox.Text = window.Expression;
                _argument.ParsedExpression = ExpressionParser.Parse(_expressionTextBox.Text);
            };
            window.Show(Services.Get<IUIService>().Screens[0]);
        }


        private void UpdateSpecialControl()
        {
            if (SupportsExpression && _argument.IsExpression)
            {
                _variableControlArea.Content = _expressionStackPanel;
                if (_argument.ParsedExpression == null)
                    _argument.ParsedExpression = new ParsedExpression(new ConstantExpressionNode(""));

                _expressionTextBox.Text = _argument.ParsedExpression.OriginalExpression;
                _textBlock.Style = _argument.EntityEvaluation ? "EntityExpressionTextBlock" : "ExpressionTextBlock";

                _fixedSubItems = new List<TreeViewItem>(Items);
                Items.Clear();
            }
            else
            {
                _textBlock.Style = "TextBlock";
                _variableControlArea.Content = _control;
                Items.AddRange(new List<TreeViewItem>(_fixedSubItems));
            }
        }



        protected override void OnLoad()
        {
            base.OnLoad();

            //UpdateSpecialControl();

            if (_canBeDragged)
                _textBlock.Cursor = _textBlock.Screen.Renderer.GetCursor("SizeAll");
        }



        protected override void OnHandleInput(InputContext context)
        {
            base.OnHandleInput(context);

            var draggedItem = TreeView.DraggedData as ArgumentTreeViewItem;
            if (draggedItem == null)
            {
                //else !InputService.IsMouseOrTouchHandled && 
                if (CanBeDragged && _textBlock.IsMouseOver && InputService.IsDown(MouseButtons.Left))
                {
                    TreeView.DraggedData = this;
                    Background = Color.DimGray;
                    InputService.IsMouseOrTouchHandled = true;
                }
            }
            else if (draggedItem == this)
            {
                if (_previouslyHoveredControl != null)
                    _previouslyHoveredControl.SetDropHighlights(null);


                //the item must be an ArgumentTreeViewItem, not be the dragged item itself and cannot be a child 
                ArgumentTreeViewItem hoveredItem = TreeView.GetMouseHoveredItem() as ArgumentTreeViewItem;
                if (hoveredItem != null && hoveredItem != draggedItem && !hoveredItem.GetAncestors().Contains(draggedItem))
                {
                    var relativePositionY = InputService.MousePosition.Y - hoveredItem.ActualY;
                    var relativePercentage = relativePositionY/hoveredItem.Header.ActualHeight;

                    var parent = hoveredItem.Parent as ArgumentTreeViewItem;
                    if (parent != null && parent.CanDragInto(draggedItem) && hoveredItem.CanDragInto(draggedItem))
                    {
                        //item can be put before, after and inside
                        if (InputService.IsDown(MouseButtons.Left))
                        {
                            if (relativePercentage < 0.3f)
                                hoveredItem.SetDropHighlights("Top");
                            else if (relativePercentage > 0.7f)
                                hoveredItem.SetDropHighlights("Bottom");
                            else
                                hoveredItem.SetDropHighlights("Middle");
                        }
                        else if (InputService.IsReleased(MouseButtons.Left))
                        {
                            var indexOf = parent.Items.IndexOf(hoveredItem);

                            if (relativePercentage < 0.3f)
                                parent.DragInto(draggedItem, indexOf); //place it above
                            else if (relativePercentage > 0.7f)
                                parent.DragInto(draggedItem, indexOf + 1); //place it below
                            else
                                hoveredItem.DragInto(draggedItem, hoveredItem.Items.OfType<ArgumentTreeViewItem>().Count()); //place it inside

                            TreeView.DraggedData = null;
                            draggedItem.Background = Color.Transparent;

                            InputService.IsMouseOrTouchHandled = true;
                        }
                    }
                    //item can be put inside
                    else if (hoveredItem.CanDragInto(draggedItem))
                    {
                        if (InputService.IsDown(MouseButtons.Left))
                        {
                            hoveredItem.SetDropHighlights("Middle");
                        }
                        else if (InputService.IsReleased(MouseButtons.Left))
                        {
                            hoveredItem.DragInto(draggedItem, hoveredItem.Items.OfType<ArgumentTreeViewItem>().Count()); //place it inside

                            TreeView.DraggedData = null;
                            draggedItem.Background = Color.Transparent;

                            InputService.IsMouseOrTouchHandled = true;
                        }
                    }
                    //item can be put before or after, not inside
                    else if (parent != null && parent.CanDragInto(draggedItem))
                    {
                        if (InputService.IsDown(MouseButtons.Left))
                        {
                            if (relativePercentage < 0.5f)
                                hoveredItem.SetDropHighlights("Top");
                            else if (relativePercentage > 0.5f)
                                hoveredItem.SetDropHighlights("Bottom");
                        }
                        else if (InputService.IsReleased(MouseButtons.Left))
                        {
                            var indexOf = parent.Items.IndexOf(hoveredItem);

                            if (relativePercentage < 0.5f)
                                parent.DragInto(draggedItem, indexOf); //place it above
                            else if (relativePercentage > 0.5f)
                                parent.DragInto(draggedItem, indexOf + 1); //place it below

                            TreeView.DraggedData = null;
                            draggedItem.Background = Color.Transparent;

                            InputService.IsMouseOrTouchHandled = true;
                        }
                    }
                    else
                    {
                        hoveredItem.SetDropHighlights(null);
                    }

                    _previouslyHoveredControl = hoveredItem;
                }
                else
                {
                    //Parent.Items.Remove(HighlightItem);
                }


                if (InputService.IsReleased(MouseButtons.Left))
                {
                    _previouslyHoveredControl = null;
                    TreeView.DraggedData = null;
                    draggedItem.Background = Color.Transparent;
                }
            }

            if (!InputService.IsMouseOrTouchHandled && _textBlock.IsMouseOver && _parameterMenu.Items.Any() &&
                (InputService.IsReleased(MouseButtons.Left) || InputService.IsPressed(MouseButtons.Right, true)))
            {
                InputService.IsMouseOrTouchHandled = true;
                _parameterMenu.Open(_textBlock, new Vector2F(_textBlock.ActualBounds.Left, _textBlock.ActualBounds.Bottom));
            }
        }



        protected override void OnUpdate(TimeSpan deltaTime)
        {
            base.OnUpdate(deltaTime);

            _synchronizer.Update();
        }



        /// <summary>
        /// Set (or removes) the highlight icon for Drag & Drop.
        /// </summary>
        /// <param name="icon">Can be either null, "Top", "Middle" or "Bottom"</param>
        private void SetDropHighlights(String icon)
        {
            _dragPointerIcon.Texture = icon == null ? null : EmbeddedResources.Load<Texture2D>("Resources/ArrowDrop" + icon + ".png");
        }
    }
}