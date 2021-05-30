namespace Sceelix.Logging
{
    public class EmptyLogger : ILogger
    {
        public void Log(object message, LogType logType = LogType.Auto)
        {
            //do nothing
        }
    }
}