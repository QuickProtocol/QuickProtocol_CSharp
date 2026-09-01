using System.Net;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quick.Protocol;
using Quick.Protocol.Tcp;
using Quick.Protocol.Pipeline;
using Quick.Protocol.Http.Client;
using Quick.Protocol.Http.Server.AspNetCore;
using Quick.Protocol.WebSocket.Client;
using Quick.Protocol.WebSocket.Server.AspNetCore;

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

    /// <summary>
    /// 等待 notice 回调设置 TCS，超时则记一次失败。返回回调捕获到的内容（超时返回 null）。
    /// </summary>
    static async Task<string> WaitForNotice(TaskCompletionSource<string> tcs, int timeoutMs, string testName)
    {
        var completed = await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs));
        if (completed == tcs.Task)
        {
            Assert(true, testName);
            return tcs.Task.Result;
        }
        Assert(false, $"{testName} (timeout)");
        return null;
    }

    static async Task<int> Main()
    {
        Console.WriteLine("=== Quick.Protocol Smoke Test ===\n");

        await TestTcpServerClient();
        await TestTcpReconnection();
        await TestTcpCompressAndEncrypt();
        await TestPipelineServerClient();
        await TestWebSocketServerClient();
        await TestHttpServerClient();
        await TestServerDispose();
        await TestClientDispose();

        Console.WriteLine($"\n=== Results: {passed} passed, {failed} failed ===");
        return failed;
    }

    static void RegisterTestHandlers(QpServerOptions serverOptions,
        Action<Quick.Protocol.Notices.PrivateNotice> onServerNoticeReceived = null)
    {
        var commandExecuterManager = new CommandExecuterManager();
        commandExecuterManager.Register<Quick.Protocol.Commands.PrivateCommand.Request, Quick.Protocol.Commands.PrivateCommand.Response>(
            async (handler, req) => new Quick.Protocol.Commands.PrivateCommand.Response { Content = req.Content });
        serverOptions.RegisterCommandExecuterManager(commandExecuterManager);
        var noticeHandlerManager = new NoticeHandlerManager();
        noticeHandlerManager.Register<Quick.Protocol.Notices.PrivateNotice>(async (handler, notice) =>
        {
            onServerNoticeReceived?.Invoke(notice);
        });
        serverOptions.RegisterNoticeHandlerManager(noticeHandlerManager);
    }

    static QpServer CreateAndStartServer(QpServerOptions serverOptions,
        Action<Quick.Protocol.Notices.PrivateNotice> onServerNoticeReceived = null)
    {
        RegisterTestHandlers(serverOptions, onServerNoticeReceived);
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

        var serverReceivedTcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var clientReceivedTcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var server = CreateAndStartServer(serverOptions, notice => serverReceivedTcs.TrySetResult(notice.Content));
        int channelConnectedCount = 0;
        int channelDisconnectedCount = 0;
        server.ChannelConnected += (_, channel) =>
        {
            // 连接建立后由服务端主动向该 client 回推一条 notice，
            // 闭合 server→client 方向的 bytes 直进直出路径断言
            _ = Task.Run(async () =>
            {
                try
                {
                    await channel.SendNoticePackage(new Quick.Protocol.Notices.PrivateNotice
                    {
                        Action = "down",
                        Content = "from-server"
                    });
                }
                catch { }
            });
        };
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
        // client 端注册 notice handler：接收 server 回推的 notice（recv bytes 路径）
        var clientNoticeManager = new NoticeHandlerManager();
        clientNoticeManager.Register<Quick.Protocol.Notices.PrivateNotice>(async (handler, notice) =>
        {
            clientReceivedTcs.TrySetResult(notice.Content);
        });
        clientOptions.RegisterNoticeHandlerManager(clientNoticeManager);
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

        // 双向 notice 往返断言（替换原先装饰性的 Assert(true)）
        // ① client → server：服务端 handler 捕获并断言内容
        await client.SendNoticePackage(new Quick.Protocol.Notices.PrivateNotice
        {
            Action = "up",
            Content = "from-client"
        });
        var serverGot = await WaitForNotice(serverReceivedTcs, 5000, "Notice round-trip (client→server)");
        Assert(serverGot == "from-client", "Notice payload client→server matched");

        // ② server → client：服务端 ChannelConnected 时已回推，这里等待并断言内容
        var clientGot = await WaitForNotice(clientReceivedTcs, 5000, "Notice round-trip (server→client)");
        Assert(clientGot == "from-server", "Notice payload server→client matched");

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

    /// <summary>
    /// 在测试宿主内起一个 Kestrel。HTTP / WebSocket 服务端是 ASP.NET Core 中间件，
    /// 不像 TCP/Pipeline 那样能用 options.CreateServer() 独立启动
    /// （其 CreateServer() 传的 urls 为 null，BindingPath 会崩）。
    /// </summary>
    static WebApplication BuildWebApp(int port)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls($"http://127.0.0.1:{port}");
        return builder.Build();
    }

    static async Task ShutdownWebApp(WebApplication app)
    {
        try
        {
            using var cts = new CancellationTokenSource(10000);
            await app.StopAsync(cts.Token);
        }
        catch { }
        try { await app.DisposeAsync(); } catch { }
    }

    /// <summary>
    /// 通用往返断言：连发 3 个负载（小 / 4KB / 200KB）。
    ///
    /// 「连发多次」是关键守卫，而非重复劳动：自定义 Stream 适配器若漏推进读取位置
    /// （例如 PipeReader 的 ReadAsync 未配对 AdvanceTo），第一次往返可能碰巧通过，
    /// 第二次必然抛异常或读到脏数据。200KB 那次同时跨越 Pipe 默认 64KB 背压阈值，
    /// 覆盖多段缓冲分支。
    /// </summary>
    static async Task AssertRoundTrips(string label, QpClient client, int timeoutMs = 30000)
    {
        var payloads = new[]
        {
            "hello-quick-protocol",
            BuildLargePayload(4096, seed: 11),
            BuildLargePayload(200 * 1024, seed: 12),
        };
        for (var i = 0; i < payloads.Length; i++)
        {
            var rep = await client.SendCommand(new Quick.Protocol.Commands.PrivateCommand.Request
            {
                Action = "echo",
                Content = payloads[i]
            }, timeoutMs);
            Assert(rep?.Content == payloads[i],
                $"[{label}] Round-trip #{i + 1} ({payloads[i].Length} chars)");
        }
    }

    /// <summary>
    /// WebSocket 传输层往返测试。
    ///
    /// 存在意义：WebSocketClientStream / WebSocketServerStream 是两个自定义 Stream 适配器，
    /// 负责 QpChannel 与 WebSocket API 之间的桥接。此前 SmokeTest 只覆盖 TCP/Pipeline，
    /// 这两个适配器（含改造后的 Memory/ReadOnlyMemory ValueTask 重载、
    /// 以及同步 Read/Write 改抛 NotSupportedException）完全没有运行时覆盖。
    /// </summary>
    static async Task TestWebSocketServerClient()
    {
        Console.WriteLine("[Test] WebSocket Server/Client round-trip (ASP.NET Core host)");
        var port = 3500 + Random.Shared.Next(100);
        const string path = "/qp_test";

        var serverOptions = new QpWebSocketServerOptions
        {
            Path = path,
            Password = "TestPass",
            ServerProgram = "SmokeTest"
        };
        RegisterTestHandlers(serverOptions);

        var app = BuildWebApp(port);
        app.UseWebSockets();
        app.UseQuickProtocolWebSocketServer(serverOptions, out var server);

        QpClient client = null;
        try
        {
            server.Start();
            await app.StartAsync();
            Assert(server.BindingPath.Contains(port.ToString()), "[ws] Server binding path resolved");

            var clientOptions = new QpWebSocketClientOptions
            {
                Url = $"qp.ws://127.0.0.1:{port}{path}",
                Password = "TestPass"
            };
            client = clientOptions.CreateClient();
            await client.ConnectAsync();
            Assert(client.IsConnected, "[ws] Client connected");

            await AssertRoundTrips("ws", client);

            client.Disconnect();
            Assert(!client.IsConnected, "[ws] Client disconnected");
        }
        catch (Exception ex)
        {
            Assert(false, $"[ws] Exception: {ex.GetType().Name}: {ex.Message}");
        }
        finally
        {
            client?.Dispose();
            server.Stop();
            await ShutdownWebApp(app);
        }
        Console.WriteLine();
    }

    /// <summary>
    /// HTTP 传输层往返测试（POST 上行 + GET 长轮循下行）。
    ///
    /// 覆盖 HttpClientsStream（客户端）与 PipesStream（服务端）两个适配器。
    /// 除常规往返外，额外守卫「长轮循重新订阅」路径：静置超过服务端长轮循超时后，
    /// 服务端会返回一次空响应结束当前 GET，客户端 beginRecv 循环必须正确重新发起 GET，
    /// 通道要能挺过空闲期继续收发。若该重订阅逻辑退化（循环 break、管道卡死等），
    /// 静置后的这次往返必然超时。
    ///
    /// 已实测确认（探针验证）：空闲返回的空响应**不会**在客户端 recvPipe 上产生一次空 flush
    /// —— PipeWriter.AsStream(leaveOpen: true) 在 Dispose 时不 flush，CopyToAsync 拷 0 字节
    /// 也不写入，故挂起的 ReadAsync 只是继续挂起。HttpClientsStream.ReadAsync 的
    /// 「缓冲为空 → 返回 0」分支实际只在拆连接时（Dispose 内 Writer.Complete，
    /// ReadResult.IsCompleted == true）触发一次，此后不再有 ReadAsync，
    /// 因此本用例**无法**守卫该分支的 AdvanceTo 配对（那处修复属契约正确性加固，非可触发缺陷）。
    /// </summary>
    static async Task TestHttpServerClient()
    {
        Console.WriteLine("[Test] HTTP Server/Client round-trip (ASP.NET Core host)");
        var port = 3600 + Random.Shared.Next(100);
        const string path = "/qp_test";
        const int longPollingTimeout = 2000;

        var serverOptions = new QpHttpServerOptions
        {
            Path = path,
            Password = "TestPass",
            ServerProgram = "SmokeTest",
            //缩短长轮循超时：既让空响应分支能在测试时限内触发，也避免收尾时挂起的 GET 拖慢宿主关闭
            LongPollingTimeout = longPollingTimeout
        };
        RegisterTestHandlers(serverOptions);

        var app = BuildWebApp(port);
        app.UseQuickProtocolHttpServer(serverOptions, out var server);

        QpClient client = null;
        try
        {
            server.Start();
            await app.StartAsync();
            Assert(server.BindingPath.Contains(port.ToString()), "[http] Server binding path resolved");

            var clientOptions = new QpHttpClientOptions
            {
                Url = $"qp.http://127.0.0.1:{port}{path}",
                Password = "TestPass",
                HttpClientTimeout = 30000
            };
            client = clientOptions.CreateClient();
            await client.ConnectAsync();
            Assert(client.IsConnected, "[http] Client connected");

            await AssertRoundTrips("http", client);

            //静置超过长轮循超时：服务端结束当前 GET 返回空响应，客户端须重新发起长轮循
            await Task.Delay(longPollingTimeout + 1000);
            const string afterIdlePayload = "after-long-polling-timeout";
            var rep = await client.SendCommand(new Quick.Protocol.Commands.PrivateCommand.Request
            {
                Action = "echo",
                Content = afterIdlePayload
            }, 30000);
            Assert(rep?.Content == afterIdlePayload,
                "[http] Round-trip after idle > long-polling timeout (GET re-subscribed)");

            client.Disconnect();
            Assert(!client.IsConnected, "[http] Client disconnected");
        }
        catch (Exception ex)
        {
            Assert(false, $"[http] Exception: {ex.GetType().Name}: {ex.Message}");
        }
        finally
        {
            client?.Dispose();
            server.Stop();
            await ShutdownWebApp(app);
        }
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
