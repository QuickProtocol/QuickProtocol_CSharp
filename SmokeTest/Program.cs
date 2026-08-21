using System.Net;
using System.Diagnostics;
using Quick.Protocol;
using Quick.Protocol.Tcp;
using Quick.Protocol.Pipeline;

class Program
{
    static int passed = 0;
    static int failed = 0;

    static void Assert(bool condition, string testName)
    {
        if (condition)
        {
            Console.WriteLine($"  [PASS] {testName}");
            passed++;
        }
        else
        {
            Console.WriteLine($"  [FAIL] {testName}");
            failed++;
        }
    }

    static async Task<int> Main()
    {
        Console.WriteLine("=== Quick.Protocol Smoke Test ===\n");

        await TestTcpServerClient();
        await TestTcpReconnection();
        await TestPipelineServerClient();
        await TestServerDispose();
        await TestClientDispose();

        Console.WriteLine($"\n=== Results: {passed} passed, {failed} failed ===");
        return failed;
    }

    static void RegisterTestHandlers(QpServerOptions serverOptions)
    {
        var commandExecuterManager = new CommandExecuterManager();
        commandExecuterManager.Register<Quick.Protocol.Commands.PrivateCommand.Request, Quick.Protocol.Commands.PrivateCommand.Response>(
            async (handler, req) => new Quick.Protocol.Commands.PrivateCommand.Response { Content = req.Content });
        serverOptions.RegisterCommandExecuterManager(commandExecuterManager);
        var noticeHandlerManager = new NoticeHandlerManager();
        noticeHandlerManager.Register<Quick.Protocol.Notices.PrivateNotice>((handler, notice) => { });
        serverOptions.RegisterNoticeHandlerManager(noticeHandlerManager);
    }

    static QpServer CreateAndStartServer(QpServerOptions serverOptions)
    {
        RegisterTestHandlers(serverOptions);
        var server = serverOptions.CreateServer();
        server.Start();
        return server;
    }

    static async Task TestTcpServerClient()
    {
        Console.WriteLine("[Test] TCP Server/Client basic communication");
        var port = 3100 + Random.Shared.Next(100);
        var serverOptions = new QpTcpServerOptions
        {
            Address = IPAddress.Loopback.ToString(),
            Port = port,
            Password = "TestPass",
            ServerProgram = "SmokeTest"
        };

        var server = CreateAndStartServer(serverOptions);
        int channelConnectedCount = 0;
        int channelDisconnectedCount = 0;
        server.ChannelConnected += (_, _) => Interlocked.Increment(ref channelConnectedCount);
        server.ChannelDisconnected += (_, _) => Interlocked.Increment(ref channelDisconnectedCount);

        Assert(server.BindingPath.Contains(port.ToString()), "Server started with correct binding");
        await Task.Delay(500);

        var clientOptions = new QpTcpClientOptions
        {
            Host = "127.0.0.1",
            Port = port,
            Password = "TestPass"
        };
        var client = clientOptions.CreateClient();
        int disconnectedCount = 0;
        client.Disconnected += (_, _) => Interlocked.Increment(ref disconnectedCount);

        try
        {
            await client.ConnectAsync();
        }
        catch (Exception ex)
        {
            Assert(false, $"Client connect failed: {ex.Message}");
            client.Dispose();
            server.Stop();
            return;
        }
        Assert(client.IsConnected, "Client connected successfully");

        // Wait for server to register the channel
        await Task.Delay(500);
        Assert(channelConnectedCount == 1, "Server received channel connected event");

        // Send a notice
        await client.SendNoticePackage(new Quick.Protocol.Notices.PrivateNotice
        {
            Action = "test",
            Content = "hello"
        });
        Assert(true, "Notice sent successfully");

        // Disconnect
        client.Disconnect();
        await Task.Delay(200);
        Assert(!client.IsConnected, "Client disconnected");
        Assert(disconnectedCount == 1, "Client disconnected event fired");

        // Cleanup
        client.Dispose();
        server.Stop();
        await Task.Delay(100);
        Assert(channelDisconnectedCount == 1, "Server received channel disconnected event");

        Console.WriteLine();
    }

    static async Task TestTcpReconnection()
    {
        Console.WriteLine("[Test] TCP Client reconnection");
        var port = 3200 + Random.Shared.Next(100);
        var serverOptions = new QpTcpServerOptions
        {
            Address = IPAddress.Loopback.ToString(),
            Port = port,
            Password = "TestPass",
            ServerProgram = "SmokeTest"
        };
        var server = CreateAndStartServer(serverOptions);
        await Task.Delay(500);

        var clientOptions = new QpTcpClientOptions
        {
            Host = "127.0.0.1",
            Port = port,
            Password = "TestPass"
        };
        var client = clientOptions.CreateClient();

        // Connect first time
        try
        {
            await client.ConnectAsync();
        }
        catch (Exception ex)
        {
            Assert(false, $"First connection failed: {ex.Message}");
            client.Dispose();
            server.Stop();
            return;
        }
        Assert(client.IsConnected, "First connection succeeded");

        client.Disconnect();
        await Task.Delay(200);
        Assert(!client.IsConnected, "Disconnected after first connection");

        // Reconnect
        try
        {
            await client.ConnectAsync();
        }
        catch (Exception ex)
        {
            Assert(false, $"Reconnection failed: {ex.Message}");
            client.Dispose();
            server.Stop();
            return;
        }
        Assert(client.IsConnected, "Reconnection succeeded");

        client.Dispose();
        server.Stop();
        Console.WriteLine();
    }

    static async Task TestPipelineServerClient()
    {
        Console.WriteLine("[Test] Pipeline Server/Client basic communication");
        var pipeName = $"SmokeTest_{Guid.NewGuid():N}";
        var serverOptions = new QpPipelineServerOptions
        {
            PipeName = pipeName,
            Password = "TestPass",
            ServerProgram = "SmokeTest"
        };
        var server = CreateAndStartServer(serverOptions);
        await Task.Delay(500);

        var clientOptions = new QpPipelineClientOptions
        {
            ServerName = ".",
            PipeName = pipeName,
            Password = "TestPass"
        };
        var client = clientOptions.CreateClient();
        try
        {
            await client.ConnectAsync();
        }
        catch (Exception ex)
        {
            Assert(false, $"Pipeline client connect failed: {ex.Message}");
            client.Dispose();
            server.Stop();
            return;
        }
        Assert(client.IsConnected, "Pipeline client connected");

        client.Disconnect();
        Assert(!client.IsConnected, "Pipeline client disconnected");

        client.Dispose();
        server.Stop();
        Console.WriteLine();
    }

    static async Task TestServerDispose()
    {
        Console.WriteLine("[Test] Server Dispose (IDisposable)");
        var port = 3300 + new Random().Next(100);
        var serverOptions = new QpTcpServerOptions
        {
            Address = IPAddress.Loopback.ToString(),
            Port = port,
            Password = "TestPass",
            ServerProgram = "SmokeTest"
        };
        RegisterTestHandlers(serverOptions);

        // Test using pattern
        using (var server = serverOptions.CreateServer())
        {
            server.Start();
            Assert(true, "Server started inside using block");
        }
        Assert(true, "Server disposed without error via using block");

        // Test that port is released
        var serverOptions2 = new QpTcpServerOptions
        {
            Address = IPAddress.Loopback.ToString(),
            Port = port,
            Password = "TestPass",
            ServerProgram = "SmokeTest"
        };
        RegisterTestHandlers(serverOptions2);
        using (var server2 = serverOptions2.CreateServer())
        {
            server2.Start();
            Assert(true, "Port released after Dispose, new server started");
        }
        Console.WriteLine();
    }

    static async Task TestClientDispose()
    {
        Console.WriteLine("[Test] Client Dispose without connect");
        var clientOptions = new QpTcpClientOptions
        {
            Host = "127.0.0.1",
            Port = 9999,
            Password = "TestPass"
        };
        var client = clientOptions.CreateClient();
        client.Dispose();
        Assert(true, "Client disposed without connect (no error)");

        // Test double dispose
        client.Dispose();
        Assert(true, "Client double-dispose is safe");

        Console.WriteLine();
    }
}
