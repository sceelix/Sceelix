using DigitalRune.Game;
using DigitalRune.Game.UI.Controls;
using Sceelix.Designer.GUI.Controls;

namespace Sceelix.Designer.Settings.Types
{
    public class IntApplicationField : PrimitiveApplicationField<int>
    {
        public IntApplicationField(int defaultValue)
            : base(defaultValue)
        {
            Increment = 1;
        }



        public int? Minimum
        {
            get;
            set;
        }



        public int? Maximum
        {
            get;
            set;
        }



        public int Increment
        {
            get;
            set;
        }



        public override UIControl GetControl()
        {
            var spinNumericControl = new IntSpinControl();
            spinNumericControl.Increment = Increment;

            if (Minimum.HasValue)
                spinNumericControl.MinValue = Minimum.Value;

            if (Maximum.HasValue)
                spinNumericControl.MaxValue = Maximum.Value;

            spinNumericControl.Value = ProposedValue != null ? (int) ProposedValue : Value;
            var gameProperty = spinNumericControl.Properties.Get<int>("Value");
            gameProperty.Changed += delegate(object sender, GamePropertyEventArgs<int> args) { this.ProposedValue = args.NewValue; };

            return spinNumericControl;
        }
    }
}