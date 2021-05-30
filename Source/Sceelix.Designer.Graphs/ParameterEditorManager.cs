using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Linq;
using Sceelix.Annotations;
using Sceelix.Designer.Annotations;
using Sceelix.Designer.Graphs.Annotations;
using Sceelix.Designer.Graphs.ParameterEditors;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Services;
using ParameterInfo = Sceelix.Core.Parameters.Infos.ParameterInfo;

namespace Sceelix.Designer.Graphs
{
    [DesignerService]
    public class ParameterEditorManager : IServiceable
    {
        private Dictionary<Type, ParameterEditor> _parameterEditors = new Dictionary<Type, ParameterEditor>();

        

        public void Initialize(IServiceLocator services)
        {
            _parameterEditors = AttributeReader.OfTypeKeyAttribute<ParameterEditorAttribute>().GetInstancesOfType<ParameterEditor>();
            _parameterEditors.Values.OfType<IServiceable>().ForEach(x => x.Initialize(services));
        }



        public ParameterEditor GetParameterEditor(Type parameterInfoType)
        {
            ParameterEditor parameterEditor;

            //look up in the dictionary, just in case we have created this manager before
            if (_parameterEditors.TryGetValue(parameterInfoType, out parameterEditor))
                return parameterEditor;

            return null;
        }



        public ParameterEditor GetParameterEditor(ParameterInfo parameterInfo)
        {
            return GetParameterEditor(parameterInfo.GetType());
        }
        

        public ParameterEditor GetParameterEditor(ArgumentTreeViewItem treeViewItem)
        {
            return GetParameterEditor(treeViewItem.Argument.GetType());
        }


        public IEnumerable<ParameterEditor> AvailableParameterEditors
        {
            get { return _parameterEditors.Values; }
        }
    }
}