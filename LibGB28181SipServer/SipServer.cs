using System;
using GB28181.Sys.XML;
using SIPSorcery.SIP;

namespace LibGB28181SipProxy
{
    public delegate void RegisterDelegate(SIPRequest sipRequest, SIPEndPoint remoteEndPoint,
        SIPEndPoint localSipEndPoint);

    public delegate void UnRegisterDelegate(SIPRequest sipRequest, SIPEndPoint remoteEndPoint,
        SIPEndPoint localSipEndPoint);

    public delegate void DeviceAlarmSubscribeDelegate(SIPTransaction sipTransaction);

    /// <summary>
    /// sip网关类
    /// </summary>
    public class SipServer
    {
        /// <summary>
        /// SIP传输通道
        /// </summary>
        private SIPTransport _sipTransport;

        /// <summary>
        /// SipUDP通道(IPV4)
        /// </summary>
        private SIPUDPChannel _sipIPv4udpChannel;

        /// <summary>
        /// SipUDP通道(IPV6)
        /// </summary>
        private SIPUDPChannel _sipudpIPv6Channel;

        /// <summary>
        /// sip服务状态
        /// </summary>
        public event Action<string, ServiceStatus> OnServiceChanged;

        /// <summary>
        /// 录像文件接收
        /// </summary>
        public event Action<RecordInfo> OnRecordInfoReceived;

        /// <summary>
        /// 设备目录接收
        /// </summary>
        public event Action<Catalog> OnCatalogReceived;

        /// <summary>
        /// 设备目录通知
        /// </summary>
        public event Action<NotifyCatalog> OnNotifyCatalogReceived;

        /// <summary>
        /// 语音广播通知
        /// </summary>
        public event Action<VoiceBroadcastNotify> OnVoiceBroadcaseReceived;

        /// <summary>
        /// 报警通知
        /// </summary>
        public event Action<Alarm> OnAlarmReceived;

        /// <summary>
        /// 平台之间心跳接收
        /// </summary>
        public event Action<SIPEndPoint, KeepAlive, string> OnKeepaliveReceived;

        /// <summary>
        /// 设备状态查询接收
        /// </summary>
        public event Action<SIPEndPoint, DeviceStatus> OnDeviceStatusReceived;

        /// <summary>
        /// 设备信息查询接收
        /// </summary>
        public event Action<SIPEndPoint, DeviceInfo> OnDeviceInfoReceived;

        /// <summary>
        /// 设备配置查询接收
        /// </summary>
        public event Action<SIPEndPoint, DeviceConfigDownload> OnDeviceConfigDownloadReceived;

        /// <summary>
        /// 历史媒体发送结束接收
        /// </summary>
        public event Action<SIPEndPoint, MediaStatus> OnMediaStatusReceived;

        /// <summary>
        /// 响应状态码接收
        /// </summary>
        public event Action<SIPResponse, string, SIPEndPoint> OnResponseCodeReceived;

        /// <summary>
        /// 响应状态码接收
        /// </summary>
        public event Action<SIPResponse, SIPRequest, string, SIPEndPoint> OnResponseNeedResponeReceived;

        /// <summary>
        /// 预置位查询接收
        /// </summary>,
        public event Action<SIPEndPoint, PresetInfo> OnPresetQueryReceived;

        /// <summary>
        /// 设备注册时
        /// </summary>
        public event RegisterDelegate OnRegisterReceived;

        /// <summary>
        /// 设备注销时
        /// </summary>
        public event UnRegisterDelegate OnUnRegisterReceived;

        /// <summary>
        /// 设备有警告时
        /// </summary>
        public event DeviceAlarmSubscribeDelegate OnDeviceAlarmSubscribe;

        public SipServer()
        {
            /*
            //（配置SIP传输层） Configure the SIP transport layer.
            _sipTransport = new SIPTransport();

            //（使用默认选项配置 SIP 通道） Use default options to set up a SIP channel.
            var sipChannel = new SIPUDPChannel(new IPEndPoint(IPAddress.Any, Configs.ListenPort));
            _sipTransport.AddSIPChannel(sipChannel);

            var ipv6SipChannel = new SIPUDPChannel(new IPEndPoint(IPAddress.IPv6Any, Configs.ListenPort));
            _sipTransport.AddSIPChannel(ipv6SipChannel);*/
        }
    }
}