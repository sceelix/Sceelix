using System;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Designer.GUI.Controls;

namespace Sceelix.Designer.GUI.TreeViewControls
{
    public class OptionalControl : ContentControl
    {
        private readonly UIControl _control;
        private readonly Action<bool> _onCheck;



        public OptionalControl(UIControl control, bool initialState, Action<bool> onCheck)
        {
            _control = control;
            _onCheck = onCheck;

            var optionalCheckBox = new CheckBox();
            optionalCheckBox.IsChecked = initialState;
            optionalCheckBox.Margin = new Vector4F(0, 5, 5, 0);
            optionalCheckBox.Height = 15;
            var property = optionalCheckBox.Properties.Get<bool>("IsChecked");
            property.Changed += PropertyOnChanged;

            control.HorizontalAlignment = HorizontalAlignment.Stretch;
            control.IsEnabled = initialState;

            Content = new FlexibleStackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Children = {optionalCheckBox, control},
            };

            Margin = new Vector4F(0);
        }



        private void PropertyOnChanged(object sender, GamePropertyEventArgs<bool> gamePropertyEventArgs)
        {
            _onCheck(gamePropertyEventArgs.NewValue);

            _control.IsEnabled = gamePropertyEventArgs.NewValue;
        }
    }
}