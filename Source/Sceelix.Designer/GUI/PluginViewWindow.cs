using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Annotations;
using Sceelix.Designer.Annotations;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.Helpers;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;
using Sceelix.Extensions;
using Sceelix.Loading;

namespace Sceelix.Designer.GUI
{
    public class PluginViewWindow : DialogWindow
    {
        private ObjectTreeView _pluginAssemblyView;
        private readonly Texture2D _defaultIcon;
        //private GroupBox _rightDescriptionPanel;



        public PluginViewWindow()
        {
            Title = "Installed Plugins";
            Width = 600;
            Height = 500;

            _defaultIcon = EmbeddedResources.Load<Texture2D>("Resources/GoOut_48x48.png", GetType().Assembly);
        }




        protected override void OnLoad()
        {
            FlexibleStackPanel horizontalColumnPanel = new FlexibleStackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Vector4F(5)
            };

            List<KeyValuePair<String, Type>> libraryTypes = new List<KeyValuePair<String, Type>>();

            foreach (Type libraryType in SceelixDomain.Types.Where(x => typeof(SceelixLibraryAttribute).IsAssignableFrom(x)))
                libraryTypes.Add(new KeyValuePair<String, Type>(GetName(libraryType), libraryType));

            //this assembly is not read by reflection, so we need to add it manually
            libraryTypes.Add(new KeyValuePair<string, Type>(GetName(typeof(DesignerLibraryAttribute)), typeof(DesignerLibraryAttribute)));

            libraryTypes = libraryTypes.OrderBy(x => x.Key).ToList();

            //also, add the supertype at the first position
            var firstItem = new KeyValuePair<String, Type>(GetName(typeof(SceelixLibraryAttribute)), typeof(SceelixLibraryAttribute));
            libraryTypes.Insert(0, firstItem);

            
            var libraryTypeView = new ObjectTreeView()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HasChildren = obj => false,
                GetColumnValue = (o, column) => ((KeyValuePair<String, Type>) o).Key,
                ViewType = ObjectTreeView.ListViewType.List,
                HasItemToolTip = true,
                SelectionType = ObjectTreeView.SelectionViewType.Line,
                ItemHeight = 18,
                ItemPadding = new Vector4F(3, 3, 3, 3),
                Items = libraryTypes.Cast<Object>(),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            libraryTypeView.ItemSelected += delegate(object item)
            {
                var type = ((KeyValuePair<String, Type>) item).Value;

                _pluginAssemblyView.Items = SceelixDomain.SceelixAssemblies.Where(x => x.HasCustomAttribute(type));

                _pluginAssemblyView.SelectFirst(true);
            };

            horizontalColumnPanel.Children.Add(new GroupBox()
            {
                Content = libraryTypeView,
                Title = "Type",
                VerticalAlignment = VerticalAlignment.Stretch,
                Width = 150
            });


            _pluginAssemblyView = new ObjectTreeView()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HasChildren = obj => false,
                ViewType = ObjectTreeView.ListViewType.List,
                HasItemToolTip = true,
                SelectionType = ObjectTreeView.SelectionViewType.Line,
                GetCellControl = (column, o) => CreatePluginItem((Assembly)o),
                ItemHeight = 60,
                ItemPadding = new Vector4F(3, 3, 3, 3),
            };
            _pluginAssemblyView.ItemSelected += delegate
            {
                //var assembly = (Assembly)item;

                //_rightDescriptionPanel.Content = CreatePluginDescription(assembly);
            };
            horizontalColumnPanel.Children.Add(new GroupBox()
            {
                Content = _pluginAssemblyView,
                Title = "Plugins",
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            });
            /*
            horizontalColumnPanel.Children.Add(_rightDescriptionPanel = new GroupBox()
            {
                Width = 200,
                Title = "Description",
                VerticalAlignment = VerticalAlignment.Stretch,
            });*/

            DialogContent = horizontalColumnPanel;

            AddOKButton();
            AddDialogButton("Open Folder", () => UrlHelper.OpenFolderInExplorer(SceelixApplicationInfo.PluginsFolder));

            base.OnLoad();

            libraryTypeView.SelectFirst(true);
        }



        private string GetName(Type libraryType)
        {
            return libraryType.GetCustomAttribute<DisplayNameAttribute>().DisplayName;
        }



        private UIControl CreatePluginItem(Assembly assembly)
        {
            var titleAttribute = assembly.GetCustomAttribute<AssemblyTitleAttribute>();
            var title = titleAttribute != null ? titleAttribute.Title : assembly.FullName;

            var descriptionAttribute = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
            var description = descriptionAttribute != null && !String.IsNullOrWhiteSpace(descriptionAttribute.Description) ? descriptionAttribute.Description : "(No description available)";

            var iconAttribute = assembly.GetCustomAttribute<AssemblyIconAttribute>();
            var icon = iconAttribute != null ? EmbeddedResources.Load<Texture2D>(iconAttribute.IconPath, assembly) : _defaultIcon;

            var newStackPanel = new StackPanel() {HorizontalAlignment = HorizontalAlignment.Stretch, Orientation = Orientation.Horizontal};

            newStackPanel.Children.Add(new Image()
            {
                Texture = icon,
                Margin = new Vector4F(5)
            });

            var textPanel = new StackPanel() {HorizontalAlignment = HorizontalAlignment.Stretch, Orientation = Orientation.Vertical, Margin = new Vector4F(3,5,30,3)};
            textPanel.Children.Add(new TextBlock() {Text = title, Font = "DefaultBold" });
            textPanel.Children.Add(new TextBlock() {Text = description, WrapText = true, ToolTip = new ToolTipControl(title,description), Margin = new Vector4F(0,5,3,3) });
            newStackPanel.Children.Add(textPanel);

            

            return newStackPanel;
        }



        private UIControl CreatePluginDescription(Assembly assembly)
        {
            var titleAttribute = assembly.GetCustomAttribute<AssemblyTitleAttribute>();
            var title = titleAttribute != null ? titleAttribute.Title : assembly.FullName;

            var descriptionAttribute = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
            var description = descriptionAttribute != null && !String.IsNullOrWhiteSpace(descriptionAttribute.Description) ? descriptionAttribute.Description : "(No description available)";
            
            var textStackPanel = new StackPanel()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                Orientation = Orientation.Vertical,
                Margin = new Vector4F(5)
            };

            textStackPanel.Children.Add(AddTextPanelContent("Author: ", title));
            textStackPanel.Children.Add(AddTextPanelContent("Email: ", "pedro.dsa@dsajk.comfjdsfjdksjfdfsd"));

            /*textStackPanel.Children.Add(new TextBlock
            {
                Margin = new Vector4F(10, 0, 10, 10),
                Text = "Author: " + title
            });
            textStackPanel.Children.Add(new TextBlock
            {
                Margin = new Vector4F(10, 0, 10, 10),
                Text = title
            });

            textStackPanel.Children.Add(new TextBlock
            {
                Margin = new Vector4F(20, 0, 0, 5),
                Text = "Name: "
            });


            textStackPanel.Children.Add(new TextBlock
            {
                Margin = new Vector4F(20, 0, 0, 5),
                Text = "Version"
            });*/

            return textStackPanel;
        }



        private UIControl AddTextPanelContent(string label, string content)
        {
            var textStackPanel = new StackPanel() {Orientation = Orientation.Horizontal};

            textStackPanel.Children.Add(new TextBlock
            {
                Margin = new Vector4F(0, 0, 0, 0),
                Text = label
            });
            textStackPanel.Children.Add(new TextBlock
            {
                Margin = new Vector4F(5, 0, 10, 20),
                Text = content,
                //UseEllipsis = true,
                WrapText = true
            });

            return textStackPanel;
        }
    }
}