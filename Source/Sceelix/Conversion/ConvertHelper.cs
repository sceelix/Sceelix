using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Sceelix.Extensions;
using Sceelix.Loading;

namespace Sceelix.Conversion
{
    /// <summary>
    /// Class dedicated to advanced conversion between types.
    /// </summary>
    public static class ConvertHelper
    {
        private static readonly Dictionary<Type, List<IConversionSet>> ConversionDictionary = new Dictionary<Type, List<IConversionSet>>();



        /// <summary>
        /// Checks if it there is a conversion possible between typeFrom and typeTo.
        /// </summary>
        /// <param name="typeFrom">Type to convert from.</param>
        /// <param name="typeTo">Type to convert to.</param>
        /// <returns>True if there is a possible conversion, false otherwise</returns>
        public static bool CanConvert(Type typeFrom, Type typeTo)
        {
            return typeTo == typeof(object) ||
                   typeFrom == typeTo ||
                   typeTo.IsAssignableFrom(typeFrom) ||
                   typeof(IConvertible).IsAssignableFrom(typeFrom)
                   && typeof(IConvertible).IsAssignableFrom(typeTo)
                   || ConversionDictionary.ContainsKey(typeFrom) && ConversionDictionary[typeFrom].Any(val => val.DestinationType == typeTo);
        }



        /// <summary>
        /// Calls the Convert function and performs the cast directly.
        /// </summary>
        /// <typeparam name="T">Type to convert to.</typeparam>
        /// <param name="value">Value to be converted.</param>
        /// <returns>The converted value.</returns>
        /// <exception cref="InvalidCastException">If the value was not conversion, the cast will fail.</exception>
        public static T Convert<T>(object value)
        {
            if (value == null)
                return default(T);

            return (T) Convert(value, typeof(T));
        }



        /// <summary>
        /// Converts the given object to the indicated type, if a conversion is available.
        /// </summary>
        /// <param name="value">Value to be converted.</param>
        /// <param name="type">Type to convert to.</param>
        /// <returns>
        /// 1) If the value is null, null is returned.
        /// 2) If the value is already of the indicated type, returns  the value.
        /// 3) If a custom conversion exists, it will be performed.
        /// 4) If it's a string to enum conversion, the Enum.Parse function will be used.
        /// 5) If both the value and type are IConvertible, the ChangeType function is used.
        /// 6) If the target type is string, the ToString function is called.
        /// 7) Otherwise, the original value is returned.
        /// </returns>
        public static object Convert(object value, Type type)
        {
            if (value == null)
                return null;

            //if the value is already of the indicated type, nothing needs to be done!
            if (type.IsInstanceOfType(value))
                return value;

            //check the "ConversionDictionary" for registered converters
            var currentType = value.GetType();
            while (currentType != null)
            {
                List<IConversionSet> converterSets;
                if (ConversionDictionary.TryGetValue(currentType, out converterSets))
                {
                    IConversionSet firstOrDefault = converterSets.FirstOrDefault(val => val.DestinationType == type);
                    if (firstOrDefault != null)
                        return firstOrDefault.Convert(value);
                }

                currentType = currentType.BaseType;
            }


            //if it is just an enum conversion...
            if (type.IsEnum && value is string)
                return Enum.Parse(type, (string) value);

            //if both are IConvertibles, just use the System.Convert
            if (typeof(IConvertible).IsAssignableFrom(type) && value is IConvertible)
                return System.Convert.ChangeType(value, type, CultureInfo.InvariantCulture);

            if (type == typeof(string))
                return value.ToString();

            return value;
        }



        public static void Initialize()
        {
            //iterates over all classes that have the ConversionFunctionsAttribute and registers its classes
            //registrations that fail are logged to the default logger
            SceelixDomain.Types.Where(x => x.HasCustomAttribute<ConversionFunctionsAttribute>()).SafeForeach(Register);
        }



        /// <summary>
        /// Registers a conversion function between two types.
        /// If a conversion between the two types already exists, the old one is kept.
        /// </summary>
        /// <typeparam name="T">Type to cast from.</typeparam>
        /// <typeparam name="TR">Type to cast to.</typeparam>
        /// <param name="conversionFunction">Conversion function between the 2. </param>
        public static void Register<T, TR>(Func<T, TR> conversionFunction)
        {
            ConversionSet<T, TR> conversionSet = new ConversionSet<T, TR>(conversionFunction);

            Register(conversionSet);
        }



        public static void Register(Type converterClass)
        {
            var methods = converterClass.GetMethods();
            foreach (MethodInfo methodInfo in methods.Where(x => !x.IsAbstract && x.IsStatic))
            {
                var destinationType = methodInfo.ReturnType;
                var parameters = methodInfo.GetParameters();

                //if the method has only one parameter and returns something else than void
                //we can consider it a valid conversion function
                if (parameters.Length == 1 && destinationType != typeof(void))
                {
                    var firstParameter = parameters.First();
                    var sourceType = firstParameter.ParameterType;

                    //create an encapsulating expression that converts to and from Object
                    var parameter = Expression.Parameter(typeof(object));
                    var expression = Expression.Convert(Expression.Call(methodInfo, Expression.Convert(parameter, sourceType)), typeof(object));

                    var compiledDelegate = Expression.Lambda<Func<object, object>>(expression, parameter).Compile();

                    Register(new DelegateConversionSet(sourceType, destinationType, compiledDelegate));
                }
            }
        }



        /// <summary>
        /// Registers an IConversionSet to the dictionary
        /// </summary>
        /// <param name="conversionSet"></param>
        private static void Register(IConversionSet conversionSet)
        {
            List<IConversionSet> converterSets;
            //if the original type does not exist on the list, add a new list
            if (!ConversionDictionary.TryGetValue(conversionSet.SourceType, out converterSets))
            {
                ConversionDictionary.Add(conversionSet.SourceType, converterSets = new List<IConversionSet>());
                converterSets.Add(conversionSet);
            }
            //otherwise, we better check if we don't have a repeated converter
            else if (converterSets.All(val => val.DestinationType != conversionSet.DestinationType))
            {
                converterSets.Add(conversionSet);
            }
        }
    }
}