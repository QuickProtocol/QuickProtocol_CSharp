using System.Text.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;

namespace Quick.Protocol
{
    /// <summary>
    /// 通知信息
    /// </summary>
    public class QpNoticeInfo
    {
        private JsonTypeInfo noticeTypeInfo;

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
        public QpNoticeInfo(string name, string description, JsonTypeInfo noticeTypeInfo, object defaultNoticeTypeInstance)
        {
            Name = name;
            Description = description;
            NoticeTypeName = noticeTypeInfo.Type.FullName;
            this.noticeTypeInfo = noticeTypeInfo;
            NoticeTypeSchemaSample = JsonSerializer.Serialize(defaultNoticeTypeInstance, noticeTypeInfo);
        }

        /// <summary>
        /// 获取通知类型
        /// </summary>
        public JsonTypeInfo GetNoticeTypeInfo() => noticeTypeInfo;
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
        public static QpNoticeInfo Create<T>(JsonTypeInfo<T> typeInfo)
            where T : new()
        {
            return Create<T>(new T(), typeInfo);
        }

        /// <summary>
        /// 创建通知信息实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static QpNoticeInfo Create<T>(T instance, JsonTypeInfo<T> typeInfo)
        {
            var type = typeof(T);
            string name = null;
            if (name == null)
                name = type.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
            if (name == null)
                name = type.FullName;
            return new QpNoticeInfo(name, type.GetCustomAttribute<DescriptionAttribute>()?.Description, typeInfo, instance);
        }
    }
}
