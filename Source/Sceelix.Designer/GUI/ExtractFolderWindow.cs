using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Ionic.Zip;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.GUI
{
    public class ExtractFolderWindow : DialogWindow
    {
        private readonly string _destinationFolder;

        private readonly Synchronizer _synchronizer = new Synchronizer();
        private readonly string _zipFilePath;
        private Button _cancelButton;

        private ProgressBar _progressBar;
        private bool _shouldCancel;
        private TextBlock _textBlock;



        public ExtractFolderWindow(String zipFilePath, String destinationFolder)
        {
            Title = "Extracting Archive";

            _zipFilePath = zipFilePath;
            _destinationFolder = destinationFolder;
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
                    Texture = EmbeddedResources.Load<Texture2D>("Resources/GoOut_48x48.png"),
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
                Text = "Extracting Files",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Vector4F(4)
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

            base.OnLoad();


            Thread thread = new Thread(ExtractFiles)
            {
                CurrentCulture = CultureInfo.InvariantCulture,
                CurrentUICulture = CultureInfo.InvariantCulture
            };
            _synchronizer.Enqueue(thread.Start);
        }



        protected override Vector2F OnMeasure(Vector2F availableSize)
        {
            var measuredSize = base.OnMeasure(availableSize);

            X = Screen.ActualWidth/2f - measuredSize.X/2f;
            Y = Screen.ActualHeight/2f - measuredSize.Y/2f;

            return measuredSize;
        }



        private void ExtractFiles()
        {
            try
            {
                int entryIndex = 1;

                using (ZipFile zipfile = ZipFile.Read(_zipFilePath))
                {
                    var totalFileNumber = zipfile.Entries.Count(x => !x.IsDirectory);

                    foreach (ZipEntry entry in zipfile)
                    {
                        var entryName = entry.FileName;

                        if (_shouldCancel)
                        {
                            _synchronizer.Enqueue(Cancel);
                            break;
                        }

                        _synchronizer.Enqueue(delegate
                        {
                            _textBlock.Text = "Extracting " + entryName + " (" + entryIndex++ + " of " + totalFileNumber + ").";
                            _textBlock.MinWidth = Math.Max(_textBlock.ActualWidth, _textBlock.MinWidth);
                        });

                        entry.Extract(_destinationFolder, ExtractExistingFileAction.OverwriteSilently);
                    }
                }

                _synchronizer.Enqueue(Accept);
            }
            catch (Exception ex)
            {
                DesignerProgram.Log.Error(ex);
                _synchronizer.Enqueue(Cancel);
            }
        }



        private void CancelButtonOnClick(object sender, EventArgs e)
        {
            _shouldCancel = true;
        }



        protected override void OnUpdate(TimeSpan deltaTime)
        {
            base.OnUpdate(deltaTime);

            _synchronizer.Update();
        }
    }
}