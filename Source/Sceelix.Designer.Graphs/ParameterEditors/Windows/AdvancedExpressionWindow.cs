using System;
using System.Linq;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Sceelix.Core.Graphs;
using Sceelix.Core.Graphs.Functions;
using Sceelix.Core.Parameters;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Designer.Graphs.ParameterEditors.Controls;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Services;
using VerticalAlignment = DigitalRune.Game.UI.VerticalAlignment;

namespace Sceelix.Designer.Graphs.ParameterEditors.Windows
{
    public class AdvancedExpressionWindow : DialogWindow
    {
        private static readonly String[] Operators = { "+", "-", "*", "/", "%", "&&", "||", "==", "!=", "<", ">", "<=", ">=" };

        private readonly ExpressionTextBox _expressionTextBox;
        private readonly ParameterInfo _argument;
        private readonly FileItem _fileItem;
        private readonly Graph _graph;
        private IServiceLocator _services;

        //private TextBlock _descriptionTextBlock;



        public AdvancedExpressionWindow(IServiceLocator services, string expression, ParameterInfo argument, FileItem fileItem, Graph graph)
            
        {
            _graph = graph;
            _argument = argument;
            _fileItem = fileItem;
            Width = 900;
            Height = 500;
            Title = "Advanced Expression Editor";

            _services = services;

            var windowStackPanel = new FlexibleStackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            //Add the top section, with several operator buttons
            var toolStackPanel = new StackPanel() {Orientation = Orientation.Horizontal, Height = 35,HorizontalAlignment = HorizontalAlignment.Stretch};
            foreach (var op in Operators )
                toolStackPanel.Children.Add(CreateToolboxButton(op));
            windowStackPanel.Children.Add(toolStackPanel);
            

            //Add the expression textbox
            windowStackPanel.Children.Add(_expressionTextBox = new ExpressionTextBox(graph)
            {
                Text = expression,
                IsEntityEvaluation = argument.EntityEvaluation,
                //Style = entityEvaluation ? "ExpressionTextBoxGreen" : "ExpressionTextBox",
                Height = 100,
                Font = "DefaultBig",
                MinLines = 3,
                MaxLines = 3,
                Margin = new Vector4F(3),
                Padding = new Vector4F(10),
                Background = new Color(38,127,0)
            });


            //add the lists for parameters, attributes and 
            windowStackPanel.Children.Add(new EqualStackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Children =
                {
                    CreateParameterControl(),
                    CreateAttributeControl(),
                    CreateFunctionControl()
                }
            });


            //Add the description textbox
            /*windowStackPanel.Children.Add(new GroupBox()
            {
                Title = "Description",
                Height = 60,
                Margin = new Vector4F(3),
                Content = _descriptionTextBlock = new TextBlock()
                {
                    WrapText = true,
                    UseEllipsis = true,
                    Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed ut ligula in neque consectetur egestas. Maecenas ornare, ipsum et volutpat fermentum, tellus nunc sollicitudin diam, sit amet tincidunt mi odio nec massa. Fusce turpis sem, aliquet et ipsum at, dictum rutrum massa."
                }
            });*/


            //add the Okay/Cancel buttons
            var buttonPanel = new StackPanel() {Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Stretch, Height = 30, Margin = new Vector4F(10)};
            {
                TextButton okButton, cancelButton;
                StackPanel panel = new StackPanel() {Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right};

                panel.Children.Add(okButton = new TextButton() {Width = 95, Height = 25, Text = "OK", Margin = new Vector4F(5)});
                okButton.Click += delegate { Accept();};

                panel.Children.Add(cancelButton = new TextButton() {Width = 95, Height = 25, Text = "Cancel", Margin = new Vector4F(5)});
                cancelButton.Click += delegate { Cancel(); };

                buttonPanel.Children.Add(panel);
            }

            windowStackPanel.Children.Add(buttonPanel);

            Content = windowStackPanel;
        }



        private UIControl CreateParameterControl()
        {
            //Add the parameter references
            var parameterItems = _graph.ParameterInfos.Select(x => Parameter.GetIdentifier(x.Label)).OrderBy(x => x).ToList();
            var parameterObjectTreeView = new ObjectTreeView()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HasChildren = obj => false,
                ViewType = ObjectTreeView.ListViewType.List,
                HasItemToolTip = true,
                SelectionType = ObjectTreeView.SelectionViewType.Line,
                ItemHeight = 18,
                ItemPadding = new Vector4F(0, 5, 0, 5),
                Foreground = ExpressionTextBox.ParameterColor,
                Items = parameterItems
            };
            parameterObjectTreeView.ItemSelected += delegate(object item) { DeselectItemFromLists(parameterObjectTreeView); };
            parameterObjectTreeView.ItemEntered += delegate(object item) { _expressionTextBox.InsertTextInCaret(item.ToString()); };

            var createParameterMenu = new TextButton() { Text = "+" };
            createParameterMenu.Click += delegate
            {
                ParameterCreateWindow parameterCreateWindow = new ParameterCreateWindow(_services, _graph, _fileItem, _argument);
                parameterCreateWindow.Accepted += delegate
                {
                    //add the reference to the textbox
                    _expressionTextBox.InsertTextInCaret(parameterCreateWindow.CreatedParameterInfo.Identifier);
                    parameterObjectTreeView.Items = _graph.ParameterInfos.Select(x => Parameter.GetIdentifier(x.Label)).OrderBy(x => x).ToList();
                    Focus();

                };
                parameterCreateWindow.Show(Screen);
            };

            return new GroupBox()
            {
                Title = "Parameters",
                Margin = new Vector4F(3),
                Content = new FlexibleStackPanel()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Children =
                    {
                        parameterObjectTreeView,
                        createParameterMenu
                    }
                }
            };
        }



        private UIControl CreateAttributeControl()
        {
            //add the names of attributes
            var attributeItems = _graph.GetReferencedAttributes().OrderBy(x => x).Distinct().Select(x => "@" + x).ToList();
            if (_argument.EntityEvaluation)
                attributeItems.AddRange(attributeItems.Select(x => "@" + x).ToList());

            var attributeObjectTreeView = new ObjectTreeView()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HasChildren = obj => false,
                ViewType = ObjectTreeView.ListViewType.List,
                HasItemToolTip = true,
                SelectionType = ObjectTreeView.SelectionViewType.Line,
                ItemHeight = 18,
                ItemPadding = new Vector4F(0, 5, 0, 5),
                Foreground = ExpressionTextBox.AttributeColor,
                //SelectionColor = Color.Red,
                Items = attributeItems
            };
            attributeObjectTreeView.ItemSelected+= delegate(object item) { DeselectItemFromLists(attributeObjectTreeView); };
            attributeObjectTreeView.ItemEntered += delegate (object item) { _expressionTextBox.InsertTextInCaret(item.ToString()); };

            return new GroupBox()
            {
                Title = "Attributes",
                Margin = new Vector4F(3),
                Content = attributeObjectTreeView
            };
        }



        private void DeselectItemFromLists(ObjectTreeView treeToSelect)
        {
            foreach (var otherTreeView in UIHelper.GetDescendants(this).OfType<ObjectTreeView>().Where(x => x != treeToSelect))
                otherTreeView.DeselectAll();
        }



        private UIControl CreateFunctionControl()
        {
            //add the functions to the list
            var functionByGroup = FunctionManager.GetRegisteredFunctionByGroup();
            var functionObjectTreeView = new ObjectTreeView()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HasChildren = obj => obj is IGrouping<String, String>,
                IsInitiallyExpanded = o => false,
                ViewType = ObjectTreeView.ListViewType.List,
                HasItemToolTip = true,
                SelectionType = ObjectTreeView.SelectionViewType.Line,
                GetChildren = delegate(object o)
                {
                    var grouping = o as IGrouping<String, String>;
                    if (grouping != null)
                    {
                        var data = grouping.ToList();
                        return data;
                    }

                    return new object[] {};
                },
                GetColumnValue = delegate(object o, ObjectTreeView.Column column)
                {
                    var grouping = o as IGrouping<String, String>;
                    if (grouping != null)
                        return grouping.Key;

                    return o.ToString();
                },
                ItemHeight = 18,
                //ItemPadding = new Vector4F(0, 5, 0, 5),
                Foreground = ExpressionTextBox.FunctionColor,
                Items = functionByGroup
            };
            functionObjectTreeView.ItemSelected += delegate(object item) { DeselectItemFromLists(functionObjectTreeView); };
            functionObjectTreeView.ItemEntered += delegate(object item)
            {
                if(!(item is IGrouping<String, String>))
                    _expressionTextBox.InsertTextInCaret(item.ToString());
            };

            return new GroupBox()
            {
                Title = "Functions",
                Margin = new Vector4F(3),
                Content = functionObjectTreeView
            };
        }




        private UIControl CreateToolboxButton(string operation)
        {
            var textButton = new TextButton() {Text = operation, Height=25, MinWidth = 25, Margin = new Vector4F(2)};
            textButton.Click += delegate
            {
                _expressionTextBox.InsertTextInCaret(operation);
            };
            return textButton;
        }



        



        public String Expression
        {
            get { return _expressionTextBox.Text; }
        }
    }

    
}