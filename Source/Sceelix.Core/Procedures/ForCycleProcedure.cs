using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;

namespace Sceelix.Core.Procedures
{
    /// <summary>
    /// Allows the definition of cycles to perform node sequences 
    /// a certain number of times, such as in a For...Loop control
    /// structure.
    /// </summary>
    [Procedure("b7594853-ebae-4cc5-a7d0-bb4390e49877", Label = "For...Loop", Category = "Basic")]
    public class ForCycleProcedure : SystemProcedure
    {
        /// <summary>
        /// Entity being manipulated in the loop.
        /// </summary>
        private readonly SingleInput<IEntity> _input = new SingleInput<IEntity>("Input");

        /// <summary>
        /// Output where the entities will be sent to when the loop is going.
        /// </summary>
        private readonly Output<IEntity> _outputLoop = new Output<IEntity>("Loop");

        /// <summary>
        /// Output where the entities will be sent to when the loop is over.
        /// </summary>
        private readonly Output<IEntity> _outputLoopExit = new Output<IEntity>("Loop Exit");

        /// <summary>
        /// Start value (inclusive).
        /// </summary>
        private readonly IntParameter _parameterStart = new IntParameter("Start", 1);

        /// <summary>
        /// End value (inclusive).
        /// </summary>
        private readonly IntParameter _parameterEnd = new IntParameter("End", 10);

        /// <summary>
        /// Increment value.
        /// </summary>
        [Section("Data")] private readonly IntParameter _parameterIncrement = new IntParameter("Increment", 1);

        /// <summary>
        /// Attribute that will store the index. This attribute is both read and written from.
        /// </summary>
        private readonly AttributeParameter<int> _attributeIndex = new AttributeParameter<int>("Index", AttributeAccess.ReadWrite);



        protected override void Run()
        {
            IEntity entity = _input.Read();

            if (!_attributeIndex.HasAttribute(entity))
                _attributeIndex[entity] = _parameterStart.Value;

            if (_attributeIndex[entity] >= _parameterEnd.Value)
                _outputLoopExit.Write(entity);
            else
            {
                _attributeIndex[entity] = _attributeIndex[entity] + _parameterIncrement.Value;
                _outputLoop.Write(entity);
            }
        }
    }
}