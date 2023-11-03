using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;

namespace StunTools
{
    public class Message
    {
        public IPEndPoint? EndPoint = null;
        public Type ObjectType;
        public string Data;

        public Message(object data)
        {
            Data = JsonConvert.SerializeObject(data, Formatting.None, new JsonSerializerSettings()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
            ObjectType = data.GetType();
        }

        [JsonConstructor]
        public Message(Type objectType, string data)
        {
            ObjectType = objectType;
            Data = data;
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
        public dynamic? GetData()
        {
            return Convert.ChangeType(JsonConvert.DeserializeObject(Data, ObjectType), ObjectType);
        }

        public T? GetData<T>()
        {
            return JsonConvert.DeserializeObject<T>(Data, new JsonSerializerSettings()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
        }

        public static Message? Deserilize(string data, IPEndPoint? endPoint = null)
        {
            Debug.WriteLine(data);
            Message? message = JsonConvert.DeserializeObject<Message>(data);
            if (message is not null) message.EndPoint = endPoint;
            return message;
        }
    }
}
