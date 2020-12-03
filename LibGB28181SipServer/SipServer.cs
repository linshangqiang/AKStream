using System;
using System.Net;
using LibCommon;
using SIPSorcery.SIP;

namespace LibGB28181SipServer
{
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

        public SIPTransport SipTransport
        {
            get => _sipTransport;
            set => _sipTransport = value;
        }

        

        /// <summary>
        /// 停止Sip服务
        /// </summary>
        /// <param name="rs"></param>
        /// <exception cref="AKStreamException"></exception>
        public void Stop(out ResponseStruct rs)
        {
            try
            {
                _sipTransport.SIPTransportRequestReceived -= SipMsgProcess.SipTransportRequestReceived;
                _sipTransport.SIPTransportResponseReceived -= SipMsgProcess.SipTransportResponseReceived;
                _sipTransport.Shutdown();
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.None,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.None],
                };
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sip_StopExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sip_StopExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
                throw new AKStreamException(rs);
            }
        }


        /// <summary>
        /// 启动Sip服务
        /// </summary>
        /// <returns></returns>
        public void Start(out ResponseStruct rs)
        {
            LibLogger.Logger.Info($"[{Common.LoggerHead}]->配置情况->Sip服务ID->{Common.SipServerConfig.ServerSipDeviceId}");
            LibLogger.Logger.Info($"[{Common.LoggerHead}]->配置情况->本机IP地址->{Common.SipServerConfig.SipIpAddress}");
            if (Common.SipServerConfig.IpV6Enable)
            {
                LibLogger.Logger.Info(
                    $"[{Common.LoggerHead}]->配置情况->本机IPV6地址->{Common.SipServerConfig.SipIpV6Address}");
            }

            LibLogger.Logger.Info($"[{Common.LoggerHead}]->配置情况->启用IPV6->{Common.SipServerConfig.IpV6Enable}");
            LibLogger.Logger.Info($"[{Common.LoggerHead}]->配置情况->Sip服务端口->{Common.SipServerConfig.SipPort}");
            LibLogger.Logger.Info($"[{Common.LoggerHead}]->配置情况->Sip服务协议->{Common.SipServerConfig.MsgProtocol}");
            LibLogger.Logger.Info($"[{Common.LoggerHead}]->配置情况->Sip服务端字符集->{Common.SipServerConfig.MsgEncode}");
            LibLogger.Logger.Info($"[{Common.LoggerHead}]->配置情况->GB28181协议版本->{Common.SipServerConfig.GbVersion}");
            LibLogger.Logger.Info($"[{Common.LoggerHead}]->配置情况->Sip服务是否启用鉴权->{Common.SipServerConfig.Authentication}");
            LibLogger.Logger.Info($"[{Common.LoggerHead}]->配置情况->Sip服务鉴权用户名->{Common.SipServerConfig.SipUsername}");
            LibLogger.Logger.Info($"[{Common.LoggerHead}]->配置情况->Sip服务鉴权密码->{Common.SipServerConfig.SipPassword}");
            LibLogger.Logger.Info(
                $"[{Common.LoggerHead}]->配置情况->Sip服务心跳周期（秒）->{Common.SipServerConfig.KeepAliveInterval}");
            LibLogger.Logger.Info(
                $"[{Common.LoggerHead}]->配置情况->Sip服务允许心跳丢失次数->{Common.SipServerConfig.KeepAliveLostNumber}");
            try
            {
                LibLogger.Logger.Info($"[{Common.LoggerHead}]->启动Sip服务.");

                //创建sip传输层
                _sipTransport = new SIPTransport();
                // 创建ipv4 udp传输层
                var sipChannel = new SIPUDPChannel(new IPEndPoint(IPAddress.Parse(Common.SipServerConfig.SipIpAddress),
                    Common.SipServerConfig.SipPort));
                _sipTransport.AddSIPChannel(sipChannel);
                // 创建ipv6 udp传输层
                if (Common.SipServerConfig.IpV6Enable)
                {
                    var ipv6SipChannel = new SIPUDPChannel(new IPEndPoint(
                        IPAddress.Parse(Common.SipServerConfig.SipIpV6Address!), Common.SipServerConfig.SipPort));
                    _sipTransport.AddSIPChannel(ipv6SipChannel);
                }
                

                switch (Common.SipServerConfig.GbVersion)
                {
                    case "GB-2016":
                        _sipTransport.SIPTransportRequestReceived += SipMsgProcess.SipTransportRequestReceived;
                     
                        _sipTransport.SIPTransportResponseReceived += SipMsgProcess.SipTransportResponseReceived;
                
                        break;
                }

                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.None,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.None],
                };
            }
            catch (Exception ex)
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
            Common.SipServer = this;
            LibLogger.Logger.Info($"[{Common.LoggerHead}]->加载配置文件成功->{Common.SipServerConfigPath}");
        }
    }
}