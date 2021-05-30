using System.Collections;
using System.Collections.Generic;

namespace Sceelix.Core.IO
{
    public class InputCollection : IEnumerable<Input>
    {
        private readonly List<Input> _list;



        public InputCollection()
        {
            //this should still be accessible to add items
            _list = new List<Input>();
        }



        public InputCollection(List<Input> list)
        {
            //this should be readonly from now on
            _list = list;
        }



        public InputCollection(params Input[] outputs)
        {
            _list = new List<Input>(outputs);
        }



        public int Count => _list.Count;



        public Input this[string label]
        {
            get
            {
                var result = _list.Find(x => x.Label == label);
                if (result == null)
                    throw new KeyNotFoundException("There is no input with label '" + label + "'.");

                return result;
            }
        }



        public Input this[int index]
        {
            get
            {
                try
                {
                    return _list[index];
                    ;
                }
                catch (KeyNotFoundException)
                {
                    throw new KeyNotFoundException("There is no input at index " + index + ".");
                }
            }
        }



        public IEnumerator<Input> GetEnumerator()
        {
            return _list.GetEnumerator();
        }



        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}