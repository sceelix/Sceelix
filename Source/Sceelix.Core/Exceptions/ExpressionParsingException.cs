using System;

namespace Sceelix.Core.Exceptions
{
    public class ExpressionParsingException : Exception
    {
        public ExpressionParsingException(string message)
            : base(message)
        {
        }
    }
}