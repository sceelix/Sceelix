using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Parameters.Infos;

namespace Sceelix.Core.Parameters
{
    public class SelectListParameter : ListParameter
    {
        public SelectListParameter(string label, params Func<Parameter>[] creationFunctions)
            : base(label, creationFunctions)
        {
            MaxSize = 1;
        }



        public SelectListParameter(string label, string defaultLabel, params Func<Parameter>[] creationFunctions)
            : base(label, creationFunctions)
        {
            MaxSize = 1;

            base.Set(defaultLabel);
        }



        protected SelectListParameter(string label, IEnumerable<Func<Parameter, Parameter>> creationFunctions)
            : base(label, creationFunctions)
        {
            MaxSize = 1;
        }



        internal SelectListParameter(ListParameterInfo listParameterInfo, List<string> modelLabels, IEnumerable<Func<Parameter, Parameter>> creationFunctions, IEnumerable<Parameter> items)
            : base(listParameterInfo, modelLabels, creationFunctions, items)
        {
        }



        public Parameter SelectedItem => Items.FirstOrDefault();



        protected internal override ParameterInfo ToParameterInfo()
        {
            InitializeDictionary();

            return new SelectListParameterInfo(this);
        }
    }


    public class SelectListParameter<T> : SelectListParameter where T : Parameter
    {
        public SelectListParameter(string label)
            : base(label, GetCreationFunctions(typeof(T)))
        {
        }



        public SelectListParameter(string label, string defaultLabel)
            : base(label, GetCreationFunctions(typeof(T)))
        {
            base.Set(defaultLabel);
        }



        public new List<T> Items => base.Items.Cast<T>().ToList();


        public new T SelectedItem => (T) base.SelectedItem;
    }
}