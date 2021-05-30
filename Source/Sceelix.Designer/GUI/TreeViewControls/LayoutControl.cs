using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Designer.GUI.Controls;

namespace Sceelix.Designer.GUI.TreeViewControls
{
    public class LayoutControl : ContentControl, IEnumerable<FlexibleStackPanel>
    {
        /// <summary>
        /// The items will be organized into an vertical stackpanel.
        /// </summary>
        private readonly StackPanel _stackPanel;



        public LayoutControl()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;

            Content = _stackPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
        }



        /// <summary>
        /// Acessor to get the rows of the Layoutcontrol
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public FlexibleStackPanel this[int index]
        {
            get { return (FlexibleStackPanel) _stackPanel.Children[index]; }
            set { _stackPanel.Children[index] = value; }
        }



        public Vector4F InnerMargin
        {
            get;
            set;
        }



        public IEnumerator<FlexibleStackPanel> GetEnumerator()
        {
            return _stackPanel.Children.OfType<FlexibleStackPanel>().GetEnumerator();
        }



        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }



        protected override Vector2F OnMeasure(Vector2F availableSize)
        {
            base.OnMeasure(availableSize);

            var textBlocks = _stackPanel.Children.Select(x => (TextBlock) ((FlexibleStackPanel) x).Children[0]).ToList();
            var maxWidth = textBlocks.Max(x => Screen.Renderer.GetFont(x.Font).MeasureString(x.Text).X);
            textBlocks.ForEach(x => x.Width = maxWidth);

            return base.OnMeasure(availableSize);
        }



        public FlexibleStackPanel Add(String text, UIControl control)
        {
            FlexibleStackPanel horizontalStackPanel = new FlexibleStackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = InnerMargin
            };

            horizontalStackPanel.Children.Add(new TextBlock
            {
                Text = text,
                Margin = new Vector4F(3, 3, 10, 3)
            });

            control.HorizontalAlignment = HorizontalAlignment.Stretch;
            control.VerticalAlignment = VerticalAlignment.Stretch;
            horizontalStackPanel.Children.Add(control);

            _stackPanel.Children.Add(horizontalStackPanel);

            return horizontalStackPanel;
        }

        public void Remove(FlexibleStackPanel panel)
        {
            _stackPanel.Children.Remove(panel);
        }
    }
}