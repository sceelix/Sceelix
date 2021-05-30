using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sceelix.Designer.Graphs.Handlers;
using Sceelix.Designer.Graphs.Tools;
using Sceelix.Designer.GUI;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.MenuControls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;
using Sceelix.Extensions;


namespace Sceelix.Designer.Graphs.GUI.Toolbox
{
    public class NodeToolboxWindow : Window
    {
        /// <summary>
        /// Storage of items and their categories
        /// </summary>
        private readonly Dictionary<String, List<IProcedureItem>> _categoryItems = new Dictionary<String, List<IProcedureItem>>();

        /// <summary>
        /// delegate of the function that creates an instance of the given procedure item.
        /// </summary>
        private readonly Action<IProcedureItem, Vector2> _creationFunction;

        /// <summary>
        /// Allows functions to be called in the update cycle, not directlty in the input functions
        /// which sometimes cause issues.
        /// </summary>
        private readonly Synchronizer _synchronizer = new Synchronizer();

        private Vector2 _clickLocation;
        private ScrollViewer _scrollViewer;

        /// <summary>
        /// textbox where one can search for items
        /// </summary>
        private TextBox _textbox;

        private TextBlock _textBoxDescription;

        private TextBlock _textBoxTags;

        /// <summary>
        /// Control where the items are listed
        /// </summary>
        private TreeView _treeView;



        public NodeToolboxWindow(Action<IProcedureItem, Vector2> creationFunction)
        {
            _creationFunction = creationFunction;

            Width = 400;
            Height = 400;

            Style = "MenuItemContent";

            LoadSystemProcedureItems();

            LoadContent();
        }



        public List<SelectableTreeViewItem> Leaves
        {
            get { return _treeView.GetLeafTreeItems().OfType<SelectableTreeViewItem>().ToList(); }
        }



        private void LoadSystemProcedureItems()
        {
            foreach (SystemProcedureItem source in ProcedureItemLoader.SystemProcedureItems.Where(val => !val.Obsolete).OrderBy(val => val.Label))
            {
                List<IProcedureItem> list;
                if (!_categoryItems.TryGetValue(source.Category, out list))
                    _categoryItems.Add(source.Category, list = new List<IProcedureItem>());

                list.Add(source);
            }
        }



        private void LoadComponentProcedureItems(GraphControl control)
        {
            foreach (var categoryItem in _categoryItems.ToList())
            {
                categoryItem.Value.RemoveAll(x => x is ComponentProcedureItem);
                if (!categoryItem.Value.Any())
                    _categoryItems.Remove(categoryItem.Key);
            }

            //we need to get the extension of the graph file
            GraphCreator creator = new GraphCreator();

            //first, access the project and get all its files
            var project = control.FileItem.Project;
            foreach (var fileItem in project.BaseFolder.Descendants.OfType<FileItem>().Where(x => x.Extension == creator.Extension))
            {
                if (fileItem == control.FileItem)
                    continue;

                var source = new ComponentProcedureItem(fileItem, control.VisualGraph.Environment);

                List<IProcedureItem> list;
                if (!_categoryItems.TryGetValue(source.Category, out list))
                    _categoryItems.Add(source.Category, list = new List<IProcedureItem>());

                list.Add(source);
            }
        }



        protected void LoadContent()
        {
            _textbox = new ExtendedTextBox() {Margin = new Vector4F(5), HorizontalAlignment = HorizontalAlignment.Stretch};
            var textProperty = _textbox.Properties.Get<String>("Text");
            textProperty.Changed += (sender, args) => UpdateTreeView();

            var windowLayoutPanel = new FlexibleStackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            _treeView = new TreeView()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Vector4F(4, 0, 4, 0)
            };
            /*_treeView.Items.CollectionChanged +=
                delegate(object sender, CollectionChangedEventArgs<TreeViewItem> args)
                {
                    foreach (TreeViewItem treeViewItem in args.NewItems)
                    {
                        var property = treeViewItem.Properties.Get<bool>("IsExpanded");
                        property.Changed += delegate(object o, GamePropertyEventArgs<bool> eventArgs)
                        {
                            this.InvalidateMeasure();
                        };
                    }
                };*/

            UpdateTreeView();

            windowLayoutPanel.Children.Add(_textbox);
            windowLayoutPanel.Children.Add(_scrollViewer = new VerticalScrollViewer()
            {
                Content = _treeView,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalAlignment = VerticalAlignment.Stretch,
            });

            var bottomStackPanel = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 63,
                Orientation = Orientation.Vertical,
                //Padding = new Vector4F(5, 0, 5, 0),
                Style = "MenuItemContent"
            };
            bottomStackPanel.Children.Add(_textBoxDescription = new TextBlock() {Text = "", Height = 35, Foreground = Color.Black, Margin = new Vector4F(5, 5, 5, 0), WrapText = true});
            bottomStackPanel.Children.Add(_textBoxTags = new TextBlock() {Text = "", Height = 13, Margin = new Vector4F(5, 5, 5, 0)});


            windowLayoutPanel.Children.Add(bottomStackPanel);

            Content = windowLayoutPanel;
        }



        private void UpdateTreeView()
        {
            string text = _textbox.Text.ToLower();

            String[] keywords = text.Split(' ');

            Dictionary<TreeViewItem, int> categoryScores = new Dictionary<TreeViewItem, int>();
            

            foreach (KeyValuePair<String, List<IProcedureItem>> categoryItem in _categoryItems)
            {
                TreeViewItem categoryTreeItem = new SelectableTreeViewItem() {Text = categoryItem.Key, Texture = EmbeddedResources.Load<Texture2D>("Resources/folder3.png")};
                
                Dictionary<TreeViewItem, int> itemScores = new Dictionary<TreeViewItem, int>();
                int totalScore = 0;

                foreach (IProcedureItem procedureItem in categoryItem.Value)
                {
                    var score = MatchWithScore(procedureItem, keywords);
                    if (score > 0)
                    {
                        var selectableTreeViewItem = new SelectableTreeViewItem() {Text = procedureItem.Label, Texture = EmbeddedResources.Load<Texture2D>("Resources/Gear_16x16.png"), UserData = procedureItem};
                        selectableTreeViewItem.Click += OnTreeViewItemClick;
                        selectableTreeViewItem.DoubleClick += OnTreeViewItemDoubleClick;
                        itemScores.Add(selectableTreeViewItem, score);

                        totalScore += score;
                    }
                }

                if (totalScore > 0)
                {
                    //add the items, prioritizing those with higher score
                    categoryTreeItem.Items.AddRange(itemScores
                        .OrderByDescending(x => x.Value)
                        .Select(x => x.Key));
                    categoryScores.Add(categoryTreeItem, totalScore);
                }
            }

            _treeView.Items.Clear();

            //add the category items, prioritizing those with higher total score
            //(i.e. so that categories with more matching items have priority)
            if (categoryScores.Any())
                _treeView.Items.AddRange(categoryScores
                    .OrderByDescending(x => x.Value)
                    .Select(x => x.Key));
            
            _synchronizer.Enqueue(() => Select(0));

            if (_scrollViewer != null)
                _synchronizer.Enqueue(_scrollViewer.InvalidateMeasure);
        }



        private void OnTreeViewItemClick(object sender, EventArgs e)
        {
            UpdateBottomTextboxes((IProcedureItem) ((SelectableTreeViewItem) sender).UserData);

            _synchronizer.Enqueue(() => _textbox.Focus());
        }



        private void OnTreeViewItemDoubleClick(object sender, EventArgs eventArgs)
        {
            IProcedureItem item = (IProcedureItem) ((SelectableTreeViewItem) sender).UserData;

            _creationFunction(item, _clickLocation);

            _synchronizer.Enqueue(Close);
        }



        public bool Match(IProcedureItem item, string[] keywords)
        {
            var lowerCaseLabel = item.Label.ToLower();
            var lowerCaseTags = item.Tags.ToLower();

            return keywords.All(x => lowerCaseLabel.Contains(x) || lowerCaseTags.Contains(x));
        }



        public int MatchWithScore(IProcedureItem item, string[] keywords)
        {
            var lowerCaseLabel = item.Label.ToLower();
            var lowerCaseTags = item.Tags.ToLower();

            //start with 1, so that, even if the list is empty, the item is valid
            int score = 1;

            foreach (string keyword in keywords)
            {
                bool containsInLabel = lowerCaseLabel.Contains(keyword);
                bool containsInTags = lowerCaseTags.Contains(keyword);

                if (!containsInLabel && !containsInTags)
                    return 0;

                var maxDistance = Math.Max(keyword.Length, lowerCaseLabel.Length);

                if (containsInLabel)
                    score += 100 * (maxDistance - lowerCaseLabel.LevenshteinDistanceTo(keyword));

                if (containsInTags)
                    score += 1 * (maxDistance - lowerCaseLabel.LevenshteinDistanceTo(keyword));
            }

            return score;
        }



        public void OpenToolbox(GraphControl control, Vector2 position)
        {
            LoadComponentProcedureItems(control);

            UpdateTreeView();

            _clickLocation = position;
            X = position.X + this.Width > control.Screen.ActualWidth ? position.X - this.Width : position.X;
            Y = position.Y + this.Height > control.Screen.ActualHeight ? position.Y - this.Height : position.Y;

            Show(control.Screen);

            _synchronizer.Enqueue(() => _textbox.Focus());
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

            if (!InputService.IsKeyboardHandled)
            {
                if (InputService.IsPressed(Keys.Down, true))
                {
                    SelectOffset(+1);

                    InputService.IsKeyboardHandled = true;
                }
                else if (InputService.IsPressed(Keys.Up, true))
                {
                    SelectOffset(-1);

                    InputService.IsKeyboardHandled = true;
                }
                else if (InputService.IsPressed(Keys.F1, true))
                {
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

                    var toolboxSystemProcedureItem = Leaves.FirstOrDefault(val => val.IsSelected);

                    if (toolboxSystemProcedureItem != null)
                    {
                        _creationFunction((IProcedureItem) toolboxSystemProcedureItem.UserData, _clickLocation);
                        Close();
                    }
                }
            }
        }



        private void SelectOffset(int offset)
        {
            Select(Leaves.FindIndex(val => val.IsSelected) + offset);
        }



        public void Select(int index)
        {
            var leaves = Leaves;
            leaves.ForEach(x => x.IsSelected = false);

            if (leaves.Any())
            {
                index = GetNormalizedIndex(index, leaves.Count);

                Select(leaves[index]);
            }
            else
            {
                UpdateBottomTextboxes(null);
            }
        }


        public void Select(SelectableTreeViewItem treeViewItem)
        {
            treeViewItem.IsSelected = true;
            _synchronizer.Enqueue(() => treeViewItem.BringIntoView());

            UpdateBottomTextboxes((IProcedureItem)treeViewItem.UserData);
        }


        private void UpdateBottomTextboxes(IProcedureItem procedureItem)
        {
            if (procedureItem != null)
            {
                _textBoxDescription.ToolTip = new ToolTipControl(_textBoxDescription.Text = procedureItem.Description.Split('\n').FirstOrDefault()); //.Replace("\n", "").Replace("\r\n", "");
                _textBoxTags.ToolTip = new ToolTipControl(_textBoxTags.Text = "Tags: " + procedureItem.Tags);
            }
            else
            {
                _textBoxTags.ToolTip = _textBoxDescription.ToolTip = _textBoxTags.Text = _textBoxDescription.Text = String.Empty;
            }
        }



        public int GetNormalizedIndex(int index, int count)
        {
            if (index < 0)
                return count + index%count;
            if (index < count) //most cases will probaly fit this case anyway, requiring only 2 simple conditions to reach it
                return index;

            return index%count;
            //this expression would be simpler, but requires the expensive modulo operation every time
            //return index < 0 ? Count + index % Count : index % Count;
        }
    }
}