using System;
using Sceelix.Core.Annotations;

namespace Sceelix.MyNewEngineLibrary
{
    /// <summary>
    /// This sample class shows how to define simple functions that can be called inside expressions.
    /// 
    /// Sceelix will look for classes carrying the ExpressionFunctions attribute and will automatically
    /// load all the static functions with dynamic parameters.
    /// </summary>
    [ExpressionFunctions("Hello Group")]
    public class HelloFunctions
    {
        public static Object Hello()
        {
            return "Hello";
        }

        //The arguments must be of dynamic type - the verification/conversion must be done inside the function
        //the return type must be dynamic (or object)
        public static dynamic GetStringIndex(dynamic val1, dynamic val2)
        {
            if (val1 is String && val2 is int)
            {
                return ((String)val1)[(int)val2].ToString();
            }

            throw new ArgumentException("The first value must be String and the second one must be Int!");
        }


        /// <summary>
        /// This function will create a new instance of the new custom type and use it in functions.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="exists"></param>
        /// <returns></returns>
        public static Object SimpleClass(dynamic name, dynamic value, dynamic exists)
        {
            return new SimpleClass(name, value, exists);
        }
    }
}
