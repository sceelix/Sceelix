using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Core.Data;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.TreeViewControls;

namespace Sceelix.Designer.Graphs.Inspector.Entities
{
    public class GeneralController : GroupBox
    {
        public GeneralController(IEntity entity)
        {
            Title = "General";
            HorizontalAlignment = HorizontalAlignment.Stretch;
            Margin = new Vector4F(0);

            //this layout control will keep the labels aligned and pretty
            LayoutControl layoutControl = new LayoutControl
            {
                Margin = new Vector4F(5)
            };

            //Adds a label textbox, which allows the label of the node to be changed
            layoutControl.Add("Type:", new ExtendedTextBox() {Text = Entity.GetDisplayName(entity.GetType()), IsReadOnly = true, Focusable = false});

            Content = layoutControl;
        }
    }
}