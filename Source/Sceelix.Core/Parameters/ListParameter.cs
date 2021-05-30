using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Sceelix.Collections;
using Sceelix.Core.Data;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;

namespace Sceelix.Core.Parameters
{
    public class ListParameter : Parameter
    {
        private static readonly Dictionary<Type, List<Func<Parameter, Parameter>>> CachedParameterCreationFuncs = new Dictionary<Type, List<Func<Parameter, Parameter>>>();


        private Dictionary<string, Func<Parameter, Parameter>> _creationFunctionsDictionary;

        //private bool _allowEmpty = true;



        public ListParameter(string label, params Func<Parameter>[] creationFunctions)
            : base(label)
        {
            CreationFunctionsList = GetCreationFunctions(creationFunctions).ToList();
            //CreateFunctionList(creationFunctions);
        }



        public ListParameter(string label, IEnumerable<Func<Parameter>> creationFunctions)
            : base(label)
        {
            CreationFunctionsList = GetCreationFunctions(creationFunctions).ToList();
            //CreateFunctionList(creationFunctions);
        }



        internal ListParameter(string label, IEnumerable<Func<Parameter, Parameter>> creationFunctions)
            : base(label)
        {
            CreationFunctionsList = creationFunctions.ToList();
            //CreateFunctionList(creationFunctions);
        }



        internal ListParameter(ListParameterInfo listParameterInfo, List<string> modelLabels, IEnumerable<Func<Parameter, Parameter>> creationFunctions, IEnumerable<Parameter> items)
            : base(listParameterInfo)
        {
            MaxSize = MaxSize;

            //CreateFunctionList(creationFunctions);
            CreationFunctionsList = creationFunctions.ToList();

            Items = items.ToList();

            _creationFunctionsDictionary = new Dictionary<string, Func<Parameter, Parameter>>();
            for (int i = 0; i < CreationFunctionsList.Count; i++) _creationFunctionsDictionary.Add(modelLabels[i], CreationFunctionsList[i]);
        }



        protected Dictionary<string, Func<Parameter, Parameter>> CreationFunctionsDictionary
        {
            get
            {
                InitializeDictionary();

                return _creationFunctionsDictionary;
            }
        }



        internal List<Func<Parameter, Parameter>> CreationFunctionsList
        {
            get;
        }


        /// <summary>
        /// Defines if the list subitems should be expanded when the list first created.
        /// </summary>
        public bool IsExpandedAsDefault
        {
            get;
            set;
        } = true;


        public List<Parameter> Items
        {
            get;
        } = new List<Parameter>();


        /// <summary>
        /// Maximum size of the list.
        /// </summary>
        public int MaxSize
        {
            get;
            set;
        }


        public bool ReachedLimit => MaxSize > 0 && Items.Count == MaxSize;



        public override IEnumerable<Input> SubInputTree
        {
            get
            {
                if (_expression != null)
                    yield break;

                foreach (var input in Items.SelectMany(val => val.SubInputTree))
                    yield return input;
            }
        }



        public override IEnumerable<Output> SubOutputTree
        {
            get
            {
                if (_expression != null)
                    yield break;

                foreach (var output in Items.SelectMany(val => val.SubOutputTree))
                    yield return output;
            }
        }



        public IEnumerable<string> SubParameterLabels => CreationFunctionsDictionary.Keys;


        public override ParameterCollection SubParameters => new ParameterCollection(Items);



        public override IEnumerable<Parameter> SubParameterTree
        {
            get
            {
                if (_expression != null)
                    yield break;

                foreach (var parameter in Items)
                    yield return parameter;
            }
        }



        public void Add(string label)
        {
            InitializeDictionary();

            Func<Parameter, Parameter> creationFunction;
            if (!_creationFunctionsDictionary.TryGetValue(label, out creationFunction))
                throw new KeyNotFoundException(string.Format("No subitem with name '{0}' exists in parameter '{1}'.", label, Label));

            if (!ReachedLimit)
            {
                var parameter = creationFunction.Invoke(this);
                Items.Add(parameter);
            }
            else if (MaxSize == 1) //if it reached the limit, but can only support one item
            {
                Items[0] = creationFunction.Invoke(this);
            }
        }



        internal override void Evaluate(InputData inputData)
        {
            if (_expression != null)
                base.Evaluate(inputData);
            //Set(_expression(inputData));
            else
                foreach (var parameter in Items)
                    parameter.Evaluate(inputData);
        }



        private static IEnumerable<Func<Parameter, Parameter>> GetCreationFunctions<T>(
            IEnumerable<Func<T>> baseCreationFunctions) where T : Parameter
        {
            return baseCreationFunctions.Select(x =>
                new Func<Parameter, Parameter>(parameter =>
                {
                    var parameterInstance = x.Invoke();
                    parameterInstance.Parent = parameter;
                    return parameterInstance;
                })).ToList();
        }



        protected static IEnumerable<Func<Parameter, Parameter>> GetCreationFunctions(Type type)
        {
            return CachedParameterCreationFuncs.GetOrCompute(type, () =>
            {
                var types = ParameterManager.ParameterTypes.Where(paramType => type.IsAssignableFrom(paramType)
                                                                               && typeof(Parameter).IsAssignableFrom(paramType)
                                                                               && !paramType.IsAbstract).OrderBy(x => x.Name);
                var funcList = new List<Func<Parameter, Parameter>>();

                foreach (Type source in types)
                {
                    var expression = Expression.New(source);

                    var compiledExpression = Expression.Lambda<Func<Parameter>>(expression).Compile();

                    funcList.Add(parent =>
                    {
                        var parameter = compiledExpression.Invoke();
                        parameter.Parent = parent;
                        return parameter;
                    });
                }

                return funcList;
            });
        }



        protected internal override object GetData()
        {
            SceeList list = new SceeList(Items.Count);

            foreach (var parameter in Items)
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
            SceeList list = new SceeList(Items.Count);

            foreach (var parameter in Items)
                list.Add(parameter.Label, parameter.GetDeep(entity));

            return list;
        }



        protected internal override string GetFullLabel(Parameter parameter)
        {
            if (parameter == this)
                return Label;

            for (int index = 0; index < Items.Count; index++)
            {
                var item = Items[index];

                var fullNameOf = item.GetFullLabel(parameter);
                if (fullNameOf != null)
                    return Label + "[" + index + "]." + fullNameOf; //Label + "." + index + "." + fullNameOf
            }

            return null;
        }



        /// <summary>
        /// Initializes the Dictionary that links name to the creation function.
        /// This is not executed in the constructor because it could, in some cases, result in endless recursive constructor calls.
        /// </summary>
        protected void InitializeDictionary()
        {
            if (_creationFunctionsDictionary != null)
                return;

            _creationFunctionsDictionary = new Dictionary<string, Func<Parameter, Parameter>>();

            foreach (var func in CreationFunctionsList)
            {
                string label = func.Invoke(this).Label;
                _creationFunctionsDictionary.Add(label, func);
            }
        }



        private void ProcessKeyValueList(IEnumerable<KeyValuePair<string, object>> keyValuePairs)
        {
            Items.Clear();

            foreach (var namedObject in keyValuePairs)
            {
                if (ReachedLimit)
                    return;

                var key = namedObject.Key;

                if (string.IsNullOrWhiteSpace(key))
                    key = _creationFunctionsDictionary.First().Key;

                Func<Parameter, Parameter> creationFunction;
                if (!_creationFunctionsDictionary.TryGetValue(key, out creationFunction))
                    throw new KeyNotFoundException(string.Format("No subitem with name '{0}' exists in parameter '{1}'.", key, Label));

                var parameter = _creationFunctionsDictionary[key].Invoke(this);
                parameter.Set(namedObject.Value);
                Items.Add(parameter);
            }
        }



        protected internal override void Set(ParameterInfo argument, Procedure masterProcedure, Procedure currentProcedure)
        {
            InitializeDictionary();

            if (argument.IsExpression)
            {
                SetExpression(argument.ParsedExpression.GetCompiledExpressionTree(masterProcedure, currentProcedure, typeof(SceeList)));
            }
            else
            {
                var primitiveArgument = (ListParameterInfo) argument;
                Items.Clear();

                foreach (var parameterInfo in primitiveArgument.Items)
                {
                    if (ReachedLimit)
                        return;

                    var parameter = _creationFunctionsDictionary[parameterInfo.Label].Invoke(this);
                    Items.Add(parameter);

                    parameter.Set(parameterInfo, masterProcedure, currentProcedure);
                }
            }
        }



        protected override void SetData(object value)
        {
            InitializeDictionary();

            if (value == null)
            {
                Items.Clear();
            }
            else if (value is string)
            {
                //if is
                Add((string) value);
            }
            else if (value is IEnumerable<string>)
            {
                //if is
                foreach (var str in (IEnumerable<string>) value)
                    Add(str);
            }
            else if (value is IEnumerable<KeyValuePair<string, object>>)
            {
                ProcessKeyValueList((IEnumerable<KeyValuePair<string, object>>) value);
            }
            else if (value is SceeList)
            {
                ProcessKeyValueList(((SceeList) value).KeyValues);
            }
            else if (value is int)
            {
                var count = (int) value;
                var key = _creationFunctionsDictionary.First().Key;
                var creationFunction = _creationFunctionsDictionary[key];

                for (int i = 0; i < count; i++)
                    Items.Add(creationFunction.Invoke(this));
            }
            else if (value is IEnumerable<object>)
            {
                Items.Clear();

                var enumerable = (IEnumerable<object>) value;

                var key = _creationFunctionsDictionary.First().Key;
                var creationFunction = _creationFunctionsDictionary[key];

                foreach (object obj in enumerable)
                {
                    var parameter = creationFunction.Invoke(this);
                    parameter.Set(obj);
                    Items.Add(parameter);
                }
            }
            else
            {
                throw new ArgumentException("Value type '" + value.GetType() + "' is not assignable to ListParameters.");
            }
        }



        protected internal override ParameterInfo ToParameterInfo()
        {
            InitializeDictionary();

            return new ListParameterInfo(this);
        }
    }


    public class ListParameter<T> : ListParameter where T : Parameter
    {
        public ListParameter(string label, params Func<T>[] creationFunctions)
            : base(label, (IEnumerable<Func<T>>) creationFunctions)
        {
        }



        public ListParameter(string label)
            : base(label, GetCreationFunctions(typeof(T)))
        {
        }



        public new List<T> Items => base.Items.Cast<T>().ToList();
    }
}