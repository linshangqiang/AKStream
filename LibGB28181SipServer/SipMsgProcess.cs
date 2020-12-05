using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using GB28181.Sys.XML;
using LibCommon;
using LibLogger;
using SIPSorcery.SIP;

namespace LibGB28181SipServer
{
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
        public static event Action<Catalog> OnCatalogReceived = null!;

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
        public static event Action<SIPEndPoint, KeepAlive, string> OnKeepaliveReceived = null!;

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
        public static event Common.RegisterDelegate OnRegisterReceived = null!;

        /// <summary>
        /// 设备注销时
        /// </summary>
        public static event Common.UnRegisterDelegate OnUnRegisterReceived = null!;

        /// <summary>
        /// 设备有警告时
        /// </summary>
        public static event Common.DeviceAlarmSubscribeDelegate OnDeviceAlarmSubscribe = null!;

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
                var tmpSipDeviceList = Common.SipDevices.FindAll(x => x.DeviceInfo!.DeviceID.Equals(catalog.DeviceID));
                
                if (tmpSipDeviceList.Count > 0)
                {
                    foreach (var tmpSipDevice in tmpSipDeviceList)
                    {
                        foreach (var tmpChannelDev in catalog.DeviceList.Items)
                        {
                            lock (tmpSipDevice.SipChannelOptLock) //锁粒度在SipDevice中，不影响其他线程的效率
                            {
                                SipChannel sipChannelInList = tmpSipDevice.SipChannels!.FindLast(x =>
                                    x.SipChannelDesc.DeviceID.Equals(tmpChannelDev.DeviceID))!;
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
                                    };
                                    if (tmpChannelDev.InfList != null)
                                    {
                                        newSipChannnel.SipChannelDesc.InfList = tmpChannelDev.InfList;
                                    }

                                    newSipChannnel.SipChanneStatus = tmpChannelDev.Status;
                                    newSipChannnel.SipChannelType = Common.GetSipChannelType(tmpChannelDev.DeviceID);
                                    tmpSipDevice.SipChannels.Add(newSipChannnel);
                                    Logger.Info(
                                        $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的Sip设备通道信息->{tmpSipDevice.Guid}:{tmpSipDevice.DeviceInfo!.DeviceID}->增加Sip通道成功->({newSipChannnel.SipChannelType.ToString()})->{newSipChannnel.Guid}:{newSipChannnel.SipChannelDesc.DeviceID}->此设备当前通道数量:{tmpSipDevice.SipChannels.Count}条");
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

                Logger.Warn(
                    $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的设备目录,处理添加时出现异常情况->Sip设备{catalog.DeviceID}不在系统列表中，已跳过处理");
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
                        await SendKeepAliveOk(sipRequest);
                        string sipDeviceId = bodyXml.Element("DeviceID")?.Value.ToUpper()!;
                        var tmpSipDevice =
                            Common.SipDevices.FindLast((x => x.DeviceInfo!.DeviceID.Equals(sipDeviceId)));
                        if (tmpSipDevice != null)
                        {
                            tmpSipDevice.KeepAliveTime = DateTime.Now;
                            Logger.Debug(
                                $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的心跳->{sipRequest}");
                        }
                        break;
                    case "CATALOG": //处理设备目录
                        await SendOkMessage(sipRequest);
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
        public static void DoKickSipDevice(string guid)
        {
            var tmpSipDevice = Common.SipDevices.FindLast(x => x.Guid!.Equals(guid));
            string tmpSipDeviceStr = JsonHelper.ToJson(tmpSipDevice!);
            OnUnRegisterReceived?.Invoke(tmpSipDeviceStr); //先做外部处理，再做内部处理
            lock (Common.SipDevicesLock)
            {
                if (tmpSipDevice != null)
                {
                    Common.SipDevices.Remove(tmpSipDevice);
                    tmpSipDevice.SipChannels = null;
                    tmpSipDevice.Dispose();

                    Logger.Info(
                        $"[{Common.LoggerHead}]->Sip设备心跳丢失超过限制，已经注销->{tmpSipDeviceStr}");
                }
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
            //碰到await时会立即返回往下执行
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
                    var tmpSipDevice = Common.SipDevices.FindLast(x => x.DeviceInfo!.DeviceID.Equals(sipDeviceId));
                    if (tmpSipDevice != null)
                    {
                        try
                        {
                            await Task.Run(() =>
                            {
                                OnUnRegisterReceived?.Invoke(JsonHelper.ToJson(tmpSipDevice));
                            }); //抛线程出去处理
                            Logger.Info(
                                $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的Sip设备注销请求->{tmpSipDevice.Guid}:{tmpSipDevice.DeviceInfo!.DeviceID}->已经注销，当前Sip设备数量:{Common.SipDevices}个");

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
                            throw new AKStreamException(rs);
                        }
                    }
                }
                else
                {
                    //设备注册
                    var tmpSipDevice = Common.SipDevices.FindLast(x => x.DeviceInfo!.DeviceID.Equals(sipDeviceId));
                    if (tmpSipDevice == null)
                    {
                        tmpSipDevice = new SipDevice();
                        tmpSipDevice.KickMe += DoKickSipDevice;
                        tmpSipDevice.Guid = UtilsHelper.CreateGUID();
                        tmpSipDevice.Username = "";
                        tmpSipDevice.Password = "";
                        tmpSipDevice.RegisterTime = DateTime.Now;
                        tmpSipDevice.SipChannels = new List<SipChannel>();
                        tmpSipDevice.KeepAliveTime = DateTime.Now;
                        tmpSipDevice.KeepAliveLostTime = 0;
                        tmpSipDevice.DeviceInfo = new DeviceInfo();
                        tmpSipDevice.DeviceInfo.DeviceID = sipDeviceId;
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
                                if (Common.SipDevices.Count(x => x.DeviceInfo!.DeviceID.Equals(sipDeviceId)) <=
                                    0) //保证不存在
                                {
                                    Common.SipDevices.Add(tmpSipDevice);
                                    Task.Run(() =>
                                    {
                                        OnRegisterReceived?.Invoke(JsonHelper.ToJson(tmpSipDevice));
                                    }); //抛线程出去处理
                                    Logger.Info(
                                        $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的Sip设备注册请求->{tmpSipDevice.Guid}:{tmpSipDevice.DeviceInfo.DeviceID}->注册完成，当前Sip设备数量:{Common.SipDevices}个");
                                }
                                else
                                {
                                    Logger.Debug(
                                        $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的Sip设备注册请求->{tmpSipDevice.Guid}:{tmpSipDevice.DeviceInfo.DeviceID}->注册请求重复已经忽略，当前Sip设备数量:{Common.SipDevices}个");
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
                            throw new AKStreamException(rs);
                        }
                    }
                    else
                    {
                        if ((DateTime.Now - tmpSipDevice.RegisterTime).Seconds > Common.SIP_REGISTER_MIN_INTERVAL_SEC)
                        {
                            tmpSipDevice.RegisterTime = DateTime.Now;
                            await Task.Run(() =>
                            {
                                OnRegisterReceived?.Invoke(JsonHelper.ToJson(tmpSipDevice));
                            }); //抛线程出去处理

                            Logger.Info(
                                $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的Sip设备注册请求->{tmpSipDevice.Guid}:{tmpSipDevice.DeviceInfo!.DeviceID}->已经更新注册时间，当前Sip设备数量:{Common.SipDevices}个");
                        }
                        else
                        {
                            Logger.Debug(
                                $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的Sip设备异常注册请求->已忽略，当前Sip设备数量:{Common.SipDevices}个");
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
            var ret = Common.NeedResponseRequests.TryRemove(sipResponse.Header.CallId, out _);
            if (ret)
            {
                Logger.Debug(
                    $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的SipResponse->{sipResponse}这个消息是回复消息，callid:{sipResponse.Header.CallId}");
            }
            else
            {
                Logger.Debug(
                    $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的SipResponse->{sipResponse}");
            }
        }
    }
}