using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Designer.Graphs.GUI.Model;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.MenuControls;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Graphs.Inspector.Nodes
{
    public class NodeInspectorControl : ContentControl
    {
        private readonly FileItem _fileItem;
        private readonly VisualNode _visualNode;
        private readonly FlexibleStackPanel _mainPanel;
        //private TreeView _treeView;

        public NodeInspectorControl(IServiceLocator services, VisualNode visualNode, FileItem fileItem)
        {
            _visualNode = visualNode;
            _fileItem = fileItem;

            _mainPanel = new FlexibleStackPanel()
            {
                Margin = new Vector4F(5),
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            _mainPanel.Children.Add(new GeneralController(visualNode));

            if (visualNode.Node.Parameters.Count > 0)
            {
                _mainPanel.Children.Add(new ParameterController(services, visualNode, fileItem));
            }

            //_mainPanel.PropertyChanged += MainPanelOnPropertyChanged;

            Content = new VerticalScrollViewer()
            {
                Content = _mainPanel,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };
        }



        /*private void MainPanelOnPropertyChanged(object sender, GamePropertyEventArgs gamePropertyEventArgs)
        {
            if (gamePropertyEventArgs.Property.Name == "Height")
            {
                _mainPanel.Orientation = Height > Width ? Orientation.Vertical : Orientation.Horizontal;
            }
        }*/


        /*protected override Vector2F OnMeasure(Vector2F availableSize)
        {
            //base.OnMeasure(availableSize);

            _mainPanel.Orientation = availableSize.Y > availableSize.X ? Orientation.Vertical : Orientation.Horizontal;

            return base.OnMeasure(availableSize);
        }*/


        /*private void AddArgument(Argument argument)
        {
            _stackPanel.Children.Add(new EqualStackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Orientation = Orientation.Horizontal,
                Children = { new TextBlock() { Text = argument.ParameterLabel}, new TextBox(){Text = argument.FixedValue.ToString()} }
            });
        }*/



        public VisualNode VisualNode
        {
            get { return _visualNode; }
        }



        public FileItem FileItem
        {
            get { return _fileItem; }
        }
    }
}