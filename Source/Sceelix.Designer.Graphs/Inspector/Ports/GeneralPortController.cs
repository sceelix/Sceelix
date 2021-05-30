using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Core.Annotations;
using Sceelix.Designer.Graphs.GUI.Model;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Extensions;

namespace Sceelix.Designer.Graphs.Inspector.Ports
{
    public class GeneralPortController : GroupBox
    {
        public GeneralPortController(VisualPort visualPort)
        {
            Title = "General";
            HorizontalAlignment = HorizontalAlignment.Stretch;
            Margin = new Vector4F(5);

            //this layout control will keep the labels aligned and pretty
            LayoutControl layoutControl = new LayoutControl
            {
                Margin = new Vector4F(10)
            };

            var attribute = visualPort.Port.ObjectType.GetCustomAttribute<EntityAttribute>();
            var portType = attribute != null ? attribute.Name : visualPort.Port.ObjectType.Name;

            layoutControl.Add("Node Type:", new ExtendedTextBox() {Text = visualPort.VisualNode.Node.NodeTypeName, AutoUnfocus = true, IsReadOnly = true});
            layoutControl.Add("Port Name:", new ExtendedTextBox() {Text = visualPort.Label, AutoUnfocus = true, IsReadOnly = true});
            layoutControl.Add("Port Nature:", new ExtendedTextBox() {Text = visualPort.Port.Nature, AutoUnfocus = true, IsReadOnly = true});
            layoutControl.Add("Entity Type:", new ExtendedTextBox() {Text = portType, AutoUnfocus = true, IsReadOnly = true});

            Content = layoutControl;
        }
    }
}