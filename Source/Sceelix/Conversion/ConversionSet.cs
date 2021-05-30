using System;

namespace Sceelix.Conversion
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ConversionFunctionsAttribute : Attribute
    {
    }


    internal interface IConversionSet
    {
        Type DestinationType
        {
            get;
        }

        Type SourceType
        {
            get;
        }


        object Convert(object obj);
    }


    internal class ConversionSet<T, TR> : IConversionSet
    {
        private readonly Func<T, TR> _conversionFunction;



        public ConversionSet(Func<T, TR> conversionFunction)
        {
            _conversionFunction = conversionFunction;
        }



        public Type DestinationType => typeof(TR);


        public Type SourceType => typeof(T);



        public object Convert(object obj)
        {
            return _conversionFunction((T) obj);
        }
    }


    internal class DelegateConversionSet : IConversionSet
    {
        private readonly Func<object, object> _conversionFunction;



        public DelegateConversionSet(Type sourceType, Type destinationType, Func<object, object> conversionFunction)
        {
            SourceType = sourceType;
            DestinationType = destinationType;
            _conversionFunction = conversionFunction;
        }



        public Type DestinationType
        {
            get;
        }


        public Type SourceType
        {
            get;
        }



        public object Convert(object obj)
        {
            return _conversionFunction(obj);
        }
    }
}