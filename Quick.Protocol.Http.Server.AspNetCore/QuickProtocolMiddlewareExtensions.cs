using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quick.Protocol.Http.Server.AspNetCore;

namespace Microsoft.AspNetCore.Builder
{
    public static class QuickProtocolMiddlewareExtensions
    {
        public static IApplicationBuilder UseQuickProtocolHttpServer(this IApplicationBuilder app, QpHttpServerOptions options, out QpHttpServer server)
        {
            var config = app.ApplicationServices.GetRequiredService<IConfiguration>();
            var urlsStr = config["Urls"];
            var urls = urlsStr?.Split(';', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

            var innerServer = new QpHttpServer(options, urls);
            server = innerServer;
            app.Use(async (context, next) =>
            {
                if (context.Request.Path == options.Path)
                {
                    await innerServer.HandleRequest(context, next);
                }
                else
                {
                    await next().ConfigureAwait(false);
                }
            });
            return app;
        }
    }
}