using System;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Designer.Graphs.Annotations;
using Sceelix.Designer.Graphs.GUI.Model;
using Sceelix.Designer.Graphs.Inspector.Graphs;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.ProjectExplorer.Management;

namespace Sceelix.Designer.Graphs.ParameterEditors
{
    [ParameterEditor(typeof(ObjectParameterInfo))]
    public class ObjectParameterEditor : ParameterEditor<ObjectParameterInfo>
    {
        public sealed override TreeViewItem CreateArgumentTreeViewItem(ParameterInfo argument, VisualNode visualNode, FileItem fileItem, string group = null)
        {
            var control = new TextBlock()
            {
                Text = "Use Expression to set value.",
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            return new ArgumentTreeViewItem(argument, control, visualNode, fileItem, group);
        }


        public override TreeViewItem CreateEditorTreeViewItem(GraphParameterEditorWindow editorWindow, ParameterInfo parameterInfo)
        {
            return new ParameterEditorTreeViewItem(parameterInfo, editorWindow);
        }
    }
}