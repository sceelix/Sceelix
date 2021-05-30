using Sceelix.Designer.Annotations;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Settings.Types;

namespace Sceelix.Designer.ProjectExplorer.Management
{
    [ApplicationSettings("Log Window")]
    public class LogSettings : ApplicationSettings
    {
        /// <summary>
        /// From the log messages that arrive at the log window, this indicator controls which ones
        /// are persisted to disk (in the log folder). The choices are ordered in the way that each
        /// level includes the previous ones. For instance, "Warning" includes error and warning messages, 
        /// while "Debug" includes all message types. As logging to disk can encompass a significant
        /// overhead, only "Error" messages are considered by default.
        /// </summary>
        public readonly ChoiceApplicationField FileLoggingLevel = new ChoiceApplicationField("Error", "None", "Error", "Warning", "Information", "Debug");

        /// <summary>
        /// Indicates if messages with the same content, type and source should be aggregated (whereas the count
        /// of repeated messages is shown). Spares resources and generally improves readability, but may affect 
        /// the notion of message order.
        /// </summary>
        public readonly BoolApplicationField AggregateMessages = new BoolApplicationField(true);


        public LogSettings() 
            : base("Log Window")
        {
        }
    }
}
