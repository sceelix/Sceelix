using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sceelix.Designer.Graphs.Handlers;
using Sceelix.Designer.Graphs.Tools;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.ProjectExplorer.FileHandlers;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Utils;


namespace Sceelix.Designer.Graphs.GUI.Toolbox
{
    //[PluginWindow("Toolbox Window")]
    public class NodeToolboxWindowOld : Window
    {
        private readonly List<ToolboxItemControl> _allItems = new List<ToolboxItemControl>();
        private readonly Action<IProcedureItem, Vector2> _creationFunction;

        private StackPanel _listpanel;
        private List<ToolboxItemControl> _selectedItems;

        private readonly Synchronizer _synchronizer = new Synchronizer();

        private TextBox _textbox;



        public NodeToolboxWindowOld(Action<IProcedureItem, Vector2> creationFunction)
            //: base(services)
        {
            _creationFunction = creationFunction;

            Width = 400;
            Height = 400;

            Style = "MenuItemContent";

            foreach (var source in ProcedureItemLoader.SystemProcedureItems.Where(val => !val.Obsolete).OrderBy(val => val.Label))
            {
                var toolboxItemControl = new ToolboxItemControl(source);
                toolboxItemControl.DoubleClick += ToolboxItemControlOnDoubleClick;

                var gameProperty = toolboxItemControl.Properties.Get<bool>(ToolboxItemControl.IsSelectedPropertyId);
                gameProperty.Changed += GamePropertyOnChanged;

                _allItems.Add(toolboxItemControl);
            }

            _selectedItems = _allItems.ToList();

            LoadContent();
        }



        private void ToolboxItemControlOnDoubleClick(object sender, EventArgs eventArgs)
        {
            IProcedureItem item = ((ToolboxItemControl) sender).Item;

            _creationFunction(item, new Vector2(X, Y));

            Close();
        }



        protected void LoadContent()
        {
            _textbox = new ExtendedTextBox() {Margin = new Vector4F(5), HorizontalAlignment = HorizontalAlignment.Stretch};
            var textProperty = _textbox.Properties.Get<String>("Text");
            textProperty.Changed += (sender, args) => UpdateUIList();

            var windowLayoutPanel = new FlexibleStackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            _listpanel = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            _listpanel.Children.AddRange(_allItems);

            windowLayoutPanel.Children.Add(_textbox);
            windowLayoutPanel.Children.Add(new ScrollViewer()
            {
                Content = _listpanel,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalAlignment = VerticalAlignment.Stretch
            });

            //select the first item always
            Select(0);

            Content = windowLayoutPanel;
        }



        private void GamePropertyOnChanged(object sender, GamePropertyEventArgs<bool> gamePropertyEventArgs)
        {
            //if the object has just become selected, deselect the others
            //this could be recursively called, so we need this check
            if (gamePropertyEventArgs.NewValue)
            {
                foreach (var procedureItem in _allItems.Where(procedureItem => procedureItem != sender))
                {
                    procedureItem.IsSelected = false;
                }
            }
        }



        private void UpdateUIList()
        {
            string text = _textbox.Text.ToLower();

            String[] keywords = text.Split(' ');

            _selectedItems = _allItems.Where(x => x.Match(keywords)).ToList();

            _listpanel.Children.Clear();
            _listpanel.Children.AddRange(_selectedItems);

            //select the first item always
            Select(0);
        }



        public void OpenToolbox(GraphControl control, Vector2 position)
        {
            RefreshComponentNodes(control);

            UpdateUIList();

            X = position.X;
            Y = position.Y;

            this.Show(control.Screen);
            //control.Screen.Children.Add(this);

            _synchronizer.Enqueue(() => _textbox.Focus());

            //Activate();

            //Cursor = Screen.Renderer.GetCursor("Wait");
        }



        private void RefreshComponentNodes(GraphControl control)
        {
            //remove previous components
            _allItems.RemoveAll(x => x.Item is ComponentProcedureItem);

            //we need to get the extension of the graph file
            GraphCreator creator = new GraphCreator();

            //first, acess the project and get all its files
            var project = control.FileItem.Project;
            foreach (var fileItem in project.BaseFolder.Descendants.OfType<FileItem>().Where(x => x.Extension == creator.Extension))
            {
                if (fileItem == control.FileItem)
                    continue;

                var toolboxItemControl = new ToolboxItemControl(new ComponentProcedureItem(fileItem,control.VisualGraph.Environment));
                toolboxItemControl.DoubleClick += ToolboxItemControlOnDoubleClick;

                var gameProperty = toolboxItemControl.Properties.Get<bool>(ToolboxItemControl.IsSelectedPropertyId);
                gameProperty.Changed += GamePropertyOnChanged;

                _allItems.Add(toolboxItemControl);
            }
        }



        protected override void OnUpdate(TimeSpan deltaTime)
        {
            base.OnUpdate(deltaTime);

            _synchronizer.Update();
        }



        protected override void OnHandleInput(InputContext context)
        {
            var inputService = InputService;

            base.OnHandleInput(context);

            if ((inputService.IsPressed(MouseButtons.Left, false) ||
                 inputService.IsPressed(MouseButtons.Right, false) ||
                 inputService.IsPressed(MouseButtons.Middle, false)) && !IsMouseOver)
            {
                Close();

                return;
            }

            /*if (!InputService.IsMouseOrTouchHandled && IsMouseOver)
            {
                InputService.IsMouseOrTouchHandled = true;

                //if (IsMouseOver)
            }*/


/*if ((InputService.IsDown(MouseButtons.Left) || 
                            InputService.IsDown(MouseButtons.Right) || 
                            InputService.IsDown(MouseButtons.Middle)) && !IsMouseOver)
                        {
                            Close();
            
                            return;
                        }
            
                        if (IsMouseOver && !InputService.IsMouseOrTouchHandled)
                        {
                            InputService.IsMouseOrTouchHandled = true;
                        }*/

            if (!InputService.IsKeyboardHandled)
            {
                if (InputService.IsPressed(Keys.Down, true))
                {
                    Select(_selectedItems.FindIndex(val => val.IsSelected) + 1);

                    InputService.IsKeyboardHandled = true;
                }
                else if (InputService.IsPressed(Keys.Up, true))
                {
                    Select(_selectedItems.FindIndex(val => val.IsSelected) - 1);

                    InputService.IsKeyboardHandled = true;
                }
                else if (InputService.IsPressed(Keys.Escape, true))
                {
                    InputService.IsKeyboardHandled = true;

                    Close();
                }
                else if (InputService.IsPressed(Keys.Enter, true))
                {
                    InputService.IsKeyboardHandled = true;

                    var toolboxSystemProcedureItem = _selectedItems.FirstOrDefault(val => val.IsSelected);

                    if (toolboxSystemProcedureItem != null)
                    {
                        _creationFunction(toolboxSystemProcedureItem.Item, new Vector2(X, Y));
                        Close();
                    }
                }
            }
        }



        public void Select(int index)
        {
            if (index >= 0 && index < _selectedItems.Count)
            {
                _selectedItems[index].IsSelected = true;

                //int caretIndex = _textbox.CaretIndex;
                //_selectedItems[index].BringIntoView();

                _synchronizer.Enqueue(() => _selectedItems[index].BringIntoView());
                //_textbox.CaretIndex = caretIndex;
                //_textbox.CaretIndex = 0;
                //_textbox.BringIntoView();
            }
        }



        /*protected override void OnLoad()
        {
            base.OnLoad();

            _textbox.Focus();
        }*/

        /*protected override Vector2F OnMeasure(Vector2F availableSize)
        {
            Vector2D measure = base.OnMeasure(availableSize);

            var size = _allItems.Select(x => x.GetLabelWidth()).Max();

            foreach (var toolboxItemControl in _allItems)
            {
                toolboxItemControl.LabelBlock.Width = size;
            }


            return base.OnMeasure(availableSize);
        }*/
    }
}