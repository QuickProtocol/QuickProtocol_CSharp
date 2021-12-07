using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Quick.Protocol
{
    /// <summary>
    /// QP指令集
    /// </summary>
    public class QpInstruction
    {
        /// <summary>
        /// 指令集编号
        /// </summary>
        [DisplayName("编号")]
        [ReadOnly(true)]
        public string Id { get; set; }
        /// <summary>
        /// 指令集名称
        /// </summary>
        [DisplayName("名称")]
        [ReadOnly(true)]
        public string Name { get; set; }
        /// <summary>
        /// 包含的通知信息数组
        /// </summary>
        [Browsable(false)]
        public QpNoticeInfo[] NoticeInfos { get; set; }
        /// <summary>
        /// 包含的命令信息数组
        /// </summary>
        [Browsable(false)]
        public QpCommandInfo[] CommandInfos { get; set; }
    }
}
/*
        数字采用大端字节序
        包长度包含4个字节的包长度数据。
        命令编号为16字节GUID

        0->心跳数据包结构：
        [4字节] [1字节]
        包长度  包类型

        1->通知数据包结构：
        [4字节] [1字节] [1字节]  [n字节]     [n字节]
        包长度  包类型   类名长度 类名(A.B.C) 类内容(JSON)
                        |-------包内容----------|

        2->命令请求结构：
        [4字节] [1字节] [16字节] [1字节]  [n字节]     [n字节]
        包长度  包类型   命令编号 类名长度 类名(A.B.C) 类内容(JSON)
                        |-------包内容----------------|

        3->命令响应结构：
        [4字节] [1字节] [16字节] [1字节]  
        包长度  包类型   命令编号 返回码(0代表成功，其他代表失败)
                                    成功：[1字节]  [n字节]     [n字节]
                                         类名长度 类名(A.B.C) 类内容(JSON)
                                    失败：[n字节]    
                                         错误消息(字符串)
                        |-------包内容----------------------------|

        255->拆分数据包结构：
        [4字节] [1字节] [4字节|0字节]                           [n字节]
        包长度  包类型   原始包长度，第一个拆分包才有4个字节的长度   类内容(Byte Array)
                        |-------包内容----------------------------|

        加密、压缩、拆分
        WcfTestClient -> QpTestClient，获取功能集列表，功能集里面的指令、通知包，测试指令调用，生成dll、jar文件。

连接流程：
客户端     服务端
连接命令请求->
<-连接命令响应
认证命令请求->
<-认证命令响应

 */