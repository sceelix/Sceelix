using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Designer.Graphs.GUI.Model;
using Sceelix.Designer.Graphs.Messages;
using Sceelix.Designer.Graphs.ParameterEditors;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Services;
using Sceelix.Extensions;

namespace Sceelix.Designer.Graphs.Inspector.Nodes
{
    public class ParameterController : GroupBox
    {
        private readonly FileItem _fileItem;
        private readonly IServiceLocator _services;
        private readonly LayoutTreeView _treeview;
        private readonly VisualNode _visualNode;
        private ParameterEditorManager _parameterEditorManager;


        public ParameterController(IServiceLocator services, VisualNode visualNode, FileItem fileItem)
        {
            _services = services;
            _visualNode = visualNode;
            _fileItem = fileItem;

            _parameterEditorManager = services.Get<ParameterEditorManager>();

            Title = "Parameters";
            HorizontalAlignment = HorizontalAlignment.Stretch;
            Margin = new Vector4F(0, 2, 0, 0);

            StackPanel panel = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Orientation = Orientation.Vertical,
                Margin = new Vector4F(5)
            };

            _treeview = new LayoutTreeView()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Vector4F(0)
            };
            panel.Children.Add(_treeview);

            String lastGroupName = null;
            foreach (var argument in visualNode.Node.Parameters.Where(x => x.IsPublic))
            {
                var parameterEditor = _parameterEditorManager.GetParameterEditor(argument.GetType());
                if (parameterEditor != null)
                    _treeview.Items.Add(parameterEditor.CreateArgumentTreeViewItem(argument, visualNode, _fileItem, argument.Section == lastGroupName ? null : argument.Section));

                lastGroupName = argument.Section;
            }

            visualNode.VisualGraph.Control.Services.Get<MessageManager>().Register<ChangeParameterValue>(OnChangeParameterValue);

            Content = panel;
        }



        protected override void OnUnload()
        {
            base.OnUnload();

            _visualNode.VisualGraph.Control.Services.Get<MessageManager>().Unregister<ChangeParameterValue>(OnChangeParameterValue);
        }



        private void OnChangeParameterValue(ChangeParameterValue obj)
        {
            var strings = new Queue<String>(obj.FullParameterName.Split('.'));

            SearchAndChange(strings, _treeview, obj);
        }



        private void SearchAndChange(Queue<string> strings, ITreeViewControl control, ChangeParameterValue changeParameterValue)
        {
            if (strings.Count == 0)
            {
                ArgumentTreeViewItem item = (ArgumentTreeViewItem) control;

                var parameterEditor = _parameterEditorManager.GetParameterEditor(item.Argument.GetType());
                if (parameterEditor.SetValue(item, changeParameterValue.Value))
                    _services.Get<MessageManager>().Publish(new GraphContentChanged(_fileItem, true));
            }
            else
            {
                int value;
                var dequeue = strings.Dequeue();
                if (Int32.TryParse(dequeue, out value))
                {
                    //ignore the next string
                    strings.Dequeue();

                    SearchAndChange(strings, control.Items[value], changeParameterValue);
                }
                else
                {
                    var firstOrDefault = control.Items.FirstOrDefault(x => x.CastTo<ArgumentTreeViewItem>().Argument.Label == dequeue);
                    if (firstOrDefault != null)
                    {
                        SearchAndChange(strings, firstOrDefault, changeParameterValue);
                    }
                }
            }
        }



        /*protected override Vector2F OnMeasure(Vector2F availableSize)
        {
            if (_treeview.Items.Count > 0)
            {
                base.OnMeasure(availableSize);

                var max = _treeview.Items.OfType<ArgumentTreeViewItem>().Max(val => val.GetMaxTextWidth(0));

                foreach (var result in _treeview.Items.OfType<ArgumentTreeViewItem>())
                {
                    result.SetMaxTextWidth(0, max);
                }
            }
            
            return base.OnMeasure(availableSize);
        }*/
    }
}