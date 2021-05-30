using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Linq;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;

namespace Sceelix.Designer.GUI.Controls
{
    public class FlexibleStackPanel : StackPanel
    {
        private readonly Dictionary<UIControl, Vector2F> _baseSizes = new Dictionary<UIControl, Vector2F>();
        private bool _proportionalFlexibility = false;

        public int Test = 0;
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



        public FlexibleStackPanel()
        {
            Children.CollectionChanged += ChildrenOnCollectionChanged;
        }



        public UIControl this[int index]
        {
            get { return Children[index]; }
            set { Children[index] = value; }
        }



        public bool ProportionalFlexibility
        {
            get { return _proportionalFlexibility; }
            set { _proportionalFlexibility = value; }
        }



        private void ChildrenOnCollectionChanged(object sender, CollectionChangedEventArgs<UIControl> collectionChangedEventArgs)
        {
            foreach (UIControl uiControl in collectionChangedEventArgs.NewItems)
                _baseSizes.Add(uiControl, new Vector2F(uiControl.Width, uiControl.Height));

            foreach (UIControl uiControl in collectionChangedEventArgs.OldItems)
                _baseSizes.Remove(uiControl);
        }



        /*protected Vector2F OnMeasure2(Vector2F availableSize)
        {
            // Similar to UIControl.OnMeasure, but we sum up the desired sizes in the stack panel 
            // orientation. In the other direction we take the maximum of the children - unless a 
            // Width or Height was set explicitly. If there are VisualChildren that are not Children, 
            // they do not contribute to the DesiredSize.

            float width = Width;
            float height = Height;
            bool hasWidth = Numeric.IsPositiveFinite(width);
            bool hasHeight = Numeric.IsPositiveFinite(height);

            if (hasWidth && width < availableSize.X)
                availableSize.X = width;
            if (hasHeight && height < availableSize.Y)
                availableSize.Y = height;

            Vector4F padding = Padding;
            availableSize.X -= padding.X + padding.Z;
            availableSize.Y -= padding.Y + padding.W;

            foreach (var child in VisualChildren)
                child.Measure(availableSize);

            if (hasWidth && hasHeight)
                return new Vector2F(width, height);

            Vector2F desiredSize = new Vector2F(width, height);

            float maxWidth = 0;
            float sumWidth = 0;
            float maxHeight = 0;
            float sumHeight = 0;

            // Sum up widths and heights.
            foreach (var child in Children)
            {
                float childWidth = child.DesiredWidth;
                float childHeight = child.DesiredHeight;
                maxWidth = Math.Max(maxWidth, childWidth);
                maxHeight = Math.Max(maxHeight, childHeight);
                sumWidth += childWidth;
                sumHeight += childHeight;
            }

            if (!hasWidth)
            {
                if (Orientation == Orientation.Horizontal)
                    desiredSize.X = sumWidth;
                else
                    desiredSize.X = maxWidth;
            }

            if (!hasHeight)
            {
                if (Orientation == Orientation.Vertical)
                    desiredSize.Y = sumHeight;
                else
                    desiredSize.Y = maxHeight;
            }

            desiredSize.X += padding.X + padding.Z;
            desiredSize.Y += padding.Y + padding.W;

            return desiredSize;
        }*/



        protected override Vector2F OnMeasure(Vector2F availableSize)
        {
            ResetStretchSizes();

            //if (Test == 1)
            //    Test = 1;

            float width = Width;
            float height = Height;
            bool hasWidth = Numeric.IsPositiveFinite(width);
            bool hasHeight = Numeric.IsPositiveFinite(height);

            if (hasWidth && width < availableSize.X)
                availableSize.X = width;
            if (hasHeight && height < availableSize.Y)
                availableSize.Y = height;

            Vector4F padding = Padding;
            availableSize.X -= padding.X + padding.Z;
            availableSize.Y -= padding.Y + padding.W;


            foreach (var child in Children)
                child.Measure(availableSize);

            if (Orientation == Orientation.Horizontal)
            {
                var sum = Children.Sum(uiControl => uiControl.HorizontalAlignment == HorizontalAlignment.Stretch ? _baseSizes[uiControl].X : uiControl.DesiredWidth);
                var remainingSpace = availableSize.X - sum;

                /*if (_proportionalFlexibility)
                {
                    float totalExtesibleFraction = remainingSpace / Children.Where(uiControl => uiControl.HorizontalAlignment == HorizontalAlignment.Stretch).Sum(uicontrol => _baseSizes[uicontrol].X);

                    foreach (UIControl uiControl in Children.Where(x => x.HorizontalAlignment == HorizontalAlignment.Stretch))
                        uiControl.Measure(new Vector2F(_baseSizes[uiControl].X + _baseSizes[uiControl].X * totalExtesibleFraction, availableSize.Y));
                }
                else*/
                {
                    var extensibleControlCount = Children.Count(uiControl => uiControl.HorizontalAlignment == HorizontalAlignment.Stretch);
                    var slice = remainingSpace/extensibleControlCount;

                    foreach (UIControl uiControl in Children.Where(x => x.HorizontalAlignment == HorizontalAlignment.Stretch))
                        uiControl.Measure(new Vector2F(_baseSizes[uiControl].X + slice, availableSize.Y));
                }
            }
            else if (Orientation == Orientation.Horizontal)
            {
                var sum = Children.Sum(uiControl => uiControl.VerticalAlignment == VerticalAlignment.Stretch ? _baseSizes[uiControl].Y : uiControl.DesiredHeight);
                var remainingSpace = availableSize.Y - sum;

                var extensibleControlCount = Children.Count(uiControl => uiControl.VerticalAlignment == VerticalAlignment.Stretch);
                var slice = remainingSpace/extensibleControlCount;

                foreach (UIControl uiControl in Children.Where(x => x.VerticalAlignment == VerticalAlignment.Stretch))
                    uiControl.Measure(new Vector2F(availableSize.X, _baseSizes[uiControl].Y + slice));
            }


            if (hasWidth && hasHeight)
                return new Vector2F(width, height);

            Vector2F desiredSize = new Vector2F(width, height);

            float maxWidth = 0;
            float sumWidth = 0;
            float maxHeight = 0;
            float sumHeight = 0;

            // Sum up widths and heights.
            foreach (var child in Children)
            {
                float childWidth = child.DesiredWidth;
                float childHeight = child.DesiredHeight;
                maxWidth = Math.Max(maxWidth, childWidth);
                maxHeight = Math.Max(maxHeight, childHeight);
                sumWidth += childWidth;
                sumHeight += childHeight;
            }

            if (!hasWidth)
            {
                if (Orientation == Orientation.Horizontal)
                    desiredSize.X = sumWidth;
                else
                    desiredSize.X = maxWidth;
            }

            if (!hasHeight)
            {
                if (Orientation == Orientation.Vertical)
                    desiredSize.Y = sumHeight;
                else
                    desiredSize.Y = maxHeight;
            }

            desiredSize.X += padding.X + padding.Z;
            desiredSize.Y += padding.Y + padding.W;

            return desiredSize;
        }



        /*private float FloatToAbsolute(float size, float totalSize)
        {
            if (size < 1 && size > 0)
                return size*totalSize;

            return size;
        }*/



        private void ResetStretchSizes()
        {
            foreach (UIControl uiControl in Children)
            {
                if (uiControl.HorizontalAlignment == HorizontalAlignment.Stretch)
                    uiControl.Width = 0;

                if (uiControl.VerticalAlignment == VerticalAlignment.Stretch)
                    uiControl.Height = 0;
            }
        }



        protected override void OnArrange(Vector2F position, Vector2F size)
        {
            if (Orientation == Orientation.Horizontal)
            {
                var sum = Children.Sum(uiControl => uiControl.HorizontalAlignment == HorizontalAlignment.Stretch ? _baseSizes[uiControl].X : uiControl.DesiredWidth);
                var remainingSpace = size.X - sum;

                /*if (_proportionalFlexibility)
                {
                    float totalExtesibleFraction = remainingSpace / Children.Where(uiControl => uiControl.HorizontalAlignment == HorizontalAlignment.Stretch).Sum(uicontrol => _baseSizes[uicontrol].X);

                    float startingX = position.X;
                    foreach (UIControl uiControl in Children)
                    {
                        float xSize = uiControl.HorizontalAlignment == HorizontalAlignment.Stretch ? _baseSizes[uiControl].X + _baseSizes[uiControl].X * totalExtesibleFraction : uiControl.DesiredWidth;

                        uiControl.Arrange(new Vector2F(startingX, position.Y), new Vector2F(xSize, size.Y));

                        startingX += xSize;
                    }
                }
                else*/
                {
                    var extensibleControlCount = Children.Count(x => x.HorizontalAlignment == HorizontalAlignment.Stretch);
                    var slice = remainingSpace/extensibleControlCount;

                    float startingX = position.X;
                    foreach (UIControl uiControl in Children)
                    {
                        float xSize = uiControl.HorizontalAlignment == HorizontalAlignment.Stretch ? _baseSizes[uiControl].X + slice : uiControl.DesiredWidth;

                        //var yPosition = uiControl.VerticalAlignment == VerticalAlignment.Center ? position.Y + size.Y/2f - uiControl.DesiredHeight/2f : position.Y;
                        //var ySize = uiControl.VerticalAlignment == VerticalAlignment.Center ? size.Y / 2f - uiControl.DesiredHeight / 2f : position.Y;

                        uiControl.Arrange(new Vector2F(startingX, position.Y), new Vector2F(xSize, size.Y));

                        startingX += xSize;
                    }
                }
            }
            else if (Orientation == Orientation.Vertical)
            {
                var sum = Children.Sum(uiControl => uiControl.VerticalAlignment == VerticalAlignment.Stretch ? _baseSizes[uiControl].Y : uiControl.DesiredHeight);
                var remainingSpace = size.Y - sum;

                var extensibleControlCount = Children.Count(x => x.VerticalAlignment == VerticalAlignment.Stretch);
                var slice = remainingSpace/extensibleControlCount;

                float startingY = position.Y;
                foreach (UIControl uiControl in Children)
                {
                    float ySize = uiControl.VerticalAlignment == VerticalAlignment.Stretch ? _baseSizes[uiControl].Y + slice : uiControl.DesiredHeight;

                    uiControl.Arrange(new Vector2F(position.X, startingY), new Vector2F(size.X, ySize));

                    startingY += ySize;
                }
            }
        }
    }
}