using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quick.Protocol;

namespace TestProgram.Units;

public class WebSocketServerUnit
{
    public static void Invoke()
    {
        CreateHostBuilder([]).Build().Run();
    }

    public class QpTestContext
    {
        private Quick.Protocol.WebSocket.Server.AspNetCore.QpWebSocketServer server;

        public QpTestContext(IApplicationBuilder app)
        {
            app.UseQuickProtocol(new Quick.Protocol.WebSocket.Server.AspNetCore.QpWebSocketServerOptions()
            {
                Path = "/qp_test",
                Password = "HelloQP",
                ServerProgram = nameof(WebSocketServerUnit) + " 1.0"
            }, out server);

            server.ChannelConnected += Server_ChannelConnected;
            server.ChannelDisconnected += Server_ChannelDisconnected;
            server.Start();
        }

        private static void Server_ChannelConnected(object sender, QpServerChannel e)
        {
            Console.WriteLine($"{DateTime.Now:T}: Channel[{e.ChannelName}] connected.");
        }


        private static void Server_ChannelDisconnected(object sender, QpServerChannel e)
        {
            Console.WriteLine($"{DateTime.Now:T}: Channel[{e.ChannelName}] disconnected.");
        }
    }

    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseWebSockets();
            new QpTestContext(app);
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://127.0.0.1:3011");
                    webBuilder.UseStartup<Startup>();
                });
}