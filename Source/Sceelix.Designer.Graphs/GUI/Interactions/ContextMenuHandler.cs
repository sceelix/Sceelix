using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Text;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Core.Environments;
using Sceelix.Core.Graphs;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Designer.Graphs.GUI.Execution;
using Sceelix.Designer.Graphs.GUI.Model;
using Sceelix.Designer.Graphs.Handlers;
using Sceelix.Designer.Graphs.Messages;
using Sceelix.Designer.GUI.MenuControls;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.Helpers;
using Sceelix.Designer.Managers;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.FileHandlers;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;
using Sceelix.Extensions;

namespace Sceelix.Designer.Graphs.GUI.Interactions
{
    public class ContextMenuHandler
    {
        //saves the position of the last right click - useful to place new nodes
        //private Vector2 _lastMouseRightClick;

        private readonly GraphControl _graphControl;
        private readonly MessageManager _messageManager;
        private readonly WindowAnimator _windowAnimator;


        public ContextMenuHandler(IServiceLocator manager, GraphControl graphControl)
        {
            _windowAnimator = manager.Get<WindowAnimator>();
            _messageManager = manager.Get<MessageManager>();
            _graphControl = graphControl;
        }



        public VisualGraph VisualGraph
        {
            get { return _graphControl.VisualGraph; }
        }



        /// <summary>
        /// Creates dynamically the context menu, depending if nodes/edges are selected.
        /// Adds the previously created list of menu items.
        /// </summary>
        public void ShowContextMenu(InputContext inputContext)
        {
            MultiContextMenu multiContextMenu = new MultiContextMenu();

            MenuChild addItem = new MenuChild(AddItemClick) {Text = "Add Node...", Icon = EmbeddedResources.Load<Texture2D>("Resources/Plus16x16.png"), UserData = inputContext.ScreenMousePosition};
            multiContextMenu.MenuChildren.Add(addItem);

            if (VisualGraph.HasSelectedUnits)
            {
                MenuChild item = new MenuChild(DeleteItemClick) {Text = "Delete", Icon = EmbeddedResources.Load<Texture2D>("Resources/Cancel.png"), BeginGroup = true};
                multiContextMenu.MenuChildren.Add(item);

                if (VisualGraph.HasSelectedNodes)
                {
                    if (VisualGraph.HasSelectedConnectedNodes)
                    {
                        MenuChild disconnectItem = new MenuChild(DisconnectItemClick) {Text = "Disconnect", Icon = EmbeddedResources.Load<Texture2D>("Resources/Burn_16x16.png")};
                        multiContextMenu.MenuChildren.Add(disconnectItem);
                    }

                    MenuChild cutItem = new MenuChild(CutItemClick) {Text = "Cut", Icon = EmbeddedResources.Load<Texture2D>("Resources/ClipboardCut_16x16.png"), BeginGroup = true};
                    multiContextMenu.MenuChildren.Add(cutItem);

                    MenuChild copyItem = new MenuChild(CopyItemClick) {Text = "Copy", Icon = EmbeddedResources.Load<Texture2D>("Resources/ClipboardCopy_16x16.png")};
                    multiContextMenu.MenuChildren.Add(copyItem);

                    if (ClipboardHelper.ContainsText())
                    {
                        MenuChild pasteItem = new MenuChild(PasteItemClick) {Text = "Paste", Icon = EmbeddedResources.Load<Texture2D>("Resources/ClipboardPaste_16x16.png")};
                        multiContextMenu.MenuChildren.Add(pasteItem);
                    }

                    MenuChild encapsulateItem = new MenuChild(EncapsulateItemClick) {Text = "Encapsulate", Icon = EmbeddedResources.Load<Texture2D>("Resources/Mail_16x16.png")};
                    multiContextMenu.MenuChildren.Add(encapsulateItem);

                    MenuChild generateCSharpCodeItem = new MenuChild(GenerateCodeClick) {Text = "Generate C# Code", Icon = EmbeddedResources.Load<Texture2D>("Resources/Loop_16x16.png")};
                    multiContextMenu.MenuChildren.Add(generateCSharpCodeItem);
                }

                if (VisualGraph.HasSelectedGraphNodes)
                {
                    MenuChild openSubgraphsItem = new MenuChild(OpenSubGraphsItemClick) {Text = "Open Subgraphs", Icon = EmbeddedResources.Load<Texture2D>("Resources/Loop_16x16.png")};
                    multiContextMenu.MenuChildren.Add(openSubgraphsItem);
                }
            }
            else if (ClipboardHelper.ContainsText())
            {
                MenuChild pasteItem = new MenuChild(PasteItemClick) {Text = "Paste", Icon = EmbeddedResources.Load<Texture2D>("Resources/ClipboardPaste_16x16.png"), BeginGroup = true, UserData = inputContext.MousePosition};
                multiContextMenu.MenuChildren.Add(pasteItem);
            }

            if (VisualGraph.HasOnlyEnabledEdgesSelected)
            {
                MenuChild item = new MenuChild(DisableItemClick) {Text = "Disable", Icon = EmbeddedResources.Load<Texture2D>("Resources/Forbidden_16x16.png")};
                multiContextMenu.MenuChildren.Add(item);
            }
            else if (VisualGraph.HasOnlyDisabledEdgesSelected)
            {
                MenuChild item = new MenuChild(EnableItemClick) {Text = "Enable", Icon = EmbeddedResources.Load<Texture2D>("Resources/Forbidden_16x16.png")};
                multiContextMenu.MenuChildren.Add(item);
            }

            MenuChild executeItem = new MenuChild(ExecuteClick) {Text = "Execute", Icon = EmbeddedResources.Load<Texture2D>("Resources/PlayerPlay_16x16.png"), BeginGroup = true};
            multiContextMenu.MenuChildren.Add(executeItem);


            var hoveredPort = VisualGraph.VisualPorts.FirstOrDefault(x => x.IsHovered);
            if (hoveredPort != null)
            {
                multiContextMenu.MenuChildren.Add(new MenuChild(delegate { _graphControl.GraphExecutionManager.GuiExecuteRequest(new ExecutionOptions(true, hoveredPort.Port, true, true)); }) {Text = "Execute until this port", Icon = EmbeddedResources.Load<Texture2D>("Resources/PlayerPlay_16x16.png")});
            }
            else
            {
                var hoveredNode = VisualGraph.VisualNodes.FirstOrDefault(x => x.IsHovered);
                if (hoveredNode != null)
                {
                    if (hoveredNode.IsTopHovered)
                    {
                        multiContextMenu.MenuChildren.Add(new MenuChild(delegate { _graphControl.GraphExecutionManager.GuiExecuteRequest(new ExecutionOptions(true, hoveredNode.Node.InputPorts.First(), false, true)); })
                            {Text = "Execute until these input ports", Icon = EmbeddedResources.Load<Texture2D>("Resources/PlayerPlay_16x16.png")});
                    }

                    else if (hoveredNode.IsBottomHovered)
                    {
                        multiContextMenu.MenuChildren.Add(new MenuChild(delegate { _graphControl.GraphExecutionManager.GuiExecuteRequest(new ExecutionOptions(true, hoveredNode.Node.OutputPorts.First(), false, true)); })
                        {
                            Text = "Execute until these output ports", Icon = EmbeddedResources.Load<Texture2D>("Resources/PlayerPlay_16x16.png")
                        });
                    }
                    else
                    {
                        multiContextMenu.MenuChildren.Add(new MenuChild(delegate { _graphControl.GraphExecutionManager.GuiExecuteRequest(new ExecutionOptions(true, hoveredNode.Node, false, true)); })
                            {Text = "Execute until this node", Icon = EmbeddedResources.Load<Texture2D>("Resources/PlayerPlay_16x16.png")});
                    }
                }
                /*else
                {
                    var hoveredEdge = VisualGraph.VisualEdges.FirstOrDefault(x => x.IsHovered);
                    if (hoveredEdge != null)
                        _control.GuiExecuteRequest(new ExecutionOptions(true, hoveredEdge.Edge, true, true));
                }*/
            }

            /*if (VisualGraph.HasHoveredItems)
            {
                String unitType = VisualGraph.VisualPorts.FirstOrDefault(x => x.IsHovered) != null ? "Port" : (VisualGraph.VisualNodes.FirstOrDefault(x => x.IsHovered) != null ? "Node" : "Edge");

                MenuChild executeSpecialItem = new MenuChild() { Text = "Execute Special", Icon = EmbeddedResources.Load<Texture2D>("Resources/PlayerPlay_16x16.png") };
                {
                    MenuChild executeUntilItem = new MenuChild(ExecuteUntilElementClick) { Text = "All Nodes until this " + unitType, Icon = EmbeddedResources.Load<Texture2D>("Resources/PlayerPlay_16x16.png") };
                    executeSpecialItem.MenuChildren.Add(executeUntilItem);

                    MenuChild executeUntilItem = new MenuChild(ExecuteUntilElementClick) { Text = "Until This Element (Sequence until this)", Icon = EmbeddedResources.Load<Texture2D>("Resources/PlayerPlay_16x16.png") };
                    executeSpecialItem.MenuChildren.Add(executeUntilItem);

                    MenuChild executeUntilItem = new MenuChild(ExecuteUntilElementClick) { Text = "Until This Element (Sequence until this)", Icon = EmbeddedResources.Load<Texture2D>("Resources/PlayerPlay_16x16.png") };
                    executeSpecialItem.MenuChildren.Add(executeUntilItem);
                }
                multiContextMenu.MenuChildren.Add(executeSpecialItem);
            }*/


            MenuChild graphPropertiesItem = new MenuChild(GraphPropertiesClick) {Text = "Graph Properties", Icon = EmbeddedResources.Load<Texture2D>("Resources/Graph1.png"), BeginGroup = true};
            multiContextMenu.MenuChildren.Add(graphPropertiesItem);

            multiContextMenu.Open(_graphControl.Screen, inputContext.ScreenMousePosition);
        }



        private void GenerateCodeClick(MenuChild obj)
        {
            
            StringBuilder stringBuilder = new StringBuilder();

            
            foreach (var visualNode in VisualGraph.SelectedUnits.OfType<VisualNode>())
            {
                if (stringBuilder.Length == 0)
                    stringBuilder.AppendLine();

                var code = visualNode.Node.GenerateCSharpCallCode();
                stringBuilder.AppendLine(code);
            }
            

            ClipboardHelper.Copy(stringBuilder.ToString());
        }



        private void ExecuteClick(MenuChild menuChild)
        {
            _graphControl.GraphExecutionManager.GuiExecuteRequest(new ExecutionOptions(true));
        }



        private void OpenSubGraphsItemClick(MenuChild menuChild)
        {
            //Project project = Messenger.RequestManager.Request<Project>(new ProjectInfoRequest(this));

            foreach (var visualNode in VisualGraph.SelectedUnits.OfType<VisualNode>().Where(val => val.Node is ComponentNode))
            {
                ComponentNode node = (ComponentNode) visualNode.Node;

                FileItem fileItem = _graphControl.FileItem.Project.GetSubFileItem(node.ProjectRelativePath);

                _graphControl.Services.Get<MessageManager>().Publish(new OpenFile(fileItem));
            }
        }



        private void DisconnectItemClick(MenuChild menuChild)
        {
            foreach (var visualNode in VisualGraph.SelectedUnits.OfType<VisualNode>())
            {
                visualNode.Disconnect();
            }
        }



        private void CutItemClick(MenuChild menuChild)
        {
            VisualGraph.CutToClipBoard();
        }



        private void CopyItemClick(MenuChild menuChild)
        {
            VisualGraph.CopyToClipBoard();
        }



        private void PasteItemClick(MenuChild menuChild)
        {
            Vector2F position = (Vector2F) menuChild.UserData;

            VisualGraph.PasteFromClipBoard(position.ToXna());
        }



        private void EncapsulateItemClick(MenuChild menuChild)
        {
            GraphCreator creator = new GraphCreator();
            FolderItem parentFolder = VisualGraph.FileItem.ParentFolder;

            //determine the name of the file, avoiding conflicts with existing file names
            String chosenName = StringExtension.FindNonConflict(parentFolder.ContainsFileWithName, index => VisualGraph.FileItem.Name + "." + "Component" + index + creator.Extension);

            InputWindow inputWindow = new InputWindow()
            {
                InputText = chosenName,
                Title = "Encapsulate Selection",
                LabelText = "File name:",
                Width = 400,
                MessageIcon = creator.Icon48X48,
                Check = CheckFunction
            };
            inputWindow.Accepted += delegate
            {
                chosenName = inputWindow.InputText;

                if (chosenName != null)
                    VisualGraph.EncapsulateSelection(chosenName.EndsWith(creator.Extension) ? chosenName : chosenName + creator.Extension);
            };

            inputWindow.Show(_windowAnimator);
            //chosenName = InputBox.Show(chosenName, "Encapsulate Selection", "File name:", creator.Icon48X48, CheckFunction);
        }



        private string CheckFunction(string text, Object userData)
        {
            return FileCreationHelper.PerformChecks(text, new GraphCreator(), VisualGraph.FileItem.ParentFolder);
        }



        private void EnableItemClick(MenuChild menuChild)
        {
            VisualGraph.EnableSelectedEdges();
            _messageManager.Publish(new GraphContentChanged(_graphControl.FileItem, true));
        }



        private void DisableItemClick(MenuChild menuChild)
        {
            VisualGraph.DisableSelectedEdges();
            _messageManager.Publish(new GraphContentChanged(_graphControl.FileItem, true));
        }



        private void GraphPropertiesClick(MenuChild menuChild)
        {
            _graphControl.ShowGraphProperties();
        }



        private void AddItemClick(MenuChild menuChild)
        {
            Vector2F position = (Vector2F) menuChild.UserData;
            _graphControl.InteractionHandler.NodeToolboxWindow.OpenToolbox(_graphControl, position.ToXna());
        }



        private void DeleteItemClick(MenuChild menuChild)
        {
            VisualGraph.DeleteSelectedUnits();
        }
    }
}