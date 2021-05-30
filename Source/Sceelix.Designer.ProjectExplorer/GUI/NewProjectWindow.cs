using System;
using System.IO;
using System.Windows.Forms;
using DigitalRune.Game.UI.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.Managers;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;
using TextBox = DigitalRune.Game.UI.Controls.TextBox;

namespace Sceelix.Designer.ProjectExplorer.GUI
{
    public class NewProjectWindow : DialogWindow
    {
        private IServiceLocator _services;

        private TextButton _browseButton;
        private TextButton _cancelButton;
        private TextButton _okButton;
        private TextBox _textBoxLocation;
        private TextBox _textBoxName;
        


        public NewProjectWindow(IServiceLocator services)
        {
            _services = services;

            Title = "New Sceelix Project";
            Width = 560;
            Height = 300;
        }



        public Project CreatedProject
        {
            get;
            private set;
        }



        protected override void OnLoad()
        {
            Canvas canvas = new Canvas();

            canvas.Children.Add(new Image()
            {
                X = 15,
                Y = 15,
                Width = 170,
                Height = 238,
                //Texture = _projectContent.LoadTexture2D("SideBanner")
                Texture = EmbeddedResources.Load<Texture2D>("Resources/sidebanner.png"),
                Foreground = Color.White
            });

            canvas.Children.Add(new TextBlock()
            {
                X = 210,
                Y = 45,
                Width = 35,
                Height = 13,
                Text = "Name:"
            });

            canvas.Children.Add(_textBoxName = new ExtendedTextBox()
            {
                X = 210,
                Y = 65,
                Width = 325,
                Height = 20,
                Text = "MyProject"
            });


            canvas.Children.Add(new TextBlock()
            {
                X = 210,
                Y = 100,
                Width = 51,
                Height = 13,
                Text = "Location:"
            });

            canvas.Children.Add(_textBoxLocation = new ExtendedTextBox()
            {
                X = 210,
                Y = 120,
                Width = 285,
                Height = 20,
                Text = Project.DefaultProjectsFolder

                //Foreground = Color.DarkRed
            });

            canvas.Children.Add(_browseButton = new TextButton()
            {
                X = 500,
                Y = 120,
                Width = 35,
                Height = 20,
                Text = "..."
            });
            _browseButton.Click += BrowseButtonOnClick;


            canvas.Children.Add(new TextBlock()
            {
                X = 210,
                Y = 155,
                Width = 327,
                Height = 30,
                Text = "A new folder with the project name will be created within the specified location.",
                WrapText = true,
            });

            canvas.Children.Add(_okButton = new TextButton()
            {
                X = 340,
                Y = 225,
                Width = 95,
                Height = 25,
                Text = "OK",
                IsDefault = true
            });
            _okButton.Click += OkButtonOnClick;

            canvas.Children.Add(_cancelButton = new TextButton()
            {
                X = 440,
                Y = 225,
                Width = 95,
                Height = 25,
                Text = "Cancel",
                IsCancel = true
            });
            _cancelButton.Click += CancelButtonOnClick;
            //_cancelButton.ContextMenu = new ExtendedContextMenu();

            //_cancelButton.ContextMenu.Items.Add(new MenuItem());

            Content = canvas;

            //CenterWindow();

            _textBoxName.SelectAll();

            base.OnLoad();
        }



        private void CancelButtonOnClick(object sender, EventArgs eventArgs)
        {
            Cancel();
        }



        private void OkButtonOnClick(object sender, EventArgs eventArgs)
        {
            //A new folder with the project name will be created within the specified location.
            String projectFolder = Path.Combine(_textBoxLocation.Text, _textBoxName.Text);
            String projectFile = Path.Combine(projectFolder, Path.ChangeExtension(_textBoxName.Text, Project.FileExtension));

            if (File.Exists(projectFile))
            {
                MessageWindow messageWindow = new MessageWindow()
                {
                    MessageIcon = MessageWindowIcon.Question,
                    Text = "A project with that name already exists. Would you like to overwrite it?",
                    Title = "Existing Project",
                    Buttons = new[] {"Yes", "No"}
                };
                messageWindow.Click += delegate
                {
                    if (messageWindow.Selection == "Yes")
                    {
                        CreateAndAccept(projectFolder);
                    }
                };
                messageWindow.Show(Screen);
                //messageWindow.Show(_services);
                _services.Get<WindowAnimator>().Show(messageWindow);
            }
            else
            {
                CreateAndAccept(projectFolder);
            }
        }



        private void CreateAndAccept(string projectFolder)
        {
            //create the project file
            CreatedProject = Project.CreateProject(_services, projectFolder, _textBoxName.Text);

            Accept();
        }



        private void BrowseButtonOnClick(object sender, EventArgs eventArgs)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            if (folderBrowserDialog.ShowCrossDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _textBoxLocation.Text = folderBrowserDialog.SelectedPath;
            }
        }
    }
}