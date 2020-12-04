using System;
using System.Collections.Generic;

namespace LibCommon.Structs
{
    [Serializable]
    public class MediaServerStreamPlayerInfo
    {
        private long _playerId;
        private string _ipAddress;
        private string? _playerParams;
        private ushort _port;
        private long? _streamBytes;
        private DateTime _startTime;
        private bool _isOnline;

        /// <summary>
        /// 播放器的tcp id
        /// </summary>
        public long PlayerId
        {
            get => _playerId;
            set => _playerId = value;
        }

        /// <summary>
        /// 播放器的ip地址
        /// </summary>
        public string IpAddress
        {
            get => _ipAddress;
            set => _ipAddress = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 播放器的参数
        /// </summary>
        public string? PlayerParams
        {
            get => _playerParams;
            set => _playerParams = value;
        }

        /// <summary>
        /// 播放器的端口
        /// </summary>
        public ushort Port
        {
            get => _port;
            set => _port = value;
        }

        /// <summary>
        /// 总计流出量
        /// </summary>
        public long? StreamBytes
        {
            get => _streamBytes;
            set => _streamBytes = value;
        }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime
        {
            get => _startTime;
            set => _startTime = value;
        }

        /// <summary>
        /// 当前是否在线
        /// </summary>
        public bool IsOnline
        {
            get => _isOnline;
            set => _isOnline = value;
        }
    }
    /// <summary>
    /// 流媒体服务中的流信息
    /// </summary>
    [Serializable]
    public class MediaServerStreamInfo
    {
        private string _mediaServerId;
        private string _mediaServerIp;
        private DateTime _startTime;
        private long _streamBytes;
        private string _vhost;
        private string _app;
        private string? _schema;
        private string _stream;
        private string _playUrl;
        private List<MediaServerStreamPlayerInfo> _playerList= new List<MediaServerStreamPlayerInfo>();

        /// <summary>
        /// 流媒体服务的ID
        /// </summary>
        public string MediaServerId
        {
            get => _mediaServerId;
            set => _mediaServerId = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 流媒体服务的IP
        /// </summary>
        public string MediaServerIp
        {
            get => _mediaServerIp;
            set => _mediaServerIp = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime
        {
            get => _startTime;
            set => _startTime = value;
        }

        /// <summary>
        /// 流入总计
        /// </summary>
        public long StreamBytes
        {
            get => _streamBytes;
            set => _streamBytes = value;
        }

        /// <summary>
        /// Vhost
        /// </summary>
        public string Vhost
        {
            get => _vhost;
            set => _vhost = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// APP
        /// </summary>
        public string App
        {
            get => _app;
            set => _app = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 协议类型
        /// </summary>
        public string? Schema
        {
            get => _schema;
            set => _schema = value;
        }

        /// <summary>
        /// 流ID
        /// </summary>
        public string Stream
        {
            get => _stream;
            set => _stream = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 播放地址
        /// </summary>
        public string PlayUrl
        {
            get => _playUrl;
            set => _playUrl = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 播放者列表
        /// </summary>
        public List<MediaServerStreamPlayerInfo> PlayerList
        {
            get => _playerList;
            set => _playerList = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}