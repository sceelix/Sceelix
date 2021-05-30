using System.ComponentModel;

namespace Sceelix.Logging
{
    /// <summary>
    /// Describes the type of a log message.
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// Determines automatically from the type of message. 
        /// </summary>
        Auto = 0,

        /// <summary>
        /// Identifies the message as debug.
        /// </summary>
        Debug = 1,

        /// <summary>
        /// Identifies the message as informative. Default for string messages.
        /// </summary>
        Information = 2,

        /// <summary>
        /// Identifies the message as a warning.
        /// </summary>
        Warning = 4,

        /// <summary>
        /// Identifies the message as an error. Default for exception messages.
        /// </summary>
        Error = 8
    }


    /// <summary>
    /// Defines methods for application logging.
    /// </summary>
    [DefaultValue(typeof(ConsoleLogger))]
    public interface ILogger
    {
        /// <summary>
        /// Logs a given message. Calls ToString as default for most objects, although it can differ depending on the message (for example, exceptions) and the implemented logger.
        /// </summary>
        /// <param name="message">Message to be logged.</param>
        /// <param name="logType">Type of message to log. Auto means that the type will be inferred from the message</param>
        void Log(object message, LogType logType = LogType.Auto);
    }
}