using System;

namespace Sceelix.Designer.Messaging
{
    public interface ISubscriber
    {
        //Type MessageType { get; }
        Delegate Action
        {
            get;
        }



        void Run(Object message);


        Object SubscribingObject
        {
            get;
        }
    }


    public class Subscriber<T> : ISubscriber
    {
        private readonly Action<T> _actionToPerform;
        private readonly Predicate<T> _predicate;



        public Subscriber(Action<T> actionToPerform)
        {
            _actionToPerform = actionToPerform;
        }



        public Subscriber(Action<T> actionToPerform, Predicate<T> predicate)
        {
            _actionToPerform = actionToPerform;
            _predicate = predicate;
        }



        public Action<T> ActionToPerform
        {
            get { return _actionToPerform; }
        }



        public void Run(Object message)
        {
            if (_predicate != null)
            {
                if (_predicate((T) message))
                    _actionToPerform((T) message);
            }
            else
                _actionToPerform((T) message);
        }



        public Delegate Action
        {
            get { return _actionToPerform; }
        }



        public Object SubscribingObject
        {
            get { return Action.Target;}
        }



        /*public Type MessageType
        {
            get { return typeof (T); }
        }*/
    }
}