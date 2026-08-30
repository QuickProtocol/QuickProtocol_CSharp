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
        await TestTcpCompressAndEncrypt();
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
        noticeHandlerManager.Register<Quick.Protocol.Notices.PrivateNotice>(async (handler, notice) => { });
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
            client.Disconnect();
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

    /// <summary>
    /// 大负载往返测试：覆盖「明文/仅压缩/仅加密/压缩+加密」四种组合。
    /// 负载取 200KB，同时是两处历史缺陷的回归守卫：
    /// 1) 包体达到 Pipe 默认背压阈值 64KB 时，await FlushAsync() 会先阻塞、
    ///    而解除背压的 ReadAtLeastAsync 排在其后 —— 顺序颠倒即死锁，通道永久挂死；
    /// 2) 每种组合连续发送两个不同负载，第二个用于验证管道读取位置被正确推进：
    ///    若 AdvanceTo 被跳过，管道残留上次字节，后续包会读到脏数据，往返内容必然不一致。
    /// </summary>
    static async Task TestTcpCompressAndEncrypt()
    {
        Console.WriteLine("[Test] TCP large-payload round-trip (200KB)");

        const int payloadLength = 200 * 1024;

        var cases = new (bool Compress, bool Encrypt, string Label)[]
        {
            (false, false, "plain"),
            (true, false, "compress"),
            (false, true, "encrypt"),
            (true, true, "compress+encrypt"),
        };

        foreach (var c in cases)
            await RunCompressEncryptRoundTrip(c.Label, c.Compress, c.Encrypt, payloadLength, 15000);

        Console.WriteLine();
    }

    /// <summary>
    /// 构造高熵大负载：压缩后仍然足够大，可跨越多个 Pipe 内存段，
    /// 从而覆盖 writePackageBuffer 中「逐段写出」的多段分支。
    /// </summary>
    static string BuildLargePayload(int length, int seed)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random(seed);
        var chars = new char[length];
        for (var i = 0; i < length; i++)
            chars[i] = alphabet[random.Next(alphabet.Length)];
        return new string(chars);
    }

    static async Task RunCompressEncryptRoundTrip(string name, bool enableCompress, bool enableEncrypt,
        int payloadLength, int timeoutMs)
    {
        var port = 3400 + Random.Shared.Next(100);
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
            Password = "TestPass",
            EnableCompress = enableCompress,
            EnableEncrypt = enableEncrypt
        };
        var client = clientOptions.CreateClient();
        try
        {
            await client.ConnectAsync();
            Assert(client.IsConnected, $"[{name}] Client connected");

            // 等待服务端完成通道注册
            await Task.Delay(500);

            var payload1 = BuildLargePayload(payloadLength, seed: 1);
            var payload2 = BuildLargePayload(Math.Max(1024, payloadLength / 2), seed: 2);

            var rep1 = await client.SendCommand(new Quick.Protocol.Commands.PrivateCommand.Request
            {
                Action = "echo",
                Content = payload1
            }, timeoutMs);
            Assert(rep1?.Content == payload1, $"[{name}] Payload round-trip");

            // 再发一次：验证管道读取位置已推进，未残留上一次的字节
            var rep2 = await client.SendCommand(new Quick.Protocol.Commands.PrivateCommand.Request
            {
                Action = "echo",
                Content = payload2
            }, timeoutMs);
            Assert(rep2?.Content == payload2, $"[{name}] Second round-trip (pipes advanced)");
        }
        catch (Exception ex)
        {
            Assert(false, $"[{name}] Exception: {ex.GetType().Name}: {ex.Message}");
        }
        finally
        {
            client.Dispose();
            server.Stop();
            await Task.Delay(100);
        }
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
