using System;
using DigitalRune.Game.UI.Controls;

namespace Sceelix.Designer.GUI.Controls
{
    public class EnumDropDownButton<T> : DropDownButton
    {
        public EnumDropDownButton()
        {
            Items.AddRange(Enum.GetNames(typeof(T)));
        }



        public T Value
        {
            get { return (T) Enum.Parse(typeof(T), (String) Items[SelectedIndex]); }
            set { SelectedIndex = Items.IndexOf(Enum.GetName(typeof(T), value)); }
        }
    }
}