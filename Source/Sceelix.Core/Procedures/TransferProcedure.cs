using Sceelix.Core.Data;
using Sceelix.Core.IO;

namespace Sceelix.Core.Procedures
{
    /// <summary>
    /// This is just a specialization of the standard SystemProcedure, for those cases where there is 
    /// only one input and one output of the same type. So it is basically as if the input entity would
    /// be received, transformed and then sent out again, without divisions or merges.
    /// 
    /// This is an example of a procedure which could be "commented out", which would result in having 
    /// the input entities sent directly to the output without any processing.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class TransferProcedure<T> : SystemProcedure
        where T : IEntity
    {
        /// <summary>
        /// The procedure's only input.
        /// </summary>
        private readonly SingleInput<T> _input = new SingleInput<T>("Input");

        /// <summary>
        /// The procedure's only output.
        /// </summary>
        private readonly Output<T> _output = new Output<T>("Output");



        protected TransferProcedure()
        {
            _input.Description = "The " + Entity.GetDisplayName(typeof(T)) + " to process.";
            _output.Description = "The " + Entity.GetDisplayName(typeof(T)) + " that was processed.";
        }



        protected abstract T Process(T entity);



        protected sealed override void Run()
        {
            _output.Write(Process(_input.Read()));
        }
    }
}