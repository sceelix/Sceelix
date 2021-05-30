using System;
using System.Linq;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Designer.Graphs.Annotations;
using Sceelix.Designer.GUI;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Extensions;

namespace Sceelix.Designer.Graphs.ParameterEditors
{
    [ParameterEditor(typeof(ChoiceParameterInfo))]
    public class ChoiceParameterEditor : PrimitiveParameterEditor<ChoiceParameterInfo>
    {
        public override UIControl CreateControl(ChoiceParameterInfo parameterInfo, FileItem fileItem, Action onChanged)
        {
            var downButton = new DropDownButton();
            downButton.Items.AddRange(parameterInfo.Choices.Select(x => String.IsNullOrEmpty(x) ? " " : x));
            downButton.SelectedIndex = downButton.Items.IndexOf(parameterInfo.FixedValue);
            downButton.UserData = parameterInfo;
            downButton.HorizontalAlignment = HorizontalAlignment.Stretch;

            var selectedIndexProperty = downButton.Properties.Get<int>("SelectedIndex");
            selectedIndexProperty.Changed += delegate
            {
                parameterInfo.FixedValue = downButton.Items[downButton.SelectedIndex].CastTo<String>();
                onChanged();
            };

            return downButton;
        }



        public override UIControl CreateInspectorContent(ParameterEditorTreeViewItem parameterEditorTreeViewItem)
        {
            var layoutControl = (LayoutControl) base.CreateInspectorContent(parameterEditorTreeViewItem);

            ChoiceParameterInfo parameterInfo = (ChoiceParameterInfo) parameterEditorTreeViewItem.ParameterInfo;

            var choiceControl = (TextBox) layoutControl.Add("Choices:", new ExtendedTextBox() {Text = String.Join("\n", parameterInfo.Choices), ToolTip = new ToolTipControl("List the choices here, one per line."), MinLines = 5, MaxLines = 5})[1];
            var text = choiceControl.Properties.Get<String>("Text");
            text.Changed += delegate(object sender, GamePropertyEventArgs<string> args) { parameterInfo.Choices = args.NewValue.Split('\n'); };

            return layoutControl;
        }
    }
}