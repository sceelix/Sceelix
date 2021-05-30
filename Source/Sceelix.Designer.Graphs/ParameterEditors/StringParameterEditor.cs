using System;
using System.Linq;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Designer.Graphs.Annotations;
using Sceelix.Designer.GUI;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.ProjectExplorer.Management;


namespace Sceelix.Designer.Graphs.ParameterEditors
{
    [ParameterEditor(typeof(StringParameterInfo))]
    public class StringParameterEditor : PrimitiveParameterEditor<StringParameterInfo>
    {
        public override UIControl CreateControl(StringParameterInfo info, FileItem fileItem, Action onChanged)
        {
            var textBox = new ExtendedTextBox
            {
                Text = info.FixedValue,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var property = textBox.Properties.Get<bool>("IsFocused");
            property.Changed += delegate
            {
                if (!textBox.IsFocused)
                {
                    info.FixedValue = textBox.Text;
                    onChanged();
                }
            };

            if (info.MaxLength > 0)
                textBox.MaxLength = info.MaxLength;

            textBox.IsPassword = info.IsPassword;

            return textBox;
        }



        public override UIControl CreateInspectorContent(ParameterEditorTreeViewItem parameterEditorTreeViewItem)
        {
            LayoutControl layoutControl = (LayoutControl) base.CreateInspectorContent(parameterEditorTreeViewItem);

            StringParameterInfo parameterInfo = (StringParameterInfo) parameterEditorTreeViewItem.ParameterInfo;

            var minimumSpinControl = (IntSpinControl) layoutControl.Add("Max. Length", new IntSpinControl() {Value = parameterInfo.MaxLength, ToolTip = new ToolTipControl("The maximum length of the string.") })[1];
            var gameProperty = minimumSpinControl.Properties.Get<int>("Value");
            gameProperty.Changed += (sender, args) => parameterInfo.MaxLength = minimumSpinControl.Value;

            layoutControl.Add("Is Password", new CheckBox() {IsChecked = parameterInfo.IsPassword, Margin = new Vector4F(0, 5, 0, 0), Height = 15, ToolTip = new ToolTipControl("Indicates if the contents should be hidden (and replaced by an asterisk). The value itself will be encrypted on save.") });
            var property3 = layoutControl.Last()[1].Properties.Get<bool>("IsChecked");
            property3.Changed += (sender, args) => parameterInfo.IsPassword = args.NewValue;

            return layoutControl;
        }
    }
}