using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using LibCommon;
using LibCommon.Structs.GB28181;
using LibCommon.Structs.GB28181.XML;
using LibLogger;
using SIPSorcery.SIP;
using SIPSorcery.SIP.App;

namespace LibGB28181SipServer
{
    /// <summary>
    /// gb28181-2016
    /// </summary>
    public static class SipMsgProcess
    {
        #region 各类事件

        /// <summary>
        /// sip服务状态
        /// </summary>
        public static event Action<string, ServiceStatus> OnServiceChanged = null!;

        /// <summary>
        /// 录像文件接收
        /// </summary>
        public static event Action<RecordInfo> OnRecordInfoReceived = null!;

        /// <summary>
        /// 设备目录接收
        /// </summary>
        ///  public static event Action<Catalog> OnCatalogReceived = null!;
        public static event GCommon.CatalogReceived OnCatalogReceived = null!;

        /// <summary>
        /// 设备目录通知
        /// </summary>
        public static event Action<NotifyCatalog> OnNotifyCatalogReceived = null!;

        /// <summary>
        /// 语音广播通知
        /// </summary>
        public static event Action<VoiceBroadcastNotify> OnVoiceBroadcaseReceived = null!;

        /// <summary>
        /// 报警通知
        /// </summary>
        public static event Action<Alarm> OnAlarmReceived = null!;

        /// <summary>
        /// 平台之间心跳接收
        /// </summary>
        //  public static event Action<SIPEndPoint, KeepAlive, string> OnKeepaliveReceived = null!;
        public static event GCommon.KeepaliveReceived OnKeepaliveReceived = null!;

        /// <summary>
        /// 设备状态查询接收
        /// </summary>
        public static event Action<SIPEndPoint, DeviceStatus> OnDeviceStatusReceived = null!;

        /// <summary>
        /// 设备信息查询接收
        /// </summary>
        public static event Action<SIPEndPoint, DeviceInfo> OnDeviceInfoReceived = null!;

        /// <summary>
        /// 设备配置查询接收
        /// </summary>
        public static event Action<SIPEndPoint, DeviceConfigDownload> OnDeviceConfigDownloadReceived = null!;

        /// <summary>
        /// 历史媒体发送结束接收
        /// </summary>
        public static event Action<SIPEndPoint, MediaStatus> OnMediaStatusReceived = null!;

        /// <summary>
        /// 响应状态码接收
        /// </summary>
        public static event Action<SIPResponse, string, SIPEndPoint> OnResponseCodeReceived = null!;

        /// <summary>
        /// 响应状态码接收
        /// </summary>
        public static event Action<SIPResponse, SIPRequest, string, SIPEndPoint> OnResponseNeedResponeReceived = null!;

        /// <summary>
        /// 预置位查询接收
        /// </summary>,
        public static event Action<SIPEndPoint, PresetInfo> OnPresetQueryReceived = null!;

        /// <summary>
        /// 设备注册时
        /// </summary>
        public static event GCommon.RegisterDelegate OnRegisterReceived = null!;

        /// <summary>
        /// 设备注销时
        /// </summary>
        public static event GCommon.UnRegisterDelegate OnUnRegisterReceived = null!;

        /// <summary>
        /// 设备有警告时
        /// </summary>
        public static event GCommon.DeviceAlarmSubscribeDelegate OnDeviceAlarmSubscribe = null!;

        #endregion


        /// <summary>
        /// 普通消息回复状态OK
        /// </summary>
        /// <param name="sipRequest"></param>
        /// <returns></returns>
        private static async Task SendOkMessage(SIPRequest sipRequest)
        {
            SIPResponseStatusCodesEnum messaageResponse = SIPResponseStatusCodesEnum.Ok;
            SIPResponse okResponse = SIPResponse.GetResponse(sipRequest, messaageResponse, null);
            await Common.SipServer.SipTransport.SendResponseAsync(okResponse);
        }

        /// <summary>
        /// 处理设备目录添加
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="catalog"></param>
        /// <returns></returns>
        private static void ProcessGetDeviceItems(SIPEndPoint remoteEndPoint, Catalog catalog)
        {
            if (catalog != null)
            {
                var tmpSipDeviceList = Common.SipDevices.FindAll(x => x.DeviceId.Equals(catalog.DeviceID));

                if (tmpSipDeviceList.Count > 0)
                {
                    foreach (var tmpSipDevice in tmpSipDeviceList)
                    {
                        foreach (var tmpChannelDev in catalog.DeviceList.Items)
                        {
                            lock (tmpSipDevice.SipChannelOptLock) //锁粒度在SipDevice中，不影响其他线程的效率
                            {
                                SipChannel sipChannelInList = tmpSipDevice.SipChannels.FindLast(x =>
                                    x.SipChannelDesc.DeviceID.Equals(tmpChannelDev.DeviceID));
                                if (sipChannelInList == null)
                                {
                                    var newSipChannnel = new SipChannel()
                                    {
                                        Guid = UtilsHelper.CreateGUID()!,
                                        LastUpdateTime = DateTime.Now,
                                        LocalSipEndPoint = tmpSipDevice.LocalSipEndPoint!,
                                        ParentGuid = tmpSipDevice.Guid!,
                                        PushStatus = DevicePushStatus.IDLE,
                                        RemoteEndPoint = tmpSipDevice.RemoteEndPoint!,
                                        SipChannelDesc = tmpChannelDev,
                                        ParentId = tmpSipDevice.DeviceId,
                                        DeviceId = tmpChannelDev.DeviceID,
                                    };
                                    if (tmpChannelDev.InfList != null)
                                    {
                                        newSipChannnel.SipChannelDesc.InfList = tmpChannelDev.InfList;
                                    }

                                    newSipChannnel.SipChanneStatus = tmpChannelDev.Status;
                                    newSipChannnel.SipChannelType = Common.GetSipChannelType(tmpChannelDev.DeviceID);
                                    tmpSipDevice.SipChannels.Add(newSipChannnel);
                                    Task.Run(() => { OnCatalogReceived?.Invoke(newSipChannnel); }); //抛线程出去处理
                                    Logger.Info(
                                        $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的Sip设备通道信息->{tmpSipDevice.Guid}:{tmpSipDevice.DeviceId}->增加Sip通道成功->({newSipChannnel.SipChannelType.ToString()})->{newSipChannnel.Guid}:{newSipChannnel.SipChannelDesc.DeviceID}->此设备当前通道数量:{tmpSipDevice.SipChannels.Count}条");
                                }
                                else
                                {
                                    sipChannelInList.LastUpdateTime = DateTime.Now; //如果sip通道已经存在，则更新相关字段
                                }
                            }
                        }

                        tmpSipDevice.DeviceInfo!.Channel = tmpSipDevice.SipChannels!.Count;
                    }
                }
                else
                {

                    Logger.Warn(
                        $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的设备目录,处理添加时出现异常情况->Sip设备{catalog.DeviceID}不在系统列表中，已跳过处理");
                }
            }
        }


        /// <summary>
        /// 保持心跳时的回复
        /// </summary>
        /// <param name="sipRequest"></param>
        /// <returns></returns>
        private static async Task SendKeepAliveOk(SIPRequest sipRequest)
        {
            SIPResponseStatusCodesEnum keepAliveResponse = SIPResponseStatusCodesEnum.Ok;
            SIPResponse okResponse = SIPResponse.GetResponse(sipRequest, keepAliveResponse, null);
            await Common.SipServer.SipTransport.SendResponseAsync(okResponse);
        }

        /// <summary>
        /// 当收到心跳数据而Sip设备处于未注册状态，发送心跳异常给设备，让设备重新注册
        /// </summary>
        /// <param name="sipRequest"></param>
        /// <returns></returns>
        private static async Task SendKeepAliveExcept(SIPRequest sipRequest)
        {
            SIPResponseStatusCodesEnum keepAliveResponse = SIPResponseStatusCodesEnum.BadRequest;
            SIPResponse okResponse = SIPResponse.GetResponse(sipRequest, keepAliveResponse, null);
            await Common.SipServer.SipTransport.SendResponseAsync(okResponse);
        }


        /// <summary>
        /// 消息处理
        /// </summary>
        /// <param name="localSipChannel"></param>
        /// <param name="localSipEndPoint"></param>
        /// <param name="remoteEndPoint"></param>
        /// <param name="sipRequest"></param>
        /// <returns></returns>
        private static async Task MessageProcess(SIPChannel localSipChannel, SIPEndPoint localSipEndPoint,
            SIPEndPoint remoteEndPoint,
            SIPRequest sipRequest)
        {
            XElement bodyXml = XElement.Parse(sipRequest.Body);
            string cmdType = bodyXml.Element("CmdType")?.Value.ToUpper()!;
            if (!string.IsNullOrEmpty(cmdType))
            {
                switch (cmdType)
                {
                    case "KEEPALIVE": //处理心跳

                        string sipDeviceId = bodyXml.Element("DeviceID")?.Value.ToUpper()!;
                        var tmpSipDevice =
                            Common.SipDevices.FindLast((x => x.DeviceId.Equals(sipDeviceId)));
                        if (tmpSipDevice != null)
                        {
                            await SendKeepAliveOk(sipRequest);
                            var time = DateTime.Now;
                            Task.Run(() =>
                            {
                                OnKeepaliveReceived?.Invoke(sipDeviceId, time, tmpSipDevice.KeepAliveLostTime);
                            }); //抛线程出去处理

                            tmpSipDevice.KeepAliveTime = time;
                            Logger.Debug(
                                $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的心跳->{sipRequest}");
                        }
                        else
                        {
                            Logger.Debug(
                                $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的心跳->{sipRequest}->但是Sip设备不存在，发送BadRequest消息,使设备重新注册");
                            await SendKeepAliveExcept(sipRequest);
                        }

                        break;
                    case "CATALOG": //处理设备目录
                        await SendOkMessage(sipRequest);
                        Logger.Debug("收到目录信息"+sipRequest.ToString());
                        ProcessGetDeviceItems(remoteEndPoint, UtilsHelper.XMLToObject<Catalog>(bodyXml));
                        break;
                }
            }
        }


        /// <summary>
        /// 处理心跳检测失败的设备，认为这类设备已经离线，需要踢除
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static void DoKickSipDevice(SipDevice sipDevice)
        {
            string tmpSipDeviceStr = JsonHelper.ToJson(sipDevice);
            Task.Run(() => { OnUnRegisterReceived?.Invoke(tmpSipDeviceStr); }); //抛线程出去处理

            lock (Common.SipDevicesLock)
            {
                Common.SipDevices.Remove(sipDevice);
                sipDevice.SipChannels = null!;
                sipDevice.Dispose();
                Logger.Info(
                    $"[{Common.LoggerHead}]->Sip设备心跳丢失超过限制，已经注销->{tmpSipDeviceStr}");
            }

            Logger.Debug(
                $"[{Common.LoggerHead}]->当前Sip设备列表数量:->{Common.SipDevices.Count}");
        }

        /// <summary>
        /// 处理sip设备注册事件
        /// </summary>
        /// <param name="localSipChannel"></param>
        /// <param name="localSipEndPoint"></param>
        /// <param name="remoteEndPoint"></param>
        /// <param name="sipRequest"></param>
        /// <returns></returns>
        private static async Task RegisterProcess(SIPChannel localSipChannel, SIPEndPoint localSipEndPoint,
            SIPEndPoint remoteEndPoint,
            SIPRequest sipRequest)
        {
            Logger.Debug(
                $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的Sip设备注册信息->{sipRequest}");

            string sipDeviceId = sipRequest.Header.From.FromURI.User;

            SIPResponse tryingResponse = SIPResponse.GetResponse(sipRequest, SIPResponseStatusCodesEnum.Trying, null);
            await Common.SipServer.SipTransport.SendResponseAsync(tryingResponse);
            SIPResponseStatusCodesEnum registerResponse = SIPResponseStatusCodesEnum.Ok;
            if (sipRequest.Header.Contact?.Count > 0)
            {
                int expiry = sipRequest.Header.Contact[0].Expires > 0
                    ? sipRequest.Header.Contact[0].Expires
                    : sipRequest.Header.Expires;
                if (expiry <= 0)
                {
                    //注销设备
                    var tmpSipDevice = Common.SipDevices.FindLast(x => x.DeviceId.Equals(sipDeviceId));
                    if (tmpSipDevice != null)
                    {
                        try
                        {
                            Task.Run(() =>
                            {
                                OnUnRegisterReceived?.Invoke(JsonHelper.ToJson(tmpSipDevice));
                            }); //抛线程出去处理

                            Logger.Info(
                                $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的Sip设备注销请求->{tmpSipDevice.Guid}:{tmpSipDevice.DeviceId}->已经注销，当前Sip设备数量:{Common.SipDevices}个");

                            lock (Common.SipDevicesLock)
                            {
                                Common.SipDevices.Remove(tmpSipDevice);
                                tmpSipDevice.Dispose();
                            }
                        }
                        catch (Exception ex)
                        {
                            ResponseStruct rs = new ResponseStruct()
                            {
                                Code = ErrorNumber.Sip_Except_DisposeSipDevice,
                                Message = ErrorMessage.ErrorDic![ErrorNumber.Sip_Except_DisposeSipDevice],
                                ExceptMessage = ex.Message,
                                ExceptStackTrace = ex.StackTrace,
                            };
                            throw new AkStreamException(rs);
                        }
                    }
                }
                else
                {
                    if (Common.SipServerConfig.Authentication)
                    {
                        if (sipRequest.Header.AuthenticationHeader == null)
                        {
                            SIPAuthenticationHeader authHeader =
                                new SIPAuthenticationHeader(SIPAuthorisationHeadersEnum.WWWAuthenticate,
                                    Common.SipServerConfig.Realm, SIPRequestAuthenticator.GetNonce());
                            var unAuthorisedHead =
                                new SIPRequestAuthenticationResult(SIPResponseStatusCodesEnum.Unauthorised, authHeader);
                            unAuthorisedHead.AuthenticationRequiredHeader.SIPDigest.Opaque = "";
                            unAuthorisedHead.AuthenticationRequiredHeader.SIPDigest.Algorithhm =
                                SIPAuthorisationDigest.AUTH_ALGORITHM;

                            var unAuthorizedResponse = SIPResponse.GetResponse(sipRequest,
                                SIPResponseStatusCodesEnum.Unauthorized, null);
                            unAuthorizedResponse.Header.AuthenticationHeader =
                                unAuthorisedHead.AuthenticationRequiredHeader;
                            unAuthorizedResponse.Header.Allow = null;
                            unAuthorizedResponse.Header.Expires = 7200;
                            await Common.SipServer.SipTransport.SendResponseAsync(unAuthorizedResponse);
                            return;
                        }
                        else
                        {
                            /*GB28181Sip注册鉴权算法：
                            HA1=MD5(username:realm:passwd) //username和realm在字段“Authorization”中可以找到，passwd这个是由客户端和服务器协商得到的，一般情况下UAC端存一个UAS也知道的密码就行了
                             HA2=MD5(Method:Uri)//Method一般有INVITE, ACK, OPTIONS, BYE, CANCEL, REGISTER；Uri可以在字段“Authorization”找到
                             response = MD5(HA1:nonce:HA2)
                             */
                            string ha1 = UtilsHelper.Md5(sipRequest.Header.AuthenticationHeader.SIPDigest.Username +
                                                         ":" + sipRequest.Header.AuthenticationHeader.SIPDigest.Realm +
                                                         ":" + Common.SipServerConfig.SipPassword);

                            string ha2 = UtilsHelper.Md5("REGISTER" + ":" +
                                                         sipRequest.Header.AuthenticationHeader.SIPDigest.URI);
                            string ha3 = UtilsHelper.Md5(ha1 + ":" +
                                                         sipRequest.Header.AuthenticationHeader.SIPDigest.Nonce + ":" +
                                                         ha2);
                            if (!ha3.Equals(sipRequest.Header.AuthenticationHeader.SIPDigest.Response))
                            {
                                Logger.Debug(
                                    $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的Sip设备注册请求->鉴权失败,注册失败");
                                SIPRequest req = SIPRequest.GetRequest(SIPMethodsEnum.BYE, sipRequest.URI);
                                req.Header.CallId = sipRequest.Header.CallId;
                                req.Header.From.FromTag = sipRequest.Header.From.FromTag;
                                req.Header.To.ToTag = sipRequest.Header.To.ToTag;
                                await Common.SipServer.SipTransport.SendRequestAsync(remoteEndPoint, req);
                                return; //验证通不过就不再回复，验证通过的话，就会往下走
                            }
                        }
                    }

                    //设备注册
                    var tmpSipDevice = Common.SipDevices.FindLast(x => x.DeviceId.Equals(sipDeviceId));
                    if (tmpSipDevice == null)
                    {
                        tmpSipDevice = new SipDevice(Common.SipServerConfig);
                        tmpSipDevice.KickMe += DoKickSipDevice;
                        tmpSipDevice.Guid = UtilsHelper.CreateGUID();
                        tmpSipDevice.Username = "";
                        tmpSipDevice.Password = "";
                        tmpSipDevice.RegisterTime = DateTime.Now;
                        tmpSipDevice.SipChannels = new List<SipChannel>();
                        tmpSipDevice.KeepAliveTime = DateTime.Now;
                        tmpSipDevice.KeepAliveLostTime = 0;
                        tmpSipDevice.DeviceInfo!.DeviceID = sipDeviceId;
                        tmpSipDevice.DeviceId = sipDeviceId;
                        tmpSipDevice.LocalSipEndPoint = localSipEndPoint;
                        tmpSipDevice.RemoteEndPoint = remoteEndPoint;
                        tmpSipDevice.SipChannelLayout = localSipChannel;
                        tmpSipDevice.IpAddress = remoteEndPoint.Address;
                        tmpSipDevice.Port = remoteEndPoint.Port;
                        tmpSipDevice.FirstSipRequest = sipRequest;
                        tmpSipDevice.ContactUri = sipRequest.Header.Contact[0].ContactURI;
                        try
                        {
                            lock (Common.SipDevicesLock)
                            {
                                if (Common.SipDevices.Count(x => x.DeviceId.Equals(sipDeviceId)) <=
                                    0) //保证不存在
                                {
                                    Common.SipDevices.Add(tmpSipDevice);
                                    Task.Run(() =>
                                    {
                                        OnRegisterReceived?.Invoke(JsonHelper.ToJson(tmpSipDevice));
                                    }); //抛线程出去处理

                                    Logger.Info(
                                        $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的Sip设备注册请求->{tmpSipDevice.Guid}:{tmpSipDevice.DeviceId}->注册完成，当前Sip设备数量:{Common.SipDevices.Count}个");
                                }
                                else
                                {
                                    Logger.Debug(
                                        $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的Sip设备注册请求->{tmpSipDevice.Guid}:{tmpSipDevice.DeviceId}->注册请求重复已经忽略，当前Sip设备数量:{Common.SipDevices.Count}个");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ResponseStruct rs = new ResponseStruct()
                            {
                                Code = ErrorNumber.Sip_Except_RegisterSipDevice,
                                Message = ErrorMessage.ErrorDic![ErrorNumber.Sip_Except_RegisterSipDevice],
                                ExceptMessage = ex.Message,
                                ExceptStackTrace = ex.StackTrace,
                            };
                            throw new AkStreamException(rs);
                        }
                    }
                    else
                    {
                        if ((DateTime.Now - tmpSipDevice.RegisterTime).Seconds > Common.SIP_REGISTER_MIN_INTERVAL_SEC)
                        {
                            tmpSipDevice.RegisterTime = DateTime.Now;

                            Task.Run(() => { OnRegisterReceived?.Invoke(JsonHelper.ToJson(tmpSipDevice)); }); //抛线程出去处理


                            Logger.Info(
                                $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的Sip设备注册请求->{tmpSipDevice.Guid}:{tmpSipDevice.DeviceId}->已经更新注册时间，当前Sip设备数量:{Common.SipDevices.Count}个");
                        }
                        else
                        {
                            Logger.Debug(
                                $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的Sip设备异常注册请求->已忽略，当前Sip设备数量:{Common.SipDevices.Count}个");
                        }
                    }
                }
            }
            else
            {
                registerResponse = SIPResponseStatusCodesEnum.BadRequest;
            }

            SIPNonInviteTransaction registerTransaction =
                new SIPNonInviteTransaction(Common.SipServer.SipTransport, sipRequest, null);
            SIPResponse retResponse = SIPResponse.GetResponse(sipRequest, registerResponse, null);
            registerTransaction.SendResponse(retResponse);
        }


        /// <summary>
        /// SipRequest数据处理
        /// </summary>
        /// <param name="localSipChannel"></param>
        /// <param name="localSipEndPoint"></param>
        /// <param name="remoteEndPoint"></param>
        /// <param name="sipRequest"></param>
        /// <returns></returns>
        public static async Task SipTransportRequestReceived(SIPChannel localSipChannel, SIPEndPoint localSipEndPoint,
            SIPEndPoint remoteEndPoint,
            SIPRequest sipRequest)
        {
            switch (sipRequest.Method)
            {
                case SIPMethodsEnum.REGISTER: //处理注册
                    await RegisterProcess(localSipChannel, localSipEndPoint, remoteEndPoint, sipRequest);
                    break;
                case SIPMethodsEnum.MESSAGE: //心跳、目录查询、设备信息、设备状态等消息的内容处理
                    await MessageProcess(localSipChannel, localSipEndPoint, remoteEndPoint, sipRequest);
                    break;
            }
        }

        /// <summary>
        /// sip Response 处理
        /// </summary>
        /// <param name="localSipChannel"></param>
        /// <param name="localSipEndPoint"></param>
        /// <param name="remoteEndPoint"></param>
        /// <param name="sipResponse"></param>
        /// <returns></returns>
        public static async Task SipTransportResponseReceived(SIPChannel localSipChannel, SIPEndPoint localSipEndPoint,
            SIPEndPoint remoteEndPoint,
            SIPResponse sipResponse)
        {
            var ret = Common.NeedResponseRequests.TryRemove(sipResponse.Header.CallId, out NeedReturnTask _task);
            if (ret && _task != null)
            {
                Logger.Debug(
                    $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的SipResponse->{sipResponse}这个消息是回复消息，callid:{sipResponse.Header.CallId}");
                _task.AutoResetEvent.Set();//通知调用者任务完成,凋用者后续要做dispose操作
            }
            else
            {
                Logger.Debug(
                    $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的SipResponse->{sipResponse}"); 
            }
            
            
            
        }
    }
}