using System;
using System.Diagnostics;
using Sceelix.Designer.Annotations;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Logging;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.Plugins;
using Sceelix.Designer.ProjectExplorer.FileHandlers;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Designer.Services;
using Sceelix.Designer.Settings;
using Sceelix.Logging;

namespace Sceelix.Designer.ProjectExplorer
{
    [DesignerService]
    public class LogPersistenceService : IServiceable
    {
        private LogSettings _logSettings;



        public void Initialize(IServiceLocator services)
        {
            services.Get<MessageManager>().Register<LogMessageSent>(OnLogMessageSent);

            _logSettings = services.Get<SettingsManager>().Get<LogSettings>();
            
            
            //Trace.Listeners.Add(new LogTraceListener(services.Get<MessageManager>()));
        }



        private void OnLogMessageSent(LogMessageSent obj)
        {
            var actualMessage = "Log Message:" + obj.Message;
            
            if (obj.LogType == LogType.Error && _logSettings.FileLoggingLevel.Index > 0)
                DesignerProgram.Log.Error(actualMessage);
            else if (obj.LogType == LogType.Warning && _logSettings.FileLoggingLevel.Index > 1)
                DesignerProgram.Log.Warn(actualMessage);
            else if (obj.LogType == LogType.Information && _logSettings.FileLoggingLevel.Index > 2)
                DesignerProgram.Log.Info(actualMessage);
            else if (_logSettings.FileLoggingLevel.Index > 3)
                DesignerProgram.Log.Debug(actualMessage);
        }





        public class LogTraceListener : TraceListener
        {
            private readonly MessageManager _messageManager;
            
            public LogTraceListener(MessageManager messageManager)
            {
                _messageManager = messageManager;
            }
            
            public override void Write(string message)
            {
                _messageManager.Publish(new LogMessageSent(message));
            }
            
            public override void WriteLine(string message)
            {
                _messageManager.Publish(new LogMessageSent(message));
            }
        }
    }
}