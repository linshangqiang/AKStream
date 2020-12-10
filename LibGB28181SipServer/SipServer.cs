using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LibCommon;
using LibCommon.Enums;
using LibCommon.Structs;
using LibCommon.Structs.GB28181;
using LibCommon.Structs.GB28181.Net.SDP;
using LibCommon.Structs.GB28181.Net.SIP;
using LibCommon.Structs.GB28181.XML;
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
        private SIPTransport _sipTransport = null!;

        /// <summary>
        /// SipUDP通道(IPV4)
        /// </summary>
        private SIPUDPChannel _sipUdpIpV4Channel = null!;

        /// <summary>
        /// SipUDP通道(IPV6)
        /// </summary>
        private SIPUDPChannel _sipUdpIpV6Channel = null!;

        /// <summary>
        /// SipTCP通道(IPV4)
        /// </summary>
        private SIPTCPChannel _sipTcpIpV4Channel = null!;

        /// <summary>
        /// SipTCP通道(IPV6)
        /// </summary>
        private SIPTCPChannel _sipTcpIpV6Channel = null!;

        /// <summary>
        /// SIP传输通道(公开)
        /// </summary>
        public SIPTransport SipTransport
        {
            get => _sipTransport;
            set => _sipTransport = value;
        }


        /// <summary>
        /// 向通道发送Sip指令
        /// </summary>
        /// <param name="sipDevice"></param>
        /// <param name="method"></param>
        /// <param name="contentType"></param>
        /// <param name="subject"></param>
        /// <param name="xmlBody"></param>
        /// <param name="needResponse"></param>
        /// <param name="evnt"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private async Task SendRequestViaSipChannel(SipDevice sipDevice, SipChannel sipChannel, SIPMethodsEnum method,
            string contentType,
            string xmlBody, bool needResponse, AutoResetEvent evnt, int timeout)
        {
            IPAddress sipDeviceIpAddr = IPAddress.Parse(sipDevice.LastSipRequest.Header.Vias.Via[0].Host);
            int sipDevicePort = sipDevice.LastSipRequest.Header.Vias.Via[0].Port;
            var toSipUri = new SIPURI(SIPSchemesEnum.sip,
                new SIPEndPoint(SIPProtocolsEnum.udp, new IPEndPoint(sipDeviceIpAddr, sipDevicePort)));
            toSipUri.User = sipChannel.DeviceId;
            SIPToHeader to = new SIPToHeader(null, toSipUri, null);
            IPAddress sipServerIpAddress = IPAddress.Parse(Common.SipServerConfig.SipIpAddress);
            var fromSipUri = new SIPURI(SIPSchemesEnum.sip, sipServerIpAddress, Common.SipServerConfig.SipPort);
            fromSipUri.User = Common.SipServerConfig.ServerSipDeviceId;

            SIPFromHeader from = new SIPFromHeader(null, fromSipUri, "AKStream");

            bool isIpV6 = (sipDevice.SipChannelLayout!.ListeningIPAddress.AddressFamily == AddressFamily.InterNetworkV6)
                ? true
                : false;
            SIPRequest req = SIPRequest.GetRequest(method, toSipUri, to,
                from,
                new SIPEndPoint(sipDevice.SipChannelLayout.SIPProtocol,
                    new IPEndPoint(
                        isIpV6
                            ? IPAddress.Parse(Common.SipServerConfig.SipIpV6Address!)
                            : IPAddress.Parse(Common.SipServerConfig.SipIpAddress), sipDevice.SipChannelLayout.Port)));

            req.Header.Allow = null;
            req.Header.Contact = new List<SIPContactHeader>()
            {
                new SIPContactHeader(null, fromSipUri)
            };
            req.Header.UserAgent = ConstString.SIP_USERAGENT_STRING;
            req.Header.ContentType = contentType;
            req.Header.Subject =
                $"{sipChannel.DeviceId}:{0},{Common.SipServerConfig.ServerSipDeviceId}:{new Random().Next(100, ushort.MaxValue)}";
            req.Header.CallId = CallProperties.CreateNewCallId();
            req.Header.CSeq = UtilsHelper.CreateNewCSeq();
            req.Body = xmlBody;

            Logger.Debug($"[{Common.LoggerHead}]->发送Sip请求->{req}");
            if (needResponse)
            {
                var nrt = new NeedReturnTask(Common.NeedResponseRequests)
                {
                    AutoResetEvent = evnt,
                    CallId = req.Header.CallId,
                    SipRequest = req,
                    Timeout = timeout,
                    SipDevice = sipDevice,
                    SipChannel = sipChannel,
                };
                Common.NeedResponseRequests.TryAdd(req.Header.CallId, nrt);
            }

            sipChannel.InviteSipRequest = req;
            await _sipTransport.SendRequestAsync(sipDevice.RemoteEndPoint, req);
        }


        /// <summary>
        /// 向设备发送Sip指令
        /// </summary>
        /// <param name="sipDevice"></param>
        /// <param name="method"></param>
        /// <param name="contentType"></param>
        /// <param name="subject"></param>
        /// <param name="xmlBody"></param>
        /// <param name="needResponse"></param>
        /// <param name="evnt"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private async Task SendRequestViaSipDevice(SipDevice sipDevice, SIPMethodsEnum method, string contentType,
            string subject,
            string xmlBody, bool needResponse, AutoResetEvent evnt, int timeout)
        {
            var to = sipDevice.LastSipRequest!.Header.To;
            var from = sipDevice.LastSipRequest.Header.From;
            var fromUri = sipDevice.LastSipRequest.URI;

            bool isIpV6 = (sipDevice.SipChannelLayout!.ListeningIPAddress.AddressFamily == AddressFamily.InterNetworkV6)
                ? true
                : false;
            SIPRequest req = SIPRequest.GetRequest(method, sipDevice.ContactUri,
                new SIPToHeader(to.ToName, to.ToURI, ""), new SIPFromHeader(fromUri.User, fromUri, "AKStream"),
                new SIPEndPoint(sipDevice.SipChannelLayout.SIPProtocol,
                    new IPEndPoint(
                        isIpV6
                            ? IPAddress.Parse(Common.SipServerConfig.SipIpV6Address!)
                            : IPAddress.Parse(Common.SipServerConfig.SipIpAddress), sipDevice.SipChannelLayout.Port)));

            req.Header.Allow = null;
            req.Header.Contact = new List<SIPContactHeader>()
            {
                new SIPContactHeader(fromUri.User, fromUri)
            };
            req.Header.UserAgent = ConstString.SIP_USERAGENT_STRING;
            req.Header.ContentType = contentType;
            req.Header.Subject = subject;
            req.Body = xmlBody;
            req.Header.CallId = CallProperties.CreateNewCallId();
            req.Header.CSeq = UtilsHelper.CreateNewCSeq();
            Logger.Debug($"[{Common.LoggerHead}]->发送Sip请求->{req}");

            if (needResponse)
            {
                var nrt = new NeedReturnTask(Common.NeedResponseRequests)
                {
                    AutoResetEvent = evnt,
                    CallId = req.Header.CallId,
                    SipRequest = req,
                    Timeout = timeout,
                    SipDevice = sipDevice,
                };
                Common.NeedResponseRequests.TryAdd(req.Header.CallId, nrt);
            }

            await _sipTransport.SendRequestAsync(sipDevice.RemoteEndPoint, req);
        }


        /// <summary>
        /// 检测请求实时视频流参数是否正确
        /// </summary>
        /// <param name="sipChannel"></param>
        /// <param name="rs"></param>
        private void CheckInviteParam(SipChannel sipChannel, out ResponseStruct rs)
        {
            if (sipChannel == null) //传入参数是否正确
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ParamsIsNotRight,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
                };
                return;
            }

            var sipDevice = Common.SipDevices.FindLast(x => x.DeviceId.Equals(sipChannel.ParentId));
            if (sipDevice == null) //sip设备是否存在
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sip_Device_NotExists,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sip_Device_NotExists],
                };
                return;
            }

            var tmpSipChannel = sipDevice.SipChannels.FindLast(x => x.DeviceId.Equals(sipChannel.DeviceId));
            if (tmpSipChannel == null) //sip 通道是否存在
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sip_Channel_NotExists,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sip_Channel_NotExists],
                };
                return;
            }

            if (!sipChannel.SipChannelType.Equals(SipChannelType.VideoChannel) &&
                !sipChannel.SipChannelType.Equals(SipChannelType.AudioChannel)) //sip通道类型是否正确
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sip_OperationNotAllowed,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sip_OperationNotAllowed],
                };
                return;
            }

            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
        }


        private string MediaSdpCreate(SipChannel sipChannel, PushMediaInfo pushMediaInfo)
        {
            var tmpSipDevice = Common.SipDevices.FindLast(x => x.DeviceId.Equals(sipChannel.ParentId));
            if (tmpSipDevice != null)
            {
                var sdpConn = new SDPConnectionInformation(pushMediaInfo.MediaServerIpAddress);
                var sdp = new SDP()
                {
                    Version = 0,
                    SessionId = "0",
                    Username = Common.SipServerConfig.ServerSipDeviceId,
                    SessionName = CommandType.Play.ToString(),
                    Connection = sdpConn,
                    Timing = "0 0",
                    Address = pushMediaInfo.MediaServerIpAddress,
                };

                var psFormat = new SDPMediaFormat(SDPMediaFormatsEnum.PS)
                {
                    IsStandardAttribute = false,
                };
                var h264Format = new SDPMediaFormat(SDPMediaFormatsEnum.H264)
                {
                    IsStandardAttribute = false,
                };
                var media = new SDPMediaAnnouncement()
                {
                    Media = SDPMediaTypesEnum.video,
                    Port = pushMediaInfo.StreamPort,
                };
                media.MediaFormats.Add(psFormat);
                media.MediaFormats.Add(h264Format);
                media.AddExtra("a=recvonly");
                if (pushMediaInfo.PushStreamSocketType == PushStreamSocketType.TCP)
                {
                    media.Transport = "TCP/RTP/AVP";
                    media.AddExtra("a=setup:passive"); //active：主动模式，由摄像头告知服务器监听哪个端口，passive：被动模式，服务器告知摄像头连接端口
                    media.AddExtra("a=connection:new");
                }

                media.AddExtra("y=" + pushMediaInfo.Stream); //设置ssrc
                media.AddFormatParameterAttribute(psFormat.FormatID, psFormat.Name);
                media.AddFormatParameterAttribute(h264Format.FormatID, h264Format.Name);
                media.Port = pushMediaInfo.StreamPort;
                sdp.Media.Add(media);
                return sdp.ToString();
            }

            return "";
        }

        /// <summary>
        /// 请求实时视频流
        /// </summary>
        /// <param name="sipChannel"></param>
        public void Invite(SipChannel sipChannel, PushMediaInfo pushMediaInfo, AutoResetEvent evnt,
            out ResponseStruct rs, int timeout = 5000)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            CheckInviteParam(sipChannel, out rs); //检测各参数是否正常
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                return;
            }

            string mediaSdp = MediaSdpCreate(sipChannel, pushMediaInfo);
            var tmpSipDevice = Common.SipDevices.FindLast(x => x.DeviceId.Equals(sipChannel.ParentId));
            if (!string.IsNullOrEmpty(mediaSdp))
            {
                SIPMethodsEnum method = SIPMethodsEnum.INVITE;
                Func<SipDevice, SipChannel, SIPMethodsEnum, string, string, bool, AutoResetEvent, int, Task> request =
                    SendRequestViaSipChannel;
                request(tmpSipDevice, sipChannel, method, ConstString.Application_SDP, mediaSdp, true, evnt, timeout);
            }
        }

        /// <summary>
        /// 请求终止实时视频流
        /// </summary>
        /// <param name="sipChannel"></param>
        public void DeInvite(SipChannel sipChannel, AutoResetEvent evnt,
            out ResponseStruct rs, int timeout = 5000)
        {
            //请求终止实时视频流时，callid,from.tag,to.tag都要与invite时一致
            CheckInviteParam(sipChannel, out rs);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                return;
            }

            var tmpSipDevice = Common.SipDevices.FindLast(x => x.DeviceId.Equals(sipChannel.ParentId));
            SIPMethodsEnum method = SIPMethodsEnum.BYE;


            IPAddress sipDeviceIpAddr = IPAddress.Parse(tmpSipDevice.LastSipRequest.Header.Vias.Via[0].Host);
            int sipDevicePort = tmpSipDevice.LastSipRequest.Header.Vias.Via[0].Port;
            var toSipUri = new SIPURI(SIPSchemesEnum.sip,
                new SIPEndPoint(SIPProtocolsEnum.udp, new IPEndPoint(sipDeviceIpAddr, sipDevicePort)));
            toSipUri.User = sipChannel.DeviceId;
            SIPToHeader to = new SIPToHeader(null, toSipUri, null);
            IPAddress sipServerIpAddress = IPAddress.Parse(Common.SipServerConfig.SipIpAddress);
            var fromSipUri = new SIPURI(SIPSchemesEnum.sip, sipServerIpAddress, Common.SipServerConfig.SipPort);
            fromSipUri.User = Common.SipServerConfig.ServerSipDeviceId;

            SIPFromHeader from = new SIPFromHeader(null, fromSipUri, "AKStream");
            var fromUri = tmpSipDevice.LastSipRequest.URI;
            bool isIpV6 = (tmpSipDevice.SipChannelLayout!.ListeningIPAddress.AddressFamily ==
                           AddressFamily.InterNetworkV6)
                ? true
                : false;
            SIPRequest req = SIPRequest.GetRequest(method, toSipUri, to,
                from,
                new SIPEndPoint(tmpSipDevice.SipChannelLayout.SIPProtocol,
                    new IPEndPoint(
                        isIpV6
                            ? IPAddress.Parse(Common.SipServerConfig.SipIpV6Address!)
                            : IPAddress.Parse(Common.SipServerConfig.SipIpAddress),
                        tmpSipDevice.SipChannelLayout.Port)));

            //  req.Header.Vias.Via[0]=new SIPViaHeader(tmpSipDevice.LocalSipEndPoint.GetIPEndPoint(),tmpSipDevice.LastSipRequest.Header.Vias.Via[0].Branch);
            req.Header.CallId = sipChannel.InviteSipRequest.Header.CallId;
            req.Header.From = new SIPFromHeader(null, fromSipUri, sipChannel.InviteSipRequest.Header.From.FromTag);
            req.Header.To = new SIPToHeader(null, toSipUri, sipChannel.InviteSipResponse.Header.To.ToTag);
            req.Header.Contact = new List<SIPContactHeader>()
            {
                new SIPContactHeader(null, fromSipUri),
            };
            req.Header.Contact[0].ContactName = null;
            req.Header.Allow = null;
            req.Header.UserAgent = ConstString.SIP_USERAGENT_STRING;

            var nrt = new NeedReturnTask(Common.NeedResponseRequests)
            {
                AutoResetEvent = evnt,
                CallId = req.Header.CallId,
                SipRequest = req,
                Timeout = timeout,
                SipDevice = tmpSipDevice,
                SipChannel = sipChannel,
            };
            Common.NeedResponseRequests.TryAdd(req.Header.CallId, nrt);
            Logger.Debug($"[{Common.LoggerHead}]->发送终止时实流请求->{req}");
            _sipTransport.SendRequestAsync(tmpSipDevice.RemoteEndPoint, req);
        }

        /// <summary>
        /// 设备目录查询请求
        /// </summary>
        /// <param name="sipDeviceId"></param>
        public void DeviceCatalogQuery(SipDevice sipDevice, AutoResetEvent evnt, int timeout = 5000)
        {
            var tmpSipDevice = Common.SipDevices.FindLast(x => x.DeviceInfo!.DeviceID.Equals(sipDevice.DeviceId));
            if (tmpSipDevice != null)
            {
                SIPMethodsEnum method = SIPMethodsEnum.MESSAGE;
                string subject =
                    $"{Common.SipServerConfig.ServerSipDeviceId}:{0},{tmpSipDevice.DeviceInfo!.DeviceID}:{new Random().Next(100, ushort.MaxValue)}";
                CatalogQuery catalogQuery = new CatalogQuery()
                {
                    CommandType = CommandType.Catalog,
                    DeviceID = tmpSipDevice.DeviceInfo.DeviceID,
                    SN = new Random().Next(1, ushort.MaxValue),
                };
                string xmlBody = CatalogQuery.Instance.Save<CatalogQuery>(catalogQuery);

                //    private async Task SendRequest(SipDevice sipDevice, SIPMethodsEnum method, string contentType, string subject,
                // string xmlBody,bool needResponse,AutoResetEvent evnt,int timeout)
                Func<SipDevice, SIPMethodsEnum, string, string, string, bool, AutoResetEvent, int, Task> request =
                    SendRequestViaSipDevice;
                request(tmpSipDevice, method, ConstString.Application_MANSCDP, subject, xmlBody, true, evnt, timeout);
            }
        }


        /// <summary>
        /// 停止Sip服务
        /// </summary>
        /// <param name="rs"></param>
        /// <exception cref="AkStreamException"></exception>
        public void Stop(out ResponseStruct rs)
        {
            try
            {
                switch (Common.SipServerConfig.GbVersion.Trim().ToUpper())
                {
                    case "GB-2016":
                        _sipTransport.SIPTransportRequestReceived -= SipMsgProcess.SipTransportRequestReceived;
                        _sipTransport.SIPTransportResponseReceived -= SipMsgProcess.SipTransportResponseReceived;
                        break;
                }

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
                throw new AkStreamException(rs);
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


                switch (Common.SipServerConfig.GbVersion.Trim().ToUpper())
                {
                    case "GB-2016":
                        _sipTransport.SIPTransportRequestReceived += SipMsgProcess.SipTransportRequestReceived;
                        _sipTransport.SIPTransportResponseReceived += SipMsgProcess.SipTransportResponseReceived;
                        Task.Factory.StartNew(() => { SipMsgProcess.ProcessCatalogThread(); });

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
                throw new AkStreamException(rs);
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
                throw new AkStreamException(rs);
            }

            Common.SipServer = this;
            Logger.Info($"[{Common.LoggerHead}]->加载配置文件成功->{Common.SipServerConfigPath}");
        }
    }
}