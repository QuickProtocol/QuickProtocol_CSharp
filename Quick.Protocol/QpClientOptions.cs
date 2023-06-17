using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Quick.Protocol
{
    public abstract class QpClientOptions : QpChannelOptions
    {
        /// <summary>
        /// 连接超时(默认为5秒)
        /// </summary>
        [DisplayName("连接超时")]
        [Category("高级")]
        public int ConnectionTimeout { get; set; } = 5 * 1000;
        /// <summary>
        /// 传输超时(默认15秒)
        /// </summary>
        [DisplayName("传输超时")]
        [Category("高级")]
        public int TransportTimeout
        {
            get { return InternalTransportTimeout; }
            set { InternalTransportTimeout = value; }
        }

        /// <summary>
        /// 启用加密(默认为false)
        /// </summary>
        [DisplayName("启用加密")]
        [Category("高级")]
        public bool EnableEncrypt { get; set; } = false;
        /// <summary>
        /// 启用压缩(默认为false)
        /// </summary>
        [DisplayName("启用压缩")]
        [Category("高级")]
        public bool EnableCompress { get; set; } = false;

        /// <summary>
        /// 当认证通过时
        /// </summary>
        public void OnAuthPassed()
        {
            InternalCompress = EnableCompress;
            InternalEncrypt = EnableEncrypt;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            InternalCompress = false;
            InternalEncrypt = false;
        }

        /// <summary>
        /// 创建客户端实例
        /// </summary>
        /// <returns></returns>
        public virtual QpClient CreateClient()
        {
            throw new NotImplementedException();
        }

        protected virtual void LoadFromUri(Uri uri)
        {
            if (string.IsNullOrEmpty(uri.Query))
                return;
            var queryString = System.Web.HttpUtility.ParseQueryString(uri.Query);
            JObject jObj = new JObject();
            foreach (var key in queryString.AllKeys)
            {
                jObj.Add(key, queryString[key]);
            }
            JsonConvert.PopulateObject(jObj.ToString(), this);
        }

        protected abstract string ToUriBasic(HashSet<string> ignorePropertyNames);

        public Uri ToUri(bool includePassword = false, bool includeOtherProperty = false)
        {
            HashSet<string> ignorePropertyNames = new HashSet<string>();
            ignorePropertyNames.Add(nameof(HeartBeatInterval));
            if (!includePassword)
                ignorePropertyNames.Add(nameof(Password));

            string baseUrl = ToUriBasic(ignorePropertyNames);
            if (includePassword || includeOtherProperty)
            {
                StringBuilder sb = new StringBuilder(baseUrl);
                int currentIndex = 0;

                var jObj = JObject.FromObject(this);
                foreach (var property in jObj.Properties())
                {
                    var key = property.Name;
                    if (ignorePropertyNames.Contains(key))
                        continue;
                    if (!includeOtherProperty && key!=nameof(Password))
                        continue;
                    if (currentIndex==0)
                        sb.Append("?");
                    if (currentIndex>0)
                        sb.Append("&");
                    currentIndex++;

                    var value = property.Value.ToString();
                    value=System.Web.HttpUtility.UrlEncode(value);
                    sb.Append($"{key}={value}");
                }
                baseUrl=sb.ToString();
            }
            Uri uri = new Uri(baseUrl);
            return uri;
        }

        public override string ToString() => ToUri().ToString();

        private static Dictionary<string, Func<QpClientOptions>> schemaQpClientOptionsFactoryDict = new Dictionary<string, Func<QpClientOptions>>();

        public static void RegisterUriSchema(string schema, Func<QpClientOptions> optionsFactory)
        {
            schemaQpClientOptionsFactoryDict[schema] = optionsFactory;
        }

        public static QpClientOptions Parse(Uri uri)
        {
            if (!schemaQpClientOptionsFactoryDict.ContainsKey(uri.Scheme))
                throw new ArgumentException($"Unknown uri schema [{uri.Scheme}],you muse register uri schema before use it.", nameof(uri));
            var optionsFactory = schemaQpClientOptionsFactoryDict[uri.Scheme];
            var qpClientOptions = optionsFactory.Invoke();
            qpClientOptions.LoadFromUri(uri);
            return qpClientOptions;
        }
    }
}
