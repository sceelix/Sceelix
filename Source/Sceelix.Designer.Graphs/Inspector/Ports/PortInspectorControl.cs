using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Designer.Graphs.GUI.Model;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.ProjectExplorer.Management;

namespace Sceelix.Designer.Graphs.Inspector.Ports
{
    public class PortInspectorControl : ContentControl
    {
        private readonly FileItem _fileItem;
        private readonly VisualPort _visualPort;



        public PortInspectorControl(VisualPort visualPort, FileItem fileItem)
        {
            _visualPort = visualPort;
            _fileItem = fileItem;

            var mainPanel = new FlexibleStackPanel()
            {
                Margin = new Vector4F(5),
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            mainPanel.Children.Add(new GeneralPortController(visualPort));

            mainPanel.Children.Add(new StatePortController(visualPort));

            Content = new ScrollViewer()
            {
                Content = mainPanel,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };
        }
    }
}