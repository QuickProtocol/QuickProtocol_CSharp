using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Quick.Protocol.WebSocket.Server.AspNetCore
{
    public class QpWebSocketServerOptions : QpServerOptions
    {
        private string _Path;
        /// <summary>
        /// WebSocket的路径
        /// </summary>
        public string Path
        {
            get { return _Path; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _Path = "/";
                    return;
                }
                if (value.StartsWith("/"))
                {
                    _Path = value;
                    return;
                }
                _Path = "/" + value;
            }
        }
    }
}