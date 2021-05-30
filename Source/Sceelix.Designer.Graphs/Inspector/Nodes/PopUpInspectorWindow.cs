using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Sceelix.Designer.Graphs.GUI;
using Sceelix.Designer.Graphs.GUI.Model;

namespace Sceelix.Designer.Graphs.Inspector.Nodes
{
    class PopUpInspectorWindow : ContentControl
    {
        private readonly VisualNode _visualNode;



        public PopUpInspectorWindow(VisualNode visualNode)
            //: base(services)
        {
            _visualNode = visualNode;
            Width = 400;
            Height = 400;

            Style = "MenuItemContent";


            /*var treeView = new TreeView();

            foreach (var argument in _visualNode.Node.Arguments)
            {
                treeView.Items.Add(new SelectableTreeViewItem() { Text = argument.ParameterLabel });
            }*/

            Content = new TextBlock() {Margin = new Vector4F(5), HorizontalAlignment = HorizontalAlignment.Stretch, Text = "Hello World"};
            ;
        }



        public void OpenWindow(GraphControl control, Vector2 position)
        {
            X = position.X;
            Y = position.Y;

            control.Screen.Children.Add(this);
        }



        protected override void OnHandleInput(InputContext context)
        {
            base.OnHandleInput(context);


            if ((InputService.IsDown(MouseButtons.Left) ||
                 InputService.IsDown(MouseButtons.Right) ||
                 InputService.IsDown(MouseButtons.Middle)) && !IsMouseOver)
            {
                Close();

                return;
            }

            if (!InputService.IsMouseOrTouchHandled)
            {
                if (IsMouseOver)
                    InputService.IsMouseOrTouchHandled = true;
            }


            /*if (!InputService.IsMouseOrTouchHandled && IsMouseOver)
            {
                InputService.IsMouseOrTouchHandled = true;
            }*/


/*if ((InputService.IsDown(MouseButtons.Left) || 
                            InputService.IsDown(MouseButtons.Right) || 
                            InputService.IsDown(MouseButtons.Middle)) && !IsMouseOver)
                        {
                            Close();
            
                            return;
                        }
            
                        if (IsMouseOver && !InputService.IsMouseOrTouchHandled)
                        {
                            InputService.IsMouseOrTouchHandled = true;
                        }*/
        }



        private void Close()
        {
            Screen.Children.Remove(this);
        }
    }
}