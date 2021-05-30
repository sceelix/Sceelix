using Sceelix.Core.Data;

namespace Sceelix.Actors.Data
{
    /// <summary>
    /// 
    /// Materials are not immutable. Once can change properties 
    /// But if a node changes a property from a material, it would be preferable to create a new
    /// 
    /// For instance, a material that references 
    /// 
    /// This will help reducing the amount of data involved and help grouping materials in the rendering phase.
    /// </summary>
    public class Material : Entity
    {
        public Material()
        {
            Type = GetType().Name;
        }



        public Material(string type)
        {
            Type = type;
        }



        public virtual string Reference => "Material_" + Attributes.Hash;


        public string Type
        {
            get;
            protected set;
        }



        public override bool Equals(object obj)
        {
            var customMaterial = obj as Material;

            if (customMaterial == null)
                return false;

            if (customMaterial.Type != Type)
                return false;

            return customMaterial.Attributes.ContentEquals(Attributes);
        }



        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }



        /*public Object this[String key]
        {
            get
            {
                Object value;
                if (_properties.TryGetValue(key, out value))
                    return value;

                return null;
            }
            set
            {
                if (!_properties.ContainsKey(key))
                    _properties.Add(key, value);
                else
                    _properties[key] = value;
            }
        }

        public T Get<T>(String propertyName)
        {
            return (T)this[propertyName];
        }

        public void Set<T>(String propertyName, T value)
        {
            this[propertyName] = value;
        }*/

        /*public Dictionary<string, object> Properties
        {
            get { return _properties; }
        }*/


        /*protected bool Equals(Material other)
        {
            return Equals(_properties, other._properties);
        }

        public override int GetHashCode()
        {
            return (_properties != null ? _properties.GetHashCode() : 0);
        }*/

        /*public Material DeepClone()
        {
            Material clonedMaterial = new Material(Type);

            foreach (KeyValuePair<string, object> keyValuePair in _properties)
                clonedMaterial._properties[keyValuePair.Key] = keyValuePair.Value.Clone();

            return clonedMaterial;
        }*/


        /*public Material(string type, params KeyValuePair<string, object>[] properties)
            :this(type)
        {

            foreach (var pair in properties)
                _properties.Add(pair.Key, pair.Value);
        }
        */

        //private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();
    }
}