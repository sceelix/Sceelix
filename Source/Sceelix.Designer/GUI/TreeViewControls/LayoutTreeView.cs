using System.Linq;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;

namespace Sceelix.Designer.GUI.TreeViewControls
{
    public interface ILabeledTreeViewItem : ITreeViewControl
    {
        TextBlock TextBlockLabel
        {
            get;
        }



        bool IsExpanded
        {
            get;
        }



        Vector4F Padding
        {
            get;
        }
    }


    public class LayoutTreeView : TreeView
    {
        protected override Vector2F OnMeasure(Vector2F availableSize)
        {
            if (Items.Count > 0)
            {
                base.OnMeasure(availableSize);

                var max = Items.OfType<ILabeledTreeViewItem>().Max(val => GetMaxTextWidth(val, 0));

                foreach (var result in Items.OfType<ILabeledTreeViewItem>())
                {
                    SetMaxTextWidth(result, 0, max);
                }
            }

            return base.OnMeasure(availableSize);
        }



        public float GetMaxTextWidth(ILabeledTreeViewItem treeViewItem, int level)
        {
            var measureString = Screen.Renderer.GetFont(treeViewItem.TextBlockLabel.Font).MeasureString(treeViewItem.TextBlockLabel.Text);

            float currentWidth = measureString.X + level*treeViewItem.Padding.X;

            if (treeViewItem.IsExpanded)
            {
                foreach (var subItem in treeViewItem.Items.OfType<ILabeledTreeViewItem>())
                {
                    var width = GetMaxTextWidth(subItem, level + 1);
                    if (width > currentWidth)
                        currentWidth = width;
                }
            }

            return currentWidth;
        }



        public void SetMaxTextWidth(ILabeledTreeViewItem treeViewItem, int level, float value)
        {
            treeViewItem.TextBlockLabel.Width = value - level*treeViewItem.Padding.X;

            if (treeViewItem.IsExpanded)
            {
                foreach (var subItem in treeViewItem.Items.OfType<ILabeledTreeViewItem>())
                {
                    SetMaxTextWidth(subItem, level + 1, value);
                }
            }
        }
    }
}