using DigitalRune.Game;
using DigitalRune.Game.UI.Controls;
using Sceelix.Designer.GUI.Controls;

namespace Sceelix.Designer.Settings.Types
{
    public class DoubleApplicationField : PrimitiveApplicationField<double>
    {
        public DoubleApplicationField(double defaultValue)
            : base(defaultValue)
        {
            Increment = 0.1;
        }



        public double? Mininum
        {
            get;
            set;
        }



        public double? Maximum
        {
            get;
            set;
        }



        public double Increment
        {
            get;
            set;
        }



        public override UIControl GetControl()
        {
            var spinNumericControl = new DoubleSpinControl();
            spinNumericControl.Increment = Increment;

            if (Mininum.HasValue)
                spinNumericControl.MinValue = Mininum.Value;

            if (Maximum.HasValue)
                spinNumericControl.MaxValue = Maximum.Value;

            spinNumericControl.Value = ProposedValue != null ? (double) ProposedValue : Value;
            var gameProperty = spinNumericControl.Properties.Get<double>("Value");
            gameProperty.Changed += delegate(object sender, GamePropertyEventArgs<double> args) { this.ProposedValue = args.NewValue; };

            return spinNumericControl;
        }
    }
}