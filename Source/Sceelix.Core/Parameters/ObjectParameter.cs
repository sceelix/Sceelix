using Sceelix.Core.Parameters.Infos;

namespace Sceelix.Core.Parameters
{
    /// <summary>
    /// A parameter that accepts any type of data. Is set as an expression by default.
    /// </summary>
    public class ObjectParameter : PrimitiveParameter<object>
    {
        public ObjectParameter(string label)
            : this(label, new object())
        {
        }



        protected ObjectParameter(string label, object defaultValue)
            : base(label, defaultValue)
        {
            //by default, the object parameter uses an expression
            IsExpression = true;
        }



        public ObjectParameter(ObjectParameterInfo objectParameterInfo)
            : base(objectParameterInfo)
        {
        }



        protected override void SetData(object value)
        {
            _value = value;
        }



        protected internal override ParameterInfo ToParameterInfo()
        {
            return new ObjectParameterInfo(this);
        }
    }

    /// <summary>
    /// A parameter that accepts any type of data. Is set as an expression by default.
    /// </summary>
    public class ObjectParameter<T> : ObjectParameter
    {
        public ObjectParameter(string label)
            : base(label, default(T))
        {
        }



        public new T Value
        {
            get { return (T) base.Value; }
            set { base.Value = value; }
        }
    }
}