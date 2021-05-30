using System;

namespace Sceelix.Core.Procedures
{
    /// <summary>
    /// Described a procedure that could not be loaded or processed.
    /// The original procedure may have been moved, renamed or deleted.
    /// This stands as a placeholder to ensure the graph connection, but
    /// should be removed or fixed in order for the graph to operate.
    /// </summary>
    public class InvalidProcedure : SystemProcedure
    {
        private readonly string _description;



        public InvalidProcedure(string description)
        {
            _description = description;
        }



        protected override void Run()
        {
            throw new Exception(_description); //"This is an invalid procedure. This node should be either fixed, replaced or deleted."
        }
    }
}