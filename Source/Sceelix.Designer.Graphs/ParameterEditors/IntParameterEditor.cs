using System;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Designer.Graphs.Annotations;
using Sceelix.Designer.GUI;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.ProjectExplorer.Management;

namespace Sceelix.Designer.Graphs.ParameterEditors
{
    [ParameterEditor(typeof(IntParameterInfo))]
    public class IntParameterEditor : PrimitiveParameterEditor<IntParameterInfo>
    {
        
        public override UIControl CreateControl(IntParameterInfo info, FileItem fileItem, Action onChanged)
        {
            var spinNumericControl = new IntSpinControl()
            {
                Increment = info.Increment,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            if (info.MinValue.HasValue)
                spinNumericControl.MinValue = info.MinValue.Value;

            if (info.MaxValue.HasValue)
                spinNumericControl.MaxValue = info.MaxValue.Value;

            spinNumericControl.Value = info.FixedValue;
            var gameProperty = spinNumericControl.Properties.Get<int>("Value");
            gameProperty.Changed += delegate (object sender, GamePropertyEventArgs<int> args)
            {
                info.FixedValue = args.NewValue;
                onChanged();
            };

            return spinNumericControl;
        }

        



        public override UIControl CreateInspectorContent(ParameterEditorTreeViewItem parameterEditorTreeViewItem)
        {
            LayoutControl layoutControl = (LayoutControl) base.CreateInspectorContent(parameterEditorTreeViewItem);

            IntParameterInfo parameterInfo = (IntParameterInfo) parameterEditorTreeViewItem.ParameterInfo;

            var minControl = new IntSpinControl() {Value = parameterInfo.MinValue ?? 0, ToolTip = new ToolTipControl("The minimum value that can be assigned to this parameter.") };
            var minGameProperty = minControl.Properties.Get<int>("Value");
            minGameProperty.Changed += (sender, args) => parameterInfo.MinValue = args.NewValue;
            layoutControl.Add("Minumum Value", new OptionalControl(minControl, parameterInfo.MinValue.HasValue, (checkValue) => parameterInfo.MinValue = checkValue ? minControl.Value : (int?) null));

            var maxControl = new IntSpinControl() {Value = parameterInfo.MaxValue ?? 0, ToolTip = new ToolTipControl("The maximum value that can be assigned to this parameter.") };
            var maxGameProperty = maxControl.Properties.Get<int>("Value");
            maxGameProperty.Changed += (sender, args) => parameterInfo.MaxValue = args.NewValue;
            layoutControl.Add("Maximum Value", new OptionalControl(maxControl, parameterInfo.MaxValue.HasValue, (checkValue) => parameterInfo.MaxValue = checkValue ? maxControl.Value : (int?) null));


            /*var minimumSpinControl = (IntSpinControl)layoutControl.Add("Minumum Value", new IntSpinControl() { Value = parameterInfo.MinValue})[1];
            minimumSpinControl.ValueChanged += (sender, args) => parameterInfo.MinValue = minimumSpinControl.Value;

            var maximumSpinControl = (IntSpinControl)layoutControl.Add("Maximum Value", new IntSpinControl() { Value = parameterInfo.MaxValue })[1];
            maximumSpinControl.ValueChanged += (sender, args) => parameterInfo.MaxValue = maximumSpinControl.Value;
            */
            var incrementSpinControl = (IntSpinControl) layoutControl.Add("Increment", new IntSpinControl() {Value = parameterInfo.Increment})[1];
            var gameProperty = incrementSpinControl.Properties.Get<int>("Value");
            gameProperty.Changed += (sender, args) => parameterInfo.Increment = args.NewValue;

            return layoutControl;
        }
    }
}