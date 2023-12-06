using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Quick.Protocol.Model
{
    public abstract class AbstractQpModel<T> : IQpModel, IQpModel<T>
    {
        protected abstract JsonTypeInfo<T> TypeInfo { get; }

        public T Deserialize(string value)
        {
            return JsonSerializer.Deserialize<T>(value, TypeInfo);
        }

        public string Serialize(T obj)
        {
            return JsonSerializer.Serialize<T>(obj, TypeInfo);
        }

        object IQpModel.Deserialize(string value)
        {
            return Deserialize(value);
        }

        string IQpModel.Serialize(object obj)
        {
            return Serialize((T)obj);
        }
    }
}
