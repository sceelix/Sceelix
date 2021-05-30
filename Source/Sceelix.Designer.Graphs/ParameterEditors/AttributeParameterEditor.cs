using System;
using System.Linq;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Core.Attributes;
using Sceelix.Core.Parameters;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Designer.Graphs.Annotations;
using Sceelix.Designer.GUI;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.ProjectExplorer.Management;
using Button = DigitalRune.Game.UI.Controls.Button;
using ContextMenu = DigitalRune.Game.UI.Controls.ContextMenu;
using HorizontalAlignment = DigitalRune.Game.UI.HorizontalAlignment;
using MenuItem = DigitalRune.Game.UI.Controls.MenuItem;
using Orientation = DigitalRune.Game.UI.Orientation;
using TextBox = DigitalRune.Game.UI.Controls.TextBox;

namespace Sceelix.Designer.Graphs.ParameterEditors
{
    [ParameterEditor(typeof(AttributeParameterInfo))]
    public class AttributeParameterEditor : PrimitiveParameterEditor<AttributeParameterInfo>
    {
        public override UIControl CreateControl(AttributeParameterInfo info, FileItem fileItem, Action onChanged)
        {
            var panel = new FlexibleStackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            

            var textBox = new ExtendedTextBox() {Text = info.AttributeString, HorizontalAlignment = HorizontalAlignment.Stretch};
            Action updateAttributeInfoAction = () =>
            {
                info.AttributeString = textBox.Text;
                onChanged();
            };

            var property = textBox.Properties.Get<bool>("IsFocused");
            property.Changed += delegate
            {
                if (!textBox.IsFocused)
                    updateAttributeInfoAction();
            };

            Button button = new Button() {Width = 20, Margin = new Vector4F(0), Padding = new Vector4F(0), VerticalAlignment = VerticalAlignment.Center}; //Height = 20,
            switch (info.Access)
            {
                case AttributeAccess.Read:
                    button.Content = new TextBlock() {Text = "<<", Margin = new Vector4F(0), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center};
                    button.ToolTip = new ToolTipControl("This attribute will be read from.");
                    break;
                case AttributeAccess.Write:
                    button.Content = new TextBlock() {Text = ">>", Margin = new Vector4F(0), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center};
                    button.ToolTip = new ToolTipControl("This attribute will be written to.");
                    AddData(textBox, button, updateAttributeInfoAction,  MetaParserManager.MetaParsers.ToArray() );
                    break;
                case AttributeAccess.ReadWrite:
                    button.Content = new TextBlock() {Text = "<>", Margin = new Vector4F(0), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center};
                    button.ToolTip = new ToolTipControl("This attribute will be read from and written to.");
                    //AddData(textBox, button, "/local");
                    break;
            }

            panel.Children.Add(button);
            panel.Children.Add(textBox);
            //panel.Children.Add(new Button() { Style = "DropDownButton", Width = 20 });

            /*Image image = new Image();
            switch (info.Access)
            {
                case AttributeAccess.Read:
                    image = new Image() { Texture = EmbeddedResources.Load<Texture2D>("Resources/R16x16.png"), ToolTip = "This attribute will be read from." };
                    break;
                case AttributeAccess.Write:
                    image = new Image() { Texture = EmbeddedResources.Load<Texture2D>("Resources/W16x16.png"), ToolTip = "This attribute will be written to." };
                    break;
                case AttributeAccess.ReadWrite:
                    image = new Image() { Texture = EmbeddedResources.Load<Texture2D>("Resources/RW16x16.png"), ToolTip = "This attribute will be read from and written to." };
                    break;
            }*/


            return panel;
        }



        private void AddData(TextBox textBox, Button button, Action updateAttributeInfoAction, params string[] options)
        {
            ContextMenu menu = new ContextMenu();
            foreach (var option in options)
            {
                var actualOption = option.ToLower();

                var item = new MenuItem {Content = new TextBlock() {Text = option}};
                item.Click += delegate
                {
                    if (!textBox.Text.Split('/').Any(x => x.Trim().StartsWith(actualOption)))
                    {
                        textBox.Text += " /" + actualOption;
                        updateAttributeInfoAction();
                    }
                };
                menu.Items.Add(item);
            }

            button.Click += (sender, args) => menu.Open(button, button.InputService.MousePosition);
        }



        public override UIControl CreateInspectorContent(ParameterEditorTreeViewItem parameterEditorTreeViewItem)
        {
            LayoutControl layoutControl = (LayoutControl) base.CreateInspectorContent(parameterEditorTreeViewItem);

            AttributeParameterInfo parameterInfo = (AttributeParameterInfo) parameterEditorTreeViewItem.ParameterInfo;

            var dropDown = (EnumDropDownButton<AttributeAccess>) layoutControl.Add("Access", new EnumDropDownButton<AttributeAccess>() {Value = parameterInfo.Access})[1];
            var dropDownProperty = dropDown.Properties.Get<int>("SelectedIndex");
            dropDownProperty.Changed += delegate { parameterInfo.Access = dropDown.Value; };

            return layoutControl;
        }
    }
}