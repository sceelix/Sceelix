using System;
using DigitalRune.Game;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;

namespace Sceelix.Designer.Settings.Types
{
    public class BoolApplicationField : PrimitiveApplicationField<bool>
    {
        public BoolApplicationField(bool defaultValue)
            : base(defaultValue)
        {
        }



        public override UIControl GetControl()
        {
            CheckBox box = new CheckBox() {Margin = new Vector4F(0, 3, 0, 0), Height = 15};
            box.IsChecked = ProposedValue != null ? (bool) ProposedValue : Value;
            var gameProperty = box.Properties.Get<bool>("IsChecked");
            gameProperty.Changed += delegate(object sender, GamePropertyEventArgs<bool> args) { this.ProposedValue = args.NewValue; };

            return box;
        }
    }
}