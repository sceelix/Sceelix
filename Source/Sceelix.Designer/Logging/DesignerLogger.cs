using System;
using System.Diagnostics;
using System.Reflection;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using Sceelix.Logging;
using ILogger = Sceelix.Logging.ILogger;

namespace Sceelix.Designer.Logging
{
    /// <summary>
    /// Default Logger for the designer, which writes to Log4Net class.
    /// </summary>
    /// <seealso cref="Sceelix.Logging.ILogger" />
    internal class DesignerLogger : ILogger
    {
        private static readonly log4net.Core.ILogger defaultLogger = LoggerManager.GetLogger(Assembly.GetCallingAssembly(), "DesignerLogger");



        public void Log(object message, LogType logType = LogType.Auto)
        {
            Level messageLevel;

            switch (logType)
            {
                case LogType.Error:
                    messageLevel = Level.Error;
                    break;
                case LogType.Warning:
                    messageLevel = Level.Warn;
                    break;
                case LogType.Information:
                    messageLevel = Level.Info;
                    break;
                case LogType.Debug:
                    messageLevel = Level.Debug;
                    break;
                default:
                    messageLevel = message is Exception ? Level.Error : Level.Info;
                    break;
            }

            if (defaultLogger.IsEnabledFor(messageLevel))
            {
                defaultLogger.Log(typeof(DesignerLogger), messageLevel, message, null);
            }
        }

    }
}