using System;

namespace LibCommon.Structs
{
    /// <summary>
    /// 流媒体服务器的心跳结构
    /// </summary>
    [Serializable]
    public class MediaServerKeepAlive
    {
        private string _ipV4Address;
        private string _ipV6Address;
        private ushort _webApiPort;
        private string _secret;
        private string _mediaServerId;
        private int _mediaServerPid;
        private string _recordPath;
        private ushort _rtpPortMin;
        private ushort _rtpPortMax; //仅使用min-max中的偶数类端口
        private ushort _zlmWebApiHttpPort;
        private DateTime _serverDateTime;//流媒体服务器当前时间
        private PerformanceInfo? _performanceInfo;
        

        /// <summary>
        /// ipv4地址
        /// </summary>
        public string IpV4Address
        {
            get => _ipV4Address;
            set => _ipV4Address = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// ipv6地址
        /// </summary>
        public string IpV6Address
        {
            get => _ipV6Address;
            set => _ipV6Address = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// keeper的WebApi端口
        /// </summary>
        public ushort WebApiPort
        {
            get => _webApiPort;
            set => _webApiPort = value;
        }

        /// <summary>
        /// ZLM的Secret值
        /// </summary>
        public string Secret
        {
            get => _secret;
            set => _secret = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// zlm的id
        /// </summary>
        public string MediaServerId
        {
            get => _mediaServerId;
            set => _mediaServerId = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// zlm的pid
        /// </summary>
        public int MediaServerPid
        {
            get => _mediaServerPid;
            set => _mediaServerPid = value;
        }

        /// <summary>
        /// 自定义视频存储路径
        /// </summary>
        public string RecordPath
        {
            get => _recordPath;
            set => _recordPath = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// rtp开放范围(最小)
        /// </summary>
        public ushort RtpPortMin
        {
            get => _rtpPortMin;
            set => _rtpPortMin = value;
        }

        /// <summary>
        /// rtp开放范围（最大）
        /// </summary>
        public ushort RtpPortMax
        {
            get => _rtpPortMax;
            set => _rtpPortMax = value;
        }

        /// <summary>
        /// zlm的webapi端口
        /// </summary>
        public ushort ZlmWebApiHttpPort
        {
            get => _zlmWebApiHttpPort;
            set => _zlmWebApiHttpPort = value;
        }

        /// <summary>
        /// 服务器当前时间
        /// </summary>
        public DateTime ServerDateTime
        {
            get => _serverDateTime;
            set => _serverDateTime = value;
        }

        /// <summary>
        /// 流媒体服务器的性能情况
        /// </summary>
        public PerformanceInfo? PerformanceInfo
        {
            get => _performanceInfo;
            set => _performanceInfo = value;
        }
    }
}