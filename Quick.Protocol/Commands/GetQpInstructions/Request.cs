using System.ComponentModel;

namespace Quick.Protocol.Commands.GetQpInstructions
{
    /// <summary>
    /// 获取全部指令集信息请求
    /// </summary>
    [DisplayName("获取全部指令集信息")]
    public class Request : IQpCommandRequest<Response>
    {
    }
}
