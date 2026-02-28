using System.Text.Json;
using System.Text;
using System;
using System.Text.Json.Serialization;

namespace Quick.Protocol.InterfaceService.JsonConverters
{
    public class QpJsonInt32Converter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return default;
                case JsonTokenType.Number:
                    return reader.GetInt32();
                case JsonTokenType.String:
                    var str = reader.GetString();
                    if (string.IsNullOrEmpty(str))
                        return default;
                    return int.Parse(str);
                default:
                    throw new FormatException($"值[{Encoding.UTF8.GetString(reader.ValueSpan)}]无法转换为int");
            }
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}