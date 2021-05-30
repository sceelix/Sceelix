using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Core.Parameters;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Designer.Graphs.Annotations;
using Sceelix.Designer.Graphs.GUI.Model;
using Sceelix.Designer.Graphs.Inspector.Graphs;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Graphs.ParameterEditors
{
    [ParameterEditor(typeof(OptionalListParameterInfo))]
    public class OptionalListParameterEditor : ParameterEditor<OptionalListParameterInfo>
    {
        private ParameterEditorManager _parameterEditorManager;


        public override void Initialize(IServiceLocator services)
        {
            base.Initialize(services);

            _parameterEditorManager = services.Get<ParameterEditorManager>();
        }


        public override TreeViewItem CreateArgumentTreeViewItem(ParameterInfo argument, VisualNode visualNode, FileItem fileItem, string group = null)
        {
            ListParameterInfo listArgument = (ListParameterInfo)argument;

            var argumentTreeViewItem = CreateCheckBoxListItem(listArgument, visualNode, fileItem, group);

            //if the user clicks the expansion button, store it in the parameter information
            argumentTreeViewItem.IsExpanded = listArgument.Expanded;
            var isExpandedProperty = argumentTreeViewItem.Properties.Get<bool>("IsExpanded");
            isExpandedProperty.Changed += (sender, args) => listArgument.Expanded = args.NewValue;

            return argumentTreeViewItem;
        }

        

        /// <summary>
        /// Handles the creation of a checkbox that, when checked, will force the subitems to appear.
        /// </summary>
        /// <param name="listParameterInfo"></param>
        /// <param name="visualNode"></param>
        /// <param name="fileItem"></param>
        /// <returns></returns>
        private ArgumentTreeViewItem CreateCheckBoxListItem(ListParameterInfo listParameterInfo, VisualNode visualNode, FileItem fileItem, String group)
        {
            var checkBox = new CheckBox();
            checkBox.Margin = new Vector4F(0, 5, 0, 0);
            checkBox.Height = 15;
            checkBox.IsChecked = false;

            ArgumentTreeViewItem argumentTreeViewItem = new ArgumentTreeViewItem(listParameterInfo, checkBox, visualNode, fileItem, group);

            if (listParameterInfo.ItemModels.Count == 0)
                return argumentTreeViewItem;

            //if it already has some items, add it as a subitem to the tree
            if (listParameterInfo.Items.Count > 0)
            {
                checkBox.IsChecked = true;

                var subParameterInfo = listParameterInfo.Items.First();

                if (subParameterInfo is CompoundParameterInfo)
                {
                    var parameterEditorManager = _parameterEditorManager.GetParameterEditor(subParameterInfo.GetType());
                    var items = parameterEditorManager.CreateArgumentTreeViewItem(subParameterInfo, visualNode, fileItem).Items;
                    argumentTreeViewItem.Items.AddRange(items);
                }
                else
                {
                    var parameterEditorManager = _parameterEditorManager.GetParameterEditor(subParameterInfo.GetType());
                    argumentTreeViewItem.Items.Add(parameterEditorManager.CreateArgumentTreeViewItem(subParameterInfo, visualNode, fileItem));
                }


                UpdatePorts(argumentTreeViewItem);
            }

            //when the checkbox is ticked
            var gameProperty = checkBox.Properties.Get<bool>("IsChecked");
            gameProperty.Changed += delegate
            {
                //if it became checked
                if (checkBox.IsChecked)
                {
                    //create a new item by copying the previous one
                    var modelInstance = (ParameterInfo)listParameterInfo.ItemModels[0].Clone();
                    listParameterInfo.Items.Add(modelInstance);

                    if (modelInstance is CompoundParameterInfo)
                    {
                        var parameterEditorManager = _parameterEditorManager.GetParameterEditor(modelInstance.GetType());
                        var items = parameterEditorManager.CreateArgumentTreeViewItem(modelInstance, visualNode, fileItem).Items;
                        argumentTreeViewItem.Items.AddRange(items);
                    }
                    else
                    {
                        var parameterEditorManager = _parameterEditorManager.GetParameterEditor(modelInstance.GetType());
                        argumentTreeViewItem.Items.Add(parameterEditorManager.CreateArgumentTreeViewItem(modelInstance, argumentTreeViewItem.VisualNode, argumentTreeViewItem.FileItem));
                    }
                }
                else
                {
                    argumentTreeViewItem.Items.Clear();
                    listParameterInfo.Items.Clear();
                }

                UpdatePorts(argumentTreeViewItem);

                AlertGraphChange(fileItem);
            };

            return argumentTreeViewItem;
        }

        private void UpdatePorts(ArgumentTreeViewItem argumentTreeViewItem)
        {
            if (argumentTreeViewItem.VisualNode != null)
            {
                argumentTreeViewItem.VisualNode.Node.RefreshParameterPorts();
                argumentTreeViewItem.VisualNode.RefreshParameterVisualPorts();
            }
        }

        public override string ParameterInfoName
        {
            get { return "Optional"; }
        }




        public override TreeViewItem CreateEditorTreeViewItem(GraphParameterEditorWindow editorWindow, ParameterInfo parameterInfo)
        {
            var listParameterInfo = (ListParameterInfo)parameterInfo;

            ParameterEditorTreeViewItem parameterEditorTreeViewItem = new ParameterEditorTreeViewItem(parameterInfo, editorWindow);
            parameterEditorTreeViewItem.AcceptsDrag += item => true;

            //load the subitems
            foreach (var itemModel in listParameterInfo.ItemModels)
                editorWindow.AddEditorTreeViewItem(parameterEditorTreeViewItem, itemModel);

            parameterEditorTreeViewItem.Items.CollectionChanged += delegate
            {
                listParameterInfo.ItemModels.Clear();
                listParameterInfo.ItemModels.AddRange(parameterEditorTreeViewItem.Items.OfType<ParameterEditorTreeViewItem>().Select(x => x.ParameterInfo));

                foreach (var item in listParameterInfo.ItemModels)
                    item.Parent = listParameterInfo;
            };

            return parameterEditorTreeViewItem;
        }
    }
}
