using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Sceelix.Network
{
    public class NetworkMessage
    {
        public object Data;


        public string Subject;



        public NetworkMessage(string subject, object data)
        {
            Subject = subject;
            Data = data;
        }



        public static JsonSerializerSettings Settings
        {
            get;
            set;
        } = new JsonSerializerSettings {PreserveReferencesHandling = PreserveReferencesHandling.Objects, TypeNameHandling = TypeNameHandling.Auto, ObjectCreationHandling = ObjectCreationHandling.Replace, Binder = new JsonBinder()};



        public static NetworkMessage Deserialize(string messageString)
        {
            return JsonConvert.DeserializeObject<NetworkMessage>(messageString, Settings);
        }



        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, Formatting.None, Settings);
        }



        //Looks like Unity does not support the .NET 4 version of SerializationBinder which has the BindToName function
        internal class JsonBinder : SerializationBinder
        {
            public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                assemblyName = null;
                typeName = serializedType.FullName;
            }



            public override Type BindToType(string assemblyName, string typeName)
            {
                return Type.GetType(typeName);
            }
        }
    }
}