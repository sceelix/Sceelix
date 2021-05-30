using System;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.Services;


namespace Sceelix.Designer.GUI.Windows
{
    /// <summary>
    /// A Window with a message and text box.
    /// </summary>
    public class InputWindow : DialogWindow
    {
        private TextBlock _errorText;
        private string _inputText = String.Empty;

        private string _labelText = "Input";
        private Texture2D _messageIcon;
        private Button _okButton, _cancelButton;
        private TextBox _textBox;
        public Func<String, Object, String> Check = delegate { return String.Empty; };



        public InputWindow()
        {
            Title = "InputWindow";
        }



        public string LabelText
        {
            get { return _labelText; }
            set { _labelText = value; }
        }



        public Texture2D MessageIcon
        {
            get { return _messageIcon; }
            set { _messageIcon = value; }
        }



        public string InputText
        {
            get { return _inputText; }
            set { _inputText = value; }
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

            if (_messageIcon != null)
            {
                var image = new ContentControl()
                {
                    Content = new Image()
                    {
                        Texture = _messageIcon,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Width = 48,
                        Height = 48,
                        Margin = new Vector4F(10)
                    },
                    VerticalAlignment = VerticalAlignment.Stretch
                };
                stackPanelIconText.Children.Add(image);
            }

            _textBox = new ExtendedTextBox()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            var textProperty = _textBox.Properties.Get<String>("Text");
            textProperty.Changed += TextPropertyOnChanged;


            stackPanelIconText.Children.Add(new ContentControl()
            {
                Content = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                    Children =
                    {
                        new TextBlock()
                        {
                            Text = _labelText,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch
                        },
                        _textBox
                    },
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            });


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

            //set the text and perform validation
            _textBox.Text = _inputText;

            _okButton.IsDefault = true;
            //AcceptButton = _okButton;
            //CancelButton = _cancelButton;

            base.OnLoad();
        }



        private void OkButtonOnClick(object sender, EventArgs eventArgs)
        {
            InputText = _textBox.Text;

            Accept();
        }



        private void CancelButtonOnClick(object sender, EventArgs eventArgs)
        {
            Cancel();
        }



        private void TextPropertyOnChanged(object sender, GamePropertyEventArgs<string> gamePropertyEventArgs)
        {
            _errorText.Text = Check(gamePropertyEventArgs.NewValue, UserData);
            _okButton.IsEnabled = String.IsNullOrEmpty(_errorText.Text);
        }



        protected override Vector2F OnMeasure(Vector2F availableSize)
        {
            var onMeasure = base.OnMeasure(availableSize);

            //CenterWindow(onMeasure);

            return onMeasure;
        }
    }
}