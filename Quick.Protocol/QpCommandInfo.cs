using System.Text.Json;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Serialization;
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
        [JsonIgnore]
        [Browsable(false)]
        public JsonSerializerContext JsonSerializerContext { get; set; }

        private Type requestType;

        private Type responseType;

        public QpCommandInfo() { }
        public QpCommandInfo(string name, string description,
            Type requestType, Type responseType,
            object defaultRequestTypeInstance, object defaultResponseTypeInstance,
            JsonSerializerContext jsonSerializerContext)
        {
            Name = name;
            Description = description;
            JsonSerializerContext = jsonSerializerContext;

            this.requestType = requestType;
            RequestTypeName = requestType.FullName;
            RequestTypeSchemaSample = JsonNode.Parse(JsonSerializer.Serialize(defaultRequestTypeInstance, requestType, jsonSerializerContext)).ToString();

            this.responseType = responseType;
            ResponseTypeName = responseType.FullName;
            ResponseTypeSchemaSample = JsonNode.Parse(JsonSerializer.Serialize(defaultResponseTypeInstance, responseType, jsonSerializerContext)).ToString();
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
        /// <typeparam name="TResponse"></typeparam>
        /// <returns></returns>
        public static QpCommandInfo Create<TResponse>(IQpCommandRequest<TResponse> request,JsonSerializerContext jsonSerializerContext)
            where TResponse : class, new()
        {
            return Create(request, new TResponse(), jsonSerializerContext);
        }

        public static QpCommandInfo Create<TResponse>(IQpCommandRequest<TResponse> request, TResponse response, JsonSerializerContext jsonSerializerContext)
            where TResponse : class, new()
        {
            var requestType = request.GetType();
            var responseType = typeof(TResponse);
            string name = null;
            if (name == null)
                name = requestType.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
            if (name == null)
                name = requestType.FullName;
            return new QpCommandInfo(name, requestType.GetCustomAttribute<DescriptionAttribute>()?.Description, requestType, responseType, request, response, jsonSerializerContext);
        }
    }
}
