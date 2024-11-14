using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Quick.Protocol.SerialPort
{
    [JsonSerializable(typeof(QpSerialPortClientOptions))]
    public partial class QpSerialPortClientOptionsSerializerContext : JsonSerializerContext { }

    public class QpSerialPortClientOptions : QpClientOptions
    {
        protected override JsonSerializerContext GetJsonSerializerContext() => QpSerialPortClientOptionsSerializerContext.Default;

        public const string URI_SCHEMA = "qp.serial";
        /// <summary>
        /// 端口名称
        /// </summary>
        [DisplayName("端口名称")]
        [Category("常用")]
        public string PortName { get; set; } = "COM1";
        /// <summary>
        /// 波特率
        /// </summary>
        [DisplayName("波特率")]
        [Category("常用")]
        public int BaudRate { get; set; } = 9600;
        /// <summary>
        /// 奇偶校验位
        /// </summary>
        [DisplayName("奇偶校验位")]
        [Category("常用")]
        public Parity Parity { get; set; } = Parity.None;
        /// <summary>
        /// 数据位
        /// </summary>
        [DisplayName("数据位")]
        [Category("常用")]
        public int DataBits { get; set; } = 8;
        /// <summary>
        /// 停止位
        /// </summary>
        [DisplayName("停止位")]
        [Category("常用")]
        public StopBits StopBits { get; set; } = StopBits.One;

        public override void Check()
        {
            base.Check();
            if (string.IsNullOrEmpty(PortName))
                throw new ArgumentNullException(nameof(PortName));
        }

        public override QpClient CreateClient()
        {
            return new QpSerialPortClient(this);
        }

        protected override void LoadFromQueryString(string key, string value)
        {
            switch (key)
            {
                case nameof(BaudRate):
                    BaudRate = int.Parse(value);
                    break;
                case nameof(Parity):
                    Parity = Enum.Parse<Parity>(value);
                    break;
                case nameof(DataBits):
                    DataBits = int.Parse(value);
                    break;
                case nameof(StopBits):
                    StopBits = Enum.Parse<StopBits>(value);
                    break;
                default:
                    base.LoadFromQueryString(key, value);
                    break;
            }
        }

        protected override void LoadFromUri(Uri uri)
        {
            PortName = uri.Host;
            base.LoadFromUri(uri);
        }

        protected override string ToUriBasic(HashSet<string> ignorePropertyNames)
        {
            ignorePropertyNames.Add(nameof(PortName));
            return $"{URI_SCHEMA}://{PortName}";            
        }

        public static void RegisterUriSchema()
        {
            RegisterUriSchema(URI_SCHEMA, () => new QpSerialPortClientOptions());
        }

        public override QpClientOptions Clone()
        {
            var json = JsonSerializer.Serialize(this, QpSerialPortClientOptionsSerializerContext.Default.QpSerialPortClientOptions);
            return JsonSerializer.Deserialize(json, QpSerialPortClientOptionsSerializerContext.Default.QpSerialPortClientOptions);
        }
    }
}
