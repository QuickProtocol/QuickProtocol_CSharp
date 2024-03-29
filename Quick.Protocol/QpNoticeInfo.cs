﻿using System.Text.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;

namespace Quick.Protocol
{
    /// <summary>
    /// 通知信息
    /// </summary>
    public class QpNoticeInfo
    {
        private readonly Type noticeType;

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

        private readonly IQpSerializer noticeSerializer;
        public IQpSerializer GetNoticeSerializer() => noticeSerializer;

        public QpNoticeInfo() { }
        public QpNoticeInfo(string name, string description, Type noticeType, object defaultNoticeTypeInstance, IQpSerializer noticeSerializer)
        {
            Name = name;
            Description = description;
            this.noticeType = noticeType;
            NoticeTypeName = noticeType.FullName;
            this.noticeSerializer = noticeSerializer;
            NoticeTypeSchemaSample = JsonNode.Parse(noticeSerializer.Serialize(defaultNoticeTypeInstance)).ToString();
        }

        /// <summary>
        /// 获取通知类型
        /// </summary>
        public Type GetNoticeType() => noticeType;
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
            where T : IQpModel<T>, new()
        {
            return Create(new T());
        }

        /// <summary>
        /// 创建通知信息实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static QpNoticeInfo Create<T>(T instance)
            where T : IQpModel<T>, new()
        {
            var type = typeof(T);
            string name = null;
            if (name == null)
                name = type.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
            if (name == null)
                name = type.FullName;
            return new QpNoticeInfo(name,
                type.GetCustomAttribute<DescriptionAttribute>()?.Description,
                type,
                instance,
                instance);
        }
    }
}
