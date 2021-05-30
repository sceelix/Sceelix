using System;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Extensions;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;
using Sceelix.Extensions;

namespace Sceelix.Designer.GUI.Windows
{
    public enum MessageWindowIcon
    {
        None,
        Information,
        Exclamation,
        Question,
        Error
    }

    public class MessageWindow : DialogWindow
    {
        private string[] _buttons = {"OK"};
        private MessageWindowIcon _messageIcon = MessageWindowIcon.None;

        private string _text = String.Empty;



        public MessageWindow()
        {
            ButtonWidth = 85;
            ButtonHeight = 25;
        }



        public String Selection
        {
            get;
            set;
        }



        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }



        public MessageWindowIcon MessageIcon
        {
            get { return _messageIcon; }
            set { _messageIcon = value; }
        }



        public string[] Buttons
        {
            get { return _buttons; }
            set { _buttons = value; }
        }



        public event EventHandler<EventArgs> Click = delegate { };



        protected override void OnLoad()
        {
            //Width = 300;
            //Height = 200;

            //CanResize = true;
            //this.
            CloseButtonStyle = null;


            String chosenTextureName = null;
            switch (MessageIcon)
            {
                case MessageWindowIcon.Information:
                    chosenTextureName = "Resources.Info_32x32.png";
                    break;
                case MessageWindowIcon.Exclamation:
                    chosenTextureName = "Resources.Exclamation_32x32.png";
                    break;
                case MessageWindowIcon.Question:
                    chosenTextureName = "Resources.Question_32x32.png";
                    break;
                case MessageWindowIcon.Error:
                    chosenTextureName = "Resources.Cancel_32x32.png";
                    break;
            }

            var stackPanelMain = new StackPanel
            {
                Margin = new Vector4F(4),
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };


            var stackPanelIconText = new StackPanel
            {
                Margin = new Vector4F(4),
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            if (chosenTextureName != null)
            {
                var image = new ContentControl()
                {
                    Content = new Image()
                    {
                        Texture = EmbeddedResources.Load<Texture2D>(chosenTextureName),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Width = 32,
                        Height = 32,
                        Margin = new Vector4F(10, 10, 15, 10)
                    },
                    /*Width = 48,
                        Height = 48,*/
                    
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };
                stackPanelIconText.Children.Add(image);
            }


            stackPanelIconText.Children.Add(new ContentControl()
            {
                Content = new ContentControl()
                {
                    Content = new TextBlock()
                    {
                        Text = _text
                    },
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                },
                VerticalAlignment = VerticalAlignment.Center
            });


            stackPanelMain.Children.Add(stackPanelIconText);


            var stackPanelButtons = new StackPanel
            {
                Margin = new Vector4F(4),
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            foreach (var buttonName in _buttons)
            {
                var buttonControl = new Button
                {
                    Content = new TextBlock()
                    {
                        Text = buttonName,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                    },
                    Margin = new Vector4F(4),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = ButtonWidth,
                    Height = ButtonHeight,
                    UserData = buttonName,
                    ToolTip = new ToolTipControl(buttonName)
                };
                buttonControl.Click += ButtonClick;

                stackPanelButtons.Children.Add(buttonControl);
            }


            stackPanelMain.Children.Add(new ContentControl()
            {
                Content = stackPanelButtons,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                //Height = 40
            });

            Content = stackPanelMain;

            //CenterWindow(this.ActualBounds.Size);

            base.OnLoad();
        }



        public float ButtonWidth
        {
            get;
            set;
        }

        public float ButtonHeight
        {
            get;
            set;
        }
        /*protected override Vector2F OnMeasure(Vector2F availableSize)
        {
            var onMeasure = base.OnMeasure(availableSize);

            CenterWindow(onMeasure);

            return onMeasure;
        }*/



        private void ButtonClick(object sender, EventArgs eventArgs)
        {
            Selection = (String) sender.CastTo<Button>().UserData;
            Click(this, EventArgs.Empty);
            Accept();
        }
    }
}