using Newtonsoft.Json;
using NJsonSchema.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

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
        /// 请求定义
        /// </summary>
        /// <returns></returns>
        [DisplayName("请求定义")]
        [ReadOnly(true)]
        public string RequestTypeSchema { get; set; }
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
        /// 响应定义
        /// </summary>
        /// <returns></returns>
        [DisplayName("响应定义")]
        [ReadOnly(true)]
        public string ResponseTypeSchema { get; set; }
        /// <summary>
        /// 响应示例
        /// </summary>
        [DisplayName("响应示例")]
        [ReadOnly(true)]
        public string ResponseTypeSchemaSample { get; set; }

        private Type requestType;

        private Type responseType;

        public QpCommandInfo() { }
        public QpCommandInfo(string name, string description, Type requestType, Type responseType)
        {
            Name = name;
            Description = description;

            this.requestType = requestType;
            RequestTypeName = requestType.FullName;
            var requestTypeSchema = NJsonSchema.JsonSchema.FromType(requestType);
            RequestTypeSchema = requestTypeSchema.ToJson(Formatting.Indented);
            RequestTypeSchemaSample = JsonConvert.SerializeObject(requestTypeSchema.ToSampleJson(), Formatting.Indented);

            this.responseType = responseType;
            ResponseTypeName = responseType.FullName;
            var responseTypeSchema = NJsonSchema.JsonSchema.FromType(responseType);
            ResponseTypeSchema = responseTypeSchema.ToJson(Formatting.Indented);
            ResponseTypeSchemaSample = JsonConvert.SerializeObject(responseTypeSchema.ToSampleJson(), Formatting.Indented);
        }

        /// <summary>
        /// 获取命令请求类型
        /// </summary>
        /// <returns></returns>
        public Type GetRequestType() => requestType ?? Type.GetType(RequestTypeName);
        /// <summary>
        /// 获取命令响应类型
        /// </summary>
        /// <returns></returns>
        public Type GetResponseType() => responseType ?? Type.GetType(ResponseTypeName);

        /// <summary>
        /// 创建命令信息实例
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <returns></returns>
        public static QpCommandInfo Create<TResponse>(IQpCommandRequest<TResponse> request)
            where TResponse : class, new()
        {
            var requestType = request.GetType();
            string name = null;
            if (name == null)
                name = requestType.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
            if (name == null)
                name = requestType.GetCustomAttribute<JsonSchemaAttribute>()?.Name;
            if (name == null)
                name = requestType.FullName;
            return new QpCommandInfo(name, requestType.GetCustomAttribute<DescriptionAttribute>()?.Description, requestType, typeof(TResponse));
        }
    }
}
