using System;
using System.Collections.Generic;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Sceelix.Core.Data;
using Sceelix.Designer.Graphs.Inspector.Entities.SubObjects;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.MenuControls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.Helpers;
using Sceelix.Extensions;

namespace Sceelix.Designer.Graphs.Inspector.Entities
{
    public class PropertyController : GroupBox
    {
        private readonly ObjectTreeView.Column _nameColumn;
        private readonly ObjectTreeView.Column _valueColumn;



        public PropertyController(IEntity entity)
        {
            Title = "Attributes";
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
            Margin = new Vector4F(0, 2, 0, 0);

            ObjectTreeView objectTreeView;
            Content = objectTreeView = new ObjectTreeView()
            {
                ShowColumnHeaders = true,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HasChildren = delegate(object o)
                {
                    if (o is IInspectionInfo)
                        return o.CastTo<IInspectionInfo>().HasChildren;

                    return false;
                },
                GetChildren = GetChildren,
                IsInitiallyExpanded = delegate(object o)
                {
                    if (o is IInspectionInfo)
                        return o.CastTo<IInspectionInfo>().IsInitiallyExpanded;

                    return false;
                },
                GetColumnValue = delegate(object o, ObjectTreeView.Column column)
                {
                    if (column == _nameColumn)
                    {
                        if (o is IInspectionInfo)
                            return o.CastTo<IInspectionInfo>().Label;
                    }
                    else if (column == _valueColumn)
                    {
                        if (o is KeyValueInspectionInfo)
                            return o.CastTo<KeyValueInspectionInfo>().ValueAsString;
                    }

                    return null;
                },
                GetForeground = GetForeground,
                ItemHeight = 18,
                SelectionType = ObjectTreeView.SelectionViewType.Line,
                SelectionColor = new Color(140, 140, 140, 140),
                GetContextMenu = GetContextMenu,
            };

            objectTreeView.Columns.Add(_nameColumn = new ObjectTreeView.Column("Name") {Flexible = false, Width = 150});
            objectTreeView.Columns.Add(_valueColumn = new ObjectTreeView.Column("Value") {Flexible = true, Width = 100});
            
            objectTreeView.Items = new EntityInspectionInfo(entity).Children;

            Content = objectTreeView;
        }



        private Color GetForeground(object o, ObjectTreeView.Column arg2)
        {
            if (o is KeyValueInspectionInfo)
                return Color.Orange;

            return Color.White;
        }



        private MultiContextMenu GetContextMenu(object o, ObjectTreeView.Column arg2)
        {
            if (o is KeyValueInspectionInfo)
            {
                var keyValue = (KeyValueInspectionInfo)o;

                MultiContextMenu multiContextMenu = new MultiContextMenu();
                multiContextMenu.MenuChildren.Add(new MenuChild(CopyText) { Text = "Copy", UserData = keyValue.Label + " = " + keyValue.ValueAsString });
                multiContextMenu.MenuChildren.Add(new MenuChild(CopyText) { Text = "Copy Key", UserData = keyValue.Label });
                multiContextMenu.MenuChildren.Add(new MenuChild(CopyText) { Text = "Copy Value", UserData = keyValue.ValueAsString });
                return multiContextMenu;
            }

            return null;
        }


        private void CopyText(MenuChild obj)
        {
            var text = (String)obj.UserData;

            ClipboardHelper.Copy(text);
        }


        private IEnumerable<object> GetChildren(object o)
        {
            if (o is IInspectionInfo)
            {
                foreach (var child in o.CastTo<IInspectionInfo>().Children)
                    yield return child;
            }
        }
    }
}