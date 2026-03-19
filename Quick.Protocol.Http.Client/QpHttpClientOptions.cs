using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Quick.Protocol.Http.Client;

[JsonSerializable(typeof(QpHttpClientOptions))]
public partial class QpHttpClientOptionsSerializerContext : JsonSerializerContext
{
    public static QpHttpClientOptionsSerializerContext Default2 { get; } = new QpHttpClientOptionsSerializerContext(new JsonSerializerOptions()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });
}

public class QpHttpClientOptions : QpClientOptions
{
    protected override JsonSerializerContext GetJsonSerializerContext() => QpHttpClientOptionsSerializerContext.Default2;

    public const string URI_SCHEMA_HTTP = "qp.http";
    public const string URI_SCHEMA_HTTPS = "qp.https";

    /// <summary>
    /// Http的URL地址
    /// </summary>
    public string Url { get; set; } = "qp.http://127.0.0.1:3011/qp_test";
    /// <summary>
    /// HTTP客户端超时时间
    /// </summary>
    public int HttpClientTimeout { get; set; } = 100 * 1000;

    public override void Check()
    {
        base.Check();
        if (Url == null)
            throw new ArgumentNullException(nameof(Url));
        if (!Url.StartsWith(URI_SCHEMA_HTTP + "://") && !Url.StartsWith(URI_SCHEMA_HTTPS + "://"))
            throw new ArgumentException($"Url must start with {URI_SCHEMA_HTTP}:// or {URI_SCHEMA_HTTPS}://", nameof(Url));
    }
    
    public override QpClient CreateClient()
    {
        return new QpHttpClient(this);
    }

    protected override void LoadFromUri(Uri uri)
    {
        Url = uri.ToString();
        base.LoadFromUri(uri);
    }

    protected override string ToUriBasic(HashSet<string> ignorePropertyNames)
    {
        ignorePropertyNames.Add(nameof(Url));
        return Url;
    }

    public static void RegisterUriSchema()
    {
        RegisterUriSchema(URI_SCHEMA_HTTP, () => new QpHttpClientOptions());
        RegisterUriSchema(URI_SCHEMA_HTTPS, () => new QpHttpClientOptions());
    }

    public override QpClientOptions Clone()
    {
        var json = JsonSerializer.Serialize(this, QpHttpClientOptionsSerializerContext.Default.QpHttpClientOptions);
        return JsonSerializer.Deserialize(json, QpHttpClientOptionsSerializerContext.Default.QpHttpClientOptions);
    }

    public override void Serialize(Stream stream)
    {
        JsonSerializer.Serialize(stream, this, QpHttpClientOptionsSerializerContext.Default.QpHttpClientOptions);
    }
}
