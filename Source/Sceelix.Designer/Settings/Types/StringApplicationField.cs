using System;
using DigitalRune.Game;
using DigitalRune.Game.UI.Controls;
using Sceelix.Designer.GUI.Controls;

namespace Sceelix.Designer.Settings.Types
{
    public class StringApplicationField : PrimitiveApplicationField<String>
    {
        public StringApplicationField(string defaultValue)
            : base(defaultValue)
        {
        }



        public int MaxLength
        {
            get;
            set;
        }



        public override UIControl GetControl()
        {
            var textbox = new ExtendedTextBox();

            textbox.Text = ProposedValue != null ? (String) ProposedValue : Value;
            var gameProperty = textbox.Properties.Get<String>("Text");
            gameProperty.Changed += delegate(object sender, GamePropertyEventArgs<String> args) { this.ProposedValue = args.NewValue; };

            return textbox;
        }
    }
}