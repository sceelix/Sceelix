using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace Sceelix.Serialization
{
    public class ServiceReferenceResolver : IReferenceResolver
    {
        private readonly Dictionary<object, string> _objectToString;
        private readonly List<object> _services = new List<object>();

        private readonly Dictionary<string, object> _stringToObject;

        private int _referenceCount;



        public ServiceReferenceResolver(params object[] services)
        {
            _stringToObject = new Dictionary<string, object>();
            _objectToString = new Dictionary<object, string>();
            _referenceCount = 0;

            foreach (object service in services) _services.Add(service);
        }



        public void AddReference(object context, string reference, object value)
        {
            _objectToString[value] = reference;
            _stringToObject[reference] = value;
        }



        public string GetReference(object context, object value)
        {
            string result = null;

            if (!_objectToString.TryGetValue(value, out result))
            {
                _referenceCount++;
                result = _referenceCount.ToString(CultureInfo.InvariantCulture);

                _objectToString.Add(value, result);
                _stringToObject.Add(result, value);
            }

            return result;
        }



        public T GetService<T>()
        {
            return (T) _services.First(x => x is T);
        }



        public bool IsReferenced(object context, object value)
        {
            return _objectToString.ContainsKey(value);
        }



        public object ResolveReference(object context, string reference)
        {
            object r = default(object);

            _stringToObject.TryGetValue(reference, out r);
            return r;
        }
    }
}