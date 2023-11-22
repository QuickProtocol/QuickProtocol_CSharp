using System.Text.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization.Metadata;

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

        private JsonTypeInfo requestTypeInfo;

        private JsonTypeInfo responseTypeInfo;

        public QpCommandInfo() { }
        public QpCommandInfo(string name, string description,
            JsonTypeInfo requestTypeInfo, JsonTypeInfo responseTypeInfo,
            object defaultRequestTypeInstance, object defaultResponseTypeInstance)
        {
            Name = name;
            Description = description;

            this.requestTypeInfo = requestTypeInfo;
            RequestTypeName = requestTypeInfo.Type.FullName;
            RequestTypeSchemaSample = JsonSerializer.Serialize(defaultRequestTypeInstance, requestTypeInfo);

            this.responseTypeInfo = responseTypeInfo;
            ResponseTypeName = responseTypeInfo.Type.FullName;
            ResponseTypeSchemaSample = JsonSerializer.Serialize(defaultResponseTypeInstance, responseTypeInfo);
        }

        /// <summary>
        /// 获取命令请求类型
        /// </summary>
        /// <returns></returns>
        public JsonTypeInfo GetRequestTypeInfo() => requestTypeInfo;
        /// <summary>
        /// 获取命令响应类型
        /// </summary>
        /// <returns></returns>
        public JsonTypeInfo GetResponseTypeInfo() => responseTypeInfo;

        /// <summary>
        /// 创建命令信息实例
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <returns></returns>
        public static QpCommandInfo Create<TRequest, TResponse>(JsonTypeInfo<TRequest> requestTypeInfo, JsonTypeInfo<TResponse> responseTypeInfo)
            where TRequest : IQpCommandRequest<TResponse>, new()
            where TResponse : class, new()
        {
            return Create(requestTypeInfo, responseTypeInfo, new TRequest(), new TResponse());
        }

        public static QpCommandInfo Create<TRequest, TResponse>(JsonTypeInfo<TRequest> requestTypeInfo, JsonTypeInfo<TResponse> responseTypeInfo, TRequest request, TResponse response)
            where TRequest : IQpCommandRequest<TResponse>, new()
            where TResponse : class, new()
        {
            var requestType = typeof(TRequest);
            string name = null;
            if (name == null)
                name = requestType.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
            if (name == null)
                name = requestType.FullName;
            return new QpCommandInfo(
                name, requestType.GetCustomAttribute<DescriptionAttribute>()?.Description,
                requestTypeInfo,
                responseTypeInfo,
                request,
                response);
        }
    }
}
