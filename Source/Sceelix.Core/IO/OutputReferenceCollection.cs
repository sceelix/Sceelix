using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Data;

namespace Sceelix.Core.IO
{
    public class OutputReferenceCollection : IEnumerable<OutputReference>
    {
        private readonly List<Output> _list;



        internal OutputReferenceCollection()
        {
            //this should still be accessible to add items
            _list = new List<Output>();
        }



        internal OutputReferenceCollection(IEnumerable<Output> list)
        {
            //this should be readonly from now on
            _list = list is List<Output> ? (List<Output>) list : new List<Output>(list);
        }



        /// <summary>
        /// The number of OutputReferences in this collection.
        /// </summary>
        public int Count => _list.Count;



        /// <summary>
        /// Gets the OutputReference with the indicated label.
        /// </summary>
        /// <param name="label">Label that identifies the OutputReference.</param>
        /// <exception cref="KeyNotFoundException">No OutputReference with such a label exists.</exception>
        /// <returns>The OutputReference with the indicated label</returns>
        public OutputReference this[string label]
        {
            get
            {
                var result = _list.Find(x => x.Label == label);
                if (result == null)
                    throw new KeyNotFoundException("There is no output with label '" + label + "'.");

                return new OutputReference(result);
            }
        }



        /// <summary>
        /// Gets the OutputReference at the indicated index.
        /// </summary>
        /// <param name="label">Index where the OutputReference can be found.</param>
        /// <exception cref="KeyNotFoundException">No OutputReference with such a label exists.</exception>
        /// <returns>The OutputReference at the indicated index.</returns>
        public OutputReference this[int index]
        {
            get
            {
                try
                {
                    return new OutputReference(_list[index]);
                }
                catch (KeyNotFoundException)
                {
                    throw new KeyNotFoundException("There is no output at index " + index + ".");
                }
            }
        }



        /// <summary>
        /// Removes all entities from all output queues and returns them.
        /// </summary>
        /// <returns>The entities removed from the output queue.</returns>
        public virtual IEnumerable<IEntity> DequeueAll()
        {
            return _list.SelectMany(output => output.DequeueAll()); //.Union(AllOutput.PopAll());
        }



        /// <summary>
        /// Removes all entities from all output queues and returns them.
        /// </summary>
        /// <typeparam name="T">Type of the entities to return.</typeparam>
        /// <returns>The entities removed from the output queue.</returns>
        public virtual IEnumerable<T> DequeueAll<T>()
        {
            return _list.SelectMany(output => output.DequeueAll()).Cast<T>(); //.Union(AllOutput.PopAll());
        }



        public IEnumerator<OutputReference> GetEnumerator()
        {
            return _list.Select(x => new OutputReference(x)).GetEnumerator();
        }



        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }



        /// <summary>
        /// Searches for the specified output and returns the zero-based index where it can be found. 
        /// </summary>
        /// <param name="output">Output to be searched.</param>
        /// <returns>The zero-based index of the first occurrence of the output, if found; otherwise, –1.</returns>
        internal int IndexOf(Output output)
        {
            return _list.IndexOf(output);
        }



        /// <summary>
        /// Returns all entities from all output queues without removing them.
        /// </summary>
        /// <returns>The entities from the output queue.</returns>
        public virtual IEnumerable<IEntity> PeekAll()
        {
            return _list.SelectMany(output => output.PeekAll());
        }



        /// <summary>
        /// Returns all entities from all output queues without removing them.
        /// </summary>
        /// <typeparam name="T">Type of the entities to return.</typeparam>
        /// <returns>The entities from the output queue.</returns>
        public virtual IEnumerable<T> PeekAll<T>()
        {
            return _list.SelectMany(output => output.PeekAll()).Cast<T>();
        }
    }
}