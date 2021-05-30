using System;
using System.Linq;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Core.Graphs;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Graphs.Inspector.Graphs
{
    public class GraphParameterController : GroupBox
    {
        private readonly FileItem _fileItem;
        private readonly Graph _graph;
        private readonly IServiceLocator _services;
        private readonly ParameterEditorManager _parameterEditorManager;
        private LayoutTreeView _treeview;

        public GraphParameterController(IServiceLocator services, Graph graph, FileItem fileItem)
        {
            _services = services;
            _graph = graph;
            _fileItem = fileItem;

            _parameterEditorManager = services.Get<ParameterEditorManager>();
        }


        protected override void OnLoad()
        {
            base.OnLoad();

            Title = "Parameters";
            HorizontalAlignment = HorizontalAlignment.Stretch;
            Margin = new Vector4F(0, 2, 0, 0);

            StackPanel panel = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Orientation = Orientation.Vertical,
                Margin = new Vector4F(5)
            };

            TextButton button = new TextButton();
            button.Text = "Edit...";
            button.HorizontalAlignment = HorizontalAlignment.Right;
            button.Margin = new Vector4F(0, 0, 0, 5);
            button.Click += ButtonOnClick;
            panel.Children.Add(button);

            _treeview = new LayoutTreeView()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Vector4F(0)
            };
            panel.Children.Add(_treeview);

            RefreshParameterTreeView();

            Content = panel;
        }


        private void RefreshParameterTreeView()
        {
            _treeview.Items.Clear();

            String lastGroupName = null;
            foreach (var argument in _graph.ParameterInfos.Where(x => x.IsPublic))
            {
                var parameterEditorManager = _parameterEditorManager.GetParameterEditor(argument.GetType());
                if (parameterEditorManager != null)
                    _treeview.Items.Add(parameterEditorManager.CreateArgumentTreeViewItem(argument, null, _fileItem, argument.Section == lastGroupName ? null : argument.Section));

                lastGroupName = argument.Section;
            }
        }



        private void ButtonOnClick(object sender, EventArgs eventArgs)
        {
            GraphParameterEditorWindow window = new GraphParameterEditorWindow(_services, _graph);
            window.Accepted += WindowOnAccepted;
            window.Show(Screen);
        }



        private void WindowOnAccepted(object sender, EventArgs eventArgs)
        {
            RefreshParameterTreeView();
        }
    }
}