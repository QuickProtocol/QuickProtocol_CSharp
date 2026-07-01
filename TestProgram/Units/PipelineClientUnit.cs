namespace TestProgram.Units;

public class PipelineClientUnit
{
    public static void Invoke()
    {
        var client = new Quick.Protocol.Pipeline.QpPipelineClient(new Quick.Protocol.Pipeline.QpPipelineClientOptions()
            {
                PipeName = "Quick.Protocol",
                Password = "HelloQP",
                EnableCompress = true,
                EnableEncrypt = true
            });

            client.Disconnected += (sender, e) =>
            {
                Console.WriteLine("Disconnected");
            };
            client.ConnectAsync().ContinueWith(async t =>
            {
                if (t.IsCanceled)
                {
                    Console.WriteLine("Connect cancelled");
                    return;
                }
                if (t.IsFaulted)
                {
                    Console.WriteLine("Connect error," + t.Exception.InnerException.ToString());
                    return;
                }
                Console.WriteLine("Connected");

                try
                {
                    var rep = await client.SendCommand(
                        new Quick.Protocol.Commands.PrivateCommand.Request()
                        {
                            Action = "Echo",
                            Content = DateTime.Now.ToString()
                        });
                    Console.WriteLine("SendCommand Success.Rep:" + rep.Serialize(rep));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SendCommand Error:" + ex.ToString());
                }

                try
                {
                    await client.SendNoticePackage(new Quick.Protocol.Notices.PrivateNotice() { Content = "Hello Quick.Protocol V2!" });
                    //await client.SendNoticePackage(new Quick.Protocol.Notices.PrivateNotice() { Content = "".PadRight(5 * 1024, '0') });
                    Console.WriteLine("SendNotice Success.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SendNotice Error:" + ex.ToString());
                }
            });
            Console.ReadLine();
    }
}