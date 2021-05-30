using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Sceelix.Designer.Graphs.Tools;
using Sceelix.Designer.GUI.Controls;

namespace Sceelix.Designer.Graphs.GUI.Toolbox
{
    public class ToolboxItemControl : ContentControl
    {
        public static readonly int IsSelectedPropertyId = CreateProperty(typeof(ToolboxItemControl), "IsSelected", GamePropertyCategories.Style, null, false, UIPropertyOptions.AffectsRender);

        private readonly IProcedureItem _item;
        //private bool _isSelected;

        public ToolboxItemControl(IProcedureItem item)
        {
            Style = "ToolBoxItem";

            _item = item;
        }



        public TextBlock LabelBlock
        {
            get;
            private set;
        }



        public override string VisualState
        {
            get
            {
                if (IsSelected)
                    return "Selected";

                return "Default";
            }
        }



        public bool IsSelected
        {
            get { return GetValue<bool>(IsSelectedPropertyId); }
            set
            {
                SetValue(IsSelectedPropertyId, value);
                InvalidateVisual();
            }
        }



        public IProcedureItem Item
        {
            get { return _item; }
        }



        public event EventHandler<EventArgs> DoubleClick = delegate { };



        protected override void OnLoad()
        {
            base.OnLoad();

            var customStackPanel = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                //VerticalAlignment = VerticalAlignment.Stretch,
                Orientation = Orientation.Vertical
            };
            customStackPanel.Children.Add(LabelBlock = new TextBlock()
            {
                Text = _item.Label,
                //HorizontalAlignment = HorizontalAlignment.Stretch,
                //Height = 20,
                Foreground = Color.Black,
                Margin = new Vector4F(6, 6, 6, 0)
            });
            customStackPanel.Children.Add(new TextBlock()
            {
                Text = _item.Tags,
                Font = "DefaultSmall",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                //Height = 20,
                Margin = new Vector4F(6, 0, 6, 6)
            });

            Content = customStackPanel;

            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;

            //LabelBlock.Width = Screen.Renderer.GetFont(LabelBlock.Font).MeasureString(LabelBlock.Text).X;
        }



        /*public bool Match(String text)
        {
            return _item.Label.ToLower().Contains(text) || _item.Tags.ToLower().Contains(text);
        }*/



        public bool Match(string[] keywords)
        {
            var lowerCaseLabel = _item.Label.ToLower();
            var lowerCaseTags = _item.Tags.ToLower();

            return keywords.All(x => lowerCaseLabel.Contains(x) || lowerCaseTags.Contains(x));

            //return _item.Label.ToLower().Any(x => )
        }



        public int MatchCount(string[] keywords)
        {
            var lowerCaseLabel = _item.Label.ToLower();
            var lowerCaseTags = _item.Tags.ToLower();


            return keywords.Count(x => lowerCaseLabel.Contains(x))*1000 + keywords.Count(x => lowerCaseTags.Contains(x))*1000;

            //return _item.Label.ToLower().Any(x => )
        }



        protected override void OnHandleInput(InputContext context)
        {
            var inputService = InputService;

            if (!inputService.IsMouseOrTouchHandled)
            {
                if (IsMouseOver && InputService.IsPressed(MouseButtons.Left, true))
                {
                    IsSelected = true;
                    inputService.IsMouseOrTouchHandled = true;
                }
                if (IsMouseOver && InputService.IsDoubleClick(MouseButtons.Left))
                {
                    DoubleClick(this, EventArgs.Empty);
                    inputService.IsMouseOrTouchHandled = true;
                }
            }
        }
    }
}