using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Core.Graphs;
using Sceelix.Designer.GUI.MenuControls;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Graphs.Inspector.Graphs
{
    public class GraphInspectorControl : ContentControl
    {
        private readonly FileItem _fileItem;
        private readonly Graph _graph;
        private readonly IServiceLocator _services;



        public GraphInspectorControl(IServiceLocator services, Graph graph, FileItem fileItem)
        {
            _services = services;
            _graph = graph;
            _fileItem = fileItem;
        }



        protected override void OnLoad()
        {
            base.OnLoad();

            StackPanel stackPanel = new StackPanel()
            {
                Margin = new Vector4F(5),
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            stackPanel.Children.Add(new GraphGeneralController(_services, _graph, _fileItem));

            stackPanel.Children.Add(new GraphParameterController(_services, _graph, _fileItem));

            Content = new VerticalScrollViewer
            {
                Content = stackPanel,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };
        }
    }
}