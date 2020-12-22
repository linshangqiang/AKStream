using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Timers;
using LibCommon;
using LibCommon.Structs;
using LibZLMediaKitMediaServer.Structs.WebHookRequest;
using LibZLMediaKitMediaServer.Structs.WebResponse.ZLMediaKit;


namespace LibZLMediaKitMediaServer
{
    /// <summary>
    /// 流媒体服务器信息
    /// </summary>
    [Serializable]
    public class ServerInstance : IDisposable
    {
        private string _ipV4Address;
        private string _ipV6Address;
        private ushort _keeperPort;
        private string _secret;
        private string _mediaServerId;
        private int _zlmediakitPid;
        private DateTime _keepAliveTime;
        private WebApiHelper _webApiHelper;
        private string _recordPath;
        private ushort _rtpPortMin;
        private ushort _rtpPortMax; //仅使用min-max中的偶数类端口
        private bool _isRunning;
        private bool _useSSL;
        private ushort _httpsPort;
        private ushort _rtmpsPort;
        private ushort _rtspsPort;
        private ushort _httpPort;
        private ushort _rtmpPort;
        private ushort _rtspPort;
        private uint _zlmRecordFileSec;
        private DateTime _serverDateTime;
        private PerformanceInfo? _performanceInfo;
        private ResZLMediaKitConfig? _config;
        private Timer _keepAliveCheckTimer;
        private int _countmod = 0;
        private ResZLMediaKitMediaList _mediaServerStreamList;
        private List<ReqForWebHookOnPlay> _mediaServerPlayerList=new List<ReqForWebHookOnPlay>();

        private static ConcurrentDictionary<string, ushort> _rtpPortDictionary =
            new ConcurrentDictionary<string, ushort>();

        public ServerInstance()
        {
            startTimer();
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (_webApiHelper != null)
            {
                if (_config == null || _countmod % 10 == 0)
                {
                    if (_countmod > 10000000)
                    {
                        _countmod = 0;
                    }
                    else
                    {
                        _countmod++;
                    }

                    
                    var ret = _webApiHelper.GetServerConfig(out  ResponseStruct rs);
                    if (ret != null && rs.Code == ErrorNumber.None)
                    {
                        this._config = ret;
                    }
                }
            }
        }

        private void startTimer()
        {
            if (_keepAliveCheckTimer == null)
            {
                _keepAliveCheckTimer = new Timer(1000);
                _keepAliveCheckTimer.Enabled = true; //启动Elapsed事件触发
                _keepAliveCheckTimer.Elapsed += OnTimedEvent; //添加触发事件的函数
                _keepAliveCheckTimer.AutoReset = true; //需要自动reset
                _keepAliveCheckTimer.Start(); //启动计时器
            }
        }


        public void Dispose()
        {
            if (_keepAliveCheckTimer != null)
            {
                _keepAliveCheckTimer.Dispose();
                _keepAliveCheckTimer = null!;
            }

            if (_rtpPortDictionary != null)
            {
                _rtpPortDictionary.Clear();
                _rtpPortDictionary = null;
            }
        }

        ~ServerInstance()
        {
            Dispose(); //释放非托管资源
        }


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

        public WebApiHelper WebApiHelper
        {
            get => _webApiHelper;
            set => _webApiHelper = value;
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

        public PerformanceInfo? PerformanceInfo
        {
            get => _performanceInfo;
            set => _performanceInfo = value;
        }

        public bool IsRunning
        {
            get => _isRunning;
            set => _isRunning = value;
        }

        public bool UseSsl
        {
            get => _useSSL;
            set => _useSSL = value;
        }

        public ushort HttpsPort
        {
            get => _httpsPort;
            set => _httpsPort = value;
        }

        public ushort RtmpsPort
        {
            get => _rtmpsPort;
            set => _rtmpsPort = value;
        }

        public ushort RtspsPort
        {
            get => _rtspsPort;
            set => _rtspsPort = value;
        }

        public ushort HttpPort
        {
            get => _httpPort;
            set => _httpPort = value;
        }

        public ushort RtmpPort
        {
            get => _rtmpPort;
            set => _rtmpPort = value;
        }

        public ushort RtspPort
        {
            get => _rtspPort;
            set => _rtspPort = value;
        }


        public uint ZlmRecordFileSec
        {
            get => _zlmRecordFileSec;
            set => _zlmRecordFileSec = value;
        }

        public DateTime ServerDateTime
        {
            get => _serverDateTime;
            set => _serverDateTime = value;
        }

        /// <summary>
        /// 此流媒体服务器当前的配置信息
        /// </summary>
        public ResZLMediaKitConfig? Config
        {
            get => _config;
            set => _config = value;
        }
        

        /// <summary>
        /// 此流媒体服务器当前的所有流信息
        /// </summary>
        public ResZLMediaKitMediaList MediaServerStreamList
        {
            get => _mediaServerStreamList;
            set => _mediaServerStreamList = value;
        }

        /// <summary>
        /// 此流媒体服务器当前的播放者列表
        /// </summary>
        public List<ReqForWebHookOnPlay> MediaServerPlayerList
        {
            get => _mediaServerPlayerList;
            set => _mediaServerPlayerList = value;
        }
    }
}