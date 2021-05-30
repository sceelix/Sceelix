using System;
using Newtonsoft.Json;

namespace Sceelix.Serialization
{
    public static class JsonSerializerExtension
    {
        public static void AddObject(this JsonSerializer serializer, string key, object value)
        {
            serializer.ReferenceResolver.AddReference(serializer, key, value);
        }



        public static object GetObject(this JsonSerializer serializer, string key)
        {
            return serializer.ReferenceResolver.ResolveReference(serializer, key);
        }



        public static string GetReference(this JsonSerializer serializer, object obj)
        {
            return serializer.ReferenceResolver.GetReference(serializer, obj);
        }



        public static T GetService<T>(this JsonSerializer serializer)
        {
            var serviceReferenceResolver = serializer.ReferenceResolver as ServiceReferenceResolver;
            if (serviceReferenceResolver == null)
                throw new InvalidOperationException("The Reference Resolver has not been set to a ServiceReferenceResolver.");

            return serviceReferenceResolver.GetService<T>();
        }



        /*public static Object GetContextObject(this JsonSerializer serializer, String key)
        {
            var serializationContext = serializer.Context.Context as StandardSerializationContext;
            if (serializationContext != null)
            {
                serializationContext.Data[key];
            }

        }*/
        public static string GetUniqueId(this JsonSerializer serializer, object obj)
        {
            return serializer.GetService<IUniqueIdGenerator>().GetId(obj);
        }



        /*public static bool HasObject(this JsonSerializer serializer, Object obj)
        {
            return serializer.ReferenceResolver.IsReferenced(serializer, obj);
        }*/



        public static bool HasReference(this JsonSerializer serializer, string key)
        {
            return serializer.ReferenceResolver.ResolveReference(serializer, key) != null;
        }



        public static void SetObject(this JsonSerializer serializer, string key, object value)
        {
            serializer.ReferenceResolver.AddReference(serializer, key, value);
        }



        /*public static bool HasObject(this JsonSerializer serializer, Type objectType, String key)
        {
            var actualKey = objectType + key;

            var reference = serializer.ReferenceResolver.ResolveReference(serializer, actualKey);
            if (reference is ReferenceContainer)
            {
                var data = ((ReferenceContainer) reference).Data;
                if (objectType.IsInstanceOfType(data))
                    return true;
            }
            else if (objectType.IsInstanceOfType(reference))
                return true;

            return false;
        }



        public static bool HasObject<T>(this JsonSerializer serializer, String key)
        {
            return serializer.HasObject(typeof(T), key);
        }



        public static Object GetObject(this JsonSerializer serializer, Type objectType, String key)
        {
            var actualKey = objectType + key;

            var reference = serializer.ReferenceResolver.ResolveReference(serializer, actualKey);
            if (reference is ReferenceContainer)
            {
                var data = ((ReferenceContainer) reference).Data;
                if (objectType.IsInstanceOfType(data))
                    return data;
            }
            else if (objectType.IsInstanceOfType(reference))
                return reference;

            return null;
        }



        public static T GetObject<T>(this JsonSerializer serializer, String key)
        {
            var obj = serializer.GetObject(typeof(T), key);
            if (obj == null)
                return default(T);

            return (T) obj;
        }



        public static void AddObject<T>(this JsonSerializer serializer, String key, T value)
        {
            serializer.AddObject(typeof(T), key, value);
        }


        


        public static void AddObject(this JsonSerializer serializer, Type objectType, String key, Object value)
        {
            var actualKey = objectType + key;

            serializer.ReferenceResolver.AddReference(serializer, actualKey, value);
        }



        public static void SetReference<T>(this JsonSerializer serializer, String key, T value)
        {
            serializer.SetReference(typeof(T), key, value);
        }



        public static void SetReference(this JsonSerializer serializer, Type objectType, String key, Object value)
        {
            var actualKey = objectType + key;

            var reference = serializer.ReferenceResolver.ResolveReference(serializer, actualKey);
            if (reference is ReferenceContainer)
            {
                var referenceContainer = (ReferenceContainer) reference;
                referenceContainer.Data = value;
            }
            else if (reference == null)
            {
                serializer.ReferenceResolver.AddReference(serializer, actualKey, new ReferenceContainer() {Data = value});
            }
            else
            {
                //meh, let the AddReference function complain that the key already exists
                serializer.ReferenceResolver.AddReference(serializer, actualKey, value);
            }
        }*/


        /*private class ReferenceContainer
        {
            public Object Data
            {
                get;
                set;
            }
        }*/
    }
}