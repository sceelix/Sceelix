using System;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Core.Graphs;
using Sceelix.Designer.Graphs.GUI.Model;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.TreeViewControls;

namespace Sceelix.Designer.Graphs.Inspector.Ports
{
    public class StatePortController : GroupBox
    {
        public StatePortController(VisualPort visualPort)
        {
            Title = "State";
            HorizontalAlignment = HorizontalAlignment.Stretch;
            Margin = new Vector4F(5);

            //this layout control will keep the labels aligned and pretty
            LayoutControl layoutControl = new LayoutControl
            {
                Margin = new Vector4F(10)
            };

            var dropdownbutton = (DropDownButton) layoutControl.Add("State:", new DropDownButton() {AutoUnfocus = true})[1];
            dropdownbutton.Items.AddRange(Enum.GetNames(typeof(PortState)));
            dropdownbutton.SelectedIndex = dropdownbutton.Items.IndexOf(Enum.GetName(typeof(PortState), visualPort.Port.PortState));

            var textboxGateLabel = (TextBox) (layoutControl.Add("Gate Name:", new ExtendedTextBox() {Text = visualPort.Port.GateLabel, AutoUnfocus = true, IsEnabled = (String) dropdownbutton.Items[dropdownbutton.SelectedIndex] == "Gate"})[1]);
            var textboxGateLabelProperty = textboxGateLabel.Properties.Get<String>("Text");
            textboxGateLabelProperty.Changed += delegate(object sender, GamePropertyEventArgs<string> args) { visualPort.Port.GateLabel = args.NewValue; };

            var dropdownbuttonProperty = dropdownbutton.Properties.Get<int>("SelectedIndex");
            dropdownbuttonProperty.Changed += delegate(object sender, GamePropertyEventArgs<int> args)
            {
                textboxGateLabel.IsEnabled = (String) dropdownbutton.Items[args.NewValue] == "Gate";
                visualPort.Port.PortState = (PortState) Enum.Parse(typeof(PortState), (String) dropdownbutton.Items[args.NewValue]);

                //update the 3D Canvas
                visualPort.VisualGraph.ShouldRender = true;

                //give the name of the port as the gate label
                if (textboxGateLabel.IsEnabled && String.IsNullOrWhiteSpace(textboxGateLabel.Text))
                    textboxGateLabel.Text = visualPort.Port.Label;
            };

            Content = layoutControl;
        }
    }
}