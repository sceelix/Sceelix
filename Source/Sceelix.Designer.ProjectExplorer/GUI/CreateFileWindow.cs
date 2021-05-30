using System;
using System.IO;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.MenuControls;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.ProjectExplorer.FileHandlers;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Services;
using HorizontalAlignment = DigitalRune.Game.UI.HorizontalAlignment;
using Orientation = DigitalRune.Game.UI.Orientation;
using TextBox = DigitalRune.Game.UI.Controls.TextBox;

namespace Sceelix.Designer.ProjectExplorer.GUI
{
    public class CreateFileWindow : DialogWindow
    {
        private readonly IServiceLocator _services;

        private readonly FolderItem _parentFolder;
        private TextButton _cancelButton;
        private TextBlock _errorTextBlock;
        private bool _isSuggestion;
        private ListView _listView;

        private TextButton _okButton;
        private IFileCreator _selectedCreator;

        private TextBox _textBoxName;



        public CreateFileWindow(IServiceLocator services, FolderItem parentFolder)
            
        {
            _services = services;
            _parentFolder = parentFolder;

            Title = "Add New Item";
            Width = 750;
            Height = 450;
            CanResize = true;
        }



        public FileItem CreatedItem
        {
            get;
            private set;
        }



        protected override void OnLoad()
        {
            FlexibleStackPanel mainStackPanel = new FlexibleStackPanel {Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch};
            {
                FlexibleStackPanel stackPanel = new FlexibleStackPanel() {Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch};
                {
                    /*TreeView treeView = new TreeView() {Width = 100, VerticalAlignment = VerticalAlignment.Stretch, Margin = new Vector4F(5)};
                    treeView.Items.Add(new SelectableTreeViewItem(){Text = "Text1"});
                    treeView.Items.Add(new SelectableTreeViewItem() { Text = "Text2" });
                    stackPanel.Children.Add(treeView);*/

                    ScrollViewer scrollViewer = new VerticalScrollViewer() {HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch, Margin = new Vector4F(10)};
                    {
                        //StackPanel panel = new StackPanel() { Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
                        _listView = new ListView() {HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch};

                        foreach (var fileCreator in _services.Get<FileHandlerManager>().FileCreators)
                        {
                            var itemStack = new FlexibleStackPanel() {HorizontalAlignment = HorizontalAlignment.Stretch, Orientation = Orientation.Horizontal, Margin = new Vector4F(5)};
                            itemStack.Children.Add(new Image() {Texture = fileCreator.Icon48X48, Height = 48, Width = 48, Margin = new Vector4F(5)});

                            EqualStackPanel equalStackPanel = new EqualStackPanel() {Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch, Margin = new Vector4F(5)};
                            equalStackPanel.Children.Add(new TextBlock() {Text = fileCreator.ItemName, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch, Margin = new Vector4F(2)});
                            equalStackPanel.Children.Add(new TextBlock() {Text = fileCreator.Description, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch, Margin = new Vector4F(2)});
                            itemStack.Children.Add(equalStackPanel);
                            itemStack.UserData = fileCreator;

                            var listViewItem = new ListViewItem() {Content = itemStack};
                            _listView.Items.Add(listViewItem);
                        }
                        scrollViewer.Content = _listView;

                        _listView.ItemSelectionChanged += OnItemSelectionChanged;
                    }
                    stackPanel.Children.Add(scrollViewer);
                }
                mainStackPanel.Children.Add(stackPanel);

                var flexibleStackPanel = new FlexibleStackPanel() {Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Vector4F(5)};
                flexibleStackPanel.Children.Add(new TextBlock() {Text = "Name: ", VerticalAlignment = VerticalAlignment.Stretch, Width = 50, Margin = new Vector4F(5)});
                flexibleStackPanel.Children.Add(_textBoxName = new ExtendedTextBox { VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Vector4F(5)});
                var textProperty = _textBoxName.Properties.Get<String>("Text");
                textProperty.Changed += TextPropertyOnChanged;
                _textBoxName.PropertyChanged += TextBoxNameOnPropertyChanged;
                mainStackPanel.Children.Add(flexibleStackPanel);

                mainStackPanel.Children.Add(_errorTextBlock = new TextBlock() {Margin = new Vector4F(5), Text = "This is a long message displaying a long error.", HorizontalAlignment = HorizontalAlignment.Stretch, Foreground = Color.Red, WrapText = true});

                var buttonPanel = new StackPanel() {Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Stretch, Height = 30, Margin = new Vector4F(10)};
                {
                    StackPanel panel = new StackPanel() {Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right};

                    panel.Children.Add(_okButton = new TextButton() {Width = 95, Height = 25, Text = "OK", Margin = new Vector4F(5), IsDefault = true});
                    _okButton.Click += OkButtonOnClick;

                    panel.Children.Add(_cancelButton = new TextButton() {Width = 95, Height = 25, Text = "Cancel", Margin = new Vector4F(5), IsCancel = true});
                    _cancelButton.Click += CancelButtonOnClick;

                    buttonPanel.Children.Add(panel);
                }
                mainStackPanel.Children.Add(buttonPanel);
            }

            Content = mainStackPanel;

            _listView.Select(0);

            _textBoxName.SelectAll();
            //CenterWindow();

            base.OnLoad();
        }



        private void TextPropertyOnChanged(object sender, GamePropertyEventArgs<string> gamePropertyEventArgs)
        {
            _errorTextBlock.Text = FileCreationHelper.PerformChecks(gamePropertyEventArgs.NewValue, _selectedCreator, _parentFolder);
            _okButton.IsEnabled = String.IsNullOrEmpty(_errorTextBlock.Text);
            _isSuggestion = false;
        }



        private void TextBoxNameOnPropertyChanged(object sender, GamePropertyEventArgs gamePropertyEventArgs)
        {
            if (gamePropertyEventArgs.Property.Name == "Text")
            {
            }
        }



        private void OnItemSelectionChanged(object sender, EventArgs e)
        {
            ListViewItem selection = (ListViewItem) sender;
            _selectedCreator = (IFileCreator) selection.Content.UserData;

            if (String.IsNullOrEmpty(_textBoxName.Text) || _isSuggestion)
            {
                _textBoxName.Text = GetSuggestion(_selectedCreator.ItemName, _selectedCreator.Extension);
                _isSuggestion = true;
            }
        }



        private void CancelButtonOnClick(object sender, EventArgs eventArgs)
        {
            Cancel();
        }



        private void OkButtonOnClick(object sender, EventArgs eventArgs)
        {
            //A new folder with the project name will be created within the specified location.
            /*String projectFolder = _textBoxLocation.Text + "\\" + _textBoxName.Text;

            //create the project file
            CreatedProject = Project.CreateProject(projectFolder, _textBoxName.Text);*/

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(_textBoxName.Text);

            CreatedItem = new FileItem(fileNameWithoutExtension, _selectedCreator.Extension, _parentFolder);

            _selectedCreator.CreatePhysicalFile(CreatedItem);

            CreatedItem.Guid = _selectedCreator.GetGuid(CreatedItem) ?? Guid.NewGuid();

            _parentFolder.AddItem(CreatedItem);

            Accept();
        }



        /*private void BrowseButtonOnClick(object sender, EventArgs eventArgs)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            
            if (folderBrowserDialog.ShowCrossDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _textBoxLocation.Text = folderBrowserDialog.SelectedPath;
            }
        }*/



        private string GetSuggestion(string itemName, string extension)
        {
            int index = 1;
            while (_parentFolder.ContainsFileWithName(itemName + index + extension))
                index++;

            return itemName + index + extension;
        }
    }
}