using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DigitalRune.Game.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Collections;
using Sceelix.Core.Annotations;
using Sceelix.Core.Attributes;
using Sceelix.Core.Data;
using Sceelix.Core.Messages;
using Sceelix.Designer.Annotations;
using Sceelix.Designer.Graphs.Extensions;
using Sceelix.Designer.Graphs.Inspector.Entities.SubObjects;
using Sceelix.Designer.Graphs.Messages;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.MenuControls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.Helpers;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;
using Sceelix.Extensions;

namespace Sceelix.Designer.DataExplorer
{
    [DesignerWindow("Data Explorer")]
    public class DataExplorerWindow : AnimatedWindow, IServiceable
    {
        private static readonly Dictionary<Type, List<KeyValuePair<SubEntityAttribute, PropertyInfo>>> _propertyInfos = new Dictionary<Type, List<KeyValuePair<SubEntityAttribute, PropertyInfo>>>();

        private ObjectTreeView _objectTreeView;
        private IServiceLocator _services;


        public void Initialize(IServiceLocator services)
        {
            _services = services;
        }


        protected override void OnLoad()
        {
            base.OnLoad();

            Title = "Data Explorer";
            Width = 500;
            Height = 500;
            CanResize = true;

            var windowStackPanel = new FlexibleStackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            //the top bar menu 
            BarMenu barMenu = new BarMenu();
            barMenu.MenuChildren.Add(new MenuChild(ClearTreeView) { Text = "Clear", Icon = EmbeddedResources.Load<Texture2D>("Resources/Trash_16x16.png") });
            windowStackPanel.Children.Add(barMenu);

            //the control that shows the data
            windowStackPanel.Children.Add(_objectTreeView = new ObjectTreeView()
            {
                ShowColumnHeaders = false,
                HasItemToolTip = true,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HasChildren = HasChildren,
                GetChildren = GetChildren,
                IsInitiallyExpanded = IsInitiallyExpanded,
                GetColumnValue = GetColumnValue,
                ItemHeight = 18,
                GetForeground = GetForeground,
                GetItemTooltip = GetItemToolTip,
                SelectionType = ObjectTreeView.SelectionViewType.Line,
                GetContextMenu = GetContextMenu,
                SelectionColor = new Color(140, 140, 140, 140)
            });
            _objectTreeView.ItemEntered += ObjectTreeViewOnItemEntered;
            _objectTreeView.ItemSelected += ObjectTreeViewOnItemSelected;

            Content = windowStackPanel;

            _services.Get<MessageManager>().Register<EntitySelected>(OnEntitySelected);
            _services.Get<MessageManager>().Register<EntityDeselected>(OnEntityDeselected);
            _services.Get<MessageManager>().Register<EntityDataReady>(OnEntityDataReady);
            _services.Get<MessageManager>().Register<GraphExecutionFinished>(OnGraphExecutionFinished);
        }

        



        private object GetItemToolTip(object obj)
        {
            if (obj is IInspectionInfo)
            {
                var inspectionInfo = (IInspectionInfo) obj;
                var description = inspectionInfo.Description;

                if (!String.IsNullOrWhiteSpace(description))
                    return new ToolTipControl(inspectionInfo.Label, description);
            }

            return null;
        }



        private MultiContextMenu GetContextMenu(object o, ObjectTreeView.Column column)
        {
            if (o is KeyValueInspectionInfo)
            {
                var keyValue = (KeyValueInspectionInfo)o;

                MultiContextMenu multiContextMenu = new MultiContextMenu();
                multiContextMenu.MenuChildren.Add(new MenuChild(CopyText) {Text = "Copy", UserData = keyValue.Label + " = " + keyValue.ValueAsString });
                multiContextMenu.MenuChildren.Add(new MenuChild(CopyText) { Text = "Copy Key", UserData = keyValue.Label });
                multiContextMenu.MenuChildren.Add(new MenuChild(CopyText) { Text = "Copy Value", UserData = keyValue.ValueAsString });
                return multiContextMenu;
            }

            return null;
        }



        private void CopyText(MenuChild obj)
        {
            var text = (String) obj.UserData;

            ClipboardHelper.Copy(text);
        }



        private void OnEntityDeselected(EntityDeselected obj)
        {
            _objectTreeView.Deselect(obj.Entity);

        }



        private void OnEntitySelected(EntitySelected obj)
        {
            if(obj.Entity != null)
                _objectTreeView.Select(new EntityInspectionInfo(obj.Entity));
        }



        private Color GetForeground(object o, ObjectTreeView.Column arg2)
        {
            if (o is KeyValueInspectionInfo)
                return Color.Orange;

            if (o is EntityInspectionInfo && ((EntityInspectionInfo)o).Entity.GetIsUnprocessedEntity()) 
                return Color.Red;

            return Color.White;
        }



        private void ObjectTreeViewOnItemSelected(object item)
        {
            
            if(item is EntityInspectionInfo)
                _services.Get<MessageManager>().Publish(new EntitySelected(((EntityInspectionInfo)item).Entity,"Data Explorer"),this);
        }



        private void ObjectTreeViewOnItemEntered(object item)
        {
            if (item is EntityInspectionInfo)
                _services.Get<MessageManager>().Publish(new EntityFocused(((EntityInspectionInfo)item).Entity, "Data Explorer"), this);
        }



        private object GetColumnValue(object obj, ObjectTreeView.Column column)
        {
            if (obj is KeyValueInspectionInfo)
            {
                var keyValue = ((KeyValueInspectionInfo)obj);

                if (keyValue.Value is SceeList)
                    return keyValue.Label;

                return keyValue.Label + " = " + keyValue.ValueAsString;
            }
            else if (obj is IInspectionInfo)
            {
                return obj.CastTo<IInspectionInfo>().Label;
            }


            return null;
        }



        private bool IsInitiallyExpanded(object o)
        {
            if (o is IInspectionInfo)
                return o.CastTo<IInspectionInfo>().IsInitiallyExpanded;
            
            return false;
        }



        private bool HasChildren(object o)
        {
            if (o is IInspectionInfo)
                return o.CastTo<IInspectionInfo>().HasChildren;

            return false;
        }



        private IEnumerable<object> GetChildren(object o)
        {
            if (o is IInspectionInfo)
            {
                foreach (var child in o.CastTo<IInspectionInfo>().Children)
                    yield return child;
            }
        }


        private void ClearTreeView(MenuChild obj)
        {
            _objectTreeView.Clear();
        }



        private void OnEntityDataReady(EntityDataReady obj)
        {
            AddEntities(obj.Data.Distinct());
        }



        private void OnGraphExecutionFinished(GraphExecutionFinished obj)
        {
            AddEntities(obj.Data.Distinct());
        }



        private void AddEntities(IEnumerable<IEntity> entities)
        {
            _objectTreeView.Items = entities.Select(x => new EntityInspectionInfo(x)).ToList();
        }
    }
}