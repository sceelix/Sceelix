using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Extensions;

namespace Sceelix.Core.Parameters
{
    public class ParameterReferenceCollection : IEnumerable<ParameterReference>
    {
        private readonly List<Parameter> _list;



        public ParameterReferenceCollection()
        {
            //this should still be accessible to add items
            _list = new List<Parameter>();
        }



        public ParameterReferenceCollection(IEnumerable<Parameter> list)
        {
            //this should be readonly from now on
            _list = list is List<Parameter> ? (List<Parameter>) list : new List<Parameter>(list);
        }



        /// <summary>
        /// The number of ParameterReferences in this collection.
        /// </summary>
        public int Count => _list.Count;



        /// <summary>
        /// Gets the ParameterReference with the indicated label.
        /// </summary>
        /// <param name="label">Label that identifies the ParameterReference.</param>
        /// <exception cref="KeyNotFoundException">No ParameterReference with such a label exists.</exception>
        /// <returns>The ParameterReference with the indicated label</returns>
        public ParameterReference this[string label]
        {
            get
            {
                var subLabels = label.SplitInTwo('/');

                var firstLabel = subLabels.FirstOrDefault();

                var result = _list.Find(x => x.Label == firstLabel);
                if (result == null)
                    throw new KeyNotFoundException("There is no parameter with label '" + firstLabel + "'.");

                var currentParameterReference = new ParameterReference(result);
                if (subLabels.Length > 1)
                    return currentParameterReference.Parameters[subLabels.Last()];

                return currentParameterReference;
            }
        }



        /// <summary>
        /// Gets the ParameterReference at the indicated index.
        /// </summary>
        /// <param name="label">Index where the ParameterReference can be found.</param>
        /// <exception cref="KeyNotFoundException">No ParameterReference with such a label exists.</exception>
        /// <returns>The OutputReference at the indicated index.</returns>
        public ParameterReference this[int index]
        {
            get
            {
                try
                {
                    return new ParameterReference(_list[index]);
                }
                catch (KeyNotFoundException)
                {
                    throw new KeyNotFoundException("There is no parameter at index " + index + ".");
                }
            }
        }



        public IEnumerator<ParameterReference> GetEnumerator()
        {
            return _list.Select(x => new ParameterReference(x)).GetEnumerator();
        }



        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}