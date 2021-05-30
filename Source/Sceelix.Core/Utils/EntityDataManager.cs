using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Sceelix.Core.Utils
{
    public class FastProperty
    {
        public Func<object, object> GetDelegate;
        public Action<object, object> SetDelegate;



        public FastProperty(PropertyInfo property)
        {
            Property = property;

            InitializeGet();
            InitializeSet();
        }



        public PropertyInfo Property
        {
            get;
            set;
        }



        public object Get(object instance)
        {
            return GetDelegate(instance);
        }



        private void InitializeGet()
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            UnaryExpression instanceCast = !Property.DeclaringType.IsValueType ? Expression.TypeAs(instance, Property.DeclaringType) : Expression.Convert(instance, Property.DeclaringType);
            GetDelegate = Expression.Lambda<Func<object, object>>(Expression.TypeAs(Expression.Call(instanceCast, Property.GetGetMethod()), typeof(object)), instance).Compile();
        }



        private void InitializeSet()
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var value = Expression.Parameter(typeof(object), "value");

            // value as T is slightly faster than (T)value, so if it's not a value type, use that
            UnaryExpression instanceCast = !Property.DeclaringType.IsValueType ? Expression.TypeAs(instance, Property.DeclaringType) : Expression.Convert(instance, Property.DeclaringType);
            UnaryExpression valueCast = !Property.PropertyType.IsValueType ? Expression.TypeAs(value, Property.PropertyType) : Expression.Convert(value, Property.PropertyType);
            SetDelegate = Expression.Lambda<Action<object, object>>(Expression.Call(instanceCast, Property.GetSetMethod(), valueCast), instance, value).Compile();
        }



        public void Set(object instance, object value)
        {
            SetDelegate(instance, value);
        }
    }

    public class FastPropertyManager
    {
        public static void Register(Type type)
        {
        }
    }

    public class ObjectDataHolder
    {
        private Dictionary<string, FastProperty> _data = new Dictionary<string, FastProperty>();



        public ObjectDataHolder(Type type)
        {
            //for each property, get a FastProperty
        }



        /*public Object this[Object entity, String key]
        {
            get
            {
                FastProperty fastProperty;
                if (_data.TryGetValue(key, out fastProperty))
                    return fastProperty.Get(entity);
            }
            set
            {
                
                _data[key] = value;
            }
        }*/
    }

    public class EntityDataManager
    {
        private readonly Dictionary<Type, ObjectDataHolder> _dictionary = new Dictionary<Type, ObjectDataHolder>();



        private ObjectDataHolder Get(object obj)
        {
            ObjectDataHolder holder;

            if (_dictionary.TryGetValue(obj.GetType(), out holder))
                return holder;

            return null;
        }



        public void Register(Type type)
        {
            _dictionary.Add(type, new ObjectDataHolder(type));
        }



        /*public Object GetData(String key, IEntity data)
        {
            if (String.IsNullOrWhiteSpace(key))
                return data.Attributes.ToSceeList();

            var parts = key.Split(new []{ '.' },2,StringSplitOptions.None);
            var firstPart = parts[0];
            //first, try to get 

            if(firstPart == "")
        }*/
    }
}