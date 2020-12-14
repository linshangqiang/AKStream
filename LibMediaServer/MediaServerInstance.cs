using System;
using System.Collections.Concurrent;
using LibSystemInfo;


namespace LibCommon.Structs
{
    /// <summary>
    /// 流媒体服务器信息
    /// </summary>
    [Serializable]
    public class MediaServerInstance
    {
        private string _ipAddress;
        private ushort _keeperPort;
        private string _secret;
        private string _mediaServerId;
        private ushort _mediaServerPort;
        private int _zlmediakitPid;
        private DateTime _keepAliveTime;
        private ZLMediaKitWebApiHelper _zlMediaKitWebApiHelper;
        private string _recordPath;
        private ushort _rtpPortMin;
        private ushort _rtpPortMax; //仅使用min-max中的偶数类端口

        private static ConcurrentDictionary<string, ushort> _rtpPortDictionary =
            new ConcurrentDictionary<string, ushort>();

        private GlobalSystemInfo _globalSystemInfo;


        public string IpAddress
        {
            get => _ipAddress;
            set => _ipAddress = value;
        }

        public ushort KeeperPort
        {
            get => _keeperPort;
            set => _keeperPort = value;
        }

        public string Secret
        {
            get => _secret;
            set => _secret = value;
        }

        public string MediaServerId
        {
            get => _mediaServerId;
            set => _mediaServerId = value;
        }

        public ushort MediaServerPort
        {
            get => _mediaServerPort;
            set => _mediaServerPort = value;
        }

        public int ZlmediakitPid
        {
            get => _zlmediakitPid;
            set => _zlmediakitPid = value;
        }

        public DateTime KeepAliveTime
        {
            get => _keepAliveTime;
            set => _keepAliveTime = value;
        }

        public ZLMediaKitWebApiHelper ZlMediaKitWebApiHelper
        {
            get => _zlMediaKitWebApiHelper;
            set => _zlMediaKitWebApiHelper = value;
        }

        public string RecordPath
        {
            get => _recordPath;
            set => _recordPath = value;
        }

        public ushort RtpPortMin
        {
            get => _rtpPortMin;
            set => _rtpPortMin = value;
        }

        public ushort RtpPortMax
        {
            get => _rtpPortMax;
            set => _rtpPortMax = value;
        }

        public static ConcurrentDictionary<string, ushort> RtpPortDictionary
        {
            get => _rtpPortDictionary;
            set => _rtpPortDictionary = value;
        }

        public GlobalSystemInfo GlobalSystemInfo
        {
            get => _globalSystemInfo;
            set => _globalSystemInfo = value;
        }
    }
}