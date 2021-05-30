using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Assets.Sceelix.Communication
{
    public class NetworkMessage
    {
        private static JsonSerializerSettings _settings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, TypeNameHandling = TypeNameHandling.Auto,ObjectCreationHandling = ObjectCreationHandling.Replace,Binder = new JsonBinder()};


        public String Subject;
        public Object Data;

        public NetworkMessage(string subject, object data)
        {
            Subject = subject;
            Data = data;
        }

        public String Serialize()
        {
            return JsonConvert.SerializeObject(this, Formatting.None, _settings);
        }

        public static NetworkMessage Deserialize(String messageString)
        {
            return JsonConvert.DeserializeObject<NetworkMessage>(messageString, _settings);
        }

        public static JsonSerializerSettings Settings
        {
            get { return _settings; }
            set { _settings = value; }
        }



        internal class JsonBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                return Type.GetType(typeName);
            }
        }
    }
}
