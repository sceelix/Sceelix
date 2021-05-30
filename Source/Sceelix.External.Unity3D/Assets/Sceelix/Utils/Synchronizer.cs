using System;
using System.Collections.Generic;
using System.Threading;

namespace Assets.Sceelix.Utils
{
    public class Synchronizer
    {
        private readonly Queue<Action> _actions = new Queue<Action>();

        public void Update()
        {
            lock (_actions)
            {
                while (_actions.Count > 0)
                {
                    var action = _actions.Dequeue();
                    action.Invoke();
                }
            }
        }

        /// <summary>
        /// Resolves "count" elements from the queue.
        /// </summary>
        /// <param name="count"></param>
        public void Update(int count)
        {
            lock (_actions)
            {
                while (_actions.Count > 0 && count > 0)
                {
                    var action = _actions.Dequeue();
                    action.Invoke();

                    count--;
                }
            }
        }

        public void Enqueue(Action action)
        {
            lock (_actions)
            {
                _actions.Enqueue(action);
            }
        }


        public void EnqueueAndWait(Action action)
        {
            ManualResetEvent mevent = new ManualResetEvent(false);

            Enqueue(
                delegate
                {
                    action();
                    mevent.Set();
                });

            mevent.WaitOne();
        }


        public Object EnqueueAndWait(Func<Object> action)
        {
            ManualResetEvent mevent = new ManualResetEvent(false);

            Object result = null;
            Enqueue(
                delegate
                {
                    result = action();
                    mevent.Set();
                });

            mevent.WaitOne();

            return result;
        }
    }
}
