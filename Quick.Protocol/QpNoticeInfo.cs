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
    /// 通知信息
    /// </summary>
    public class QpNoticeInfo
    {
        private Type noticeType;

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
        /// 通知类型名称
        /// </summary>
        [DisplayName("类型")]
        [ReadOnly(true)]
        public string NoticeTypeName { get; set; }
        
        public QpNoticeInfo() { }
        public QpNoticeInfo(string name, string description, Type noticeType)
        {
            Name = name;
            Description = description;
            this.noticeType = noticeType;
            NoticeTypeName = noticeType.FullName;
            var noticeTypeSchema = NJsonSchema.JsonSchema.FromType(noticeType);
            NoticeTypeSchema = noticeTypeSchema.ToJson(Formatting.Indented);
            NoticeTypeSchemaSample = JsonConvert.SerializeObject(noticeTypeSchema.ToSampleJson(), Formatting.Indented);
        }

        /// <summary>
        /// 获取通知类型
        /// </summary>
        public Type GetNoticeType() => noticeType ?? Type.GetType(NoticeTypeName);

        /// <summary>
        /// 定义
        /// </summary>
        /// <returns></returns>
        [DisplayName("定义")]
        [ReadOnly(true)]
        public string NoticeTypeSchema { get; set; }
        /// <summary>
        /// 示例
        /// </summary>
        [DisplayName("示例")]
        [ReadOnly(true)]
        public string NoticeTypeSchemaSample { get; set; }

        /// <summary>
        /// 创建通知信息实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static QpNoticeInfo Create<T>()
            where T : class, new()
        {
            return Create(typeof(T));
        }

        /// <summary>
        /// 创建通知信息实例
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static QpNoticeInfo Create(Type type)
        {
            string name = null;
            if (name == null)
                name = type.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
            if (name == null)
                name = type.GetCustomAttribute<JsonSchemaAttribute>()?.Name;
            if (name == null)
                name = type.FullName;
            return new QpNoticeInfo(name, type.GetCustomAttribute<DescriptionAttribute>()?.Description, type);
        }

        /// <summary>
        /// 创建通知信息实例
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static QpNoticeInfo Create(object instance)
        {
            return Create(instance.GetType());
        }
    }
}
