using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Data;
using Sceelix.Core.Graphs;
using Sceelix.Core.Parameters;

namespace Sceelix.Core.IO
{
    public class OutputReference
    {
        internal OutputReference(Output output)
        {
            Output = output;
        }



        /// <summary>
        /// The number of items in the output port.
        /// </summary>
        public int Count => Output.CurrentObjects.Count;


        /// <summary>
        /// A label that uniquely identifies this output in the whole node.
        /// Composed by the original node label and, if defined inside a parameter, its full name.
        /// </summary>
        /*public String UniqueLabel
        {
            get { return _output.UniqueLabel; }
        }*/


        /// <summary>
        /// The type of Entity that this output holds.
        /// </summary>
        public Type EntityType => Output.EntityType;


        public string Identifier => Output.Identifier;


        /// <summary>
        /// Indicates whether this output is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this output is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty => Output.IsEmpty;


        /// <summary>
        /// The label that identifies this output.
        /// </summary>
        public string Label => Output.Label;


        internal Output Output
        {
            get;
        }


        public OutputPort OutputPort => Output.ToOutputPort(Output.Parent is Parameter);



        /// <summary>
        /// Removes an entity from the output queue and returns it.
        /// </summary>
        /// <returns>The entity removed from the output queue.</returns>
        public IEntity Dequeue()
        {
            return Output.Dequeue();
        }



        /// <summary>
        /// Removes an entity from the output queue and returns it.
        /// </summary>
        /// <typeparam name="T">Type of the entity to return.</typeparam>
        /// <returns>The entity removed from the output queue.</returns>
        public T Dequeue<T>() where T : IEntity
        {
            return (T) Output.Dequeue();
        }



        /// <summary>
        /// Removes all entities from the output queue and returns them.
        /// </summary>
        /// <returns>The entities removed from the output queue.</returns>
        public IEnumerable<IEntity> DequeueAll()
        {
            return Output.DequeueAll();
        }



        /// <summary>
        /// Removes all entities from the output queue and returns them.
        /// </summary>
        /// <typeparam name="T">Type of the entities to return.</typeparam>
        /// <returns>The entities removed from the output queue.</returns>
        public IEnumerable<T> DequeueAll<T>() where T : IEntity
        {
            return Output.DequeueAll().Cast<T>();
        }



        /// <summary>
        /// Returns the entity at the the top of the output queue without removing it.
        /// </summary>
        /// <returns>The entity at the top of the output queue or null, if no entity exists.</returns>
        public IEntity Peek()
        {
            return Output.Peek();
        }



        /// <summary>
        /// Returns the entity at the the top of the output queue without removing it.
        /// </summary>
        /// <typeparam name="T">Type of the entity to return.</typeparam>
        /// <returns>The entity at the top of the output queue or null, if no entity exists.</returns>
        public T Peek<T>() where T : IEntity
        {
            var item = Output.Peek();

            return item != null ? (T) item : default(T);
        }



        /// <summary>
        /// Returns all entities from the output queue without removing them.
        /// </summary>
        /// <returns>The entities from the output queue.</returns>
        public IEnumerable<IEntity> PeekAll()
        {
            return Output.PeekAll();
        }



        /// <summary>
        /// Returns all entities from the output queue without removing them.
        /// </summary>
        /// <typeparam name="T">Type of the entities to return.</typeparam>
        /// <returns>The entities from the output queue.</returns>
        public IEnumerable<T> PeekAll<T>() where T : IEntity
        {
            return Output.PeekAll().Cast<T>();
        }
    }
}