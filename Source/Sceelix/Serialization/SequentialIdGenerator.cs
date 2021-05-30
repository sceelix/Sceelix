using System.Collections.Generic;

namespace Sceelix.Serialization
{
    public class SequentialIdGenerator : IUniqueIdGenerator
    {
        private readonly string _prefix;
        private readonly Dictionary<object, int> _objectIds = new Dictionary<object, int>();
        private int _lastId;



        public SequentialIdGenerator(string prefix)
        {
            _prefix = prefix;
        }



        public string GetId(object obj)
        {
            int id;
            if (!_objectIds.TryGetValue(obj, out id))
            {
                id = _lastId++;
                _objectIds.Add(obj, id);
            }

            return _prefix + id;
        }
    }
}