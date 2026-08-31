using System.Buffers;
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

        /// <summary>
        /// 原生 UTF-8 字节反序列化：直接消费报文正文字节，省去「字节 → UTF-16 string → UTF-8」的二次转码。
        /// </summary>
        public object Deserialize(ReadOnlySequence<byte> utf8Value)
        {
            var reader = new Utf8JsonReader(utf8Value);
            return JsonSerializer.Deserialize(ref reader, GetTypeInfo());
        }

        /// <summary>
        /// 原生 UTF-8 字节序列化：模型直写 IBufferWriter，无中间 string 分配。
        /// 与通道报文正文固定 UTF-8 编码一致，线上字节序列与旧版 Serialize→GetBytes 完全等价。
        /// </summary>
        public void Serialize(object obj, IBufferWriter<byte> writer)
        {
            using var jsonWriter = new Utf8JsonWriter(writer);
            JsonSerializer.Serialize(jsonWriter, (T)obj, GetTypeInfo());
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
