using System.Text.RegularExpressions;
using Sceelix.Core.Annotations;

namespace Sceelix.Core.Graphs.Functions
{
    [ExpressionFunctions("Regex")]
    public class RegexFunctions
    {
        public static dynamic Match(dynamic input, dynamic pattern)
        {
            return Regex.Match(input, pattern);
        }



        public static dynamic MatchNumber(dynamic input)
        {
            return Regex.Match(input, "-?[0-9]+(\\.[0-9]+)?(d|f|l)?").ToString();
        }



        public static dynamic MatchWord(dynamic input)
        {
            return Regex.Match(input, "[\\w]+").ToString();
        }
    }
}