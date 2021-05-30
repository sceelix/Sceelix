using System;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using Microsoft.Xna.Framework;
using Sceelix.Designer.Actors.Utils;
using Sceelix.Designer.Graphs.Annotations;
using Sceelix.Designer.Graphs.ParameterEditors;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Mathematics.Parameters.Infos;

namespace Sceelix.Designer.Actors.ParameterEditors
{
    [ParameterEditor(typeof(ColorParameterInfo))]
    public class ColorParameterEditor : PrimitiveParameterEditor<ColorParameterInfo>
    {
        public override UIControl CreateControl(ColorParameterInfo argument, FileItem fileItem, Action onChanged)
        {
            var colorPickerControl = new ColorPickerControl()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            colorPickerControl.SelectedColor = argument.FixedValue.ToXnaColor();
            var property = colorPickerControl.Properties.Get<Color>("SelectedColor");
            property.Changed += delegate (object sender, GamePropertyEventArgs<Color> args)
            {
                argument.FixedValue = args.NewValue.ToSceelixColor();
                onChanged();
            };

            return colorPickerControl;
        }
    }
}