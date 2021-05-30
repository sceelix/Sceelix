using System.Collections;
using System.Collections.Generic;

namespace Sceelix.Core.IO
{
    public class OutputCollection : IEnumerable<Output>
    {
        private readonly List<Output> _list;



        public OutputCollection()
        {
            //this should still be accessible to add items
            _list = new List<Output>();
        }



        public OutputCollection(params Output[] outputs)
        {
            _list = new List<Output>(outputs);
        }



        public OutputCollection(List<Output> list)
        {
            //this should be readonly from now on
            _list = list;
        }



        public int Count => _list.Count;



        public Output this[string label]
        {
            get
            {
                var result = _list.Find(x => x.Label == label);
                if (result == null)
                    throw new KeyNotFoundException("There is no output with label '" + label + "'.");

                return result;
            }
        }



        public Output this[int index]
        {
            get
            {
                try
                {
                    return _list[index];
                }
                catch (KeyNotFoundException)
                {
                    throw new KeyNotFoundException("There is no output at index " + index + ".");
                }
            }
        }



        public IEnumerator<Output> GetEnumerator()
        {
            return _list.GetEnumerator();
        }



        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}