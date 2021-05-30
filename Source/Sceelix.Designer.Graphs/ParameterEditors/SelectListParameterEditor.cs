using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalRune.Game.UI.Controls;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Designer.Graphs.Annotations;
using Sceelix.Designer.Graphs.GUI.Model;
using Sceelix.Designer.Graphs.Inspector.Graphs;
using Sceelix.Designer.GUI;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Graphs.ParameterEditors
{
    [ParameterEditor(typeof(SelectListParameterInfo))]
    class SelectListParameterEditor : ParameterEditor<SelectListParameterInfo>
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

            var argumentTreeViewItem = CreateDropDownListItem(listArgument, visualNode, fileItem, group);

            //if the user clicks the expansion button, store it in the parameter information
            argumentTreeViewItem.IsExpanded = listArgument.Expanded;
            var isExpandedProperty = argumentTreeViewItem.Properties.Get<bool>("IsExpanded");
            isExpandedProperty.Changed += (sender, args) => listArgument.Expanded = args.NewValue;

            return argumentTreeViewItem;
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



        public override string ParameterInfoName
        {
            get { return "Select"; }
        }
        

        private ArgumentTreeViewItem CreateDropDownListItem(ListParameterInfo listParameterInfo, VisualNode visualNode, FileItem fileItem, string group)
        {
            var downButton = new DropDownButton();

            //if (listParameterInfo.)
            //downButton.Items.Add(" ");
            downButton.Items.AddRange(listParameterInfo.ItemModels.Select(x => x.Label));

            ArgumentTreeViewItem argumentTreeViewItem = new ArgumentTreeViewItem(listParameterInfo, downButton, visualNode, fileItem, group);

            //if there are no models (if the user got in wrong in the parameter editor, for instance)
            //just return an empty combobox
            if (listParameterInfo.ItemModels.Count == 0)
                return argumentTreeViewItem;


            if (listParameterInfo.Items.Count == 0)
            {
                var modelInstance = (ParameterInfo)listParameterInfo.ItemModels.First().Clone();
                listParameterInfo.Items.Insert(0, modelInstance);
            }

            //if (listParameterInfo.Items.Count > 0)
            {
                downButton.SelectedIndex = downButton.Items.IndexOf(listParameterInfo.Items[0].Label);

                var subArgument = listParameterInfo.Items[0];

                if (subArgument is CompoundParameterInfo)
                {
                    var parameterEditorManager = _parameterEditorManager.GetParameterEditor(subArgument.GetType());
                    var items = parameterEditorManager.CreateArgumentTreeViewItem(subArgument, visualNode, fileItem).Items;
                    argumentTreeViewItem.Items.AddRange(items);

                    //add a description tooltip to the combobox itself
                    downButton.ToolTip = new ToolTipControl(subArgument.Label, subArgument.Description);
                }
                else
                {
                    var parameterEditorManager = _parameterEditorManager.GetParameterEditor(subArgument.GetType());
                    argumentTreeViewItem.Items.Add(parameterEditorManager.CreateArgumentTreeViewItem(subArgument, visualNode, fileItem));
                }

                UpdatePorts(argumentTreeViewItem);
            }
            var property = downButton.Properties.Get<int>("SelectedIndex");
            property.Changed += delegate
            {
                string selectedLabel = (String)downButton.Items[downButton.SelectedIndex];

                //always remove the existing item
                argumentTreeViewItem.Items.Clear();
                //listParameterInfo.Items.Clear();

                if (!String.IsNullOrWhiteSpace(selectedLabel))
                {
                    var modelInstance = listParameterInfo.Items.FirstOrDefault(x => x.Label == selectedLabel);
                    if (modelInstance != null)
                    {
                        listParameterInfo.Items.Remove(modelInstance);
                        listParameterInfo.Items.Insert(0, modelInstance);
                    }
                    else
                    {
                        modelInstance = (ParameterInfo)listParameterInfo.ItemModels.First(x => x.Label == selectedLabel).Clone();
                        listParameterInfo.Items.Insert(0, modelInstance);
                    }

                    if (modelInstance is CompoundParameterInfo)
                    {
                        var parameterEditorManager = _parameterEditorManager.GetParameterEditor(modelInstance.GetType());
                        var items = parameterEditorManager.CreateArgumentTreeViewItem(modelInstance, argumentTreeViewItem.VisualNode, argumentTreeViewItem.FileItem).Items;
                        argumentTreeViewItem.Items.AddRange(items);

                        //add a description tooltip to the combobox itself
                        downButton.ToolTip = new ToolTipControl(modelInstance.Label, modelInstance.Description);
                    }
                    else
                    {
                        var parameterEditorManager = _parameterEditorManager.GetParameterEditor(modelInstance.GetType());
                        argumentTreeViewItem.Items.Add(parameterEditorManager.CreateArgumentTreeViewItem(modelInstance, argumentTreeViewItem.VisualNode, argumentTreeViewItem.FileItem));
                    }

                    //var parameterEditorManager = GetParameterEditor(modelInstance.GetType());
                    //argumentTreeViewItem.Items.Add(parameterEditorManager.CreateArgumentTreeViewItem(modelInstance, argumentTreeViewItem.VisualNode, argumentTreeViewItem.FileItem));
                }

                UpdatePorts(argumentTreeViewItem);

                AlertGraphChange(argumentTreeViewItem.FileItem);
            };

            //downButton.UserData = argumentTreeViewItem;
            //downButton.PropertyChanged += DropDownOnPropertyChanged;

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
    }
}
