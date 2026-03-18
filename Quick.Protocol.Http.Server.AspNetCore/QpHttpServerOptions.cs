using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Quick.Protocol.Http.Server.AspNetCore
{
    [JsonSerializable(typeof(QpHttpServerOptions))]
    public partial class QpHttpServerOptionsSerializerContext : JsonSerializerContext
    {
        public static QpHttpServerOptionsSerializerContext Default2 { get; } = new QpHttpServerOptionsSerializerContext(new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    public class QpHttpServerOptions : QpServerOptions
    {
        protected override JsonSerializerContext GetJsonSerializerContext() => QpHttpServerOptionsSerializerContext.Default2;

        private string _Path;
        /// <summary>
        /// Http的路径
        /// </summary>
        public string Path
        {
            get { return _Path; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _Path = "/";
                    return;
                }
                if (value.StartsWith("/"))
                {
                    _Path = value;
                    return;
                }
                _Path = "/" + value;
            }
        }

        public override QpServer CreateServer()
        {
            return new QpHttpServer(this);
        }
    }
}