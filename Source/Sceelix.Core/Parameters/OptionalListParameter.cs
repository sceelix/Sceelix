using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Parameters.Infos;

namespace Sceelix.Core.Parameters
{
    public class OptionalListParameter : ListParameter
    {
        public OptionalListParameter(string label, Func<Parameter> creationFunctions)
            : base(label, creationFunctions)
        {
            MaxSize = 1;
        }



        public OptionalListParameter(string label, string defaultLabel, Func<Parameter> creationFunction)
            : base(label, creationFunction)
        {
            MaxSize = 1;

            base.Set(defaultLabel);
        }



        protected OptionalListParameter(string label, Func<Parameter, Parameter> creationFunction)
            : base(label, new[] {creationFunction})
        {
            MaxSize = 1;
        }



        internal OptionalListParameter(ListParameterInfo listParameterInfo, List<string> modelLabels, IEnumerable<Func<Parameter, Parameter>> creationFunctions, IEnumerable<Parameter> items)
            : base(listParameterInfo, modelLabels, creationFunctions, items)
        {
        }



        public bool HasValue => Items.Count > 0;


        public Parameter Value => Items.FirstOrDefault();



        protected internal override ParameterInfo ToParameterInfo()
        {
            InitializeDictionary();

            return new OptionalListParameterInfo(this);
        }
    }


    public class OptionalListParameter<T> : OptionalListParameter where T : Parameter
    {
        public OptionalListParameter(string label)
            : base(label, GetCreationFunctions(typeof(T)).FirstOrDefault())
        {
        }



        public new T Value => (T) Items.FirstOrDefault();
    }
}