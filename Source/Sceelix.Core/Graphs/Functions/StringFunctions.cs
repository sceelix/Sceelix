using Sceelix.Core.Annotations;

namespace Sceelix.Core.Graphs.Functions
{
    [ExpressionFunctions("String")]
    public class StringFunctions
    {
        public static object Contains(dynamic str, dynamic content)
        {
            return str.Contains(content);
        }



        public static object EndsWith(dynamic str, dynamic content)
        {
            return str.EndsWith(content);
        }



        public static object EndsWith(dynamic str, dynamic oldContent, dynamic newContent)
        {
            return str.Replace(oldContent, newContent);
        }



        public static object Length(dynamic str)
        {
            return str.Length;
        }



        public static object StartsWith(dynamic str, dynamic content)
        {
            return str.StartsWith(content);
        }



        public static object Trim(dynamic str)
        {
            return str.Trim();
        }
    }
}