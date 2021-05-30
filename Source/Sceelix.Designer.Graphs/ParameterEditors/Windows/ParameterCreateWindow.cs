using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Core.Graphs;
using Sceelix.Core.Parameters;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Graphs.ParameterEditors.Windows
{
    public class ParameterCreateWindow : DialogWindow
    {
        private readonly Graph _graph;
        private readonly FileItem _fileItem;
        private ParameterInfo _parameterInfo;

        private List<ParameterEditor> _availableEditors;
        private Dictionary<ParameterEditor, ParameterInfo> _defaultParameterInfos;


        private TextBlock _errorText;
        private Button _okButton, _cancelButton;
        private TextBox _textboxLabel;
        private DropDownButton _dropdownType;
        private TextBox _textBoxIdentifier;
        private LayoutControl _layoutControl;
        private FlexibleStackPanel _currentValuePanel;
        private ParameterEditorManager _parameterEditorManager;


        public ParameterCreateWindow(IServiceLocator services, Graph graph, FileItem fileItem, ParameterInfo parameterInfo)
        {
            _graph = graph;
            _fileItem = fileItem;
            _parameterInfo = parameterInfo;
            Title = "Create Graph Parameter";
            Width = 450;

            _parameterEditorManager = services.Get<ParameterEditorManager>();

            _availableEditors = _parameterEditorManager.AvailableParameterEditors.Where(y => y.CanExistAtRoot).OrderBy(x => x.ParameterInfoName).ToList();//.Where(x => !(x is RecursiveParameterEditor) && !x.CanHaveSubItems).ToList();
            _defaultParameterInfos = new Dictionary<ParameterEditor, ParameterInfo>();
        }





        protected override void OnLoad()
        {
            CloseButtonStyle = null;

            var stackPanelMain = new StackPanel
            {
                Margin = new Vector4F(4),
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };


            var stackPanelIconText = new FlexibleStackPanel
            {
                Margin = new Vector4F(4),
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            var image = new ContentControl()
            {
                Content = new Image()
                {
                    Texture = EmbeddedResources.Load<Texture2D>("Resources/Graph48x48.png"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 48,
                    Height = 48,
                    Margin = new Vector4F(10)
                },
                VerticalAlignment = VerticalAlignment.Stretch
            };
            stackPanelIconText.Children.Add(image);

            

            ParameterEditor parameterEditor = _parameterEditorManager.GetParameterEditor(_parameterInfo);
            _defaultParameterInfos.Add(parameterEditor, _parameterInfo);




            _layoutControl = new LayoutControl();

            _textboxLabel = new ExtendedTextBox();
            var textProperty = _textboxLabel.Properties.Get<String>("Text");
            textProperty.Changed += TextPropertyOnChanged;
            _layoutControl.Add("Label:", _textboxLabel);

            _textBoxIdentifier = new ExtendedTextBox() {Text = Parameter.GetIdentifier(_parameterInfo.Label), IsReadOnly = true, Foreground = Color.DarkGray};
            _layoutControl.Add("Identifier:", _textBoxIdentifier);
            
            _dropdownType = new DropDownButton();
            _dropdownType.Items.AddRange(_availableEditors.Select(x => x.ParameterInfoName));
            _layoutControl.Add("Type:", _dropdownType);
            var selectedIndexProperty = _dropdownType.Properties.Get<int>("SelectedIndex");
            selectedIndexProperty.Changed += (sender, args) =>
            {
                ParameterEditor chosenEditor = _availableEditors[args.NewValue];

                //keep a dictionary of the ParameterInfos and their default values
                if (!_defaultParameterInfos.TryGetValue(chosenEditor, out _parameterInfo))
                {
                    _parameterInfo = (ParameterInfo) Activator.CreateInstance(chosenEditor.ParameterInfoType, _textboxLabel.Text);
                    _defaultParameterInfos.Add(chosenEditor,_parameterInfo);
                }
                
                //the "value" line may be empty if a non-simple type is selected
                if (_currentValuePanel != null)
                {
                    _layoutControl.Remove(_currentValuePanel);
                    _currentValuePanel = null;
                }

                //if the type has a simple control, allow its use
                var control = chosenEditor.CreateControl(_parameterInfo, _fileItem, () => { });
                if (control != null)
                {
                    _currentValuePanel = _layoutControl.Add("Value:", control);
                }
            };


            stackPanelIconText.Children.Add(_layoutControl);


            stackPanelMain.Children.Add(stackPanelIconText);


            stackPanelMain.Children.Add(_errorText = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Foreground = Color.Red,
                Margin = new Vector4F(10, 0, 10, 0)
            });

            var stackPanelButtons = new StackPanel
            {
                Margin = new Vector4F(4),
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };


            stackPanelButtons.Children.Add(_okButton = new Button
            {
                Content = new TextBlock()
                {
                    Text = "OK",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                Margin = new Vector4F(4),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 85,
                Height = 25,
                IsDefault = true
            });
            _okButton.Click += OkButtonOnClick;


            stackPanelButtons.Children.Add(_cancelButton = new Button
            {
                Content = new TextBlock()
                {
                    Text = "Cancel",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                Margin = new Vector4F(4),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 85,
                Height = 25,
                IsCancel = true
            });
            _cancelButton.Click += CancelButtonOnClick;


            stackPanelMain.Children.Add(new ContentControl()
            {
                Content = stackPanelButtons,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
            });

            Content = stackPanelMain;
            
            //set the label to invoke the validation functions
            _textboxLabel.Text = _parameterInfo.Label;
            _dropdownType.SelectedIndex = _availableEditors.FindIndex(x => x.ParameterInfoName == parameterEditor.ParameterInfoName);

            _okButton.IsDefault = true;
            //AcceptButton = _okButton;
            //CancelButton = _cancelButton;

            base.OnLoad();
        }



        private void OkButtonOnClick(object sender, EventArgs eventArgs)
        {
            _parameterInfo = (ParameterInfo)CreatedParameterInfo.Clone();
            _parameterInfo.ClearExpressions();
            _parameterInfo.Label = _textboxLabel.Text;
            _graph.ParameterInfos.Add(_parameterInfo);

            Accept();
        }



        private void CancelButtonOnClick(object sender, EventArgs eventArgs)
        {
            _parameterInfo = null;

            Cancel();
        }



        private void TextPropertyOnChanged(object sender, GamePropertyEventArgs<string> gamePropertyEventArgs)
        {
            _errorText.Text = Check(gamePropertyEventArgs.NewValue);
            _okButton.IsEnabled = String.IsNullOrEmpty(_errorText.Text);
            _textBoxIdentifier.Text = Parameter.GetIdentifier(gamePropertyEventArgs.NewValue);
        }



        private string Check(string inputString)
        {
            String identifier = Parameter.GetIdentifier(inputString);

            if (String.IsNullOrWhiteSpace(identifier))
                return "The name cannot be empty. Please introduce a valid name.";
            if (!Regex.IsMatch(identifier, "^[a-zA-Z_][a-zA-Z0-9_]*$"))
                return "The chosen name is not valid. It should start with underscore/alphabetic while the remaining letters should either alphanumeric or underscore.";
            if (_graph.ParameterInfos.Any(x => x.Identifier == identifier))
                return "A parameter with that identifier already exists.";
            //check if it doesn't collide with other, does not have invalid chars, etc...

            return String.Empty;
        }



        protected override Vector2F OnMeasure(Vector2F availableSize)
        {
            var onMeasure = base.OnMeasure(availableSize);

            //CenterWindow(onMeasure);

            return onMeasure;
        }



        public ParameterInfo CreatedParameterInfo
        {
            get { return _parameterInfo; }
        }

    }
}
