using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sceelix.Core.IO
{
    public class InputReferenceCollection : IEnumerable<InputReference>
    {
        private readonly List<Input> _list;



        internal InputReferenceCollection()
        {
            //this should still be accessible to add items
            _list = new List<Input>();
        }



        internal InputReferenceCollection(IEnumerable<Input> list)
        {
            //this should be readonly from now on
            _list = list is List<Input> ? (List<Input>) list : new List<Input>(list);
        }



        /// <summary>
        /// The number of InputReferences in this collection.
        /// </summary>
        public int Count => _list.Count;



        /// <summary>
        /// Gets the InputReference with the indicated label.
        /// </summary>
        /// <param name="label">Label that identifies the InputReference.</param>
        /// <exception cref="KeyNotFoundException">No InputReference with such a label exists.</exception>
        /// <returns>The InputReference with the indicated label</returns>
        public InputReference this[string label]
        {
            get
            {
                var result = _list.Find(x => x.Label == label);
                if (result == null)
                    throw new KeyNotFoundException("There is no input with label '" + label + "'.");

                return new InputReference(result);
            }
        }



        /// <summary>
        /// Gets the InputReferences at the indicated index.
        /// </summary>
        /// <param name="label">Index where the InputReferences can be found.</param>
        /// <exception cref="KeyNotFoundException">No InputReferences with such a label exists.</exception>
        /// <returns>The InputReferences at the indicated index.</returns>
        public InputReference this[int index]
        {
            get
            {
                try
                {
                    return new InputReference(_list[index]);
                }
                catch (KeyNotFoundException)
                {
                    throw new KeyNotFoundException("There is no input at index " + index + ".");
                }
            }
        }



        public IEnumerator<InputReference> GetEnumerator()
        {
            return _list.Select(x => new InputReference(x)).GetEnumerator();
        }



        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }



        /// <summary>
        /// Searches for the specified input and returns the zero-based index where it can be found. 
        /// </summary>
        /// <param name="input">Input to be searched.</param>
        /// <returns>The zero-based index of the first occurrence of the input, if found; otherwise, –1.</returns>
        internal int IndexOf(Input input)
        {
            return _list.IndexOf(input);
        }



        internal IEnumerable<int> IndicesWithDataLeft()
        {
            for (int i = 0; i < _list.Count; i++)
                if (_list[i].HasDataReady())
                    yield return i;
        }
    }
}