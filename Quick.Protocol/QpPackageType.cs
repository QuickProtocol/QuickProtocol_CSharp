namespace Quick.Protocol
{
    /// <summary>
    /// 数据包类型
    /// </summary>
    public static class QpPackageType
    {
        /// <summary>
        /// 心跳数据包
        /// </summary>
        public const byte Heartbeat = 0;
        /// <summary>
        /// 通知数据包
        /// </summary>
        public const byte Notice = 1;
        /// <summary>
        /// 指令请求数据包
        /// </summary>
        public const byte CommandRequest = 2;
        /// <summary>
        /// 指令响应数据包
        /// </summary>
        public const byte CommandResponse = 3;
    }
}
