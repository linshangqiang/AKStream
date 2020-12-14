using System;

namespace LibCommon.Structs
{
    [Serializable]
    public class PushMediaInfo
    {
        private string _mediaServerId;
        private string _mediaServerIpAddress;
        private ushort _http_wsMediaServerPlayPort;
        private ushort _rtmpMediaServerPlayPort;
        private ushort _rtspMediaServerPlayPort;
        private ushort _https_wssMediaServerPlayPort=0;
        private ushort _rtmpsMediaServerPlayPort=0;
        private ushort _rtspsMediaServerPlayPort=0;
        private string _app;
        private string _vhost;
        private string _stream;
        private PushStreamSocketType _pushStreamSocketType;
        private int _streamPort;
        private string? _recordPath;
        private bool? _startRecord;
        private uint _ssrc;


        /// <summary>
        /// 流媒体服务器id
        /// </summary>
        public string MediaServerId
        {
            get => _mediaServerId;
            set => _mediaServerId = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 流媒体服务器ip地址
        /// </summary>
        public string MediaServerIpAddress
        {
            get => _mediaServerIpAddress;
            set => _mediaServerIpAddress = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 流媒体http和ws的播放端口
        /// </summary>
        public ushort Http_WsMediaServerPlayPort
        {
            get => _http_wsMediaServerPlayPort;
            set => _http_wsMediaServerPlayPort = value;
        }

        /// <summary>
        /// rtmp的播放端口
        /// </summary>
        public ushort RtmpMediaServerPlayPort
        {
            get => _rtmpMediaServerPlayPort;
            set => _rtmpMediaServerPlayPort = value;
        }

        /// <summary>
        /// rtsp的播放端口
        /// </summary>
        public ushort RtspMediaServerPlayPort
        {
            get => _rtspMediaServerPlayPort;
            set => _rtspMediaServerPlayPort = value;
        }

        /// <summary>
        /// https，wss播放端口
        /// </summary>
        public ushort HttpsWssMediaServerPlayPort
        {
            get => _https_wssMediaServerPlayPort;
            set => _https_wssMediaServerPlayPort = value;
        }

        /// <summary>
        /// rtmps播放端口
        /// </summary>
        public ushort RtmpsMediaServerPlayPort
        {
            get => _rtmpsMediaServerPlayPort;
            set => _rtmpsMediaServerPlayPort = value;
        }
        /// <summary>
        /// rtsps播放端口
        /// </summary>
        public ushort RtspsMediaServerPlayPort
        {
            get => _rtspsMediaServerPlayPort;
            set => _rtspsMediaServerPlayPort = value;
        }

        /// <summary>
        /// 流媒体服务器app
        /// </summary>
        public string App
        {
            get => _app;
            set => _app = value ?? throw new ArgumentNullException(nameof(value));
        }

      

        /// <summary>
        /// 流媒体服务器vhost
        /// </summary>
        public string Vhost
        {
            get => _vhost;
            set => _vhost = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 流媒体服务器Streamid
        /// </summary>
        public string Stream
        {
            get => _stream;
            set => _stream = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 推流socket类型
        /// </summary>
        public PushStreamSocketType PushStreamSocketType
        {
            get => _pushStreamSocketType;
            set => _pushStreamSocketType = value;
        }

        /// <summary>
        /// 流媒体服务器收流端口
        /// </summary>
        public int StreamPort
        {
            get => _streamPort;
            set => _streamPort = value;
        }

        /// <summary>
        /// 录制文件自定义路径
        /// </summary>

        public string? RecordPath
        {
            get => _recordPath;
            set => _recordPath = value;
        }

        /// <summary>
        /// 是否启动录制
        /// </summary>
        public bool? StartRecord
        {
            get => _startRecord;
            set => _startRecord = value;
        }

        /// <summary>
        /// gb28181推流的ssrc值
        /// </summary>
        public uint Ssrc
        {
            get => _ssrc;
            set => _ssrc = value;
        }
    }
}