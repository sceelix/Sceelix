using System;
using System.Linq;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Linq;
using Sceelix.Extensions;

namespace Sceelix.Designer.Settings.Types
{
    public class ChoiceApplicationField : PrimitiveApplicationField<String>
    {
        private string[] _choices;



        public ChoiceApplicationField(string defaultValue, params String[] choices)
            : base(defaultValue)
        {
            _choices = choices;
        }



        public string[] Choices
        {
            get { return _choices; }
            set { _choices = value; }
        }



        public int Index
        {
            get { return Choices.IndexOf(x => x == Value); }
        }



        public override UIControl GetControl()
        {
            var downButton = new DropDownButton();
            downButton.Items.AddRange(Choices.Select(x => String.IsNullOrEmpty(x) ? " " : x));
            downButton.SelectedIndex = downButton.Items.IndexOf(ProposedValue != null ? (String) ProposedValue : Value);
            var selectedIndexProperty = downButton.Properties.Get<int>("SelectedIndex");
            selectedIndexProperty.Changed += delegate { this.ProposedValue = downButton.Items[downButton.SelectedIndex].CastTo<String>(); };

            return downButton;
        }
    }
}