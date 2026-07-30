using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quick.Protocol;

namespace TestProgram.Units;

public class WebSocketServerUnit:AbstractServerUnit
{
    public override string Name => "WebSocket server";

    private Quick.Protocol.WebSocket.Server.AspNetCore.QpWebSocketServerOptions GetWebSocketServerOptions() => new Quick.Protocol.WebSocket.Server.AspNetCore.QpWebSocketServerOptions()
    {
        Path = "/qp_test",
        Password = "HelloQP",
        ServerProgram = nameof(WebSocketServerUnit) + " 1.0"
    };

    protected override QpServerOptions GetServerOptions() => GetWebSocketServerOptions();


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
        app.UseQuickProtocolWebSocketServer(GetWebSocketServerOptions(), out var server);
        StartServer(server);
        app.MapGet("/", async context =>
        {
            await context.Response.WriteAsync("Hello World!");
        });
        app.Run();
        server.Stop();
    }
}