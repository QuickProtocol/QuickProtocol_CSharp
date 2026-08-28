using System.Threading;
using System.Threading.Tasks;

namespace Quick.Protocol.SerialPort
{
    public class QpSerialPortServer : QpServer
    {
        private QpSerialPortServerOptions options;
        private System.IO.Ports.SerialPort serialPort;
        private bool isAccepted = false;
        // 端口实例与生命周期（Open/Close/Dispose）统一加锁，避免断开回调、accept 循环、Stop() 跨线程竞争
        private readonly object portLock = new object();
        public override string BindingPath => $"{QpSerialPortClientOptions.URI_SCHEMA}://./{options.PortName}";

        public QpSerialPortServer(QpSerialPortServerOptions options) : base(options)
        {
            this.options = options;
        }

        public override void Start()
        {
            this.ChannelDisconnected += QpSerialPortServer_ChannelDisconnected;
            Options.Logger?.Log($"Opening SerialPort[{options.PortName}]...");
            lock (portLock)
            {
                serialPort = new System.IO.Ports.SerialPort(options.PortName,
                                                            options.BaudRate,
                                                            options.Parity,
                                                            options.DataBits,
                                                            options.StopBits);
                serialPort.Open();
            }
            Options.Logger?.Log($"SerialPort[{options.PortName}] open success.");
            isAccepted = false;
            base.Start();
        }

        public override void Stop()
        {
            base.Stop();
            // 端口的释放统一收口到这里（断开回调也可能已释放，这里做兜底，避免重复释放/空引用）
            lock (portLock)
            {
                if (serialPort != null)
                {
                    try { if (serialPort.IsOpen) serialPort.Close(); } catch { }
                    try { serialPort.Dispose(); } catch { }
                    serialPort = null;
                }
            }
            ChannelDisconnected -= QpSerialPortServer_ChannelDisconnected;
        }

        private void QpSerialPortServer_ChannelDisconnected(object sender, QpServerChannel e)
        {
            // 断开时复位标记，并释放当前连接独占的端口。
            // 关键点：System.IO.Ports.SerialPort.BaseStream 是缓存流，会在通道 Disconnect 时被 Dispose；
            // 必须先 Close 端口，下次 Open 才会得到全新的 BaseStream，否则重连会复用已释放的流而崩溃。
            // 端口操作统一加锁，避免与 Stop()/accept 循环跨线程竞争。
            lock (portLock)
            {
                isAccepted = false;
                if (serialPort != null)
                {
                    try { if (serialPort.IsOpen) serialPort.Close(); } catch { }
                    try { serialPort.Dispose(); } catch { }
                    serialPort = null;
                }
            }
        }

        protected override Task InnerAcceptAsync(CancellationToken token)
        {
            if (isAccepted)
                return Task.Delay(1000, token);
            isAccepted = true;
            return Task.Run(() =>
            {
                System.IO.Ports.SerialPort sp;
                lock (portLock)
                {
                    // 端口为 null（上一次连接已 Close 释放）时重新创建并打开，从而获得全新的 BaseStream；
                    // 已存在但未打开时同样重新打开。空引用/已释放情况均做兜底。
                    if (serialPort == null)
                    {
                        serialPort = new System.IO.Ports.SerialPort(options.PortName,
                                                                    options.BaudRate,
                                                                    options.Parity,
                                                                    options.DataBits,
                                                                    options.StopBits);
                        serialPort.Open();
                    }
                    else if (!serialPort.IsOpen)
                    {
                        serialPort.Open();
                    }
                    sp = serialPort;
                }
                OnNewChannelConnected(sp.BaseStream, $"{QpSerialPortClientOptions.URI_SCHEMA}://./{options.PortName}", token, false);
            }, token)
            .ContinueWith(task =>
            {
                // 打开/连接失败（端口被占用、拔出等）：复位标记并释放刚创建的端口实例，
                // 避免异常冒泡打断事件订阅者，也防止端口泄漏。
                if (task.IsCanceled || task.IsFaulted)
                {
                    isAccepted = false;
                    lock (portLock)
                    {
                        if (serialPort != null)
                        {
                            try { serialPort.Dispose(); } catch { }
                            serialPort = null;
                        }
                    }
                }
            });
        }
    }
}
