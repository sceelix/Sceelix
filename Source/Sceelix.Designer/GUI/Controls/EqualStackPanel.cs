using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;

namespace Sceelix.Designer.GUI.Controls
{
    public class EqualStackPanel : StackPanel
    {
        /*protected override Vector2F OnMeasure(Vector2F availableSize)
        {
            float sumX = 0f;
            float maxY = 0f;

            foreach (var child in Children)
            {
                child.Measure(new Vector2F((float)Math.Max(availableSize.X - sumX, 0.0), availableSize.Y));
                sumX += child.DesiredWidth;
                maxY = Math.Max(maxY, child.DesiredHeight);
            }
            return new Vector2F(sumX, maxY);
        }*/



        protected override void OnArrange(Vector2F position, Vector2F size)
        {
            if (Orientation == Orientation.Horizontal)
            {
                var parcelSize = size.X/Children.Count;

                float startingX = position.X;
                foreach (UIControl uiControl in Children)
                {
                    uiControl.Arrange(new Vector2F(startingX, position.Y), new Vector2F(parcelSize, size.Y));
                    startingX += parcelSize;
                }
            }
            else if (Orientation == Orientation.Vertical)
            {
                var parcelSize = size.Y/Children.Count;

                float startingY = position.Y;
                foreach (UIControl uiControl in Children)
                {
                    uiControl.Arrange(new Vector2F(position.X, startingY), new Vector2F(size.X, parcelSize));
                    startingY += parcelSize;
                }
            }
        }



        /*float x = position.X;
        for (int i = 0; i < this.Children.Count - 1; i++)
        {
            var child = this.Children[i];
            child.Arrange(new Vector2F(x, position.Y), new Vector2F(child.DesiredWidth, child.DesiredHeight));
            x += child.DesiredWidth;
        }
        if (this.Children.Count > 0)
        {
            var lastChild = this.Children[this.Children.Count - 1];
            lastChild.Arrange(new Vector2F(x, position.Y), new Vector2F((float)Math.Max(size.X - x, 0.0), lastChild.DesiredHeight));
        }*/

        /*protected override Vector2F OnMeasure(Vector2F availableSize)
        {

            if (Orientation == Orientation.Horizontal)
            {

                foreach (UIControl uiControl in Children)
                    uiControl.Width = float.IsNaN(uiControl.Width) ? 0 : uiControl.Width;

                var uiControls = Children.Where(x => x.HorizontalAlignment == HorizontalAlignment.Stretch).ToList();
                var sum = Children.Sum(x => x.Width);
                var remainingSpace = availableSize.X - sum;

                var slice = remainingSpace / uiControls.Count();

                foreach (UIControl uiControl in uiControls)
                    uiControl.Width += slice;
            }

            if (Orientation == Orientation.Vertical)
            {
                foreach (UIControl uiControl in Children)
                    uiControl.Height = float.IsNaN(uiControl.Height) ? 0 : uiControl.Height;

                var uiControls = Children.Where(x => x.VerticalAlignment == VerticalAlignment.Stretch).ToList();
                var sum = Children.Sum(x => x.Height);
                var remainingSpace = availableSize.Y - sum;

                var slice = remainingSpace / uiControls.Count();

                foreach (UIControl uiControl in uiControls)
                    uiControl.Height += slice;
            }

            return base.OnMeasure(availableSize);
        }*/


        /*protected override Vector2F OnMeasure(Vector2F availableSize)
        {

            if (Orientation == Orientation.Horizontal)
            {

                foreach (UIControl uiControl in Children)
                    uiControl.Width = float.IsNaN(uiControl.Width) ? 0 : uiControl.Width;

                var uiControls = Children.Where(x => x.HorizontalAlignment == HorizontalAlignment.Stretch).ToList();
                var sum = Children.Sum(x => x.Width);
                var remainingSpace = availableSize.X - sum;
                
                var slice = remainingSpace/uiControls.Count();

                foreach (UIControl uiControl in uiControls)
                    uiControl.Width += slice;
            }
            
            if (Orientation == Orientation.Vertical)
            {
                foreach (UIControl uiControl in Children)
                    uiControl.Height = float.IsNaN(uiControl.Height) ? 0 : uiControl.Height;

                var uiControls = Children.Where(x => x.VerticalAlignment == VerticalAlignment.Stretch).ToList();
                var sum = Children.Sum(x => x.Height);
                var remainingSpace = availableSize.Y - sum;

                var slice = remainingSpace / uiControls.Count();

                foreach (UIControl uiControl in uiControls)
                    uiControl.Height += slice;
            }

            return base.OnMeasure(availableSize);
        }*/

        /*protected override void OnArrange(Vector2F position, Vector2F size)
        {
            
            if (Orientation == Orientation.Horizontal)
            {

                foreach (UIControl uiControl in Children)
                    uiControl.Width = float.IsNaN(uiControl.Width) ? 0 : uiControl.Width;

                var uiControls = Children.Where(x => x.HorizontalAlignment == HorizontalAlignment.Stretch).ToList();
                var sum = Children.Sum(x => x.Width);
                var remainingSpace = size.X - sum;

                var slice = remainingSpace / uiControls.Count();

                foreach (UIControl uiControl in uiControls)
                    uiControl.Width += slice;
            }

            if (Orientation == Orientation.Vertical)
            {
                foreach (UIControl uiControl in Children)
                    uiControl.Height = float.IsNaN(uiControl.Height) ? 0 : uiControl.Height;

                var uiControls = Children.Where(x => x.VerticalAlignment == VerticalAlignment.Stretch).ToList();
                var sum = Children.Sum(x => x.Height);
                var remainingSpace = size.Y - sum;

                var slice = remainingSpace / uiControls.Count();

                foreach (UIControl uiControl in uiControls)
                    uiControl.Height += slice;
            }

            base.OnArrange(position, size);
        }*/
    }
}