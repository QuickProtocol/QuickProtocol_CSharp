using System;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Nodes;

namespace Quick.Protocol
{
    /// <summary>
    /// 命令信息
    /// </summary>
    public class QpCommandInfo
    {
        /// <summary>
        /// 名称
        /// </summary>
        [DisplayName("名称")]
        [ReadOnly(true)]
        public string Name { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        [DisplayName("描述")]
        [ReadOnly(true)]
        public string Description { get; set; }
        /// <summary>
        /// 命令请求类型名称
        /// </summary>
        [DisplayName("请求类型")]
        [ReadOnly(true)]
        public string RequestTypeName { get; set; }
        /// <summary>
        /// 请求示例
        /// </summary>
        [DisplayName("请求示例")]
        [ReadOnly(true)]
        public string RequestTypeSchemaSample { get; set; }
        /// <summary>
        /// 命令响应类型名称
        /// </summary>
        [DisplayName("响应类型")]
        [ReadOnly(true)]
        public string ResponseTypeName { get; set; }
        /// <summary>
        /// 响应示例
        /// </summary>
        [DisplayName("响应示例")]
        [ReadOnly(true)]
        public string ResponseTypeSchemaSample { get; set; }
        private readonly IQpSerializer requestSerializer;
        private readonly IQpSerializer responseSerializer;
        public IQpSerializer GetRequestSeriliazer() => requestSerializer;
        public IQpSerializer GetResponseSeriliazer() => responseSerializer;

        private readonly Type requestType;

        private readonly Type responseType;

        public QpCommandInfo() { }
        public QpCommandInfo(string name, string description,
            Type requestType, Type responseType,
            object defaultRequestTypeInstance, object defaultResponseTypeInstance,
            IQpSerializer requestSerializer, IQpSerializer responseSerializer)
        {
            Name = name;
            Description = description;
            this.requestSerializer = requestSerializer;
            this.responseSerializer = responseSerializer;

            this.requestType = requestType;
            RequestTypeName = requestType.FullName;
            RequestTypeSchemaSample = JsonNode.Parse(requestSerializer.Serialize(defaultRequestTypeInstance)).ToString();

            this.responseType = responseType;
            ResponseTypeName = responseType.FullName;
            ResponseTypeSchemaSample = JsonNode.Parse(responseSerializer.Serialize(defaultResponseTypeInstance)).ToString();
        }

        /// <summary>
        /// 获取命令请求类型
        /// </summary>
        /// <returns></returns>
        public Type GetRequestType() => requestType;
        /// <summary>
        /// 获取命令响应类型
        /// </summary>
        /// <returns></returns>
        public Type GetResponseType() => responseType;

        /// <summary>
        /// 创建命令信息实例
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public static QpCommandInfo Create<TRequest, TResponse>(IQpCommandRequest<TRequest, TResponse> request)
            where TRequest : IQpModel<TRequest>, IQpCommandRequest<TRequest, TResponse>, new()
            where TResponse : IQpModel<TResponse>, new()
        {
            return Create((TRequest)request, new TResponse());
        }

        public static QpCommandInfo Create<TRequest, TResponse>(TRequest request, TResponse response)
            where TRequest : IQpModel<TRequest>, IQpCommandRequest<TRequest, TResponse>, new()
            where TResponse : IQpModel<TResponse>, new()
        {
            var requestType = typeof(TRequest);
            var responseType = typeof(TResponse);
            string name = null;
            if (name == null)
                name = requestType.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
            if (name == null)
                name = requestType.FullName;
            return new QpCommandInfo(name, requestType.GetCustomAttribute<DescriptionAttribute>()?.Description,
                requestType, responseType,
                request, response,
                request, response);
        }
    }
}
