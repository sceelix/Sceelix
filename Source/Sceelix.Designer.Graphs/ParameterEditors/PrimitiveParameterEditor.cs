using System;
using DigitalRune.Game.UI.Controls;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Designer.Graphs.GUI.Model;
using Sceelix.Designer.Graphs.Inspector.Graphs;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.ProjectExplorer.Management;

namespace Sceelix.Designer.Graphs.ParameterEditors
{
    /*public interface IPrimitiveParameterEditor
    {
        UIControl CreateControl(ParameterInfo parameterInfo, FileItem fileItem);
    }*/

    public abstract class PrimitiveParameterEditor<T> : ParameterEditor<T> where T : ParameterInfo //, IPrimitiveParameterEditor
    {
        public sealed override TreeViewItem CreateArgumentTreeViewItem(ParameterInfo argument, VisualNode visualNode, FileItem fileItem, string group = null)
        {
            Action onChanged = () => AlertGraphChange(fileItem);
            
            return new ArgumentTreeViewItem(argument, CreateControl((ParameterInfo)argument, fileItem,onChanged), visualNode, fileItem, group);
        }

        public override TreeViewItem CreateEditorTreeViewItem(GraphParameterEditorWindow editorWindow, ParameterInfo parameterInfo)
        {
            return new ParameterEditorTreeViewItem(parameterInfo, editorWindow);
        }



        public sealed override UIControl CreateControl(ParameterInfo argument, FileItem fileItem, Action onChanged)
        {
            return CreateControl((T)argument, fileItem, onChanged);
        }

        public virtual UIControl CreateControl(T argument, FileItem fileItem, Action onChanged)
        {
            return null;
        }
    }
}