using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Logging;

namespace Sceelix.Core.Procedures
{
    /// <summary>
    /// Writes logging messages to the designated log panel.
    /// </summary>
    [Procedure("3ec9e7df-c668-4b94-920c-143e01083414", Label = "Log", Category = "Basic")]
    public class LogProcedure : SystemProcedure
    {
        /// <summary>
        /// The type of input port. <br/>
        /// Setting a <b>Single</b> (circle) input means that the node will be executed once per entity. Useful to log a different message for each entity. <br/>
        /// Setting a <b>Collective</b> (square) input means that the node will be executed once for all entities. Useful to log a message for a whole set of entities.
        /// </summary>
        private readonly SingleOrCollectiveInputChoiceParameter<IEntity> _parameterInput = new SingleOrCollectiveInputChoiceParameter<IEntity>("Inputs", "Single");

        /// <summary>
        /// Entities that were sent to the input.
        /// </summary>
        private readonly Output<IEntity> _output = new Output<IEntity>("Output");

        /// <summary>
        /// Type of message to printed.
        /// </summary>
        private readonly EnumChoiceParameter<LogType> _parameterLogType = new EnumChoiceParameter<LogType>("Type", LogType.Information);

        /// <summary>
        /// The list of messages to be printed.
        /// </summary>
        private readonly ListParameter<StringParameter> _parameterMessages = new ListParameter<StringParameter>("Messages", () => new StringParameter("Text", "") {Description = "A message to log."});



        public LogProcedure()
        {
            _parameterMessages.Set("Text");
        }



        protected override void Run()
        {
            var loggerService = Environment.GetService<ILogger>();

            foreach (var stringParameter in _parameterMessages.Items)
                loggerService.Log(stringParameter.Value, _parameterLogType.Value);

            _output.Write(_parameterInput.Read());
        }
    }
}