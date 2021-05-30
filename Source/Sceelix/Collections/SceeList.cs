using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Extensions;

namespace Sceelix.Collections
{
    public class SceeList : IList<object>, ICloneable
    {
        private readonly List<KeyValuePair<string, object>> _list;



        public SceeList()
        {
            _list = new List<KeyValuePair<string, object>>();
        }



        public SceeList(int capacity)
        {
            _list = new List<KeyValuePair<string, object>>(capacity);
        }



        public SceeList(IEnumerable<KeyValuePair<string, object>> keyValuePairs)
        {
            _list = new List<KeyValuePair<string, object>>(keyValuePairs);
        }



        public SceeList(IEnumerable<string> keys, IEnumerable<object> values)
        {
            var keyArray = keys.ToArray();
            var valueArray = values.ToArray();

            _list = new List<KeyValuePair<string, object>>(valueArray.Length);
            for (int i = 0; i < valueArray.Length; i++)
            {
                var key = i < keyArray.Length ? keyArray[i] : string.Empty;
                _list.Add(new KeyValuePair<string, object>(key, valueArray[i]));
            }
        }



        public SceeList(IEnumerable<object> values)
        {
            _list = values.Select(x => new KeyValuePair<string, object>(string.Empty, x)).ToList();
        }



        public SceeList(params KeyValuePair<string, object>[] keyValuePairs)
        {
            _list = new List<KeyValuePair<string, object>>(keyValuePairs);
        }



        public int Count => _list.Count;


        public bool IsAssociative => !Keys.All(string.IsNullOrWhiteSpace);


        public bool IsReadOnly => false;



        public object this[string name]
        {
            get
            {
                var lastIndex = _list.FindIndex(val => val.Key == name);
                if (lastIndex >= 0)
                    return this[lastIndex];

                return null;
            }
            set
            {
                var lastIndex = _list.FindIndex(val => val.Key == name);
                if (lastIndex >= 0)
                    this[lastIndex] = value;
                else
                    Add(name, value);
            }
        }



        public object this[int index]
        {
            get { return _list[index].Value; }
            set { _list[index] = new KeyValuePair<string, object>(_list[index].Key, value); }
        }



        public List<string> Keys
        {
            get { return _list.Select(x => x.Key).ToList(); }
        }



        public IEnumerable<KeyValuePair<string, object>> KeyValues => _list;



        public IEnumerable<object> Values
        {
            get { return _list.Select(x => x.Value); }
        }



        public void Add(object item)
        {
            _list.Add(new KeyValuePair<string, object>(string.Empty, item));
        }



        public void Add(string key, object item)
        {
            _list.Add(new KeyValuePair<string, object>(key, item));
        }



        public object Average()
        {
            return _list.Average(x => (dynamic) x);
        }



        public void Clear()
        {
            _list.Clear();
        }



        public object Clone()
        {
            var sceelist = new SceeList();
            foreach (KeyValuePair<string, object> keyValuePair in _list) sceelist.Add(keyValuePair.Key, keyValuePair.Value.Clone());

            return sceelist;
        }



        public bool Contains(object item)
        {
            return _list.Select(x => x.Value).Contains(item);
        }



        public void CopyTo(object[] array, int arrayIndex)
        {
            _list.Select(x => x.Value).ToList().CopyTo(array, arrayIndex);
        }



        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var otherSceeList = obj as SceeList;
            if (otherSceeList == null)
                return false;

            //perform equals for each keyvaluepair
            for (int i = 0; i < Count; i++)
                if (!_list[i].Key.Equals(otherSceeList._list[i].Key) || !_list[i].Value.Equals(otherSceeList._list[i].Value))
                    return false;

            return true;
        }



        public IEnumerator<object> GetEnumerator()
        {
            return _list.Select(x => x.Value).GetEnumerator();
        }



        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }



        public override int GetHashCode()
        {
            return _list.GetHashCode();
        }



        public int IndexOf(object item)
        {
            return _list.FindIndex(x => x.Value == item);
        }



        public void Insert(int index, object item)
        {
            _list.Insert(index, new KeyValuePair<string, object>(string.Empty, item));
        }



        public object Max()
        {
            return _list.Max(x => (dynamic) x.Value);
        }



        public object Min()
        {
            return _list.Min(x => (dynamic) x.Value);
        }



        public static SceeList operator +(SceeList c1, SceeList c2)
        {
            SceeList list = new SceeList(c1.Count);
            for (int i = 0; i < c1.Count; i++) list.Add(c1.Keys[i], (dynamic) c1[i] + (dynamic) c2[i]);

            return list;
        }



        public static SceeList operator +(SceeList c1, object c2)
        {
            SceeList list = new SceeList(c1.Count);
            for (int i = 0; i < c1.Count; i++) list.Add(c1.Keys[i], (dynamic) c1[i] + (dynamic) c2);

            return list;
        }



        public static SceeList operator /(SceeList c1, SceeList c2)
        {
            SceeList list = new SceeList(c1.Count);
            for (int i = 0; i < c1.Count; i++) list.Add(c1.Keys[i], (dynamic) c1[i] / (dynamic) c2[i]);

            return list;
        }



        public static SceeList operator /(SceeList c1, object c2)
        {
            SceeList list = new SceeList(c1.Count);
            for (int i = 0; i < c1.Count; i++) list.Add(c1.Keys[i], (dynamic) c1[i] / (dynamic) c2);

            return list;
        }



        public static SceeList operator *(SceeList c1, SceeList c2)
        {
            SceeList list = new SceeList(c1.Count);
            for (int i = 0; i < c1.Count; i++) list.Add(c1.Keys[i], (dynamic) c1[i] * (dynamic) c2[i]);

            return list;
        }



        public static SceeList operator *(SceeList c1, object c2)
        {
            SceeList list = new SceeList(c1.Count);
            for (int i = 0; i < c1.Count; i++) list.Add(c1.Keys[i], (dynamic) c1[i] * (dynamic) c2);

            return list;
        }



        public static SceeList operator -(SceeList c1, SceeList c2)
        {
            SceeList list = new SceeList(c1.Count);
            for (int i = 0; i < c1.Count; i++) list.Add(c1.Keys[i], (dynamic) c1[i] - (dynamic) c2[i]);

            return list;
        }



        public static SceeList operator -(SceeList c1, object c2)
        {
            SceeList list = new SceeList(c1.Count);
            for (int i = 0; i < c1.Count; i++) list.Add(c1.Keys[i], (dynamic) c1[i] - (dynamic) c2);

            return list;
        }



        public bool Remove(object item)
        {
            return _list.RemoveAll(x => x.Value == item) > 0;
        }



        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }



        public SceeList ShallowClone()
        {
            var sceelist = new SceeList();
            foreach (KeyValuePair<string, object> keyValuePair in _list) sceelist.Add(keyValuePair.Key, keyValuePair.Value);

            return sceelist;
        }



        public object Sum()
        {
            return _list.Sum(x => (dynamic) x.Value);
        }



        public override string ToString()
        {
            return "[" + string.Join(",", _list.Select(x => ToString(x))) + "]";
        }



        private string ToString(KeyValuePair<string, object> pair)
        {
            string key = !string.IsNullOrWhiteSpace(pair.Key) ? "\"" + pair.Key + "\" : " : string.Empty;

            string value = pair.Value is string ? "\"" + pair.Value + "\"" : "" + pair.Value;

            return key + value;
        }
    }
}