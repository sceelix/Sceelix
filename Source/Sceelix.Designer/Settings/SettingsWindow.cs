using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Sceelix.Designer.GUI;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.Helpers;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;
using Sceelix.Extensions;

namespace Sceelix.Designer.Settings
{
    public class SettingsWindow : DialogWindow
    {
        private readonly string _sectionToFocus;
        private readonly TreeView _treeview;
        private readonly ScrollViewer _inspectorPanel;
        private readonly SettingsManager _settingsManager;



        public SettingsWindow(SettingsManager settingsManager, string sectionToFocus)
        {
            _sectionToFocus = sectionToFocus;

            Width = 650;
            Height = 500;
            Title = "Sceelix Settings";

            _settingsManager = settingsManager;
            

            var stackPanelTreeViewInspector = new FlexibleStackPanel()
            {
                Margin = new Vector4F(4),
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            //Left side, the treeViewControl containing the parameters
            stackPanelTreeViewInspector.Children.Add(
                new GroupBox()
                {
                    Title = "Sections",
                    Content = new ScrollViewer()
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Content = _treeview = new TreeView()
                        {
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch
                        }
                    },
                    //HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                });

            foreach (var settingsRegistry in _settingsManager.SettingsRegistries)
            {
                var names = settingsRegistry.RegisteredName.Split('/');

                ProcessIntoTree(names, _treeview, settingsRegistry);
            }


            //Right side, where the options of the parameterinfo will appear
            stackPanelTreeViewInspector.Children.Add(
                new GroupBox()
                {
                    Title = "Inspector",
                    Content = _inspectorPanel = new ScrollViewer()
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch
                    },
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                });
            
            AddOKCancelButton();
            AddDialogButton("Open Folder", () => UrlHelper.OpenUrlInBrowser(SceelixApplicationInfo.SettingsFolder));
            
            DialogContent = stackPanelTreeViewInspector;

            Accepted += OnAccepted;
            Canceled += OnCanceled;
        }



        private void OnAccepted(object sender, EventArgs eventArgs)
        {
            foreach (var settingsRegistry in _settingsManager.SettingsRegistries)
            {
                foreach (var applicationSetting in settingsRegistry.Settings.OfType<IVisualApplicationField>())
                {
                    applicationSetting.ApplyProposal();
                }
            }
        }

        private void OnCanceled(object sender, EventArgs eventArgs)
        {
            foreach (var settingsRegistry in _settingsManager.SettingsRegistries)
            {
                foreach (var applicationSetting in settingsRegistry.Settings.OfType<IVisualApplicationField>())
                {
                    applicationSetting.RejectProposal();
                }
            }
        }



        protected override void OnLoad()
        {
            base.OnLoad();

            foreach (SelectableTreeViewItem treeViewItem in _treeview.Items.OfType<SelectableTreeViewItem>())
                FocusOnSelection(_sectionToFocus, treeViewItem, treeViewItem.Text);
        }



        private void FocusOnSelection(string sectionToFocus, SelectableTreeViewItem treeViewItem, string currentSection)
        {
            if (currentSection == sectionToFocus)
            {
                treeViewItem.IsSelected = true;
                OnClick(treeViewItem, EventArgs.Empty);
            }
            else
            {
                foreach (SelectableTreeViewItem subViewItem in treeViewItem.Items.OfType<SelectableTreeViewItem>())
                {
                    FocusOnSelection(sectionToFocus, subViewItem, currentSection + "/" + subViewItem.Text);
                }
            }
        }



        private void ProcessIntoTree(IEnumerable<String> names, ITreeViewControl parentTreeViewControl, SettingsRegistry settingsRegistry)
        {
            var namesList = names.ToList();

            //if there are no more names, let's end up here
            if (!namesList.Any())
            {
                ((SelectableTreeViewItem) parentTreeViewControl).UserData = settingsRegistry;
            }
            else
            {
                //otherwise, try to look for the first item
                var subTreeViewControl = parentTreeViewControl.Items.OfType<SelectableTreeViewItem>().FirstOrDefault(x => x.Text == namesList[0]);
                if (subTreeViewControl == null)
                {
                    subTreeViewControl = new SelectableTreeViewItem() {Text = namesList[0]};
                    subTreeViewControl.Click += OnClick;
                    parentTreeViewControl.Items.Add(subTreeViewControl);
                }

                ProcessIntoTree(namesList.Skip(1), subTreeViewControl, settingsRegistry);
            }
        }



        private void OnClick(object sender, EventArgs eventArgs)
        {
            SelectableTreeViewItem item = (SelectableTreeViewItem) sender;

            var settingsRegistry = item.UserData as SettingsRegistry;
            if (settingsRegistry != null)
            {
                StackPanel inspectorVerticalStack = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                foreach (IGrouping<string, IVisualApplicationField> visualApplicationFields in settingsRegistry.Settings.OfType<IVisualApplicationField>().GroupBy(x => x.Category))
                {
                    LayoutControl layoutControl = new LayoutControl();

                    foreach (IVisualApplicationField visualApplicationField in visualApplicationFields)
                    {
                        var title = visualApplicationField.Label.ToSplitCamelCase();
                        var panel = layoutControl.Add(title, visualApplicationField.GetControl());
                        var commentText = settingsRegistry.GetComment(visualApplicationField.Label);
                        if (!String.IsNullOrEmpty(commentText))
                            panel.ToolTip = new ToolTipControl(title, commentText);
                    }

                    if (String.IsNullOrWhiteSpace(visualApplicationFields.Key))
                    {
                        inspectorVerticalStack.Children.Add(layoutControl);
                    }
                    else
                    {
                        GroupBox groupBox = new GroupBox()
                        {
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            Title = visualApplicationFields.Key,
                            Content = layoutControl
                        };

                        inspectorVerticalStack.Children.Add(groupBox);
                    }
                }

                _inspectorPanel.Content = inspectorVerticalStack;
            }
            else
            {
                _inspectorPanel.Content = null;
            }
        }


        
    }
}