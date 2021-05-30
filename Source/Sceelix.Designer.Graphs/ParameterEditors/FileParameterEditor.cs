using System;
using System.Linq;
using System.Windows.Forms;
using DigitalRune.Game;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Storages;
using Sceelix.Core.Parameters;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Core.Utils;
using Sceelix.Designer.Graphs.Annotations;
using Sceelix.Designer.Graphs.GUI.Model;
using Sceelix.Designer.Graphs.ParameterEditors.Windows;
using Sceelix.Designer.GUI;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;
using Sceelix.Extensions;
using Sceelix.Helpers;
using ContextMenu = DigitalRune.Game.UI.Controls.ContextMenu;
using HorizontalAlignment = DigitalRune.Game.UI.HorizontalAlignment;
using MenuItem = DigitalRune.Game.UI.Controls.MenuItem;
using Orientation = DigitalRune.Game.UI.Orientation;
using TextBox = DigitalRune.Game.UI.Controls.TextBox;

namespace Sceelix.Designer.Graphs.ParameterEditors
{
    [ParameterEditor(typeof(FileParameterInfo))]
    public class FileParameterEditor : PrimitiveParameterEditor<FileParameterInfo>
    {
        private IServiceLocator _services;


        public override void Initialize(IServiceLocator services)
        {
            base.Initialize(services);

            _services = services;
        }


        public override UIControl CreateControl(FileParameterInfo info, FileItem fileItem, Action onChanged)
        {
            var stackPanel = new FlexibleStackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            
            //add the textbox that, when changed, will affect the parameterinfo value
            var textBox = new ExtendedTextBox { Text = info.FixedValue, HorizontalAlignment = HorizontalAlignment.Stretch,};
            var gameProperty = textBox.Properties.Get<String>("Text");
            gameProperty.Changed += delegate
            {
                info.FixedValue = textBox.Text;

                var projectItem = fileItem.Project.GetSubProjectItem(info.FixedValue);
                if (projectItem != null)
                    info.FileGuid = projectItem.Guid.ToString();

                onChanged();
            };
            stackPanel.Children.Add(textBox);
            

            //add a textbutton that will open a savefiledialog that will add text to the 
            var button = new TextButton() {Text = "...", ToolTip = new ToolTipControl(info.IOOperation + " Location.") };
            button.Click += delegate
            {
                //for saving operations, one can only select for external, system locations
                if (info.IOOperation == IOOperation.Save)
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = FilterString(info);

                    if (!String.IsNullOrWhiteSpace(textBox.Text))
                    {
                        saveFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(textBox.Text);
                        saveFileDialog.FileName = System.IO.Path.GetFileName(textBox.Text);
                    }

                    if (saveFileDialog.ShowCrossDialog() == DialogResult.OK)
                        textBox.Text = PathHelper.ToUniversalPath(saveFileDialog.FileName);
                }
                //for loading, one can choose from the project or from system locations
                else if (info.IOOperation == IOOperation.Load)
                {
                    ContextMenu menu = new ContextMenu();
                    var projectItem = new MenuItem {Content = new TextBlock() {Text = "Project"}};
                    var systemItem = new MenuItem {Content = new TextBlock() {Text = "System"}};
                    menu.Items.Add(projectItem);
                    menu.Items.Add(systemItem);

                    projectItem.Click += delegate(object sender, EventArgs args)
                    {
                        FileSelectorWindow window = new FileSelectorWindow(_services, fileItem.Project, info.ExtensionFilter.Where(x => !String.IsNullOrWhiteSpace(x)).Select(x => new FileExtensionInfo(x)).ToArray()) {SelectedPath = textBox.Text};
                        window.Closed += delegate
                        {
                            if (!String.IsNullOrWhiteSpace(window.SelectedPath))
                                textBox.Text = PathHelper.ToUniversalPath(window.SelectedPath);
                        };
                        window.Show(button);
                    };

                    systemItem.Click += delegate(object sender, EventArgs args)
                    {
                        if (info.IOOperation == IOOperation.Load)
                        {
                            OpenFileDialog openFileDialog = new OpenFileDialog();
                            openFileDialog.Filter = FilterString(info);

                            if (!String.IsNullOrWhiteSpace(textBox.Text))
                            {
                                openFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(textBox.Text);
                                openFileDialog.FileName = System.IO.Path.GetFileName(textBox.Text);
                            }

                            if (openFileDialog.ShowCrossDialog() == DialogResult.OK)
                                textBox.Text = PathHelper.ToUniversalPath(openFileDialog.FileName);
                        }
                    };

                    menu.Open(button, button.InputService.MousePosition);
                }
            };
            stackPanel.Children.Add(button);

            var relativeButton = new TextButton() { Text = "R", ToolTip = new ToolTipControl(" Convert to relative path.") };
            relativeButton.Click += delegate
            {
                if (Path.IsPathRooted(textBox.Text))
                {
                    string relativePath = PathHelper.MakeRelative(textBox.Text, fileItem.Project.FolderPath + "/");
                    textBox.Text = relativePath;
                }

            };
            stackPanel.Children.Add(relativeButton);

            return stackPanel;
        }

        public String FilterString(FileParameterInfo info)
        {
            var filesExtensionInfos = info.ExtensionFilter.Where(x => !String.IsNullOrWhiteSpace(x)).Select(x => new FileExtensionInfo(x)).ToList();
            var filterString = String.Empty;

            if (filesExtensionInfos.Any())
            {
                String joinedFileExtensions = String.Join(";", filesExtensionInfos.SelectMany(x => x.Extensions));
                filterString = "Supported Files (" + joinedFileExtensions + ")|" + joinedFileExtensions + "|";

                //individual extensions
                foreach (FileExtensionInfo filter in filesExtensionInfos)
                {
                    filterString += filter.Title + "|" + String.Join(";", filter.Extensions) + "|";
                }
            }

            return filterString + "All files (*.*)|*.*";
        }

        public override UIControl CreateInspectorContent(ParameterEditorTreeViewItem parameterEditorTreeViewItem)
        {
            LayoutControl layoutControl = (LayoutControl) base.CreateInspectorContent(parameterEditorTreeViewItem);

            FileParameterInfo parameterInfo = (FileParameterInfo) parameterEditorTreeViewItem.ParameterInfo;

            var extensionsControl = (TextBox) layoutControl.Add("Extensions:", new ExtendedTextBox() {Text = String.Join("\n", parameterInfo.ExtensionFilter), ToolTip = new ToolTipControl("List of file extensions and/or descriptions (Ex. \".txt\", \"Image|.jpg\", \"Image file |.bmp, .png\"), one per line."), MinLines = 5, MaxLines = 5})[1];
            var text = extensionsControl.Properties.Get<String>("Text");
            text.Changed += delegate(object sender, GamePropertyEventArgs<string> args) { parameterInfo.ExtensionFilter = args.NewValue.Split('\n'); };

            /*var dropDownButtonScope = (DropDownButton)layoutControl.Add("Search Scope:",new DropDownButton())[1];
            dropDownButtonScope.Items.AddRange(Enum.GetNames(typeof (SearchScope)));
            dropDownButtonScope.SelectedIndex = dropDownButtonScope.Items.IndexOf(Enum.GetName(typeof (SearchScope),parameterInfo.SearchScope));
            var scopeIndex = dropDownButtonScope.Properties.Get<int>("SelectedIndex");
            scopeIndex.Changed += delegate(object sender, GamePropertyEventArgs<int> args)
            {
                parameterInfo.SearchScope = (SearchScope)Enum.Parse(typeof(SearchScope),(String)dropDownButtonScope.Items[args.NewValue]);
            };*/

            var dropDownButtonOperation = (DropDownButton) layoutControl.Add("Operation:", new DropDownButton())[1];
            dropDownButtonOperation.Items.AddRange(Enum.GetNames(typeof(IOOperation)));
            dropDownButtonOperation.SelectedIndex = dropDownButtonOperation.Items.IndexOf(Enum.GetName(typeof(IOOperation), parameterInfo.IOOperation));
            var operationIndex = dropDownButtonOperation.Properties.Get<int>("SelectedIndex");
            operationIndex.Changed += delegate(object sender, GamePropertyEventArgs<int> args) { parameterInfo.IOOperation = (IOOperation) Enum.Parse(typeof(IOOperation), (String) dropDownButtonOperation.Items[args.NewValue]); };

            return layoutControl;
        }
    }
}