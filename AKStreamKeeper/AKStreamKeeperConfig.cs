using System;
using System.Collections.Generic;

namespace AKStreamKeeper
{
    /// <summary>
    /// AKStreamKeeper配置文件结构
    /// </summary>
    [Serializable]
    public class AKStreamKeeperConfig
    {
        private string _ipV4Address;
        private string _ipV6Address;
        private ushort _webApiPort;
        private string _mediaServerPath;
        private string _akStreamWebRegisterUrl;
        private List<string> _customRecordPathList;
        private bool _useSSL = false;
        private ushort _minRtpPort = 10001;
        private ushort _maxRtpPort = 30000;
        private string _ffmpegPath;
        private string _accessKey;


        /// <summary>
        /// ipv4地址
        /// </summary>
        public string IpV4Address
        {
            get => _ipV4Address;
            set => _ipV4Address = value;
        }

        /// <summary>
        /// ipv6地址
        /// </summary>
        public string IpV6Address
        {
            get => _ipV6Address;
            set => _ipV6Address = value;
        }

        /// <summary>
        /// webapi端口
        /// </summary>
        public ushort WebApiPort
        {
            get => _webApiPort;
            set => _webApiPort = value;
        }

        /// <summary>
        /// 流媒体服务器路径
        /// </summary>
        public string MediaServerPath
        {
            get => _mediaServerPath;
            set => _mediaServerPath = value;
        }

        /// <summary>
        /// 流媒体注册地址
        /// </summary>
        public string AkStreamWebRegisterUrl
        {
            get => _akStreamWebRegisterUrl;
            set => _akStreamWebRegisterUrl = value;
        }

        /// <summary>
        /// 自定义录像路径列表
        /// </summary>
        public List<string> CustomRecordPathList
        {
            get => _customRecordPathList;
            set => _customRecordPathList = value;
        }

        /// <summary>
        /// 流媒体服务器是否使用SSL安全连接
        /// </summary>
        public bool UseSsl
        {
            get => _useSSL;
            set => _useSSL = value;
        }

        /// <summary>
        /// rtp端口范围（小）
        /// </summary>
        public ushort MinRtpPort
        {
            get => _minRtpPort;
            set => _minRtpPort = value;
        }

        /// <summary>
        /// rtp端口范围（大）
        /// </summary>
        public ushort MaxRtpPort
        {
            get => _maxRtpPort;
            set => _maxRtpPort = value;
        }

        /// <summary>
        /// ffmpeg的可执行文件路径
        /// </summary>
        public string FFmpegPath
        {
            get => _ffmpegPath;
            set => _ffmpegPath = value;
        }

        /// <summary>
        /// 访问webapi需要携带的key
        /// </summary>
        public string AccessKey
        {
            get => _accessKey;
            set => _accessKey = value;
        }
    }
}