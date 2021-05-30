namespace Sceelix.Core.Parameters
{
    public class TypedParameterReference<T>
    {
        private readonly Parameter _parameter;



        public TypedParameterReference(Parameter parameter)
        {
            _parameter = parameter;
        }



        public void Set(T value)
        {
            _parameter.Set(value);
        }
    }
}