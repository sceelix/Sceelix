using System;
using System.Linq;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Collections;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Designer.Graphs.Annotations;
using Sceelix.Designer.Graphs.GUI.Model;
using Sceelix.Designer.Graphs.Inspector.Graphs;
using Sceelix.Designer.GUI;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Services;
using Sceelix.Extensions;

namespace Sceelix.Designer.Graphs.ParameterEditors
{
    [ParameterEditor(typeof(ListParameterInfo))]
    public class ListParameterEditor : ParameterEditor<ListParameterInfo>
    {
        private ParameterEditorManager _parameterEditorManager;


        public override void Initialize(IServiceLocator services)
        {
            base.Initialize(services);

            _parameterEditorManager = services.Get<ParameterEditorManager>();
        }

        /// <summary>
        /// Add controls to configure the parameter when added to the graph parameter list.
        /// </summary>
        /// <param name="parameterEditorTreeViewItem"></param>
        /// <returns></returns>
        public override UIControl CreateInspectorContent(ParameterEditorTreeViewItem parameterEditorTreeViewItem)
        {
            LayoutControl layoutControl = (LayoutControl) base.CreateInspectorContent(parameterEditorTreeViewItem);

            ListParameterInfo parameterInfo = (ListParameterInfo) parameterEditorTreeViewItem.ParameterInfo;

            var incrementSpinControl = (IntSpinControl) layoutControl.Add("Max. Size", new IntSpinControl() {Value = parameterInfo.MaxSize})[1];
            var gameProperty = incrementSpinControl.Properties.Get<int>("Value");
            gameProperty.Changed += (sender, args) => parameterInfo.MaxSize = args.NewValue;

            return layoutControl;
        }



        public override TreeViewItem CreateArgumentTreeViewItem(ParameterInfo argument, VisualNode visualNode, FileItem fileItem, string group = null)
        {
            ListParameterInfo listArgument = (ListParameterInfo) argument;
            ArgumentTreeViewItem argumentTreeViewItem = null;

            //if there's one only choice of item and maximum of over one item, use a simple button
            if (listArgument.ItemModels.Count == 1)
            {
                argumentTreeViewItem = CreateSingleModelListItem(listArgument, visualNode, fileItem);
            }
            //otherwise, use a button which allows the user to choose the type of item to add
            else
            {
                argumentTreeViewItem = CreateMultiModelListItem(listArgument, visualNode, fileItem);
            }

            //if the user clicks the expansion button, store it in the parameter information
            argumentTreeViewItem.IsExpanded = listArgument.Expanded;
            var isExpandedProperty = argumentTreeViewItem.Properties.Get<bool>("IsExpanded");
            isExpandedProperty.Changed += (sender, args) => listArgument.Expanded = args.NewValue;

            return argumentTreeViewItem;
        }

        

        private void AddDeleteMenuItem(ArgumentTreeViewItem childTreeViewItem)
        {
            //var listParameterInfo = (ListParameterInfo) parentTreeViewItem.Argument;

            var menuItem = new MenuItem() {Content = new TextBlock() {Text = "Delete"}};
            menuItem.Click += delegate { DeleteListItem(childTreeViewItem); };

            childTreeViewItem.ParameterMenu.Items.Add(menuItem);
        }



        private void DeleteListItem(ArgumentTreeViewItem childTreeViewItem)
        {
            var parentTreeViewItem = (ArgumentTreeViewItem) childTreeViewItem.Parent;
            var listParameterInfo = (ListParameterInfo) parentTreeViewItem.Argument;

            parentTreeViewItem.Items.Remove(childTreeViewItem);
            listParameterInfo.Items.Remove(childTreeViewItem.Argument);

            AlertGraphChange(parentTreeViewItem.FileItem);

            UpdatePorts(parentTreeViewItem);

            UpdateAddButton(listParameterInfo, ((FlexibleStackPanel) parentTreeViewItem.Control).Children.Last());
        }



        private void UpdateAddButton(ListParameterInfo listParameterInfo, UIControl control)
        {
            control.IsEnabled = !listParameterInfo.ReachedLimit;
        }



        private void UpdatePorts(ArgumentTreeViewItem argumentTreeViewItem)
        {
            if (argumentTreeViewItem.VisualNode != null)
            {
                argumentTreeViewItem.VisualNode.Node.RefreshParameterPorts();
                argumentTreeViewItem.VisualNode.RefreshParameterVisualPorts();
            }
        }




        public override TreeViewItem CreateEditorTreeViewItem(GraphParameterEditorWindow editorWindow, ParameterInfo parameterInfo)
        {
            var listParameterInfo = (ListParameterInfo) parameterInfo;

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



        public override bool SetValue(ArgumentTreeViewItem item, object value)
        {
            var listArgument = (ListParameterInfo) item.Argument;
            var newListValue = (SceeList) value;
            var valueChanged = false;

            var extraCount = newListValue.Count - listArgument.Items.Count;

            //some data has been deleted
            if (extraCount < 0)
            {
                var children = item.Items.OfType<ArgumentTreeViewItem>().ToList();

                //delete the extras
                for (int i = 0; i < -extraCount; i++)
                {
                    //always delete the last item
                    DeleteListItem(children.Last());
                }
            }
            else if (extraCount > 0)
            {
                //if there is only one model
                if (listArgument.ItemModels.Count == 1)
                {
                    for (int i = 0; i < extraCount; i++)
                    {
                        var childItem = AddNewSingleListItem(item);
                        var childItemEditor = _parameterEditorManager.GetParameterEditor(childItem.Argument.GetType());
                        childItemEditor.SetValue(childItem, newListValue[listArgument.Items.Count - 1]);
                        childItem.BringIntoView();
                    }
                }
                else
                {
                    //TODO: Deletion for more models!
                }

                valueChanged = true;
            }


            return valueChanged;
        }

        #region CreateSingleModelListItem

        private ArgumentTreeViewItem CreateSingleModelListItem(ListParameterInfo listArgument, VisualNode visualNode, FileItem fileItem)
        {
            FlexibleStackPanel flexibleStack = new FlexibleStackPanel() {HorizontalAlignment = HorizontalAlignment.Stretch, Orientation = Orientation.Horizontal};
            flexibleStack.Children.Add(new TextBlock() {Text = " ", HorizontalAlignment = HorizontalAlignment.Stretch});

            var textButton = new TextButton() {Text = "+", Width = 25};
            flexibleStack.Children.Add(textButton);
            var argumentTreeViewItem = new ArgumentTreeViewItem(listArgument, flexibleStack, visualNode, fileItem, null) {CanBeDragged = true};
            argumentTreeViewItem.CanDragInto = item => listArgument.SupportsItemModel(item.Argument) && !listArgument.ReachedLimit;
            argumentTreeViewItem.DragInto = delegate(ArgumentTreeViewItem item, int i)
            {
                //if the parent is the same and the item is going to a higher index...
                if (item.Parent == argumentTreeViewItem
                    && i >= item.Parent.Items.IndexOf(item))
                    i--;

                var parentArgumentTreeViewItem = (ArgumentTreeViewItem) item.Parent;
                var parentArgument = item.Argument.Parent;

                item.Parent.Items.Remove(item);
                argumentTreeViewItem.Items.Insert(i, item);

                item.Argument.Parent.CastTo<ListParameterInfo>().Items.Remove(item.Argument);
                item.Argument.Parent = listArgument;
                listArgument.Items.Insert(i, item.Argument);

                UpdatePorts(argumentTreeViewItem);

                //update the old list
                UpdateAddButton((ListParameterInfo) parentArgument, parentArgumentTreeViewItem.Control.CastTo<FlexibleStackPanel>().Children.Last());

                //update the new list
                UpdateAddButton(listArgument, textButton);
            };

            textButton.UserData = argumentTreeViewItem;
            textButton.Click += delegate
            {
                AddNewSingleListItem(argumentTreeViewItem);

                AlertGraphChange(argumentTreeViewItem.FileItem);
            };

            foreach (var subArgument in listArgument.Items)
            {
                var parameterEditorManager = _parameterEditorManager.GetParameterEditor(subArgument.GetType());
                if (parameterEditorManager != null)
                {
                    var childItem = (ArgumentTreeViewItem) parameterEditorManager.CreateArgumentTreeViewItem(subArgument, visualNode, fileItem);
                    childItem.CanBeDragged = true;
                    argumentTreeViewItem.Items.Add(childItem);

                    AddDeleteMenuItem(childItem);
                }
            }

            return argumentTreeViewItem;
        }



        private ArgumentTreeViewItem AddNewSingleListItem(ArgumentTreeViewItem argumentTreeViewItem)
        {
            var listArgument = (ListParameterInfo) argumentTreeViewItem.Argument;

            var subArgument = listArgument.ItemModels[0].CloneModel(true).CastTo<ParameterInfo>();
            subArgument.Parent = listArgument;
            listArgument.Items.Add(subArgument);

            var parameterEditorManager = _parameterEditorManager.GetParameterEditor(subArgument.GetType());
            var childItem = (ArgumentTreeViewItem) parameterEditorManager.CreateArgumentTreeViewItem(subArgument, argumentTreeViewItem.VisualNode, argumentTreeViewItem.FileItem);
            childItem.CanBeDragged = true;
            AddDeleteMenuItem(childItem);

            UpdatePorts(argumentTreeViewItem);
            UpdateAddButton(listArgument, argumentTreeViewItem.Control.GetDescendants().OfType<TextButton>().First());

            argumentTreeViewItem.Items.Add(childItem);

            return childItem;
        }

        #endregion

        #region CreateMultiModelListItem

        private ArgumentTreeViewItem CreateMultiModelListItem(ListParameterInfo listArgument, VisualNode visualNode, FileItem fileItem)
        {
            FlexibleStackPanel flexibleStack = new FlexibleStackPanel() {HorizontalAlignment = HorizontalAlignment.Stretch, Orientation = Orientation.Horizontal};
            flexibleStack.Children.Add(new TextBlock() {Text = " ", HorizontalAlignment = HorizontalAlignment.Stretch});

            var textButton = new TextButton() {Text = "+", Width = 25};
            flexibleStack.Children.Add(textButton);
            var argumentTreeViewItem = new ArgumentTreeViewItem(listArgument, flexibleStack, visualNode, fileItem, null);
            argumentTreeViewItem.CanDragInto = item => listArgument.SupportsItemModel(item.Argument) && !listArgument.ReachedLimit;
            argumentTreeViewItem.DragInto = delegate(ArgumentTreeViewItem item, int i)
            {
                //if the parent is the same and the item is going to a higher index...
                if (item.Parent == argumentTreeViewItem
                    && i >= item.Parent.Items.IndexOf(item))
                    i--;

                var parentArgumentTreeViewItem = (ArgumentTreeViewItem) item.Parent;
                var parentArgument = item.Argument.Parent;

                //remove the 
                item.Parent.Items.Remove(item);
                argumentTreeViewItem.Items.Insert(i, item);

                //do the same inside the arguments
                item.Argument.Parent.CastTo<ListParameterInfo>().Items.Remove(item.Argument);
                item.Argument.Parent = listArgument;
                listArgument.Items.Insert(i, item.Argument);

                UpdatePorts(argumentTreeViewItem);

                //update the old list
                UpdateAddButton((ListParameterInfo) parentArgument, parentArgumentTreeViewItem.Control.CastTo<FlexibleStackPanel>().Children.Last());

                //update the new list
                UpdateAddButton(listArgument, textButton);
            };

            textButton.UserData = argumentTreeViewItem;
            textButton.Click += TextButtonMultipleModelsOnClick;

            foreach (var subArgument in listArgument.Items)
            {
                var parameterEditorManager = _parameterEditorManager.GetParameterEditor(subArgument.GetType());
                if (parameterEditorManager != null)
                {
                    var childItem = (ArgumentTreeViewItem) parameterEditorManager.CreateArgumentTreeViewItem(subArgument, visualNode, fileItem);
                    childItem.CanBeDragged = true;
                    argumentTreeViewItem.Items.Add(childItem);

                    AddDeleteMenuItem(childItem);
                }
            }

            return argumentTreeViewItem;
        }



        /// <summary>
        /// For the third case, max bigger than 1, multiple models
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextButtonMultipleModelsOnClick(object sender, EventArgs eventArgs)
        {
            var textButton = (TextButton) sender;
            var argumentTreeViewItem = textButton.UserData.CastTo<ArgumentTreeViewItem>();

            var parameterInfo = (ListParameterInfo) argumentTreeViewItem.Argument;

            ContextMenu menu = new ContextMenu();
            foreach (var modelInfo in parameterInfo.ItemModels)
            {
                var item = new MenuItem {Content = new TextBlock() {Text = modelInfo.Label}, UserData = new Object[] {argumentTreeViewItem, modelInfo}};
                item.Click += ItemOnClick;
                menu.Items.Add(item);
            }

            menu.Open(textButton, textButton.InputService.MousePosition);


            /*MultiContextMenu contextMenu = new MultiContextMenu();

            foreach (var modelInfo in parameterInfo.ParameterModelInfos)
            {
                contextMenu.MenuChildren.Add(new MenuChild(CreateItem) { Text = modelInfo.Label,UserData = new Object[] { argumentTreeViewItem, modelInfo } });
            }
            contextMenu.Open(textButton.Screen,textButton.InputService.MousePosition);*/
        }



        private void ItemOnClick(object sender, EventArgs eventArgs)
        {
            var userData = (Object[]) ((MenuItem) sender).UserData;

            var argumentTreeViewItem = (ArgumentTreeViewItem) userData[0];
            var parameterModelInfo = (ParameterInfo) userData[1];

            var parameterInfo = (ListParameterInfo) argumentTreeViewItem.Argument;

            var subArgument = parameterModelInfo.CloneModel(true).CastTo<ParameterInfo>();//CloneRecursive!
            subArgument.Parent = parameterInfo;
            parameterInfo.Items.Add(subArgument);

            var parameterEditorManager = _parameterEditorManager.GetParameterEditor(subArgument.GetType());
            if (parameterEditorManager != null)
            {
                var childItem = (ArgumentTreeViewItem) parameterEditorManager.CreateArgumentTreeViewItem(subArgument, argumentTreeViewItem.VisualNode, argumentTreeViewItem.FileItem);
                childItem.CanBeDragged = true;
                AddDeleteMenuItem(childItem);

                argumentTreeViewItem.Items.Add(childItem);

                UpdatePorts(argumentTreeViewItem);
                UpdateAddButton(parameterInfo, ((FlexibleStackPanel) argumentTreeViewItem.Control).Children.Last());

                AlertGraphChange(argumentTreeViewItem.FileItem);

                //if (!argumentTreeViewItem.IsExpanded)
                argumentTreeViewItem.IsExpanded = true;
            }
        }

        #endregion

        /*private void CreateItem(MenuChild obj)
        {
            var userData = (Object[]) obj.UserData;

            var argumentTreeViewItem = (ArgumentTreeViewItem) userData[0];
            var parameterModelInfo = (ParameterInfo)userData[1];

            var parameterInfo = (AlternativeListParameterInfo)argumentTreeViewItem.Argument;

            var subArgument = parameterModelInfo.Clone().As<ParameterInfo>();
            parameterInfo.ParameterInfos.Add(subArgument);

            var parameterEditorManager = GetParameterEditorManager(subArgument.GetType());
            if (parameterEditorManager != null)
                argumentTreeViewItem.Items.Add(parameterEditorManager.CreateInspectorTreeViewItem(subArgument, argumentTreeViewItem.Node));
        }*/


        /*private void DeleteItem(MenuChild obj)
        {
            var item = (EditorTreeViewItem)obj.UserData;

            var parentInfo = (ListParameterInfo)((EditorTreeViewItem) item.Parent).ParameterInfo;

            item.Parent.Items.Remove(item);

            //remove it from the list of listparameter items
            parentInfo.ItemModels.Remove(item.ParameterInfo);
        }*/


        /*private void AddEditorTreeViewItem(GraphParameterEditorWindow editorWindow, EditorTreeViewItem parent, ParameterInfo parameterInfo)
        {
            var parameterEditor = GetParameterEditor(parameterInfo.GetType());

            var editorTreeViewItem = (EditorTreeViewItem)parameterEditor.CreateEditorTreeViewItem(editorWindow,parameterInfo);

            //when the user clicks on it, the properties will appear on the right
            editorTreeViewItem.Click += editorWindow.EditorTreeViewItemOnClick;

            //add the option for deletion in a right-click menu
            editorTreeViewItem.MultiContextMenu.MenuChildren.Add(new MenuChild(DeleteItem) { Text = "Delete", UserData = editorTreeViewItem });

            //now add it to the treeview
            parent.Items.Add(editorTreeViewItem);
        }*/


        /*private void SubItemCreated(ParameterEditorTreeViewItem item, ParameterInfo newChild)
        {
            var parentListInfo = (ListParameterInfo)item.ParameterInfo;

            parentListInfo.ItemModels.Add(newChild);
        }

        private void EditorTreeViewItemOnDeleted(object sender, EventArgs eventArgs)
        {
            var itemToDelete = (ParameterEditorTreeViewItem)sender;
            var parent = (ParameterEditorTreeViewItem) itemToDelete.Parent;
            var parentListInfo = (ListParameterInfo)parent.ParameterInfo;

            //remove from the treeview
            parent.Items.Remove(itemToDelete);

            //remove it from the list of listparameter items
            parentListInfo.ItemModels.Remove(itemToDelete.ParameterInfo);
        }*/


        /*private void DropDownOnPropertyChanged(object sender, GamePropertyEventArgs e)
        {
            if (e.Property.Name == "SelectedIndex")
            {
                var dropDownButton = sender.CastTo<DropDownButton>();
                var argumentTreeViewItem = dropDownButton.UserData.CastTo<ArgumentTreeViewItem>();
                
                var alternativeListParameterInfo = (ListParameterInfo)argumentTreeViewItem.Argument;

                string selectedLabel = (String)dropDownButton.Items[dropDownButton.SelectedIndex];

                
                //alternativeListParameterInfo.Items[0] ?? alternativeListParameterInfo.Items[0].Value;

                //always remove the existing item
                argumentTreeViewItem.Items.Clear();
                alternativeListParameterInfo.Items.Clear();

                if (!String.IsNullOrWhiteSpace(selectedLabel))
                {
                    var modelInstance = (ParameterInfo)alternativeListParameterInfo.ItemModels.First(x => x.Label == selectedLabel).Clone();
                    alternativeListParameterInfo.Items.Add(modelInstance);

                    if (modelInstance is CompoundParameterInfo)
                    {
                        var parameterEditorManager = GetParameterEditor(modelInstance.GetType());
                        var items = parameterEditorManager.CreateArgumentTreeViewItem(modelInstance, argumentTreeViewItem.VisualNode, argumentTreeViewItem.FileItem).Items;
                        argumentTreeViewItem.Items.AddRange(items);
                    }
                    else
                    {
                        var parameterEditorManager = GetParameterEditor(modelInstance.GetType());
                        argumentTreeViewItem.Items.Add(parameterEditorManager.CreateArgumentTreeViewItem(modelInstance, argumentTreeViewItem.VisualNode, argumentTreeViewItem.FileItem));
                    }

                    //var parameterEditorManager = GetParameterEditor(modelInstance.GetType());
                    //argumentTreeViewItem.Items.Add(parameterEditorManager.CreateArgumentTreeViewItem(modelInstance, argumentTreeViewItem.VisualNode, argumentTreeViewItem.FileItem));
                }

                AlertGraphChange(argumentTreeViewItem.FileItem);
            }
        }*/


        /*private void CheckBoxOnPropertyChanged(object sender, GamePropertyEventArgs e)
        {
            if (e.Property.Name == "IsChecked")
            {
                var checkBox = sender.CastTo<CheckBox>();
                var argumentTreeViewItem = checkBox.UserData.CastTo<ArgumentTreeViewItem>();
                var alternativeListParameterInfo = (ListParameterInfo)argumentTreeViewItem.Argument;

                if (checkBox.IsChecked)
                {
                    var modelInstance = (ParameterInfo)alternativeListParameterInfo.ItemModels[0].Clone();
                    alternativeListParameterInfo.Items.Add(modelInstance);

                    var parameterEditorManager = GetParameterEditor(modelInstance.GetType());
                    argumentTreeViewItem.Items.Add(parameterEditorManager.CreateArgumentTreeViewItem(modelInstance, argumentTreeViewItem.VisualNode, argumentTreeViewItem.FileItem));
                }
                else
                {
                    argumentTreeViewItem.Items.Clear();
                    alternativeListParameterInfo.Items.Clear();
                }
            }
        }*/
    }
}