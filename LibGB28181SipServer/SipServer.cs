using System;
using System.Net;
using System.Threading.Tasks;
using GB28181.Sys.XML;
using LibCommon;
using NetTopologySuite.Precision;
using Newtonsoft.Json;
using SIPSorcery.SIP;

namespace LibGB28181SipServer
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

        
        //sip request处理
        private async Task SIPTransportRequestReceived(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPRequest sipRequest)
        {
            LibLogger.Logger.Debug($"[{Common.LoggerHead}]->收到来自{remoteEndPoint.ToString()}的SipRequest->{sipRequest.ToString()}");
        }

        /// <summary>
        /// sip Response 处理
        /// </summary>
        /// <param name="localSIPEndPoint"></param>
        /// <param name="remoteEndPoint"></param>
        /// <param name="sipResponse"></param>
        /// <returns></returns>
        private async Task SIPTransportResponseReceived(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPResponse sipResponse)
        {
            LibLogger.Logger.Debug($"[{Common.LoggerHead}]->收到来自{remoteEndPoint.ToString()}的SipResponse->{sipResponse.ToString()}");
 
        }


        /*
        public bool StopSipServer(out ResponseStruct rs)
        {
            
        }
        */
        
        
        /// <summary>
        /// 启动Sip服务
        /// </summary>
        /// <returns></returns>
        public bool StartSipServer(out ResponseStruct rs)
        {
            
            LibLogger.Logger.Info($"[{Common.LoggerHead}]->配置情况->Sip服务ID->{Common.SipServerConfig.ServerSipDeviceId}");
            LibLogger.Logger.Info($"[{Common.LoggerHead}]->配置情况->本机IP地址->{Common.SipServerConfig.SipIpAddress}");
            if (Common.SipServerConfig.IpV6Enable)
            {
                LibLogger.Logger.Info($"[{Common.LoggerHead}]->配置情况->本机IPV6地址->{Common.SipServerConfig.SipIpV6Address}");
            }
            LibLogger.Logger.Info($"[{Common.LoggerHead}]->配置情况->启用IPV6->{Common.SipServerConfig.IpV6Enable}");
            LibLogger.Logger.Info($"[{Common.LoggerHead}]->配置情况->Sip服务端口->{Common.SipServerConfig.SipPort}");
            LibLogger.Logger.Info($"[{Common.LoggerHead}]->配置情况->Sip服务协议->{Common.SipServerConfig.MsgProtocol}");
            LibLogger.Logger.Info($"[{Common.LoggerHead}]->配置情况->Sip服务端字符集->{Common.SipServerConfig.MsgEncode}");
            LibLogger.Logger.Info($"[{Common.LoggerHead}]->配置情况->GB28181协议版本->{Common.SipServerConfig.GbVersion}");
            LibLogger.Logger.Info($"[{Common.LoggerHead}]->配置情况->Sip服务是否启用鉴权->{Common.SipServerConfig.Authentication}");
            LibLogger.Logger.Info($"[{Common.LoggerHead}]->配置情况->Sip服务鉴权用户名->{Common.SipServerConfig.SipUsername}");
            LibLogger.Logger.Info($"[{Common.LoggerHead}]->配置情况->Sip服务鉴权密码->{Common.SipServerConfig.SipPassword}");
            LibLogger.Logger.Info($"[{Common.LoggerHead}]->配置情况->Sip服务心跳周期（秒）->{Common.SipServerConfig.KeepAliveInterval}");
            LibLogger.Logger.Info($"[{Common.LoggerHead}]->配置情况->Sip服务允许心跳丢失次数->{Common.SipServerConfig.KeepAliveLostNumber}");
            try
            {
                LibLogger.Logger.Info($"[{Common.LoggerHead}]->启动Sip服务.");

                //创建sip传输层
                _sipTransport = new SIPTransport();
                // 创建ipv4 udp传输层
                var sipChannel = new SIPUDPChannel(new IPEndPoint(IPAddress.Parse(Common.SipServerConfig.SipIpAddress), Common.SipServerConfig.SipPort));
                _sipTransport.AddSIPChannel(sipChannel);
                  // 创建ipv6 udp传输层
                if (Common.SipServerConfig.IpV6Enable)
                {  
                    var ipv6SipChannel = new SIPUDPChannel(new IPEndPoint(
                        IPAddress.Parse(Common.SipServerConfig.SipIpV6Address!), Common.SipServerConfig.SipPort));
                    _sipTransport.AddSIPChannel(ipv6SipChannel);
                }

                _sipTransport.SIPTransportRequestReceived += SIPTransportRequestReceived;
                _sipTransport.SIPTransportResponseReceived+=SIPTransportResponseReceived;
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.None,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.None],
                };
             
                return true;
            }
            catch(Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sip_StartExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sip_StartExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
                throw new AKStreamException(rs);
            }
        }
        public SipServer()
        {
            ResponseStruct rs;
            LibLogger.Logger.Info($"[{Common.LoggerHead}]->加载配置文件->{Common.SipServerConfigPath}");
            var ret = Common.ReadSipServerConfig(out rs);

            if (ret < 0 || !rs.Code.Equals(ErrorNumber.None))
            {
                LibLogger.Logger.Error($"[{Common.LoggerHead}]->加载配置文件失败->{Common.SipServerConfigPath}");
                throw new AKStreamException(rs);
            }
            LibLogger.Logger.Info($"[{Common.LoggerHead}]->加载配置文件成功->{Common.SipServerConfigPath}");
        }
    }
}