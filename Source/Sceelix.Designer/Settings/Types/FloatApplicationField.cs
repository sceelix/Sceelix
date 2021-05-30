using System;
using DigitalRune.Game;
using DigitalRune.Game.UI.Controls;
using Sceelix.Designer.GUI.Controls;

namespace Sceelix.Designer.Settings.Types
{
    public class FloatApplicationField : PrimitiveApplicationField<float>
    {
        public FloatApplicationField(float defaultValue)
            : base(defaultValue)
        {
            Increment = 0.1f;
        }



        public float? Mininum
        {
            get;
            set;
        }



        public float? Maximum
        {
            get;
            set;
        }



        public float Increment
        {
            get;
            set;
        }



        public override UIControl GetControl()
        {
            var spinNumericControl = new FloatSpinControl();
            spinNumericControl.Increment = Increment;

            if (Mininum.HasValue)
                spinNumericControl.MinValue = Mininum.Value;

            if (Maximum.HasValue)
                spinNumericControl.MaxValue = Maximum.Value;

            spinNumericControl.Value = ProposedValue != null ? (float) ProposedValue : Value;
            var gameProperty = spinNumericControl.Properties.Get<float>("Value");
            gameProperty.Changed += delegate(object sender, GamePropertyEventArgs<float> args) { this.ProposedValue = args.NewValue; };

            return spinNumericControl;
        }
    }
}