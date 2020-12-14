using System;
using LibCommon.Enums;

namespace LibCommon.Structs.GB28181
{
    /// <summary>
    /// 用于生成ssrc的类结构
    /// </summary>
    [Serializable]
    public class PushMediaInfoToCreateSSRC
    {
        private string _mediaServerId;
        private string _mediaServerIp;
        private string _app;
        private string _vhost;
        private string _sipDeviceId;
        private string _sipChannelId;
        private PushStreamSocketType _pushStreamSocketType;
        private string? _startTime;
        private string? _endtime;


        /// <summary>
        /// 流媒体服务器ID
        /// </summary>
        public string MediaServerId
        {
            get => _mediaServerId;
            set => _mediaServerId = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 流媒体服务器IP
        /// </summary>
        public string MediaServerIp
        {
            get => _mediaServerIp;
            set => _mediaServerIp = value ?? throw new ArgumentNullException(nameof(value));
        }


        /// <summary>
        /// 流媒体APP
        /// </summary>

        public string App
        {
            get => _app;
            set => _app = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 流媒体Vhost
        /// </summary>
        public string Vhost
        {
            get => _vhost;
            set => _vhost = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// sip设备id
        /// </summary>
        public string SipDeviceId
        {
            get => _sipDeviceId;
            set => _sipDeviceId = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// sip通道id
        /// </summary>
        public string SipChannelId
        {
            get => _sipChannelId;
            set => _sipChannelId = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 推流的socket类型 
        /// </summary>
        public PushStreamSocketType PushStreamSocketType
        {
            get => _pushStreamSocketType;
            set => _pushStreamSocketType = value;
        }

        /// <summary>
        /// 如果是回放流，加上starttime(long)
        /// </summary>

        public string? StartTime
        {
            get => _startTime;
            set => _startTime = value;
        }

        /// <summary>
        /// 如果是回放流，加上endtime(long)
        /// </summary>

        public string? Endtime
        {
            get => _endtime;
            set => _endtime = value;
        }
    }
}