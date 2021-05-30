using System;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Designer.Graphs.Annotations;
using Sceelix.Designer.GUI;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.ProjectExplorer.Management;

namespace Sceelix.Designer.Graphs.ParameterEditors
{
    [ParameterEditor(typeof(DoubleParameterInfo))]
    public class DoubleParameterEditor : PrimitiveParameterEditor<DoubleParameterInfo>
    {
        public override UIControl CreateControl(DoubleParameterInfo info, FileItem fileItem, Action onChanged)
        {
            var spinNumericControl = new DoubleSpinControl
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
            var gameProperty = spinNumericControl.Properties.Get<double>("Value");
            gameProperty.Changed += delegate(object sender, GamePropertyEventArgs<double> args)
            {
                info.FixedValue = args.NewValue;

                onChanged();
            };

            return spinNumericControl;
        }



        public override UIControl CreateInspectorContent(ParameterEditorTreeViewItem parameterEditorTreeViewItem)
        {
            LayoutControl layoutControl = (LayoutControl) base.CreateInspectorContent(parameterEditorTreeViewItem);

            DoubleParameterInfo parameterInfo = (DoubleParameterInfo) parameterEditorTreeViewItem.ParameterInfo;


            //in this case, the 
            var minControl = new DoubleSpinControl() {Value = parameterInfo.MinValue ?? 0, ToolTip = new ToolTipControl("The minimum value that can be assigned to this parameter.") };
            
            var minGameProperty = minControl.Properties.Get<double>("Value");
            minGameProperty.Changed += (sender, args) => parameterInfo.MinValue = args.NewValue;
            layoutControl.Add("Minumum Value", new OptionalControl(minControl, parameterInfo.MinValue.HasValue, (checkValue) => parameterInfo.MinValue = checkValue ? minControl.Value : (double?) null));

            var maxControl = new DoubleSpinControl() {Value = parameterInfo.MaxValue ?? 0, ToolTip = new ToolTipControl("The maximum value that can be assigned to this parameter.") };
            
            var maxGameProperty = maxControl.Properties.Get<double>("Value");
            maxGameProperty.Changed += (sender, args) => parameterInfo.MaxValue = args.NewValue;
            layoutControl.Add("Maximum Value", new OptionalControl(maxControl, parameterInfo.MaxValue.HasValue, (checkValue) => parameterInfo.MaxValue = checkValue ? maxControl.Value : (double?) null));

            /* 
         minimumSpinControl.ValueChanged += (sender, args) => parameterInfo.MinValue = minimumSpinControl.Value;

         var maxmimumSpinControl = (DoubleSpinControl)layoutControl.Add("Maximum Value", new DoubleSpinControl() { Value = parameterInfo.MaxValue })[1];
         maxmimumSpinControl.ValueChanged += (sender, args) => parameterInfo.MaxValue = maxmimumSpinControl.Value;*/

            var incrementSpinControl = (DoubleSpinControl) layoutControl.Add("Increment", new DoubleSpinControl() {Value = parameterInfo.Increment})[1];
            var incrementProperty = incrementSpinControl.Properties.Get<double>("Value");
            incrementProperty.Changed += (sender, args) => parameterInfo.Increment = args.NewValue;

            var decimalDigitsControl = new IntSpinControl() { Value = parameterInfo.DecimalDigits ?? 0, MinValue = 0 };
            var decimalDigitsProperty = decimalDigitsControl.Properties.Get<int>("Value");
            decimalDigitsProperty.Changed += (sender, args) => parameterInfo.DecimalDigits = args.NewValue;
            layoutControl.Add("Decimal Digits", new OptionalControl(decimalDigitsControl, parameterInfo.DecimalDigits.HasValue, (checkValue) => parameterInfo.DecimalDigits = checkValue ? decimalDigitsControl.Value : (int?)null));

            //decimalDigitsControl.ValueChanged += (sender, args) => parameterInfo.MinDecimalDigits = decimalDigitsControl.Value;

            return layoutControl;
        }
    }
    
}