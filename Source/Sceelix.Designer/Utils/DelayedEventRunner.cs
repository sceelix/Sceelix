using System;
using System.Timers;

namespace Sceelix.Designer.Utils
{
    public class DelayedEventRunner
    {
        private readonly Timer _timer = new Timer();
        private Action _action;


        public DelayedEventRunner()
        {
            _timer.AutoReset = false;
            _timer.Elapsed += delegate
            {
                if(_action != null)
                    _action.Invoke();
                    
                _timer.Stop();
            };
        }


        /// <summary>
        /// Schedules the execution of the action within [interval] milliseconds.
        /// If an action was already scheduled, it is replaced with this one.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="interval"></param>
        public void Run(Action action, double interval)
        {
            _action = action;
            _timer.Interval = interval;
            _timer.Start();
        }
    }
}