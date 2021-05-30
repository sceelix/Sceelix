using System;
using System.Collections.Generic;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;

namespace Sceelix.Designer.GUI.MenuControls
{
    public class VerticalScrollViewer : ScrollViewer
    {
        public VerticalScrollViewer()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch;
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            VerticalAlignment = VerticalAlignment.Stretch;

            var property = Properties.Get<float>("VerticalOffset");
            property.Changed += delegate(object sender, GamePropertyEventArgs<float> args)
            {
                CheckVerticalOffset();
                //UpdateVisibility();
            };
        }



        /*public void UpdateVisibility()
        {
            var uiControls = UIHelper.GetDescendants(this);
            foreach (UIControl uiControl in uiControls)
                uiControl.IsVisible = uiControl.IsVisible && ActualBounds.Intersects(uiControl.ActualBounds);
        }*/



        protected override Vector2F OnMeasure(Vector2F availableSize)
        {
            var measure = base.OnMeasure(availableSize);

            CheckVerticalOffset();

            return measure;
        }



        private void CheckVerticalOffset()
        {
            if (VerticalOffset < 0)
                VerticalOffset = 0;
            else if (VerticalOffset > ExtentHeight - ViewportHeight)
                VerticalOffset = Math.Max(0, ExtentHeight - ViewportHeight);
        }
    }
}