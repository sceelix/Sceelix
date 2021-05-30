using System.Collections.Generic;
using Sceelix.Core.Data;
using Sceelix.Core.IO;

namespace Sceelix.Core.Procedures
{
    /// <summary>
    /// This is just a specialization of the standard SystemProcedure, for those cases where there is 
    /// only one output of a type, which can create entities "from nothing", i.e. just based on a set of parameters. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SourceProcedure<T> : SystemProcedure
        where T : IEntity
    {
        /// <summary>
        /// The procedure's only output.
        /// </summary>
        private readonly Output<T> _output = new Output<T>("Output");



        protected SourceProcedure()
        {
            _output.Description = "The " + Entity.GetDisplayName(typeof(T)) + " that was created.";
        }



        protected abstract IEnumerable<T> Create();



        protected sealed override void Run()
        {
            _output.Write(Create());
        }
    }
}