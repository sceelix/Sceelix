using System.Collections.Generic;
using System.Linq;
using Sceelix.Collections;
using Sceelix.Conversion;
using Sceelix.Core.Data;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Core.Procedures;
using Sceelix.Core.Utils;
using Sceelix.Loading;

namespace Sceelix.Core.Parameters
{
    public class CompoundParameter : Parameter
    {
        private readonly List<Parameter> _fields = new List<Parameter>();



        public CompoundParameter(string label)
            : base(label)
        {
            ReadSubclassFields();

            Description = CommentLoader.GetComment(GetType()).Summary;
        }



        public CompoundParameter(string label, params Parameter[] fields)
            : base(label)
        {
            _fields = fields.ToList();

            _fields.ForEach(x => x.Parent = this);

            Description = CommentLoader.GetComment(GetType()).Summary;
        }



        internal CompoundParameter(CompoundParameterInfo compoundParameterInfo, IEnumerable<Parameter> fields)
            : base(compoundParameterInfo)
        {
            _fields = fields.ToList();

            _fields.ForEach(x => x.Parent = this);
        }



        public bool ArrangeInSingleLine
        {
            get;
            set;
        } = false;


        public IEnumerable<Parameter> Fields => _fields;


        /*public override Procedure Procedure
        {
            get { return base.Procedure; }
            internal set
            {
                base.Procedure = value;

                foreach (Parameter parameter in Fields)
                    parameter.Procedure = value;
            }
        }*/


        public bool IsExpandedAsDefault
        {
            get;
            set;
        } = true;



        public Parameter this[string label]
        {
            get { return _fields.Find(val => val.Label == label); }
        }



        public override InputCollection SubInputs => new InputCollection(_inputs);



        public override IEnumerable<Input> SubInputTree
        {
            get
            {
                if (_expression != null)
                    yield break;

                foreach (var abstractInput in _inputs)
                    yield return abstractInput;

                foreach (var input in Fields.SelectMany(val => val.SubInputTree))
                    yield return input;
            }
        }



        public override OutputCollection SubOutputs => new OutputCollection(_outputs);



        public override IEnumerable<Output> SubOutputTree
        {
            get
            {
                if (_expression != null)
                    yield break;

                foreach (var output in _outputs)
                    yield return output;

                foreach (var output in Fields.SelectMany(val => val.SubOutputTree))
                    yield return output;
            }
        }



        public override ParameterCollection SubParameters => new ParameterCollection(_fields);



        public override IEnumerable<Parameter> SubParameterTree
        {
            get
            {
                if (_expression != null)
                    yield break;

                foreach (var parameter in _fields)
                    yield return parameter;
            }
        }



        internal override void Evaluate(InputData inputData)
        {
            if (_expression != null)
                base.Evaluate(inputData);
            //Set(_expression(inputData));
            else
                foreach (var parameter in Fields)
                    parameter.Evaluate(inputData);
        }



        protected internal override object GetData()
        {
            SceeList list = new SceeList(_fields.Count);

            foreach (var parameter in _fields)
                list.Add(parameter.Label, parameter.Get());

            return list;
        }



        public override object GetDeep(IEntity entity)
        {
            if (_entityExpression != null)
            {
                var data = _entityExpression(entity);
                SetData(data);
            }

            //do the same as GetData, but passing the entity
            SceeList list = new SceeList(_fields.Count);

            foreach (var parameter in _fields)
                list.Add(parameter.Label, parameter.Get(entity));

            return list;
        }



        protected internal override string GetFullLabel(Parameter parameter)
        {
            if (parameter == this)
                return Label;

            foreach (var field in _fields)
            {
                var fullLabel = field.GetFullLabel(parameter);
                if (fullLabel != null)
                    return Label + "." + fullLabel;
            }

            return null;
        }



        /// <summary>
        /// Reads the subclass fields. This function is automatically called from the CompoundParameter to get the defined parameters using reflection.
        /// In the sole case that parameters are edited in the parameter's constructor, this function can be called again.
        /// </summary>
        protected void ReadSubclassFields()
        {
            _fields.Clear();
            _inputs.Clear();
            _outputs.Clear();

            ProcedureParameterHelper.ReadParametersInputsOutputs(this, _fields, _inputs, _outputs);
        }



        protected internal override void Set(ParameterInfo argument, Procedure masterProcedure, Procedure currentProcedure)
        {
            if (argument.IsExpression)
            {
                SetExpression(argument.ParsedExpression.GetCompiledExpressionTree(masterProcedure, currentProcedure, typeof(List<object>)));
            }
            else
            {
                var compoundParameterInfo = (CompoundParameterInfo) argument;

                foreach (var parameterInfo in compoundParameterInfo.Fields) this[parameterInfo.Label].Set(parameterInfo, masterProcedure, currentProcedure);

                //Owner.Inputs.Add(Inputs);
            }
        }



        protected override void SetData(object value)
        {
            SceeList list = ConvertHelper.Convert<SceeList>(value);
            //SceeList list = (SceeList)Convert.ChangeType(value, typeof(SceeList), CultureInfo.InvariantCulture); ;

            //if there are no labels, 
            //and the number of data entries match the number of fields of this parameter
            if (!list.IsAssociative
                && list.Count == _fields.Count)
                for (int i = 0; i < list.Count; i++)
                    _fields[i].Set(list[i]);
            //otherwise, do the matching by label name
            else
                foreach (var keyValue in list.KeyValues)
                {
                    var parameter = this[keyValue.Key];
                    if (parameter == null)
                        throw new KeyNotFoundException(string.Format("Could not find a field with the label '{0}'.", keyValue.Key));

                    parameter.Set(keyValue.Value);
                }
        }



        protected internal override ParameterInfo ToParameterInfo()
        {
            if (ParameterAncestors.Any(val => val.Label == Label && val.GetType() == GetType()))
                return new RecursiveParameterInfo(this);
            //return new RecursiveParameterInfo(this, () => new CompoundParameterInfo(this));


            /*var parameter = _creationFunction.Invoke();

            if (GetParameterAncestors().Any(val => val.Label == parameter.Label))
            {
                return new RecursiveParameterInfo(parameter);
            }

            foreach (var field in _fields)
            {                
            }*/

            return new CompoundParameterInfo(this);
        }



        /*public override ReadOnlyCollection<Input> Inputs
        {
            get
            {
                return new ReadOnlyCollection<Input>(new List<Input>());
            }
        }*/
    }
}