using System;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;

namespace Sceelix.Designer.GUI.Controls
{
    public class IconMenuStrip : StackPanel
    {
        public IconMenuStrip()
        {
            Orientation = Orientation.Horizontal;
        }



        public void Add(String text, Texture2D image, EventHandler<EventArgs> eventHandler)
        {
            var newButton = new ImageButton() {Text = text, Margin = new Vector4F(2), Texture = image};
            newButton.Click += eventHandler;
            Children.Add(newButton);
        }
    }
}