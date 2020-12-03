using System;
using System.Net;
using LibCommon;
using LibLogger;
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
        private SIPUDPChannel _sipUdpIpV4Channel;

        /// <summary>
        /// SipUDP通道(IPV6)
        /// </summary>
        private SIPUDPChannel _sipUdpIpV6Channel;

        /// <summary>
        /// SipTCP通道(IPV4)
        /// </summary>
        private SIPTCPChannel _sipTcpIpV4Channel;

        /// <summary>
        /// SipTCP通道(IPV6)
        /// </summary>
        private SIPTCPChannel _sipTcpIpV6Channel;

        /// <summary>
        /// SIP传输通道(公开)
        /// </summary>
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
            Logger.Info($"[{Common.LoggerHead}]->配置情况->Sip服务ID->{Common.SipServerConfig.ServerSipDeviceId}");
            Logger.Info($"[{Common.LoggerHead}]->配置情况->本机IP地址->{Common.SipServerConfig.SipIpAddress}");
            if (Common.SipServerConfig.IpV6Enable)
            {
                Logger.Info(
                    $"[{Common.LoggerHead}]->配置情况->本机IPV6地址->{Common.SipServerConfig.SipIpV6Address}");
            }

            Logger.Info($"[{Common.LoggerHead}]->配置情况->启用IPV6->{Common.SipServerConfig.IpV6Enable}");
            Logger.Info($"[{Common.LoggerHead}]->配置情况->Sip服务端口->{Common.SipServerConfig.SipPort}");
            Logger.Info($"[{Common.LoggerHead}]->配置情况->Sip服务协议->{Common.SipServerConfig.MsgProtocol}");
            Logger.Info($"[{Common.LoggerHead}]->配置情况->Sip服务端字符集->{Common.SipServerConfig.MsgEncode}");
            Logger.Info($"[{Common.LoggerHead}]->配置情况->GB28181协议版本->{Common.SipServerConfig.GbVersion}");
            Logger.Info($"[{Common.LoggerHead}]->配置情况->Sip服务是否启用鉴权->{Common.SipServerConfig.Authentication}");
            Logger.Info($"[{Common.LoggerHead}]->配置情况->Sip服务鉴权用户名->{Common.SipServerConfig.SipUsername}");
            Logger.Info($"[{Common.LoggerHead}]->配置情况->Sip服务鉴权密码->{Common.SipServerConfig.SipPassword}");
            Logger.Info(
                $"[{Common.LoggerHead}]->配置情况->Sip服务心跳周期（秒）->{Common.SipServerConfig.KeepAliveInterval}");
            Logger.Info(
                $"[{Common.LoggerHead}]->配置情况->Sip服务允许心跳丢失次数->{Common.SipServerConfig.KeepAliveLostNumber}");
            try
            {
                Logger.Info($"[{Common.LoggerHead}]->启动Sip服务.");

                //创建sip传输层
                _sipTransport = new SIPTransport();
                // 创建ipv4 udp传输层
                _sipUdpIpV4Channel = new SIPUDPChannel(new IPEndPoint(IPAddress.Any,
                    Common.SipServerConfig.SipPort));
                if (Common.SipServerConfig.MsgProtocol.Trim().ToUpper().Equals("TCP"))
                {
                    _sipTcpIpV4Channel = new SIPTCPChannel(new IPEndPoint(IPAddress.Any,
                        Common.SipServerConfig.SipPort));
                    _sipTransport.AddSIPChannel(_sipTcpIpV4Channel);
                    Logger.Info(
                        $"[{Common.LoggerHead}]->监听端口成功,监听情况->{_sipTcpIpV4Channel.ListeningEndPoint.Address}:{_sipTcpIpV4Channel.ListeningEndPoint.Port}(TCP via IPV4)");
                }

                _sipTransport.AddSIPChannel(_sipUdpIpV4Channel);

                Logger.Info(
                    $"[{Common.LoggerHead}]->监听端口成功,监听情况->{_sipUdpIpV4Channel.ListeningEndPoint.Address}:{_sipUdpIpV4Channel.ListeningEndPoint.Port}(UDP via IPV4)");

                // 创建ipv6 udp传输层

                if (Common.SipServerConfig.IpV6Enable)
                {
                    _sipUdpIpV6Channel = new SIPUDPChannel(new IPEndPoint(
                        IPAddress.IPv6Any, Common.SipServerConfig.SipPort));
                    if (Common.SipServerConfig.MsgProtocol.Trim().ToUpper().Equals("TCP"))
                    {
                        _sipTcpIpV6Channel = new SIPTCPChannel(new IPEndPoint(IPAddress.IPv6Any,
                            Common.SipServerConfig.SipPort));
                        _sipTransport.AddSIPChannel(_sipTcpIpV6Channel);
                        Logger.Info(
                            $"[{Common.LoggerHead}]->监听端口成功,监听情况->{_sipTcpIpV6Channel.ListeningEndPoint.Address}:{_sipTcpIpV6Channel.ListeningEndPoint.Port}(TCP via IPV6)");
                    }

                    _sipTransport.AddSIPChannel(_sipUdpIpV6Channel);
                    Logger.Info(
                        $"[{Common.LoggerHead}]->监听端口成功,监听情况->{_sipUdpIpV6Channel.ListeningEndPoint.Address}:{_sipUdpIpV6Channel.ListeningEndPoint.Port}(UDP via IPV6)");
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
            Logger.Info($"[{Common.LoggerHead}]->加载配置文件->{Common.SipServerConfigPath}");
            var ret = Common.ReadSipServerConfig(out rs);

            if (ret < 0 || !rs.Code.Equals(ErrorNumber.None))
            {
                Logger.Error($"[{Common.LoggerHead}]->加载配置文件失败->{Common.SipServerConfigPath}");
                throw new AKStreamException(rs);
            }

            Common.SipServer = this;
            Logger.Info($"[{Common.LoggerHead}]->加载配置文件成功->{Common.SipServerConfigPath}");
        }
    }
}