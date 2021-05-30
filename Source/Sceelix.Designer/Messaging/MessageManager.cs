using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Extensions;

namespace Sceelix.Designer.Messaging
{
    /// <summary>
    /// The message manager allows a simple, quick and decoupled way of sending commands, events and data among components.
    /// A component can subscribe to a certain type of messages or send new ones without compromise.
    /// 
    /// Messages are not bound to any special definition. They are simple objects, however they should represent meaningful actions/events.
    /// Naming should be cared for. 
    /// Messages that are meant to order something should be named with Verb + Noun (ex. PlaySound)
    /// Messages that are meant to inform something should be named with Noun + Past Verb (ex. SoundPlayed)
    /// </summary>
    public class MessageManager
    {
        private static Dictionary<Type, List<Type>> _superTypes = new Dictionary<Type, List<Type>>();

        private readonly Dictionary<Type, List<ISubscriber>> _subscribers = new Dictionary<Type, List<ISubscriber>>();
        //private readonly Queue<Object> _undeliveredMessages = new Queue<Object>();
        private readonly Queue<Action> _undeliveredMessages = new Queue<Action>();



        /// <summary>
        /// Registers a subscription to a certain type of message, so that when messages of that type are send, they are
        /// delivered and handled by the indicated delegate function.
        /// </summary>
        /// <typeparam name="T">Type of message to subscribe to.</typeparam>
        /// <param name="actionToPerform">Action to perform.</param>
        public void Register<T>(Action<T> actionToPerform)
        {
            Type messageType = typeof(T);

            if (!_subscribers.ContainsKey(messageType))
                _subscribers.Add(messageType, new List<ISubscriber>());

            _subscribers[messageType].Add(new Subscriber<T>(actionToPerform));
        }



        /// <summary>
        /// Registers a subscription to a certain type of message, so that when messages of that type are send, they are
        /// delivered and handled by the indicated delegate function.
        /// </summary>
        /// <param name="actionToPerform">Action to perform.</param>
        public void Register(Type messageType, Action<Object> actionToPerform)
        {
            if (!_subscribers.ContainsKey(messageType))
                _subscribers.Add(messageType, new List<ISubscriber>());

            _subscribers[messageType].Add(new Subscriber<Object>(actionToPerform));
        }



        /// <summary>
        /// Registers a subscription to a certain type of message, so that when messages of that type are send, they are
        /// delivered and handled by the indicated delegate function.
        /// </summary>
        /// <typeparam name="T">Type of message to subscribe to.</typeparam>
        /// <param name="actionToPerform">Action to perform.</param>
        /// <param name="predicate">Condition based on the type of message.</param>
        public void Register<T>(Action<T> actionToPerform, Predicate<T> predicate)
        {
            Type messageType = typeof(T);

            if (!_subscribers.ContainsKey(messageType))
                _subscribers.Add(messageType, new List<ISubscriber>());

            _subscribers[messageType].Add(new Subscriber<T>(actionToPerform, predicate));
        }



        /// <summary>
        /// Registers a subscription to a certain type of message, so that when messages of that type are send, they are
        /// delivered and handled by the indicated delegate function.
        /// </summary>
        /// <param name="messageType">Type of message to send.</param>
        /// <param name="actionToPerform">Action to perform.</param>
        /// <param name="predicate">Condition based on the type of message.</param>
        public void Register(Type messageType, Action<Object> actionToPerform, Predicate<Object> predicate)
        {
            if (!_subscribers.ContainsKey(messageType))
                _subscribers.Add(messageType, new List<ISubscriber>());

            _subscribers[messageType].Add(new Subscriber<Object>(actionToPerform, predicate));
        }



        /// <summary>
        /// Publishes a message, sending it to all registered subscribers.
        /// </summary>
        /// <param name="message">Message to publish.</param>
        /// <param name="ignoreSubscribers">Subscribers not to send the message. Useful to isolate the sender from the receivers, for instance</param>
        public void Publish(Object message, params Object[] ignoreSubscribers)
        {
            Type messageType = message.GetType();
            
            //send the message to those subscribers 
            foreach (Type superType in GetSuperTypes(messageType))
            {
                List<ISubscriber> subscribers;
                if(_subscribers.TryGetValue(superType, out subscribers))
                {
                    foreach (ISubscriber subscriber in _subscribers[superType].Where(x => !x.SubscribingObject.IsIn(ignoreSubscribers) ))
                    {
                        subscriber.Run(message);
                    }
                }
            }
        }



        private List<Type> GetSuperTypes(Type type)
        {
            List<Type> superTypes;
            if (!_superTypes.TryGetValue(type, out superTypes))
            {
                //cache the next result
                _superTypes[type] = superTypes = GetSuperTypesAux(type).Distinct().ToList();
            }

            return superTypes;
        }



        private IEnumerable<Type> GetSuperTypesAux(Type type)
        {
            //if the type eventually has no parents or interfaces, end
            if (type != null)
            {
                yield return type;

                //get the parent types and all their parents
                foreach (var parentType in GetSuperTypesAux(type.BaseType))
                    yield return parentType;

                //get the interfaces and all their interfaces, too
                foreach (var interfaceType in type.GetInterfaces())
                {
                    foreach (Type interfaceTypeTypes in GetSuperTypesAux(interfaceType))
                        yield return interfaceTypeTypes;
                }
            }
        }



        /// <summary>
        /// Publishes a message, sending it to all registered subscribers. 
        /// Should be used if the message is sent from another thread to the main one the main thread.
        /// </summary>
        /// <param name="message">Message to publish.</param>
        /// <param name="ignoreSubscribers">Subscribers not to send the message. Useful to isolate the sender from the receivers, for instance</param>
        public void PublishSync(Object message, params Object[] ignoreSubscribers)
        {
            lock (_undeliveredMessages)
            {
                _undeliveredMessages.Enqueue(() => Publish(message, ignoreSubscribers));
            }
        }



        public void Update(TimeSpan deltaTime)
        {
            lock (_undeliveredMessages)
            {
                while (_undeliveredMessages.Count > 0)
                {
                    _undeliveredMessages.Dequeue().Invoke();
                }
            }
        }



        public void Unregister<T>(Action<T> actionToUnregister)
        {
            List<ISubscriber> subscribers;

            if (_subscribers.TryGetValue(typeof(T), out subscribers))
            {
                subscribers.RemoveAll(x => ((Subscriber<T>) x).ActionToPerform == actionToUnregister);
            }

            CleanUp();
        }



        public void Unregister(object registeredClass)
        {
            var a = _subscribers.Values.Any(x => x.Any(y => y.Action.Target == registeredClass));
            
            foreach (var value in _subscribers.Values)
                value.RemoveAll(x => x.Action.Target == registeredClass);

            CleanUp();
        }



        /// <summary>
        /// Cleans ups "Type" entries for which the list of subscribers is empty.
        /// </summary>
        private void CleanUp()
        {
            foreach (var subscriberByType in _subscribers.ToList())
            {
                if (subscriberByType.Value.Count == 0)
                    _subscribers.Remove(subscriberByType.Key);
            }
        }
    }
}