using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using GB28181.Sys.XML;
using LibCommon;
using SIPSorcery.SIP;
using SIPSorcery.SIP.App;

#pragma warning disable 4014

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


        private static async Task SendKeepAliveOk(SIPRequest sipRequest)
        {
            SIPResponseStatusCodesEnum keepAliveResponse = SIPResponseStatusCodesEnum.Ok;
            SIPResponse okResponse = SIPResponse.GetResponse(sipRequest, keepAliveResponse, null);
            await Common.SipServer.SipTransport.SendResponseAsync(okResponse);
        }

        private static async Task MessageProcess(SIPEndPoint localSipEndPoint, SIPEndPoint remoteEndPoint,
            SIPRequest sipRequest)
        {
            string bodyStr = sipRequest.Body;
            XElement bodyXml = XElement.Parse(bodyStr);
            string cmdType = bodyXml.Element("CmdType")?.Value.ToUpper()!;
            if (!string.IsNullOrEmpty(cmdType))
            {
                switch (cmdType)
                {
                    case "KEEPALIVE": //处理心跳
                        string sipDeviceId = bodyXml.Element("DeviceID")?.Value.ToUpper()!;
                        var tmpSipDevice = Common.SipDevices.FindLast((x => x.DeviceInfo.DeviceID.Equals(sipDeviceId)));
                        if (tmpSipDevice != null)
                        {
                            tmpSipDevice.KeepAliveTime = DateTime.Now;
                            await SendKeepAliveOk(sipRequest);
                            LibLogger.Logger.Debug(
                                $"[{Common.LoggerHead}]->收到来自{remoteEndPoint.ToString()}的心跳->\r\n{sipRequest}\r\n");
                        }

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
            var tmpSipDevice = Common.SipDevices.FindLast(x => x.Guid.Equals(guid));
            string tmpSipDeviceStr = JsonHelper.ToJson(tmpSipDevice);
            OnUnRegisterReceived?.Invoke(tmpSipDeviceStr);

            lock (Common.SipDevicesLock)
            {
                if (tmpSipDevice != null)
                {
                    Common.SipDevices.Remove(tmpSipDevice);
                    tmpSipDevice.Dispose();

                    LibLogger.Logger.Info(
                        $"[{Common.LoggerHead}]->Sip设备心跳丢失超过限制，已经注销->{tmpSipDeviceStr}");
                }
            }

            LibLogger.Logger.Debug(
                $"[{Common.LoggerHead}]->当前Sip设备列表数量:->{Common.SipDevices.Count}");
        }

        /// <summary>
        /// 处理sip设备注册事件
        /// </summary>
        /// <param name="localSipEndPoint"></param>
        /// <param name="remoteEndPonit"></param>
        /// <param name="sipRequest"></param>
        /// <returns></returns>
        private static async Task RegisterProcess(SIPEndPoint localSipEndPoint, SIPEndPoint remoteEndPoint,
            SIPRequest sipRequest)
        {
            LibLogger.Logger.Debug(
                $"[{Common.LoggerHead}]->收到来自{remoteEndPoint.ToString()}的Sip设备注册信息->\r\n{sipRequest}\r\n");

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
                    var tmpSipDevice = Common.SipDevices.FindLast(x => x.DeviceInfo.DeviceID.Equals(sipDeviceId));
                    if (tmpSipDevice != null)
                    {
                        try
                        {
                            OnUnRegisterReceived?.Invoke(JsonHelper.ToJson(tmpSipDevice));
                            LibLogger.Logger.Info(
                                $"[{Common.LoggerHead}]->Sip设备销完成->{JsonHelper.ToJson(tmpSipDevice)}");
                            lock (Common.SipDevicesLock)
                            {
                                Common.SipDevices.Remove(tmpSipDevice);
                                tmpSipDevice.Dispose();
                            }

                            LibLogger.Logger.Debug(
                                $"[{Common.LoggerHead}]->当前Sip设备列表数量:->{Common.SipDevices.Count}");
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
                    var tmpSipDevice = Common.SipDevices.FindLast(x => x.DeviceInfo.DeviceID.Equals(sipDeviceId));
                    if (tmpSipDevice == null)
                    {
                        tmpSipDevice = new SipDevice();
                        tmpSipDevice.KickMe += DoKickSipDevice;
                        tmpSipDevice.Guid = UtilsHelper.generalGuid();
                        tmpSipDevice.Username = "";
                        tmpSipDevice.Password = "";
                        tmpSipDevice.IpAddress = remoteEndPoint.GetIPEndPoint().Address.ToString();
                        tmpSipDevice.RegisterTime = DateTime.Now;
                        tmpSipDevice.SipChannels = new List<SipChannel>();
                        tmpSipDevice.KeepAliveTime = DateTime.Now;
                        tmpSipDevice.KeepAliveLostTime = 0;
                        tmpSipDevice.DeviceInfo = new DeviceInfo();
                        tmpSipDevice.DeviceInfo.DeviceID = sipDeviceId;
                        try
                        {
                            lock (Common.SipDevicesLock)
                            {
                                Common.SipDevices.Add(tmpSipDevice);
                            }

                            OnRegisterReceived?.Invoke(JsonHelper.ToJson(tmpSipDevice));
                            LibLogger.Logger.Debug(
                                $"[{Common.LoggerHead}]->当前Sip设备列表数量:->{Common.SipDevices.Count}");
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

                        LibLogger.Logger.Info(
                            $"[{Common.LoggerHead}]->Sip设备注册完成->{JsonHelper.ToJson(tmpSipDevice)}");
                    }
                    else
                    {
                        if ((DateTime.Now - tmpSipDevice.RegisterTime).Seconds > Common.SIP_REGISTER_MIN_INTERVAL_SEC)
                        {
                            tmpSipDevice.RegisterTime = DateTime.Now;
                            OnRegisterReceived?.Invoke(JsonHelper.ToJson(tmpSipDevice));
                            LibLogger.Logger.Info(
                                $"[{Common.LoggerHead}]->Sip设备发起重新注册事件，已更新注册时间->{JsonHelper.ToJson(tmpSipDevice)}");
                        }
                        else
                        {
                            LibLogger.Logger.Debug(
                                $"[{Common.LoggerHead}]->Sip设备注册消息重复发送，已忽略->{sipRequest.ToString()}");
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
        /// <param name="localSipEndPoint"></param>
        /// <param name="remoteEndPoint"></param>
        /// <param name="sipRequest"></param>
        /// <returns></returns>
        public static async Task SipTransportRequestReceived(SIPEndPoint localSipEndPoint,
            SIPEndPoint remoteEndPoint,
            SIPRequest sipRequest)
        {
            switch (sipRequest.Method)
            {
                case SIPMethodsEnum.REGISTER: //处理注册
                    await Task.Run(() => { RegisterProcess(localSipEndPoint, remoteEndPoint, sipRequest); });
                    break;
                case SIPMethodsEnum.MESSAGE: //心跳、目录查询、设备信息、设备状态等消息的内容处理
                    await Task.Run(() => { MessageProcess(localSipEndPoint, remoteEndPoint, sipRequest); });
                    break;
            }
        }

        /// <summary>
        /// sip Response 处理
        /// </summary>
        /// <param name="localSipEndPoint"></param>
        /// <param name="remoteEndPoint"></param>
        /// <param name="sipResponse"></param>
        /// <returns></returns>
        public static async Task SipTransportResponseReceived(SIPEndPoint localSipEndPoint,
            SIPEndPoint remoteEndPoint,
            SIPResponse sipResponse)
        {
            LibLogger.Logger.Debug(
                $"[{Common.LoggerHead}]->收到来自{remoteEndPoint.ToString()}的SipResponse->{sipResponse.ToString()}");
        }
    }
}