using Sceelix.Conversion;
using Sceelix.Core.Data;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Core.Procedures;

namespace Sceelix.Core.Parameters
{
    public abstract class PrimitiveParameter<T> : Parameter
    {
        protected T _value;



        protected PrimitiveParameter(ParameterInfo parameterInfo)
            : base(parameterInfo)
        {
            _value = default(T);
        }



        protected PrimitiveParameter(PrimitiveParameterInfo<T> parameterInfo)
            : base(parameterInfo)
        {
            _value = parameterInfo.FixedValue;
        }



        protected PrimitiveParameter(string label)
            : base(label)
        {
            _value = default(T);
        }



        protected PrimitiveParameter(string label, T defaultValue)
            : base(label)
        {
            _value = defaultValue;
        }



        public override bool IsExpression
        {
            get { return _expression != null; }
            set
            {
                if (value)
                    _expression = delegate { return default(T); };
                else
                    _expression = null;
            }
        }



        public T Value
        {
            get { return (T) Get(); }
            set { Set(value); }
        }



        public new T Get(IEntity entity)
        {
            return (T) base.Get(entity);
        }



        protected internal override object GetData()
        {
            return _value;
        }



        protected internal override void Set(ParameterInfo argument, Procedure masterProcedure, Procedure currentProcedure)
        {
            if (argument.IsExpression)
            {
                SetExpression(argument.ParsedExpression.GetCompiledExpressionTree(masterProcedure, currentProcedure, typeof(T)));
            }
            else
            {
                var primitiveArgument = (PrimitiveParameterInfo<T>) argument;

                Set(primitiveArgument.FixedValue);
            }
        }



        protected override void SetData(object value)
        {
            _value = ConvertHelper.Convert<T>(value);
        }



        /*public override IEnumerable<Parameter> SubParameters
        {
            get { yield break; }
        }*/

        /*public override object Clone()
        {
            var clone = (PrimitiveParameter<T>)base.Clone();
            clone._value = (T) clone._value.Clone();

            return clone;
        }*/
    }
}