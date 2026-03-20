using System.Text.Json;
using System.Text;
using System;
using System.Text.Json.Serialization;

namespace Quick.Protocol.JsonConverters
{
    public class QpJsonBoolConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return default;
                case JsonTokenType.Number:
                    return reader.GetInt32() == 1;
                case JsonTokenType.String:
                    return bool.Parse(reader.GetString());
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.False:
                    return false;
                default:
                    throw new FormatException($"值[{Encoding.UTF8.GetString(reader.ValueSpan)}]无法转换为bool");
            }
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }
}