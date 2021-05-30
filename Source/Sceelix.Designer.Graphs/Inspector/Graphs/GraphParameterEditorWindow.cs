using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Core.Graphs;
using Sceelix.Core.Parameters;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Designer.Graphs.ParameterEditors;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.MenuControls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Graphs.Inspector.Graphs
{
    public class GraphParameterEditorWindow : DialogWindow
    {
        private readonly Graph _graph;
        private Button _cancelButton;
        private ContentControl _inspectorPanel;
        private Button _okButton;
        private TreeView _treeview;
        
        private ParameterEditorManager _parameterEditorManager;


        public GraphParameterEditorWindow(IServiceLocator services, Graph graph)
        {
            _graph = graph;
            _parameterEditorManager = services.Get<ParameterEditorManager>();

        }







        protected override void OnLoad()
        {
            base.OnLoad();


            Width = 800;
            Height = 500;
            Title = "Edit Parameters";



            var stackPanelMain = new FlexibleStackPanel
            {
                Margin = new Vector4F(4),
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            var stackPanelTreeViewInspector = new EqualStackPanel()
            {
                Margin = new Vector4F(4),
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            //Left side, the treeview containing the parameters
            stackPanelTreeViewInspector.Children.Add(
                new GroupBox()
                {
                    Title = "Parameters",
                    Content = new VerticalScrollViewer()
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Content = _treeview = new TreeView()
                        {
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch
                        }
                    },
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                });

            //_treeview.InputProcessed += TreeviewOnInputProcessed;


            //Right side, where the options of the parameterinfo will appear
            stackPanelTreeViewInspector.Children.Add(
                new GroupBox()
                {
                    Title = "Inspector",
                    Content = _inspectorPanel = new VerticalScrollViewer()
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch
                    },
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                });


            stackPanelMain.Children.Add(stackPanelTreeViewInspector);

            var stackPanelButtons = new StackPanel
            {
                Margin = new Vector4F(4),
                Orientation = Orientation.Horizontal,
                //Height = 40,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            stackPanelButtons.Children.Add(_okButton = new Button
            {
                Content = new TextBlock()
                {
                    Text = "OK",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                Margin = new Vector4F(4),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 85,
                Height = 25,
                IsDefault = true
            });
            _okButton.Click += OkButtonOnClick;


            stackPanelButtons.Children.Add(_cancelButton = new Button
            {
                Content = new TextBlock()
                {
                    Text = "Cancel",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                Margin = new Vector4F(4),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 85,
                Height = 25,
                IsCancel = true
            });
            _cancelButton.Click += CancelButtonOnClick;


            stackPanelMain.Children.Add(new ContentControl()
            {
                Content = stackPanelButtons,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
            });

            Content = stackPanelMain;

            FillParameterGroupBox();
        }



        private void FillParameterGroupBox()
        {
            EditorTreeViewItem superItem = new EditorTreeViewItem("Parameters");
            superItem.AcceptsDrag = item => true;

            //when the user clicks on it, the properties will appear on the right
            superItem.Click += (sender, args) => _inspectorPanel.Content = null;

            var addMenuItem = new MenuChild() { Text = "Add" };
            superItem.MultiContextMenu.MenuChildren.Add(addMenuItem);

            addMenuItem.MenuChildren.AddRange(CreateAddMenu(superItem));

            _treeview.Items.Add(superItem);

            //load the items to the treeview
            foreach (var parameterInfo in _graph.ParameterInfos)
            {
                AddEditorTreeViewItem(superItem, parameterInfo);
                //var item = 
                //item.Deleted += ItemOnDeleted;
            }

            superItem.Items.CollectionChanged += delegate
            {
                //for simplification at this point, just refresh the whole list of parameterinfos
                _graph.ParameterInfos.Clear();
                _graph.ParameterInfos.AddRange(superItem.Items.OfType<ParameterEditorTreeViewItem>().Select(x => x.ParameterInfo));
            };
        }


        public ParameterEditorTreeViewItem AddEditorTreeViewItem(ITreeViewControl parent, ParameterInfo parameterInfo)
        {
            ParameterEditor parameterEditor = _parameterEditorManager.GetParameterEditor(parameterInfo.GetType());

            var editorTreeViewItem = (ParameterEditorTreeViewItem) parameterEditor.CreateEditorTreeViewItem(this, parameterInfo);

            //when the user clicks on it, the properties will appear on the right
            editorTreeViewItem.Click += EditorTreeViewItemOnClick;

            //if it supports subItems, a contextmenu with an add option will appear everytime the user right clicks it
            if (parameterInfo.CanHaveSubItems)
            {
                var addMenuItem = new MenuChild() {Text = "Add"};
                editorTreeViewItem.MultiContextMenu.MenuChildren.Add(addMenuItem);

                addMenuItem.MenuChildren.AddRange(CreateAddMenu(editorTreeViewItem));
            }


            //add the option for deletion in a right-click menu
            editorTreeViewItem.MultiContextMenu.MenuChildren.Add(new MenuChild(delegate { editorTreeViewItem.Parent.Items.Remove(editorTreeViewItem); })
            {
                Text = "Delete", UserData = editorTreeViewItem, BeginGroup = true
            });

            //now add it to the treeview
            parent.Items.Add(editorTreeViewItem);

            return editorTreeViewItem;
        }



        private IEnumerable<MenuChild> CreateAddMenu(EditorTreeViewItem editorTreeViewItem)
        {
            foreach (ParameterEditor parameterEditorToAdd in _parameterEditorManager.AvailableParameterEditors.OrderBy(x => x.ParameterInfoName))
            {
                //the recursive item is very specific, only a few items can have it
                if (parameterEditorToAdd is RecursiveParameterEditor && !editorTreeViewItem.CanHaveRecursiveItem)
                    continue;

                yield return new MenuChild(AddItemAction) {Text = parameterEditorToAdd.ParameterInfoName, UserData = new Object[] {parameterEditorToAdd, editorTreeViewItem}};
            }
        }



        private void ItemOnDeleted(object sender, EventArgs eventArgs)
        {
            var item = (ParameterEditorTreeViewItem) sender;

            //remove it from the tree
            _treeview.Items.Remove(item);

            //and remove it from the source, the graph parameters
            _graph.ParameterInfos.Remove(item.ParameterInfo);
        }



        private void AddItemAction(MenuChild obj)
        {
            var data = (Object[]) obj.UserData;
            var parameterEditor = (ParameterEditor) data[0];
            ITreeViewControl parentTreeViewItem = data[1] as ITreeViewControl ?? _treeview;

            var chosenName = "New " + parameterEditor.ParameterInfoName;

            var siblingIdentifiers = GetParentParameterInfos(parentTreeViewItem);

            if (siblingIdentifiers.Any(val => val == Parameter.GetIdentifier(chosenName)))
            {
                int index = 1;

                //increment an index number so as to avoid conflicts
                while (siblingIdentifiers.Any(val => val == Parameter.GetIdentifier(chosenName + " (" + index + ")")))
                {
                    index++;
                }

                chosenName += " (" + index + ")";
            }

            //add the parameterinfo to the graph
            ParameterInfo parameterInfo = (ParameterInfo) Activator.CreateInstance(parameterEditor.ParameterInfoType, new object[] {chosenName});

            AddEditorTreeViewItem(parentTreeViewItem, parameterInfo);
        }



        private List<String> GetParentParameterInfos(ITreeViewControl parentTreeViewItem)
        {
            return parentTreeViewItem.Items.OfType<ParameterEditorTreeViewItem>().Select(x => Parameter.GetIdentifier(x.ParameterInfo.Label)).ToList();
        }



        /// <summary>
        /// When a user clicks on a treeitem, show the corresponding configuration controls on the inspector panel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        public void EditorTreeViewItemOnClick(object sender, EventArgs eventArgs)
        {
            var editorTreeViewItem = (ParameterEditorTreeViewItem) sender;
            var parameterEditor = _parameterEditorManager.GetParameterEditor(editorTreeViewItem.ParameterInfo.GetType());

            _inspectorPanel.Content = parameterEditor.CreateInspectorContent(editorTreeViewItem);
        }



        /*private void AddItem(MenuChild obj)
        {
            var data = (Object[])obj.UserData;
            var parameterEditor = (ParameterEditor)data[0];
            //var parameterEditor = (EditorTreeViewItem)data[1];

            //let's determine the name of the parameter
            var chosenName = "New " + parameterEditor.ParameterInfoName;

            if (_treeview.Items.OfType<EditorTreeViewItem>().Any(val => val.Text == chosenName))
            {
                int index = 1;

                //increment an index number so as to avoid conflicts
                while (_treeview.Items.OfType<EditorTreeViewItem>().Any(val => val.Text == chosenName + " (" + index + ")"))
                {
                    index++;
                }

                chosenName += " (" + index + ")";
            }

            //add the parameterinfo to the graph
            ParameterInfo parameterInfo = (ParameterInfo)Activator.CreateInstance(parameterEditor.ParameterInfoType, new object[] { chosenName });
            _graph.ParameterInfos.Add(parameterInfo);

            AddEditorTreeViewItem(_treeview, parameterInfo);
        }

        public ParameterInfo GetNonConflictingParameterInfo(ITreeViewControl parent, ParameterEditor editor)
        {
            var chosenName = "New " + editor.ParameterInfoName;

            if (parent.Items.OfType<EditorTreeViewItem>().Any(val => val.Text == chosenName))
            {
                int index = 1;

                //increment an index number so as to avoid conflicts
                while (parent.Items.OfType<EditorTreeViewItem>().Any(val => val.Text == chosenName + " (" + index + ")"))
                {
                    index++;
                }

                chosenName += " (" + index + ")";
            }

            //add the parameterinfo to the graph
            return (ParameterInfo)Activator.CreateInstance(editor.ParameterInfoType, new object[] { chosenName });
        }*/


        /*private void DeleteItem(MenuChild obj)
        {
            
        }*/



        private void OkButtonOnClick(object sender, EventArgs e)
        {
            Accept();
        }



        private void CancelButtonOnClick(object sender, EventArgs e)
        {
            Cancel();
        }





        public Graph Graph
        {
            get { return _graph; }
        }




        /*public MenuChild GetAddMenuChild(EditorTreeViewItem editorTreeViewItem, Action<EditorTreeViewItem, object> onAdd)
        {
            var addMenuChild = new MenuChild() { Text = "Add" };
            editorTreeViewItem.MultiContextMenu.MenuChildren.Add(addMenuChild);

            foreach (ParameterEditor parameterEditorToAdd in ParameterEditor.AvailableParameterEditors)
            {
                var editorToAdd = parameterEditorToAdd;

                addMenuChild.MenuChildren.Add(new MenuChild(delegate
                {
                    var parameterInfo = GetNonConflictingParameterInfo(editorTreeViewItem, editorToAdd);
                    onAdd(editorTreeViewItem, parameterInfo);
                    
                }
                    )
                {
                    Text = parameterEditorToAdd.ParameterInfoName, UserData = new Object[] { parameterEditorToAdd, (EditorTreeViewItem)editorTreeViewItem }
                });
            }
        }*/
    }
}