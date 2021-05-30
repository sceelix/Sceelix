using System;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Designer.Graphs.Annotations;
using Sceelix.Designer.ProjectExplorer.Management;

namespace Sceelix.Designer.Graphs.ParameterEditors
{
    [ParameterEditor(typeof(BoolParameterInfo))]
    public class BoolParameterEditor : PrimitiveParameterEditor<BoolParameterInfo>
    {
        public override UIControl CreateControl(BoolParameterInfo info, FileItem fileItem, Action onChanged)
        {
            var checkBox = new CheckBox();
            checkBox.Margin = new Vector4F(0, 5, 0, 0);
            checkBox.Height = 15;
            checkBox.IsChecked = info.FixedValue;
            checkBox.UserData = info;
            var isCheckedProperty = checkBox.Properties.Get<bool>("IsChecked");
            isCheckedProperty.Changed += delegate
            {
                info.FixedValue = checkBox.IsChecked;

                onChanged();
            };

            return checkBox;
        }
    }
}