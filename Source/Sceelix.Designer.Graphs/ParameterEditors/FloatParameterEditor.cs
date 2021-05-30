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
    [ParameterEditor(typeof(FloatParameterInfo))]
    public class FloatParameterEditor : PrimitiveParameterEditor<FloatParameterInfo>
    {

        public override UIControl CreateControl(FloatParameterInfo info, FileItem fileItem, Action onChanged)
        {
            var spinNumericControl = new FloatSpinControl
            {
                Increment = info.Increment,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            if (info.DecimalDigits.HasValue)
                spinNumericControl.DecimalDigits = info.DecimalDigits.Value;

            if (info.MinValue.HasValue)
                spinNumericControl.MinValue = info.MinValue.Value;

            if (info.MaxValue.HasValue)
                spinNumericControl.MaxValue = info.MaxValue.Value;

            spinNumericControl.Value = info.FixedValue;
            var gameProperty = spinNumericControl.Properties.Get<float>("Value");
            gameProperty.Changed += delegate (object sender, GamePropertyEventArgs<float> args)
            {
                info.FixedValue = args.NewValue;

                onChanged();
            };

            return spinNumericControl;
        }



        public override bool SetValue(ArgumentTreeViewItem item, object value)
        {
            var floatSpinControl = (FloatSpinControl) item.Control;
            var newValue = (float) value;
            if (Math.Abs(floatSpinControl.Value - newValue) > float.Epsilon)
            {
                floatSpinControl.Value = newValue;
                return true;
            }

            return false;
        }



        public override UIControl CreateInspectorContent(ParameterEditorTreeViewItem parameterEditorTreeViewItem)
        {
            LayoutControl layoutControl = (LayoutControl) base.CreateInspectorContent(parameterEditorTreeViewItem);

            FloatParameterInfo parameterInfo = (FloatParameterInfo) parameterEditorTreeViewItem.ParameterInfo;

            var minControl = new FloatSpinControl() {Value = parameterInfo.MinValue ?? 0, ToolTip = new ToolTipControl("The minimum value that can be assigned to this parameter.") };
            var minGameProperty = minControl.Properties.Get<float>("Value");
            minGameProperty.Changed += (sender, args) => parameterInfo.MinValue = args.NewValue;
            layoutControl.Add("Minumum Value", new OptionalControl(minControl, parameterInfo.MinValue.HasValue, (checkValue) => parameterInfo.MinValue = checkValue ? minControl.Value : (float?) null));

            var maxControl = new FloatSpinControl() {Value = parameterInfo.MaxValue ?? 0, ToolTip = new ToolTipControl("The maximum value that can be assigned to this parameter.") };
            var maxGameProperty = maxControl.Properties.Get<float>("Value");
            maxGameProperty.Changed += (sender, args) => parameterInfo.MaxValue = args.NewValue;
            layoutControl.Add("Maximum Value", new OptionalControl(maxControl, parameterInfo.MaxValue.HasValue, (checkValue) => parameterInfo.MaxValue = checkValue ? maxControl.Value : (float?) null));


            /*var minimumSpinControl = (FloatSpinControl)layoutControl.Add("Minumum Value", new FloatSpinControl() { Value = parameterInfo.MinValue })[1];
            minimumSpinControl.ValueChanged += (sender, args) => parameterInfo.MinValue = minimumSpinControl.Value;

            var maxmimumSpinControl = (FloatSpinControl)layoutControl.Add("Maximum Value", new FloatSpinControl() { Value = parameterInfo.MaxValue })[1];
            maxmimumSpinControl.ValueChanged += (sender, args) => parameterInfo.MaxValue = maxmimumSpinControl.Value;*/

            var incrementSpinControl = (FloatSpinControl) layoutControl.Add("Increment", new FloatSpinControl() {Value = parameterInfo.Increment})[1];
            var incrementProperty = incrementSpinControl.Properties.Get<float>("Value");
            incrementProperty.Changed += (sender, args) => parameterInfo.Increment = args.NewValue;

            var decimalDigitsControl = new IntSpinControl() { Value = parameterInfo.DecimalDigits ?? 0, MinValue = 0 };
            var decimalDigitsProperty = decimalDigitsControl.Properties.Get<int>("Value");
            decimalDigitsProperty.Changed += (sender, args) => parameterInfo.DecimalDigits = args.NewValue;
            layoutControl.Add("Decimal Digits", new OptionalControl(decimalDigitsControl, parameterInfo.DecimalDigits.HasValue, (checkValue) => parameterInfo.DecimalDigits = checkValue ? decimalDigitsControl.Value : (int?)null));

            return layoutControl;
        }
    }
}