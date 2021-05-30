using System;
using System.Globalization;
using System.Threading;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.GUI.Windows
{
    public class ProgressWindow : DialogWindow
    {
        private readonly Action<ProgressHandler> _backgroundAction;

        //use the synchronizer to execute GUI related tasks in the main thread
        private readonly Synchronizer _synchronizer = new Synchronizer();
        private Button _cancelButton;

        //if the user presses the cancel button, warn the thread
        private bool _processCancelled = false;


        //window gui controls that must be changed
        private ProgressBar _progressBar;
        private TextBlock _textBlock;



        public ProgressWindow(Action<ProgressHandler> backgroundAction)
        {
            _backgroundAction = backgroundAction;

            Width = 500;
        }


        /// <summary>
        /// Icon that appears on the left of the dialog. Expected size is 48x48 pixels.
        /// </summary>
        public Texture2D LargeIcon
        {
            get;
            set;
        }



        /// <summary>
        /// Drawing of the window with progressbar and such.
        /// </summary>
        protected override void OnLoad()
        {
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
                    Texture = LargeIcon,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 48,
                    Height = 48,
                    Margin = new Vector4F(10)
                },
                VerticalAlignment = VerticalAlignment.Stretch
            };
            stackPanelIconText.Children.Add(image);

            _progressBar = new ProgressBar
            {
                IsIndeterminate = true,
                Margin = new Vector4F(4),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            _textBlock = new TextBlock()
            {
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Vector4F(4),
            };

            stackPanelIconText.Children.Add(new ContentControl()
            {
                Content = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                    Children =
                    {
                        _textBlock,
                        _progressBar
                    },
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                ClipContent = true
            });


            stackPanelMain.Children.Add(stackPanelIconText);

            var stackPanelButtons = new StackPanel
            {
                Margin = new Vector4F(4),
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };


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
                Height = 25
            });
            _cancelButton.Click += CancelButtonOnClick;


            stackPanelMain.Children.Add(new ContentControl()
            {
                Content = stackPanelButtons,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
            });

            Content = stackPanelMain;

            Thread thread = new Thread(PerformProgressAction)
            {
                CurrentCulture = CultureInfo.InvariantCulture,
                CurrentUICulture = CultureInfo.InvariantCulture
            };
            thread.Start();

            base.OnLoad();
        }



        private void PerformProgressAction()
        {
            var progressHandler = new ProgressHandler(this);

            _backgroundAction.Invoke(progressHandler);

            _synchronizer.Enqueue(Close);
        }



        private void CancelButtonOnClick(object sender, EventArgs e)
        {
            _processCancelled = true;
        }



        protected override void OnUpdate(TimeSpan deltaTime)
        {
            base.OnUpdate(deltaTime);

            _synchronizer.Update();
        }



        public class ProgressHandler
        {
            private readonly ProgressWindow _progressWindow;



            public ProgressHandler(ProgressWindow progressWindow)
            {
                _progressWindow = progressWindow;
            }



            public bool ShouldCancel
            {
                get { return _progressWindow._processCancelled; }
            }



            public void SetText(String text)
            {
                _progressWindow._synchronizer.Enqueue(() => _progressWindow._textBlock.Text = text);
            }



            public void PerformOnMainThread(Action action)
            {
                _progressWindow._synchronizer.Enqueue(action);
            }
        }


    }
}