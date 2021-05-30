using System;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Designer.Graphs.Annotations;
using Sceelix.Designer.Graphs.ParameterEditors;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Mathematics.Parameters.Infos;
using Vector2D = Sceelix.Mathematics.Data.Vector2D;

namespace Sceelix.Designer.Actors.ParameterEditors
{
    [ParameterEditor(typeof(Vector2DParameterInfo))]
    public class Vector2DParameterEditor : PrimitiveParameterEditor<Vector2DParameterInfo>
    {


        public override UIControl CreateControl(Vector2DParameterInfo argument, FileItem fileItem, Action onChanged)
        {
            EqualStackPanel horizontalStackPanel = new EqualStackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            FloatSpinControl xSpin, ySpin;

            FlexibleStackPanel panelX = new FlexibleStackPanel() {Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Stretch}; //CreatePanel("X",new Vector4F(0,5,5,0))
            panelX.Children.Add(new TextBlock {Text = "X", Margin = new Vector4F(0, 3, 5, 0)});
            panelX.Children.Add(xSpin = new FloatSpinControl {Value = argument.FixedValue.X, HorizontalAlignment = HorizontalAlignment.Stretch});

            FlexibleStackPanel panelY = new FlexibleStackPanel() {Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Stretch};
            panelY.Children.Add(new TextBlock {Text = "Y", Margin = new Vector4F(5, 3, 5, 0)});
            panelY.Children.Add(ySpin = new FloatSpinControl {Value = argument.FixedValue.Y, HorizontalAlignment = HorizontalAlignment.Stretch});

            EventHandler<GamePropertyEventArgs<float>> changedValue = delegate
            {
                argument.FixedValue = new Vector2D(xSpin.Value, ySpin.Value);
                onChanged();
            };

            var xSpinProperty = xSpin.Properties.Get<float>("Value");
            xSpinProperty.Changed += changedValue;

            var ySpinProperty = ySpin.Properties.Get<float>("Value");
            ySpinProperty.Changed += changedValue;

            horizontalStackPanel.Children.Add(panelX);
            horizontalStackPanel.Children.Add(panelY);

            return horizontalStackPanel;
        }
    }
}