using System;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Designer.Graphs.GUI.Model;
using Sceelix.Designer.GUI;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.TreeViewControls;

namespace Sceelix.Designer.Graphs.Inspector.Nodes
{
    public class GeneralController : GroupBox
    {
        public GeneralController(VisualNode visualNode)
        {
            Title = "General";
            HorizontalAlignment = HorizontalAlignment.Stretch;
            Margin = new Vector4F(0);

            UpdateLayoutControl(visualNode);
        }

        private void UpdateLayoutControl(VisualNode visualNode)
        {
            //this layout control will keep the labels aligned and pretty
            LayoutControl layoutControl = new LayoutControl
            {
                Margin = new Vector4F(5)
            };

            //Adds a label textbox, which allows the label of the node to be changed
            var rowLabel = layoutControl.Add("Label:", new ExtendedTextBox() { Text = visualNode.Label, AutoUnfocus = true, MaxLines = 5 });
            var textboxProperty = rowLabel[1].Properties.Get<String>("Text");
            rowLabel[0].ToolTip = rowLabel[1].ToolTip = new ToolTipControl("The label that appears on the node. Can be changed to better describe the actual use of this node in the graph.");
            textboxProperty.Changed += (sender, args) => visualNode.Label = args.NewValue;

            if (!visualNode.Node.ProcedureAttribute.IsDummy)
            {
                //adds a checkbox, which allows the activation/deactivation of the impulse port
                if (visualNode.Node.IsSourceNode)
                {
                    var rowSource = layoutControl.Add("Impulse:", new CheckBox() { IsChecked = visualNode.Node.HasImpulsePort, Margin = new Vector4F(0, 5, 0, 0), Height = 15 });
                    var checkboxProperty = rowSource[1].Properties.Get<bool>("IsChecked");
                    rowSource[0].ToolTip = rowSource[1].ToolTip = new ToolTipControl("Toggles the impulse port. This is available only for source nodes, so as to trigger their execution and pass on attributes.");
                    checkboxProperty.Changed += (sender, args) =>
                    {
                        visualNode.SetImpulsePort(args.NewValue);
                        UpdateLayoutControl(visualNode);
                    };
                }

                //adds a checkbox, which allows the activation/deactivation of the cache
                if (visualNode.Node.IsSourceNode && !visualNode.Node.HasImpulsePort)
                {
                    var rowCache = layoutControl.Add("Cache:", new CheckBox() { IsChecked = visualNode.Node.UseCache, Margin = new Vector4F(0, 5, 0, 0), Height = 15 });
                    var checkboxProperty = rowCache[1].Properties.Get<bool>("IsChecked");
                    rowCache[0].ToolTip = rowCache[1].ToolTip = new ToolTipControl("Toggles caching of results, avoiding the need for process re-execution if the parameter values are the same. Available only for source nodes.");
                    checkboxProperty.Changed += (sender, args) => visualNode.Node.UseCache = args.NewValue;
                }

                if (!visualNode.Node.HasInputPorts)
                {
                    var rowDisableInSubgraph = layoutControl.Add("Disable in Subgraphs:", new CheckBox() { IsChecked = visualNode.Node.DisableInSubgraphs, Margin = new Vector4F(0, 5, 0, 0), Height = 15 });
                    var checkboxProperty = rowDisableInSubgraph[1].Properties.Get<bool>("IsChecked");
                    rowDisableInSubgraph[0].ToolTip = rowDisableInSubgraph[1].ToolTip = new ToolTipControl("If checked, indicates that the node should not be executed if the parent graph is being used as a subgraph. Available only for source nodes.");
                    checkboxProperty.Changed += (sender, args) => visualNode.Node.DisableInSubgraphs = args.NewValue;
                }
            }
            

            Content = layoutControl;
        }
    }
}