using System;

namespace Sceelix.Meshes.Exceptions
{
    public class InvalidGeometryException : Exception
    {
        public InvalidGeometryException()
        {
        }



        public InvalidGeometryException(string message)
            : base(message)
        {
        }
    }
}