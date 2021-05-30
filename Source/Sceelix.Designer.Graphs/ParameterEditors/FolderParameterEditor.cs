using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using DigitalRune.Game;
using DigitalRune.Game.UI.Controls;
using Sceelix.Core.Parameters;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Designer.Graphs.Annotations;
using Sceelix.Designer.Graphs.GUI.Model;
using Sceelix.Designer.Graphs.ParameterEditors.Windows;
using Sceelix.Designer.GUI;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Utils;
using Sceelix.Extensions;
using Sceelix.Helpers;
using ContextMenu = DigitalRune.Game.UI.Controls.ContextMenu;
using Orientation = DigitalRune.Game.UI.Orientation;
using TextBox = DigitalRune.Game.UI.Controls.TextBox;
using HorizontalAlignment = DigitalRune.Game.UI.HorizontalAlignment;
using MenuItem = DigitalRune.Game.UI.Controls.MenuItem;

namespace Sceelix.Designer.Graphs.ParameterEditors
{
    [ParameterEditor(typeof(FolderParameterInfo))]
    public class FolderParameterEditor : PrimitiveParameterEditor<FolderParameterInfo>
    {
        public override UIControl CreateControl(FolderParameterInfo info, FileItem fileItem, Action onChanged)
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
                    info.FolderGuid = projectItem.Guid.ToString();

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
                    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

                    if (folderBrowserDialog.ShowCrossDialog() == DialogResult.OK)
                        textBox.Text = PathHelper.ToUniversalPath(folderBrowserDialog.SelectedPath);
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
                        FolderSelectorWindow window = new FolderSelectorWindow(fileItem.Project)
                        {
                            SelectedPath = textBox.Text
                        };
                        window.Closed += delegate
                        {
                            if (!String.IsNullOrWhiteSpace(window.SelectedPath))
                                textBox.Text = PathHelper.ToUniversalPath(window.SelectedPath);
                        };
                        window.Show(button);
                    };

                    systemItem.Click += delegate(object sender, EventArgs args)
                    {
                        FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

                        if (folderBrowserDialog.ShowCrossDialog() == DialogResult.OK)
                            textBox.Text = PathHelper.ToUniversalPath(folderBrowserDialog.SelectedPath);
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



        public override UIControl CreateInspectorContent(ParameterEditorTreeViewItem parameterEditorTreeViewItem)
        {
            LayoutControl layoutControl = (LayoutControl) base.CreateInspectorContent(parameterEditorTreeViewItem);

            FolderParameterInfo parameterInfo = (FolderParameterInfo) parameterEditorTreeViewItem.ParameterInfo;

            var dropDownButtonOperation = (DropDownButton) layoutControl.Add("Operation:", new DropDownButton())[1];
            dropDownButtonOperation.Items.AddRange(Enum.GetNames(typeof(IOOperation)));
            dropDownButtonOperation.SelectedIndex = dropDownButtonOperation.Items.IndexOf(Enum.GetName(typeof(IOOperation), parameterInfo.IOOperation));
            var operationIndex = dropDownButtonOperation.Properties.Get<int>("SelectedIndex");
            operationIndex.Changed += delegate(object sender, GamePropertyEventArgs<int> args) { parameterInfo.IOOperation = (IOOperation) Enum.Parse(typeof(IOOperation), (String) dropDownButtonOperation.Items[args.NewValue]); };

            return layoutControl;
        }
    }
}