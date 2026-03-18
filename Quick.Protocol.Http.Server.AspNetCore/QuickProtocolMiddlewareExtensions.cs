using Microsoft.AspNetCore.Http;
using Quick.Protocol;
using Quick.Protocol.Http.Server.AspNetCore;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;

namespace Microsoft.AspNetCore.Builder
{
    public static class QuickProtocolMiddlewareExtensions
    {
        public static IApplicationBuilder UseQuickProtocolHttp(this IApplicationBuilder app, QpHttpServerOptions options, out QpHttpServer server)
        {
            var innerServer = new QpHttpServer(options);
            server = innerServer;
            app.Use((async (context, next) =>
            {
                if (context.Request.Path == options.Path)
                {
                    await innerServer.HandleRequest(context, next);
                }
                else
                {
                    await next().ConfigureAwait(false);
                }
            }));
            return app;
        }
    }
}