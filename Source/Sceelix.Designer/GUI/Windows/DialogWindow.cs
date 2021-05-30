using System;
using DigitalRune.Animation;
using DigitalRune.Animation.Easing;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.GUI.Windows
{
    /// <summary>
    /// The dialogWindow is a window that, besides having a default animation,
    /// is always centered, it modal (i.e. will block the input on background items)
    /// and puts on a darker shade on the background items.
    /// </summary>
    public class DialogWindow : AnimatedWindow
    {
        private Image _blackImage;
        private ContentControl _dialogContent;
        private StackPanel _stackPanelButtons;



        public DialogWindow()
        {
            //services.Get<IGraphicsService>();
            IsModal = true;
            KeepCentered = true;

            RenderTransformOrigin = new Vector2F(0.5f, 0.5f);

            LoadingAnimation = new Vector2FFromToByAnimation
            {
                TargetProperty = "RenderScale", // Animate the property UIControl.RenderScale 
                From = new Vector2F(0, 0), // from (0, 0) to its actual value (1, 1)
                Duration = TimeSpan.FromSeconds(0.3), // over 0.3 seconds.
                EasingFunction = new ElasticEase {Mode = EasingMode.EaseOut, Oscillations = 1},
            };

            ClosingAnimation = new Vector2FFromToByAnimation
            {
                TargetProperty = "RenderScale", // Animate the property UIControl.RenderScale
                To = new Vector2F(0, 0), // from its current value to (0, 0)
                Duration = TimeSpan.FromSeconds(0.3), // over 0.3 seconds.
                EasingFunction = new HermiteEase {Mode = EasingMode.EaseIn},
            };

            Closing += delegate { Screen.Children.Remove(_blackImage); };
            Closed += OnClosed;

            _dialogContent = new ContentControl()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            _stackPanelButtons = new StackPanel
            {
                Margin = new Vector4F(4),
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            Content = new FlexibleStackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Vector4F(4),
                Children =
                {
                    _dialogContent,
                    new ContentControl()
                    {
                        Content = _stackPanelButtons,
                        Height = 35,
                    }
                }
            };
        }



        /// <summary>
        /// Event called on Window.Closed if the DialogResult is true.
        /// </summary>
        public event EventHandler Accepted = delegate { };



        /// <summary>
        /// Event called on Window.Closed if the DialogResult is false.
        /// </summary>
        public event EventHandler Canceled = delegate { };



        private void OnClosed(object sender, EventArgs eventArgs)
        {
            if (DialogResult.HasValue && DialogResult.Value)
                Accepted(this, eventArgs);
            else
                Canceled(this, eventArgs);
        }



        protected override void OnLoad()
        {
            
            Texture2D tex = new Texture2D(this.Screen.Renderer.GraphicsDevice, 1, 1);
            tex.SetData(new[] {Color.Black});

            //EmbeddedResources.Load<Texture2D>("Resources/Background.png")
            Screen.Children.Add(_blackImage = new Image
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            });

            base.OnLoad();
        }
        
        /// <summary>
        /// Accepts the result and closes the window, calling the Accepted event.
        /// </summary>
        protected void Accept()
        {
            DialogResult = true;
            Close();
        }



        /// <summary>
        /// Rejects the result and closes the window, calling the Canceled event.
        /// </summary>
        protected void Cancel()
        {
            DialogResult = false;
            Close();
        }



        public UIControl DialogContent
        {
            get { return _dialogContent.Content; }
            set
            {
                _dialogContent.Content = value;
            }
        }



        public void AddOKButton()
        {
            var okayButton = AddDialogButton("OK", () => Accept());
            okayButton.IsDefault = okayButton.IsCancel = true;
        }


        public void AddOKCancelButton()
        {
            AddDialogButton("OK", () => Accept()).IsDefault = true;
            AddDialogButton("Cancel", () => Cancel()).IsCancel = true;
        }


        public Button AddDialogButton(String buttonName, Action action)
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
                Width = 85,
                Height = 25,
                UserData = buttonName,
                ToolTip = new ToolTipControl(buttonName)
            };
            buttonControl.Click += delegate(object sender, EventArgs args) { action(); };

            _stackPanelButtons.Children.Add(buttonControl);

            return buttonControl;
        }

        /*public ContentControl CreateOKButtonPanel()
        {
            return CreateButtonPanel(new string[] { "OK" }, new EventHandler<EventArgs>[] { delegate { Accept(); }});
        }

        public ContentControl CreateOKCancelButtonPanel()
        {
            return CreateButtonPanel(new string[] {"OK", "Cancel"}, new EventHandler<EventArgs>[] {delegate { Accept(); }, delegate { Cancel(); }});
        }


        protected static ContentControl CreateButtonPanel(String[] buttonNames, System.EventHandler<EventArgs>[] actions, HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center)
        {
            var stackPanelButtons = new StackPanel
            {
                Margin = new Vector4F(4),
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            for (int index = 0; index < buttonNames.Length; index++)
            {
                var buttonName = buttonNames[index];
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
                    Width = 85,
                    Height = 25,
                    UserData = buttonName,
                    ToolTip = new ToolTipControl(buttonName)
                };
                buttonControl.Click += actions[index];

                stackPanelButtons.Children.Add(buttonControl);
            }


            return new ContentControl()
            {
                Content = stackPanelButtons,
                Height = 35,
                //HorizontalAlignment = HorizontalAlignment.Stretch,
                //VerticalAlignment = VerticalAlignment.Top,
            };
        }*/
    }
}