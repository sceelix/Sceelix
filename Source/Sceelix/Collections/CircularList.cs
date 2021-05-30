using System.Collections;
using System.Collections.Generic;
using Sceelix.Extensions;

namespace Sceelix.Collections
{
    /// <summary>
    /// A simple List whose indexing operation supports 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CircularList<T> : IList<T>
    {
        private readonly List<T> _internalList = new List<T>();



        public CircularList()
        {
        }



        public CircularList(IEnumerable<T> collection)
        {
            _internalList = new List<T>(collection);
        }



        public CircularList(int capacity)
        {
            _internalList = new List<T>(capacity);
        }



        public int Count => _internalList.Count;


        public bool IsReadOnly => false;



        /// <summary>
        /// Returns the item at the indicated position, going around if the index exceeds the minimum or maximum limits.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index]
        {
            get { return _internalList.GetCircular(index); }
            set { _internalList[_internalList.GetCircularIndex(index)] = value; }
        }



        public void Add(T item)
        {
            _internalList.Add(item);
        }



        public void AddRange(IEnumerable<T> items)
        {
            _internalList.AddRange(items);
        }



        public void Clear()
        {
            _internalList.Clear();
        }



        public bool Contains(T item)
        {
            return _internalList.Contains(item);
        }



        public void CopyTo(T[] array, int arrayIndex)
        {
            _internalList.CopyTo(array, arrayIndex);
        }



        public IEnumerator<T> GetEnumerator()
        {
            return _internalList.GetEnumerator();
        }



        IEnumerator IEnumerable.GetEnumerator()
        {
            return _internalList.GetEnumerator();
        }



        public CircularList<T> GetRangeAt(int indexBegin, int indexEnd)
        {
            indexBegin = _internalList.GetCircularIndex(indexBegin);
            indexEnd = _internalList.GetCircularIndex(indexEnd);

            if (indexBegin > indexEnd)
            {
                CircularList<T> vTemp = new CircularList<T>(_internalList.GetRange(indexBegin, Count - indexBegin));
                vTemp.AddRange(_internalList.GetRange(0, indexEnd + 1));
                return vTemp;
            }

            return new CircularList<T>(_internalList.GetRange(indexBegin, indexEnd - indexBegin + 1));
        }



        public int IndexOf(T item)
        {
            return _internalList.IndexOf(item);
        }



        public void Insert(int index, T item)
        {
            _internalList.Insert(index, item);
        }



        public void InsertRange(int index, IEnumerable<T> newList)
        {
            _internalList.InsertRange(index, newList);
        }



        public bool Remove(T item)
        {
            return _internalList.Remove(item);
        }



        public void RemoveAt(int index)
        {
            _internalList.RemoveAt(index);
        }



        public void Reverse()
        {
            _internalList.Reverse();
        }
    }
}