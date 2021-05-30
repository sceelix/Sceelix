using System;
using System.Linq;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Conversion;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Designer.Graphs.Annotations;
using Sceelix.Designer.Graphs.GUI.Model;
using Sceelix.Designer.Graphs.ParameterEditors;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Mathematics.Parameters.Infos;
using Vector3D = Sceelix.Mathematics.Data.Vector3D;

namespace Sceelix.Designer.Actors.ParameterEditors
{
    [ParameterEditor(typeof(Vector3DParameterInfo))]
    public class Vector3DParameterEditor : PrimitiveParameterEditor<Vector3DParameterInfo>
    {
        public override UIControl CreateControl(Vector3DParameterInfo argument, FileItem fileItem, Action onChanged)
        {
            EqualStackPanel horizontalStackPanel = new EqualStackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            FloatSpinControl xSpin, ySpin, zSpin;

            FlexibleStackPanel panelX = new FlexibleStackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Stretch }; //CreatePanel("X",new Vector4F(0,5,5,0))
            panelX.Children.Add(new TextBlock { Text = "X", Margin = new Vector4F(0, 3, 5, 0) });
            panelX.Children.Add(xSpin = new FloatSpinControl { Value = argument.FixedValue.X, HorizontalAlignment = HorizontalAlignment.Stretch });

            FlexibleStackPanel panelY = new FlexibleStackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Stretch };
            panelY.Children.Add(new TextBlock { Text = "Y", Margin = new Vector4F(5, 3, 5, 0) });
            panelY.Children.Add(ySpin = new FloatSpinControl { Value = argument.FixedValue.Y, HorizontalAlignment = HorizontalAlignment.Stretch });

            FlexibleStackPanel panelZ = new FlexibleStackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Stretch };
            panelZ.Children.Add(new TextBlock() { Text = "Z", Margin = new Vector4F(5, 3, 5, 0) });
            panelZ.Children.Add(zSpin = new FloatSpinControl() { Value = argument.FixedValue.Z, HorizontalAlignment = HorizontalAlignment.Stretch });

            EventHandler<GamePropertyEventArgs<float>> changedValue = delegate
            {
                argument.FixedValue = new Vector3D(xSpin.Value, ySpin.Value, zSpin.Value);
                onChanged();
            };

            var xSpinProperty = xSpin.Properties.Get<float>("Value");
            xSpinProperty.Changed += changedValue;

            var ySpinProperty = ySpin.Properties.Get<float>("Value");
            ySpinProperty.Changed += changedValue;

            var zSpinProperty = zSpin.Properties.Get<float>("Value");
            zSpinProperty.Changed += changedValue;

            horizontalStackPanel.Children.Add(panelX);
            horizontalStackPanel.Children.Add(panelY);
            horizontalStackPanel.Children.Add(panelZ);

            return horizontalStackPanel;
        }



        public override bool SetValue(ArgumentTreeViewItem item, object value)
        {
            var equalStackPanel = (EqualStackPanel) item.Control;

            var convertedValue = ConvertHelper.Convert<Vector3D>(value);

            var panelX = (FlexibleStackPanel) equalStackPanel.Children[0];
            var panelY = (FlexibleStackPanel) equalStackPanel.Children[1];
            var panelZ = (FlexibleStackPanel) equalStackPanel.Children[2];

            var floatControlX = (FloatSpinControl) panelX.Children.Last();
            var floatControlY = (FloatSpinControl) panelY.Children.Last();
            var floatControlZ = (FloatSpinControl) panelZ.Children.Last();

            var valueChanged = false;

            if (Math.Abs(floatControlX.Value - convertedValue.X) > float.Epsilon)
            {
                floatControlX.Value = convertedValue.X;
                valueChanged = true;
            }

            if (Math.Abs(floatControlY.Value - convertedValue.Y) > float.Epsilon)
            {
                floatControlY.Value = convertedValue.Y;
                valueChanged = true;
            }

            if (Math.Abs(floatControlZ.Value - convertedValue.Z) > float.Epsilon)
            {
                floatControlZ.Value = convertedValue.Z;
                valueChanged = true;
            }


            return valueChanged;
        }
    }
}