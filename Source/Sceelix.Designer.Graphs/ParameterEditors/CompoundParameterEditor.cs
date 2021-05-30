using System;
using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Collections;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Designer.Graphs.Annotations;
using Sceelix.Designer.Graphs.GUI.Model;
using Sceelix.Designer.Graphs.Inspector.Graphs;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Graphs.ParameterEditors
{
    [ParameterEditor(typeof(CompoundParameterInfo))]
    public class CompoundParameterEditor : ParameterEditor<CompoundParameterInfo>, IServiceable
    {
        private ParameterEditorManager _parameterEditorManager;


        public override void Initialize(IServiceLocator services)
        {
            base.Initialize(services);

            _parameterEditorManager = services.Get<ParameterEditorManager>();
        }



        public override TreeViewItem CreateArgumentTreeViewItem(ParameterInfo argument, VisualNode visualNode, FileItem fileItem, string group)
        {
            ArgumentTreeViewItem argumentTreeViewItem;

            CompoundParameterInfo compoundArgument = (CompoundParameterInfo) argument;

            if (compoundArgument.ArrangeInSingleLine)
            {
                FlexibleStackPanel horizontalStackPanel = new FlexibleStackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                Action onChanged = () => AlertGraphChange(fileItem);

                foreach (var subArgument in compoundArgument.Fields)
                {
                    var parameterEditorManager = _parameterEditorManager.GetParameterEditor(subArgument.GetType());
                    if (parameterEditorManager != null)
                    {
                        UIControl uiControl = parameterEditorManager.CreateControl(subArgument, fileItem, onChanged);
                        if (uiControl != null)
                        {
                            var leftMargin = horizontalStackPanel.Children.Count > 0 ? 5 : 0;

                            horizontalStackPanel.Children.Add(new TextBlock {Text = subArgument.Label, Margin = new Vector4F(leftMargin, 3, 5, 0)});
                            horizontalStackPanel.Children.Add(uiControl);
                        }
                    }
                }

                argumentTreeViewItem = new ArgumentTreeViewItem(argument, horizontalStackPanel, visualNode, fileItem, group);
            }
            else
            {
                argumentTreeViewItem = new ArgumentTreeViewItem(argument, new UIControl(), visualNode, fileItem, group);

                String lastGroupName = null;
                foreach (var subArgument in compoundArgument.Fields)
                {
                    var parameterEditorManager = _parameterEditorManager.GetParameterEditor(subArgument.GetType());
                    if (parameterEditorManager != null)
                        argumentTreeViewItem.Items.Add(parameterEditorManager.CreateArgumentTreeViewItem(subArgument, visualNode, fileItem, subArgument.Section == lastGroupName ? null : subArgument.Section));

                    lastGroupName = subArgument.Section;
                }
            }

            //slight visual style improvement: if this treeitem has no children, remove the ":" from the label
            if (compoundArgument.Fields.Count == 0)
                argumentTreeViewItem.TextBlock.Text = compoundArgument.Label;

            argumentTreeViewItem.IsExpanded = compoundArgument.Expanded;
            var isExpandedProperty = argumentTreeViewItem.Properties.Get<bool>("IsExpanded");
            isExpandedProperty.Changed += (sender, args) => compoundArgument.Expanded = args.NewValue;

            return argumentTreeViewItem;
        }



        public override UIControl CreateInspectorContent(ParameterEditorTreeViewItem parameterEditorTreeViewItem)
        {
            LayoutControl layoutControl = (LayoutControl) base.CreateInspectorContent(parameterEditorTreeViewItem);

            CompoundParameterInfo parameterInfo = (CompoundParameterInfo) parameterEditorTreeViewItem.ParameterInfo;

            layoutControl.Add("Expanded:", new CheckBox() {IsChecked = parameterInfo.Expanded, Margin = new Vector4F(0, 5, 0, 0), Height = 15});
            var property1 = layoutControl.Last()[1].Properties.Get<bool>("IsChecked");
            property1.Changed += (sender, args) => parameterInfo.Expanded = args.NewValue;

            layoutControl.Add("Arrange In Single Line:", new CheckBox() {IsChecked = parameterInfo.ArrangeInSingleLine, Margin = new Vector4F(0, 5, 0, 0), Height = 15});
            var property2 = layoutControl.Last()[1].Properties.Get<bool>("IsChecked");
            property2.Changed += (sender, args) => parameterInfo.ArrangeInSingleLine = args.NewValue;

            return layoutControl;
        }



        public override TreeViewItem CreateEditorTreeViewItem(GraphParameterEditorWindow editorWindow, ParameterInfo parameterInfo)
        {
            var compoundParameterInfo = (CompoundParameterInfo) parameterInfo;

            ParameterEditorTreeViewItem parameterEditorTreeViewItem = new ParameterEditorTreeViewItem(parameterInfo, editorWindow);
            parameterEditorTreeViewItem.AcceptsDrag += item => true;

            //add the items
            foreach (var itemModel in compoundParameterInfo.Fields)
                editorWindow.AddEditorTreeViewItem(parameterEditorTreeViewItem, itemModel);

            parameterEditorTreeViewItem.Items.CollectionChanged += delegate
            {
                compoundParameterInfo.Fields.Clear();
                compoundParameterInfo.Fields.AddRange(parameterEditorTreeViewItem.Items.OfType<ParameterEditorTreeViewItem>().Select(x => x.ParameterInfo));

                foreach (var field in compoundParameterInfo.Fields)
                    field.Parent = compoundParameterInfo;
            };

            return parameterEditorTreeViewItem;
        }



        public override bool SetValue(ArgumentTreeViewItem item, object value)
        {
            var compoundValue = (SceeList) value;
            var valueChanged = false;

            foreach (var keyValue in compoundValue.KeyValues)
            {
                var childItem = item.Items.OfType<ArgumentTreeViewItem>().FirstOrDefault(x => x.Argument.Label == keyValue.Key);
                if (childItem != null)
                {
                    var childEditor = _parameterEditorManager.GetParameterEditor(childItem);
                    valueChanged |= childEditor.SetValue(childItem, keyValue.Value);
                }
            }

            return valueChanged;
        }



        private void ItemsOnCollectionChanged(object sender, CollectionChangedEventArgs<TreeViewItem> collectionChangedEventArgs)
        {
            //for simplification at this point, just refresh the whole list of parameterinfos
        }



        private void DragInto(EditorTreeViewItem argumentTreeViewItem, int i)
        {
        }



        private void SubItemCreated(ParameterEditorTreeViewItem item, ParameterInfo newChild)
        {
            var parentListInfo = (CompoundParameterInfo) item.ParameterInfo;

            parentListInfo.Fields.Add(newChild);
        }



        private void EditorTreeViewItemOnDeleted(object sender, EventArgs eventArgs)
        {
            var itemToDelete = (ParameterEditorTreeViewItem) sender;
            var parent = (ParameterEditorTreeViewItem) itemToDelete.Parent;
            var parentListInfo = (CompoundParameterInfo) parent.ParameterInfo;

            //remove from the treeview
            parent.Items.Remove(itemToDelete);

            //remove it from the list of listparameter items
            parentListInfo.Fields.Remove(itemToDelete.ParameterInfo);
        }
    }
}