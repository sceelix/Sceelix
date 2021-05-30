using System;
using System.Runtime.Serialization;

namespace Sceelix.Core.Exceptions
{
    public class EntityTypeException : Exception
    {
        public EntityTypeException()
        {
        }



        public EntityTypeException(string message)
            : base(message)
        {
        }



        protected EntityTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }



        public EntityTypeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}