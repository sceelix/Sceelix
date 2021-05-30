using System;
using System.Linq;
using System.Windows.Forms;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Core.Graphs;
using Sceelix.Core.Utils;
using Sceelix.Designer.Graphs.GUI.Execution;
using Sceelix.Designer.Graphs.GUI.Model;
using Sceelix.Designer.Graphs.GUI.Navigation;
using Sceelix.Designer.Graphs.GUI.Toolbox;
using Sceelix.Designer.Graphs.Inspector.Nodes;
using Sceelix.Designer.Graphs.Messages;
using Sceelix.Designer.Graphs.Tools;
using Sceelix.Designer.GUI;
using Sceelix.Designer.GUI.MenuControls;
using Sceelix.Designer.Managers;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Designer.Services;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using MouseButtons = DigitalRune.Game.Input.MouseButtons;

namespace Sceelix.Designer.Graphs.GUI.Interactions
{
    public enum ControlState
    {
        Normal,
        DraggingNode,
        CreatingEdge,
        DraggingSelectionRectangle
    }


    public class InteractionHandler
    {
        //private MultiContextMenu _contextMenu;
        private readonly ContextMenuHandler _contextMenuHandler;
        private readonly IServiceLocator _services;
        private readonly GraphControl _control;
        private readonly MessageManager _messageManager;

        private readonly NodeToolboxWindow _nodeToolboxWindow;
        private readonly SelectionRectangleHandler _selectionRectangleHandler;

        //private Vector2 _mouseScreenPosition = Vector2.Zero;
        private ControlState _controlState;

        private EdgeDrawer _edgeDrawer;
        private bool _isZoomingMouse;
        private bool _middleMouseMoving = false;
        private Vector2 _mouseScreenPosition;
        private Vector2 _mouseZoomPoint;
        private bool _nodesMoved;
        

        //private InspectorPopupWindow _inspectorPopupWindow;

        public InteractionHandler(IServiceLocator services, GraphControl control)
        {
            _services = services;
            _control = control;
            _messageManager = services.Get<MessageManager>();

            _selectionRectangleHandler = new SelectionRectangleHandler(control);
            _contextMenuHandler = new ContextMenuHandler(services, control);
            _nodeToolboxWindow = new NodeToolboxWindow(ToolboxItemSelected);

            
        }



        private void ToolboxItemSelected(IProcedureItem toolboxItem, Vector2 screenPosition)
        {
            Vector2 modelPosition = _control.Camera.ToModelPosition(screenPosition - new Vector2(_control.ActualX, _control.ActualY));

            VisualGraph.AddNode(toolboxItem, modelPosition);
        }



        public void LoadContent(ContentManager manager)
        {
            _selectionRectangleHandler.LoadContent(manager);
        }



        public void OnLoad()
        {
        }



        public void OnHandleInput(IInputService inputService, InputContext context)
        {
            Vector2 offSet = Camera.ToModelOffset(context.MousePositionDelta.ToXna());
            Vector2 mouseModelPosition = Camera.ToModelPosition(context.MousePosition.ToXna());

            if (context.IsMouseOver && !inputService.IsMouseOrTouchHandled)
            {
                //Handle(inputService, context, offSet, mouseModelPosition);

                if (inputService.IsDoubleClick(DigitalRune.Game.Input.MouseButtons.Left))
                {
                    //#if !WINDOWS
                        OnDoubleClick(inputService);
                    //#endif

                }
                else if (inputService.IsPressed(DigitalRune.Game.Input.MouseButtons.Left, false))
                {
                    inputService.IsMouseOrTouchHandled = true;

                    MouseDown(inputService, context, offSet, mouseModelPosition);
                }
                if (Math.Abs(inputService.MouseWheelDelta) > float.Epsilon)
                {
                    inputService.IsMouseOrTouchHandled = true;

                    Camera.ZoomCanvas(inputService.MouseWheelDelta, Camera.ToModelPosition(context.MousePosition.ToXna()));
                }
                if (!context.MousePositionDelta.IsNumericallyZero)
                {
                    inputService.IsMouseOrTouchHandled = true;

                    MouseMove(inputService, context, offSet, mouseModelPosition);
                }
                if (inputService.IsPressed(MouseButtons.Right, false))
                {
                    inputService.IsMouseOrTouchHandled = true;

                    //_toolboxWindow.OpenWindow(_control,context.ScreenMousePosition.ToXna());
                    if (inputService.ModifierKeys.HasFlag(ModifierKeys.Alt))
                    {
                        _isZoomingMouse = true;
                        _mouseZoomPoint = context.MousePosition.ToXna();
                    }
                    else
                    {
                        if (VisualGraph.HasHoveredUnits)
                        {
                            ISelectableUnit hoveredSelectedUnit = VisualGraph.GetHoveredSelectedUnit();

                            //unless Ctrl is pressed, one should deselect all the nodes/edges
                            if (!inputService.ModifierKeys.HasFlag(ModifierKeys.Control) && hoveredSelectedUnit == null)
                                VisualGraph.DeselectAll();

                            //select hovered units
                            VisualGraph.SelectHoveredUnits(false);
                        }
                        else
                        {
                            VisualGraph.DeselectAll();

                            //clear out the inspector window
                            //_messageManager.Publish(new ShowPropertiesRequest(null));
                        }

                        _contextMenuHandler.ShowContextMenu(context);
                    }
                }
                if (inputService.IsReleased(DigitalRune.Game.Input.MouseButtons.Left))
                {
                    inputService.IsMouseOrTouchHandled = true;

                    if (_controlState == ControlState.DraggingSelectionRectangle)
                    {
                        _selectionRectangleHandler.ApplySelection();
                        _control.ShouldRender = true;
                    }
                    else if (_controlState == ControlState.CreatingEdge)
                    {
                        VisualPort hoveredPort = VisualGraph.FindHoveredPort();
                        if (hoveredPort != null && _edgeDrawer.IsValidTargetPort)
                        {
                            _edgeDrawer.CreateEdge(hoveredPort);
                            _messageManager.Publish(new GraphContentChanged(_control.FileItem));
                        }
                    }
                    else if (_controlState == ControlState.DraggingNode)
                    {
                        if (!VisualGraph.TryIsolatedNodeIntegration())
                            VisualGraph.SelectHoveredUnits(true);

                        if (_nodesMoved)
                        {
                            _nodesMoved = false;
                            VisualGraph.AlertForFileChange(false);
                        }
                    }

                    //if the mouse is released, everything goes back to normal
                    _controlState = ControlState.Normal;
                }
            }

            else if (!inputService.IsMouseOrTouchHandled && _middleMouseMoving)
            {
                if (inputService.IsDown(MouseButtons.Middle) ||
                    (inputService.ModifierKeys.HasFlag(ModifierKeys.Shift) && inputService.IsDown(MouseButtons.Left)))
                {
                    Camera.Move(offSet);
                    inputService.IsMouseOrTouchHandled = true;
                }
                else
                {
                    _middleMouseMoving = false;
                }
            }
            else if (_controlState == ControlState.DraggingNode &&
                     inputService.IsReleased(DigitalRune.Game.Input.MouseButtons.Left))
            {
                if (_nodesMoved)
                {
                    _nodesMoved = false;
                    VisualGraph.AlertForFileChange(false);
                }

                _controlState = ControlState.Normal;
            }
            if (_isZoomingMouse)
            {
                //inputService.IsMouseOrTouchHandled = true;

                if (inputService.IsDown(MouseButtons.Right) && inputService.ModifierKeys.HasFlag(ModifierKeys.Alt))
                    //System.Diagnostics.Debug.WriteLine(offSet.Y);
                    Camera.ZoomCanvas(offSet.Y, Camera.ToModelPosition(_mouseZoomPoint));
                else
                    _isZoomingMouse = false;
            }

            


            OnKeyDown(inputService, context, mouseModelPosition);
        }



        private void OnDoubleClick(IInputService inputService)
        {
            //var doubleC = inputService.IsDoubleClick(MouseButtons.Left);
            if (VisualGraph.HasHoveredItems)
            {
                inputService.IsMouseOrTouchHandled = true;
                //var isCtrlPressed = inputService.ModifierKeys.HasFlag(ModifierKeys.Control);
                //var isAltPressed = inputService.ModifierKeys.HasFlag(ModifierKeys.Alt);

                var hoveredPort = VisualGraph.VisualPorts.FirstOrDefault(x => x.IsHovered);
                if (hoveredPort != null)
                {
                    _control.GraphExecutionManager.GuiExecuteRequest(new ExecutionOptions(true, hoveredPort.Port, true, true));
                }
                else
                {
                    var hoveredNode = VisualGraph.VisualNodes.FirstOrDefault(x => x.IsHovered);
                    if (hoveredNode != null)
                    {
                        if (hoveredNode.IsTopHovered)
                            _control.GraphExecutionManager.GuiExecuteRequest(new ExecutionOptions(true, hoveredNode.Node.InputPorts.First(), false, true));
                        else if (hoveredNode.IsBottomHovered)
                            _control.GraphExecutionManager.GuiExecuteRequest(new ExecutionOptions(true, hoveredNode.Node.OutputPorts.First(), false, true));
                        else
                            _control.GraphExecutionManager.GuiExecuteRequest(new ExecutionOptions(true, hoveredNode.Node, false, true));
                    }
                    /*else
                    {
                        var hoveredEdge = VisualGraph.VisualEdges.FirstOrDefault(x => x.IsHovered);
                        if (hoveredEdge != null)
                            _control.GuiExecuteRequest(new ExecutionOptions(true, hoveredEdge.Edge, true, true));
                    }*/
                }
            }
            else if (!inputService.ModifierKeys.HasFlag(ModifierKeys.Alt) && !inputService.ModifierKeys.HasFlag(ModifierKeys.Control))
            {
                inputService.IsMouseOrTouchHandled = true;

                _control.GraphExecutionManager.GuiExecuteRequest(new ExecutionOptions(true));
            }
        }
        


        private void MouseDown(IInputService inputService, InputContext context, Vector2 offSet, Vector2 mouseModelPosition)
        {
            VisualPort hoveredPort = VisualGraph.FindHoveredPort();
            if (hoveredPort != null)
            {
                _edgeDrawer = new EdgeDrawer(hoveredPort);
                _controlState = ControlState.CreatingEdge;
                VisualGraph.DeselectAll();

                if (inputService.ModifierKeys.HasFlag(ModifierKeys.Alt) && !inputService.ModifierKeys.HasFlag(ModifierKeys.Control))
                {
                    hoveredPort.CycleStateNext();

                    //give the name of the port as the gate label
                    if (hoveredPort.Port.PortState == PortState.Gate && String.IsNullOrWhiteSpace(hoveredPort.Port.GateLabel))
                        hoveredPort.Port.GateLabel = hoveredPort.Port.Label;
                }
                else if (!inputService.ModifierKeys.HasFlag(ModifierKeys.Control))
                {
                    hoveredPort.Select();
                    hoveredPort.ShowProperties();
                }

                return;
            }

            VisualEdge visualEdge = VisualGraph.FindHoveredEdge();
            if (visualEdge != null && inputService.ModifierKeys.HasFlag(ModifierKeys.Alt))
            {
                visualEdge.Enabled = !visualEdge.Enabled;
                _control.ShouldRender = true;
                visualEdge.VisualGraph.AlertForFileChange(true);
            }

            if (VisualGraph.HasHoveredUnits)
            {
                ISelectableUnit hoveredSelectedUnit = VisualGraph.GetHoveredSelectedUnit();

                //unless Ctrl is pressed, one should deselect all the nodes/edges
                if (!inputService.ModifierKeys.HasFlag(ModifierKeys.Control) && hoveredSelectedUnit == null)
                    VisualGraph.DeselectAll();

                //select hovered units
                VisualGraph.SelectHoveredUnits(true);

                //mark the state as such
                _controlState = ControlState.DraggingNode;

                //unless Ctrl is pressed, one should deselect all the nodes/edges
                //now, we have to handle the case where we want to deselect a unit from the selection using the control
                if (inputService.ModifierKeys.HasFlag(ModifierKeys.Control) && hoveredSelectedUnit != null)
                {
                    hoveredSelectedUnit.Deselect();
                    VisualGraph.SelectFirstSelectedUnit();
                }
            }
            else
            {
                //once we start dragging the rectangle, deselect everything
                VisualGraph.DeselectAll();

                //clear out the inspector window
                //_messageManager.Publish(new ShowPropertiesRequest(null, _control.GraphDocumentControl));
                VisualGraph.Control.ShowGraphProperties();
                //_control.GraphEditorForm.ShowNodePanel(null);

                //mark the state as such
                _controlState = ControlState.DraggingSelectionRectangle;

                //start drawing the rectangle
                _selectionRectangleHandler.StartDragging(new Vector2(context.MousePosition.X, context.MousePosition.Y));
            }
        }



        private void MouseMove(IInputService inputService, InputContext context, Vector2 offSet, Vector2 mouseModelPosition)
        {
            if (inputService.IsDown(MouseButtons.Middle) ||
                (inputService.ModifierKeys.HasFlag(ModifierKeys.Shift) && inputService.IsDown(MouseButtons.Left)))
            {
                Camera.Move(offSet);
                _middleMouseMoving = true;
                _control.GraphDocumentControl.DocumentAreaWindow.Activate();
            }


            if (_controlState == ControlState.DraggingNode)
            {
                VisualGraph.TranslateVisualNodes(offSet, mouseModelPosition);

                _nodesMoved = true;
                _control.ShouldRender = true;
                //_control.GraphEditorForm.ShowNodePanel(null);
            }
            else
            {
                IHoverableUnit hoveredUnit = VisualGraph.UpdateUnitHovers(mouseModelPosition, _controlState == ControlState.DraggingSelectionRectangle ? _selectionRectangleHandler.BoundingRectangle : null);
                if (_controlState == ControlState.CreatingEdge)
                {
                    VisualGraph.UpdatePortEmphasis(_edgeDrawer.StartPort);
                }
                else
                {
                    VisualGraph.UpdatePortEmphasis(hoveredUnit);
                }
            }

            _mouseScreenPosition = context.MousePosition.ToXna();
        }



        private void OnKeyDown(IInputService inputService, InputContext context, Vector2 mouseModelPosition)
        {
            if (!inputService.IsKeyboardHandled)
            {
                //if the delete key is pressed and there are selected units
                if (inputService.IsPressed(Keys.Delete, false) && VisualGraph.HasSelectedUnits)
                {
                    inputService.IsKeyboardHandled = true;

                    VisualGraph.DeleteSelectedUnits();

                    //Messenger.MessageManager.Publish(new GraphContentChanged(_control.FileItem,true));
                }
                else if (inputService.IsPressed(Keys.F, false))
                {
                    inputService.IsKeyboardHandled = true;

                    _control.FrameSelection(true);
                }
                else if (inputService.ModifierKeys.HasFlag(ModifierKeys.Control)) //.IsDown(Keys.LeftControl) || inputService.IsDown(Keys.RightControl)
                {
                    if (inputService.IsPressed(Keys.Space, false) && context.IsMouseOver)
                    {
                        inputService.IsKeyboardHandled = true;

                        //_toolboxWindow.Location = _control.PointToScreen(new System.Drawing.Point((int)_mouseScreenPosition.X, (int)_mouseScreenPosition.Y));
                        _nodeToolboxWindow.OpenToolbox(_control, context.ScreenMousePosition.ToXna());
                    }
                    else if (inputService.IsPressed(Keys.Z, false))
                    {
                        inputService.IsKeyboardHandled = true;

                        VisualGraph.Undo();
                    }
                    else if (inputService.IsPressed(Keys.Y, false))
                    {
                        inputService.IsKeyboardHandled = true;

                        VisualGraph.Redo();
                    }
                    else if (inputService.IsPressed(Keys.X, false))
                    {
                        inputService.IsKeyboardHandled = true;

                        VisualGraph.CutToClipBoard();
                    }
                    else if (inputService.IsPressed(Keys.C, false))
                    {
                        inputService.IsKeyboardHandled = true;

                        VisualGraph.CopyToClipBoard();
                    }
                    else if (inputService.IsPressed(Keys.V, false))
                    {
                        inputService.IsKeyboardHandled = true;

                        VisualGraph.PasteFromClipBoard(context.MousePosition.ToXna());
                    }
                    else if (inputService.IsPressed(Keys.L, false))
                    {
                        inputService.IsKeyboardHandled = true;

                        _control.ClearCache();
                    }
                    else if (inputService.IsPressed(Keys.S, false))
                    {
                        inputService.IsKeyboardHandled = true;

                        _control.Save();
                        _control.GraphDocumentControl.AlertFileSave();

                        //Messenger.MessageManager.Publish(new FileSaved(_control.FileItem));
                    }
                }
            }
        }

        


        public void Update(TimeSpan deltaTime)
        {
            if (_controlState == ControlState.DraggingSelectionRectangle)
            {
                _selectionRectangleHandler.Update(deltaTime, _mouseScreenPosition);
            }
            else if (_controlState == ControlState.CreatingEdge)
            {
                _edgeDrawer.Update(deltaTime, Camera.ToModelPosition(_mouseScreenPosition));
            }
        }



        public void Draw(SpriteBatch spriteBatch)
        {
            if (_controlState == ControlState.CreatingEdge)
            {
                _edgeDrawer.Draw(spriteBatch);
            }
            if (_controlState == ControlState.DraggingSelectionRectangle)
            {
                _selectionRectangleHandler.Draw(spriteBatch, _controlState);
            }
        }



        public void ShowNodeProperties(VisualNode visualNode)
        {
            var inspectorWindow = new PopUpInspectorWindow(visualNode);

            inspectorWindow.OpenWindow(_control, _control.InputService.MousePosition.ToXna());
        }

        #region Properties

        private VisualGraph VisualGraph
        {
            get { return _control.VisualGraph; }
        }



        private Camera2D Camera
        {
            get { return _control.Camera; }
        }



        public NodeToolboxWindow NodeToolboxWindow
        {
            get { return _nodeToolboxWindow; }
        }



        public Vector2 MouseScreenPosition
        {
            get { return _mouseScreenPosition; }
        }

        #endregion
    }
}