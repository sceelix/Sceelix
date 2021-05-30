using System;

namespace Sceelix.Extensions
{
    public static class ExceptionExtension
    {
        public static Exception GetRealException(this Exception exception)
        {
            //go deep down to understand the actual reason
            while (exception.InnerException != null) exception = exception.InnerException;

            return exception;
        }
    }
}