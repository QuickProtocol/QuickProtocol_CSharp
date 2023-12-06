using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Quick.Protocol
{
    public abstract class AbstractQpSerializer<T> : IQpSerializer, IQpModel<T>
    {
        protected abstract JsonTypeInfo<T> GetTypeInfo();

        public T Deserialize(string value)
        {
            return JsonSerializer.Deserialize(value, GetTypeInfo());
        }

        public string Serialize(T obj)
        {
            return JsonSerializer.Serialize(obj, GetTypeInfo());
        }

        object IQpSerializer.Deserialize(string value)
        {
            return Deserialize(value);
        }

        string IQpSerializer.Serialize(object obj)
        {
            return Serialize((T)obj);
        }
    }
}
