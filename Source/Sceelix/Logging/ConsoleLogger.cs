using System;

namespace Sceelix.Logging
{
    /// <summary>
    /// An ILogger implementation that writes to the console.
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        /// <summary>
        /// Logs a given message. Calls ToString as default for most objects, although it can differ depending on the message (for example, exceptions) and the implemented logger.
        /// </summary>
        /// <param name="message">Message to be logged.</param>
        /// <param name="logType">Type of message to log. Auto means that the type will be inferred from the message</param>
        public void Log(object message, LogType logType = LogType.Information)
        {
            Console.WriteLine(message);
        }
    }
}