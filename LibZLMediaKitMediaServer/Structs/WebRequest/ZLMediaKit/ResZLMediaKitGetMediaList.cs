using System;

namespace LibZLMediaKitMediaServer.Structs.WebRequest.ZLMediaKit
{
    /// <summary>
    /// 请求zlmediakit的流列表结构
    /// </summary>
    [Serializable]
    public class ResZLMediaKitGetMediaList : ReqZLMediaKitRequestBase
    {
        private string? _schema;
        private string? _vhost;
        private string? _app;

        /// <summary>
        /// 协议类型
        /// </summary>
        public string? Schema
        {
            get => _schema;
            set => _schema = value;
        }

        /// <summary>
        /// vhost
        /// </summary>
        public string? Vhost
        {
            get => _vhost;
            set => _vhost = value;
        }

        /// <summary>
        /// app
        /// </summary>
        public string? App
        {
            get => _app;
            set => _app = value;
        }
    }
}