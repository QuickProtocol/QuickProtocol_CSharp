using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Quick.Protocol;

namespace TestProgram.Units;

public class HttpServerUnit : AbstractServerUnit
{
    public override string Name => "HTTP server";

    private Quick.Protocol.Http.Server.AspNetCore.QpHttpServerOptions GetHttpServerOptions() => new Quick.Protocol.Http.Server.AspNetCore.QpHttpServerOptions()
    {
        Path = "/qp_test",
        Password = "HelloQP",
        ServerProgram = nameof(HttpServerUnit) + " 1.0"
    };
    protected override QpServerOptions GetServerOptions() => GetHttpServerOptions();

    public override void Invoke()
    {
        var urls = new string[] { "http://127.0.0.1:3011" };
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost
            .UseUrls(urls)
            .ConfigureKestrel(options => options.AddServerHeader = false);
        var app = builder.Build();
        app.UseRouting();
        app.UseWebSockets();
        app.UseQuickProtocolHttpServer(GetHttpServerOptions(), out var server);
        StartServer(server);
        app.MapGet("/", async context =>
        {
            await context.Response.WriteAsync("Hello World!");
        });
        app.Run();
        server.Stop();
    }
}