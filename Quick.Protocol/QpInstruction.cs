using System.Text.Json;
using System.Text.Json.Serialization;

namespace Quick.Protocol
{
    [JsonSerializable(typeof(QpInstruction))]
    public partial class QpInstructionSerializerContext : JsonSerializerContext { }

    /// <summary>
    /// QP指令集
    /// </summary>
    public class QpInstruction
    {
        /// <summary>
        /// 指令集编号
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 指令集名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 包含的通知信息数组
        /// </summary>
        public QpNoticeInfo[] NoticeInfos { get; set; }
        /// <summary>
        /// 包含的命令信息数组
        /// </summary>
        public QpCommandInfo[] CommandInfos { get; set; }
        public QpInstruction Clone()
        {
            var json = JsonSerializer.Serialize(this, QpInstructionSerializerContext.Default.QpInstruction);
            return JsonSerializer.Deserialize(json, QpInstructionSerializerContext.Default.QpInstruction);
        }
    }
}