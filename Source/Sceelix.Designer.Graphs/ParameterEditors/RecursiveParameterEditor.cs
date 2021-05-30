using System;
using Sceelix.Core.Parameters;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Designer.Graphs.Annotations;
using Sceelix.Designer.Graphs.GUI.Model;
using Sceelix.Designer.Graphs.Inspector.Graphs;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Graphs.ParameterEditors
{
    [ParameterEditor(typeof(RecursiveParameterInfo))]
    public class RecursiveParameterEditor : ParameterEditor<RecursiveParameterInfo>
    {
        private ParameterEditorManager _parameterEditorManager;


        public override void Initialize(IServiceLocator services)
        {
            base.Initialize(services);

            _parameterEditorManager = services.Get<ParameterEditorManager>();
        }

        public override TreeViewItem CreateArgumentTreeViewItem(ParameterInfo argument, VisualNode visualNode, FileItem fileItem, string group = null)
        {
            //throw new Exception("This should not be called");

            RecursiveParameterInfo info = (RecursiveParameterInfo) argument;

            var actualParameterInfo = info.GetActualParameterInfo();

            var parameterEditorManager = _parameterEditorManager.GetParameterEditor(actualParameterInfo.GetType());

            return parameterEditorManager.CreateArgumentTreeViewItem(actualParameterInfo, visualNode, fileItem);
        }



        public override TreeViewItem CreateEditorTreeViewItem(GraphParameterEditorWindow editorWindow, ParameterInfo parameterInfo)
        {
            return new ParameterEditorTreeViewItem(parameterInfo, editorWindow);
        }



        public override bool CanExistAtRoot
        {
            get { return false; }
        }
    }
}