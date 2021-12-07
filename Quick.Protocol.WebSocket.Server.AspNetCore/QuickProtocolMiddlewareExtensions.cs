using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Quick.Protocol.WebSocket.Server.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;

namespace Microsoft.AspNetCore.Builder
{
    public static class QuickProtocolMiddlewareExtensions
    {
        public static IApplicationBuilder UseQuickProtocol(this IApplicationBuilder app, QpWebSocketServerOptions options, out QpWebSocketServer server)
        {
            var innerServer = new QpWebSocketServer(options);
            server = innerServer;
            app.Use((async (context, next) =>
            {
                if (context.Request.Path == options.Path)
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await innerServer.OnNewConnection(webSocket, context.Connection);
                        return;
                    }
                    else
                    {
                        var message = $@"
<html>
    <head>
        <title>Quick.Protocol</title>
    </head>
    <body>
        <p>Welcome to use <b>Quick.Protocol</b>.</p>
        <p>Source Code:<a href=""http://github.com/aaasoft/Quick.Protocol"">http://github.com/aaasoft/Quick.Protocol</a></p>
        <p>ServerProgram:{string.Join(" | ", options.InstructionSet.Select(t => $"{t.Name}({t.Id})"))}</p>
        <p>InstructionSet:{DateTime.Now}</p>
        <p>MaxPackageSize:{options.MaxPackageSize}</p>
        <p>BufferSize:{options.BufferSize}</p>
        <p>BufferSize:{options.BufferSize}</p>
        <p>HeartBeatInterval:{options.HeartBeatInterval}</p>
        <p>Time:{DateTime.Now}</p>
    </body>
</html>";
                        var rep = context.Response;
                        rep.ContentType = "text/html; charset=utf-8";
                        rep.ContentLength = Encoding.UTF8.GetByteCount(message);
                        await context.Response.WriteAsync(message, Encoding.UTF8);
                    }
                }
                else
                {
                    await next();
                }
            }));
            return app;
        }
    }
}