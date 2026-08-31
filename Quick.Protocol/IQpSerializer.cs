using System.Buffers;
using System.Text;

namespace Quick.Protocol;

public interface IQpSerializer
{
    string Serialize(object obj);
    object Deserialize(string value);

    /// <summary>
    /// 从 UTF-8 字节序列直接反序列化模型，避免「字节 → string → UTF-8」的二次转码。
    /// 默认实现回退到 string 版 API，因此已有（外部）实现者无需任何改动即兼容。
    /// 通道的报文正文始终以 UTF-8 编码（见 QpChannel.encoding），故此处固定用 UTF-8。
    /// </summary>
    object Deserialize(ReadOnlySequence<byte> utf8Value)
        => Deserialize(Encoding.UTF8.GetString(utf8Value.ToArray()));

    /// <summary>
    /// 将模型直接序列化进 IBufferWriter&lt;byte&gt;（UTF-8），避免「模型 → string → GetBytes」的中间分配。
    /// 默认实现回退到 string 版 API；AbstractQpSerializer&lt;T&gt; 已用原生 JSON 覆盖以获得零拷贝收益。
    /// </summary>
    void Serialize(object obj, IBufferWriter<byte> writer)
        => Encoding.UTF8.GetBytes(Serialize(obj), writer);
}

public interface IQpModel<T> : IQpSerializer
{
    string Serialize(T obj);
    new T Deserialize(string value);
}
