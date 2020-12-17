using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using LibCommon;
using LibCommon.Enums;
using LibCommon.Structs;
using LibCommon.Structs.GB28181;
using LibCommon.Structs.GB28181.Net.SIP;
using LibCommon.Structs.GB28181.XML;
using LibLogger;
using Newtonsoft.Json;
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
        public static event GCommon.KeepaliveReceived OnKeepaliveReceived = null!;


        /// <summary>
        /// 设备就绪通知，当设备准备好的时候触发
        /// </summary>
        public static event GCommon.SipDeviceReadyReceived OnDeviceReadyReceived = null;

        /// <summary>
        /// 设备状态查询接收
        /// </summary>
        public static event GCommon.DeviceStatusReceived OnDeviceStatusReceived = null;

        /// <summary>
        /// 点播完成或结束事件
        /// </summary>
        public static event GCommon.InviteHistroyVideoFinished OnInviteHistoryVideoFinished = null;

        /// <summary>
        /// 设备信息查询接收
        /// </summary>
        public static event GCommon.DeviceInfoReceived OnDeviceInfoReceived = null;


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
        /// 线程处理队列中的设备目录
        /// </summary>
        public static void ProcessCatalogThread()
        {
            while (true)
            {
                while (!Common.TmpCatalogs.IsEmpty)
                {
                    var ret = Common.TmpCatalogs.TryDequeue(out Catalog tmpCatalog);
                    if (ret && tmpCatalog != null)
                    {
                        try
                        {
                            InsertDeviceItems(tmpCatalog);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(
                                $"[{Common.LoggerHead}]->插入设备目录时发生异常->{ex.Message}\r\n{ex.StackTrace}");
                        }
                    }

                    Thread.Sleep(10);
                }

                Thread.Sleep(1000);
            }
        }


        /// <summary>
        /// 处理设备目录添加
        /// </summary>
        /// <param name="catalog"></param>
        /// <returns></returns>
        private static void InsertDeviceItems(Catalog catalog)
        {
            if (catalog != null)
            {
                var tatolNum = catalog.SumNum;
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
                                    var newSipChannel = new SipChannel()
                                    {
                                        LastUpdateTime = DateTime.Now,
                                        LocalSipEndPoint = tmpSipDevice.LocalSipEndPoint!,
                                        PushStatus = PushStatus.IDLE,
                                        RemoteEndPoint = tmpSipDevice.RemoteEndPoint!,
                                        SipChannelDesc = tmpChannelDev,
                                        ParentId = tmpSipDevice.DeviceId,
                                        DeviceId = tmpChannelDev.DeviceID,
                                    };
                                    if (tmpChannelDev.InfList != null)
                                    {
                                        newSipChannel.SipChannelDesc.InfList = tmpChannelDev.InfList;
                                    }

                                    newSipChannel.SipChannelStatus = tmpChannelDev.Status;
                                    newSipChannel.SipChannelType = Common.GetSipChannelType(tmpChannelDev.DeviceID);
                                    if (newSipChannel.SipChannelType == SipChannelType.AudioChannel ||
                                        newSipChannel.SipChannelType == SipChannelType.VideoChannel)
                                    {
                                        var canPushChannelCount = tmpSipDevice.SipChannels.Count(x =>
                                            x.SipChannelType == SipChannelType.VideoChannel ||
                                            x.SipChannelType == SipChannelType.AudioChannel); //可推流设备被记录序号
                                        var tag = "0" + newSipChannel.DeviceId.Substring(3, 5) +
                                                  canPushChannelCount.ToString().PadLeft(4, '0') +
                                                  tmpSipDevice.DeviceId + newSipChannel.DeviceId;
                                        var crc32 = CRC32Helper.GetCRC32(tag);
                                        var crc32Str = crc32.ToString().PadLeft(10, '0');
                                        char[] tmpChars = crc32Str.ToCharArray();
                                        tmpChars[0] = '0'; //实时流ssrc第一位是0
                                        string ssrcid = new string(tmpChars);
                                        newSipChannel.SsrcId = ssrcid;
                                        newSipChannel.Stream =
                                            string.Format("{0:X8}", uint.Parse(ssrcid)); //ssrc的16进制表示
                                    }

                                    tmpSipDevice.SipChannels.Add(newSipChannel);


                                    Task.Run(() => { OnCatalogReceived?.Invoke(newSipChannel); }); //抛线程出去处理
                                    Logger.Info(
                                        $"[{Common.LoggerHead}]->Sip设备通道信息->{tmpSipDevice.DeviceId}->增加Sip通道成功->({newSipChannel.SipChannelType.ToString()})->{newSipChannel.SipChannelDesc.DeviceID}->此设备当前通道数量:{tmpSipDevice.SipChannels.Count}条");
                                }
                                else
                                {
                                    sipChannelInList.LastUpdateTime = DateTime.Now; //如果sip通道已经存在，则更新相关字段
                                    sipChannelInList.SipChannelStatus = tmpChannelDev.Status;
                                    sipChannelInList.SipChannelDesc = tmpChannelDev;
                                    if (tmpChannelDev.InfList != null)
                                    {
                                        sipChannelInList.SipChannelDesc.InfList = tmpChannelDev.InfList;
                                    }
                                }
                            }

                            if (tmpSipDevice.SipChannels.Count >= tatolNum || tatolNum > 256
                            ) //当设备数量与设备总数相等或者设备数量很大，需要很长时间时通知外部获取成功
                            {
                                var _taskTag = $"CATALOG:{tmpSipDevice.DeviceId}";
                                var ret = Common.NeedResponseRequests.TryRemove(_taskTag,
                                    out NeedReturnTask _task);
                                if (ret && _task != null && _task.AutoResetEvent2 != null)
                                {
                                    _task.AutoResetEvent2.Set();
                                }
                            }
                        }

                        tmpSipDevice.DeviceInfo!.Channel = tmpSipDevice.SipChannels!.Count;
                    }
                }
                else
                {
                    Logger.Warn(
                        $"[{Common.LoggerHead}]->处理添加时出现异常情况->Sip设备{catalog.DeviceID}不在系统列表中，已跳过处理");
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
                            if (!tmpSipDevice.IsReday)
                            {
                                tmpSipDevice.IsReday = true; //设备已就绪
                                Task.Run(() => //设备就绪通知
                                {
                                    OnDeviceReadyReceived?.Invoke(tmpSipDevice);
                                });
                            }

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
                        Logger.Debug("------------------>" + sipRequest.Body);
                        Common.TmpCatalogs.Enqueue(UtilsHelper.XMLToObject<Catalog>(bodyXml));
                        Logger.Debug(
                            $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}设备目录信息->{sipRequest}");

                        break;
                    case "DEVICEINFO":
                        await SendOkMessage(sipRequest);
                        var tmpDeviceInfo = UtilsHelper.XMLToObject<DeviceInfo>(bodyXml);
                        if (tmpDeviceInfo != null)
                        {
                            var tmpSipDeviceFind =
                                Common.SipDevices.FindLast(x => x.DeviceId.Equals(tmpDeviceInfo.DeviceID));
                            if (tmpSipDeviceFind != null)
                            {
                                Task.Run(() =>
                                {
                                    OnDeviceInfoReceived?.Invoke(tmpSipDeviceFind, tmpDeviceInfo);
                                }); //抛线程出去处理
                                tmpSipDeviceFind.DeviceInfo = tmpDeviceInfo;
                                tmpSipDeviceFind.DeviceInfo.Channel = tmpSipDeviceFind.SipChannels.Count;
                                Logger.Debug(
                                    $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}设备信息->{sipRequest}");
                            }
                        }

                        break;
                    case "DEVICESTATUS":
                        await SendOkMessage(sipRequest);
                        var tmpDeviceStatus = UtilsHelper.XMLToObject<DeviceStatus>(bodyXml);
                        if (tmpDeviceStatus != null)
                        {
                            var tmpSipDeviceFind =
                                Common.SipDevices.FindLast(x => x.DeviceId.Equals(tmpDeviceStatus.DeviceID));
                            if (tmpSipDeviceFind != null)
                            {
                                Task.Run(() =>
                                {
                                    OnDeviceStatusReceived?.Invoke(tmpSipDeviceFind, tmpDeviceStatus);
                                }); //抛线程出去处理

                                tmpSipDeviceFind.DeviceStatus = tmpDeviceStatus;
                                Logger.Debug(
                                    $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}设备状态信息->{sipRequest}");
                            }
                        }

                        break;
                    case "RECORDINFO":
                        await SendOkMessage(sipRequest);
                        string tmpSipDevId = sipRequest.Header.From.FromURI.User;
                        var recordInfo = UtilsHelper.XMLToObject<RecordInfo>(bodyXml);
                        var tmpSipDevice1 = Common.SipDevices.FindLast(x => x.DeviceId.Equals(tmpSipDevId));
                        if (recordInfo != null && tmpSipDevice1 != null)
                        {
                            int tatolNum = recordInfo.SumNum;

                            int sn = recordInfo.SN;
                            var sipChannel1 =
                                tmpSipDevice1.SipChannels.FindLast(x => x.DeviceId.Equals(recordInfo.DeviceID));
                            if (sipChannel1 != null)
                            {
                                if (sipChannel1.LastRecordInfos.Count > 0 &&
                                    sipChannel1.LastRecordInfos[0].Key.Equals(sn))
                                {
                                    foreach (var item in recordInfo.RecordItems.Items)
                                    {
                                        var tmp = sipChannel1.LastRecordInfos.FindLast(x =>
                                            x.Value.FilePath.Equals(item.FilePath));
                                        if (tmp.Value == null)
                                        {
                                            var tag = item.Address + item.Name + item.Secrecy + item.Type +
                                                      item.EndTime +
                                                      item.FilePath + item.StartTime + item.DeviceID + item.RecorderID +
                                                      tmpSipDevice1.DeviceId;
                                            var crc32 = CRC32Helper.GetCRC32(tag);
                                            var crc32Str = crc32.ToString().PadLeft(10, '0');
                                            char[] tmpChars = crc32Str.ToCharArray();
                                            tmpChars[0] = '1'; //回放流的ssrc第一位是1
                                            string itemId = new string(tmpChars);
                                            item.SsrcId = itemId; //ssrc的值
                                            item.Stream = string.Format("{0:X8}", uint.Parse(itemId)); //ssrc的16进制表示
                                            item.SipDevice = tmpSipDevice1;
                                            item.SipChannel = sipChannel1;
                                            item.PushStatus = PushStatus.IDLE;
                                            item.MediaServerStreamInfo = new MediaServerStreamInfo();
                                            sipChannel1.LastRecordInfos.Add(
                                                new KeyValuePair<int, RecordInfo.Item>(sn, item));
                                        }
                                    }
                                }

                                if (sipChannel1.LastRecordInfos.Count > 0 &&
                                    !sipChannel1.LastRecordInfos[0].Key.Equals(sn))
                                {
                                    sipChannel1.LastRecordInfos.Clear();
                                    foreach (var item in recordInfo.RecordItems.Items)
                                    {
                                        var tmp = sipChannel1.LastRecordInfos.FindLast(x =>
                                            x.Value.FilePath.Equals(item.FilePath));
                                        if (tmp.Value == null)
                                        {
                                            var tag = item.Address + item.Name + item.Secrecy + item.Type +
                                                      item.EndTime +
                                                      item.FilePath + item.StartTime + item.DeviceID + item.RecorderID +
                                                      tmpSipDevice1.DeviceId;
                                            var crc32 = CRC32Helper.GetCRC32(tag);
                                            var crc32Str = crc32.ToString().PadLeft(10, '0');
                                            char[] tmpChars = crc32Str.ToCharArray();
                                            tmpChars[0] = '1'; //回放流的ssrc第一位是1
                                            string itemId = new string(tmpChars);
                                            item.SsrcId = itemId; //ssrc的值
                                            item.Stream = string.Format("{0:X8}", uint.Parse(itemId)); //ssrc的16进制表示
                                            item.SipDevice = tmpSipDevice1;
                                            item.SipChannel = sipChannel1;
                                            item.PushStatus = PushStatus.IDLE;
                                            item.MediaServerStreamInfo = new MediaServerStreamInfo();
                                            sipChannel1.LastRecordInfos.Add(
                                                new KeyValuePair<int, RecordInfo.Item>(sn, item));
                                        }
                                    }
                                }

                                if (sipChannel1.LastRecordInfos.Count == 0)
                                {
                                    foreach (var item in recordInfo.RecordItems.Items)
                                    {
                                        var tmp = sipChannel1.LastRecordInfos.FindLast(x =>
                                            x.Value.FilePath.Equals(item.FilePath));
                                        if (tmp.Value == null)
                                        {
                                            var tag = item.Address + item.Name + item.Secrecy + item.Type +
                                                      item.EndTime +
                                                      item.FilePath + item.StartTime + item.DeviceID + item.RecorderID+tmpSipDevice1.DeviceId;
                                                      var crc32 = CRC32Helper.GetCRC32(tag);
                                            var crc32Str = crc32.ToString().PadLeft(10, '0');
                                            char[] tmpChars = crc32Str.ToCharArray();
                                            tmpChars[0] = '1'; //回放流的ssrc第一位是1
                                            string itemId = new string(tmpChars);
                                            item.SsrcId = itemId; //ssrc的值
                                            item.Stream = string.Format("{0:X8}", uint.Parse(itemId)); //ssrc的16进制表示
                                            item.SipDevice = tmpSipDevice1;
                                            item.SipChannel = sipChannel1;
                                            item.PushStatus = PushStatus.IDLE;
                                            item.MediaServerStreamInfo = new MediaServerStreamInfo();

                                            sipChannel1.LastRecordInfos.Add(
                                                new KeyValuePair<int, RecordInfo.Item>(sn, item));
                                        }
                                    }
                                }

                                Logger.Debug(
                                    $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的录像查询结果->{tmpSipDevice1.DeviceId}->{sipChannel1.DeviceId}->录像结果总数为:{tatolNum}->当前已获取数量:{sipChannel1.LastRecordInfos.Count}");
                                if (sipChannel1.LastRecordInfos.Count >= tatolNum || tatolNum > 256) //数量相等或才总数特别大时，返回成功
                                {
                                    string _taskTag = $"RECORDINFO:{tmpSipDevice1.DeviceId}:{sipChannel1.DeviceId}";
                                    var ret = Common.NeedResponseRequests.TryRemove(_taskTag, out NeedReturnTask _task);
                                    if (ret && _task != null && _task.AutoResetEvent2 != null)
                                    {
                                        _task.AutoResetEvent2.Set();
                                    }
                                }
                            }
                        }

                        break;
                    case "MEDIASTATUS":
                        await SendOkMessage(sipRequest);
                        MediaStatus mediaStatus = UtilsHelper.XMLToObject<MediaStatus>(bodyXml);
                        if (mediaStatus != null)
                        {
                            var callId = "MEDIASTATUS"+sipRequest.Header.CallId;
                            var ret = Common.NeedResponseRequests.TryRemove(callId, out NeedReturnTask _task);
                            if (ret && _task != null)
                            {
                                ((RecordInfo.Item) _task.Obj).PushStatus = PushStatus.IDLE;
                                ((RecordInfo.Item) _task.Obj).MediaServerStreamInfo = null;
                                Task.Run(() =>
                                {
                                    OnInviteHistoryVideoFinished?.Invoke((RecordInfo.Item)_task.Obj);
                                }); //抛线程出去处理

                                Logger.Debug(
                                    $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的点播结束消息->{_task.SipDevice.DeviceId}->{_task.SipChannel.DeviceId}->Stream->{((RecordInfo.Item)_task.Obj).SsrcId}:{((RecordInfo.Item)_task.Obj).Stream}");
                            }
                        
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
                                $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的Sip设备注销请求->{tmpSipDevice.DeviceId}->已经注销，当前Sip设备数量:{Common.SipDevices.Count}个");

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
                        tmpSipDevice.ContactUri = sipRequest.Header.Contact[0].ContactURI;
                        tmpSipDevice.LastSipRequest = sipRequest;
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
                                        $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的Sip设备注册请求->{tmpSipDevice.DeviceId}->注册完成，当前Sip设备数量:{Common.SipDevices.Count}个");
                                }
                                else
                                {
                                    Logger.Debug(
                                        $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的Sip设备注册请求->{tmpSipDevice.DeviceId}->注册请求重复已经忽略，当前Sip设备数量:{Common.SipDevices.Count}个");
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
                                $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的Sip设备注册请求->{tmpSipDevice.DeviceId}->已经更新注册时间，当前Sip设备数量:{Common.SipDevices.Count}个");
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
        /// 结束invite的回复
        /// </summary>
        /// <param name="sipResponse"></param>
        /// <param name="sipChannel"></param>
        /// <returns></returns>
        private static async Task InviteEnd(SIPResponse sipResponse, RecordInfo.Item record)
        {
            var from = sipResponse.Header.From;
            var to = sipResponse.Header.To;
            string callId = sipResponse.Header.CallId;

            SIPRequest req = SIPRequest.GetRequest(SIPMethodsEnum.BYE, sipResponse.Header.To.ToURI,
                new SIPToHeader(to.ToName, to.ToURI, to.ToTag),
                new SIPFromHeader(@from.FromTag, @from.FromURI, @from.FromTag));
            req.Header.Contact = new List<SIPContactHeader>()
                {new SIPContactHeader(sipResponse.Header.From.FromName, sipResponse.Header.From.FromURI)};
            req.Header.UserAgent = ConstString.SIP_USERAGENT_STRING;
            req.Header.Allow = null;
            req.Header.Vias = sipResponse.Header.Vias;
            req.Header.CallId = callId;

            Logger.Debug(
                $"[{Common.LoggerHead}]->回复终止实时流请求状态Bye{sipResponse.RemoteSIPEndPoint}->{req}");
            await Common.SipServer.SipTransport.SendRequestAsync(sipResponse.RemoteSIPEndPoint, req);
        }

        
        /// <summary>
        /// 结束invite的回复
        /// </summary>
        /// <param name="sipResponse"></param>
        /// <param name="sipChannel"></param>
        /// <returns></returns>
        private static async Task InviteEnd(SIPResponse sipResponse, SipChannel sipChannel)
        {
            var from = sipResponse.Header.From;
            var to = sipResponse.Header.To;
            string callId = sipResponse.Header.CallId;

            SIPRequest req = SIPRequest.GetRequest(SIPMethodsEnum.BYE, sipResponse.Header.To.ToURI,
                new SIPToHeader(to.ToName, to.ToURI, to.ToTag),
                new SIPFromHeader(@from.FromTag, @from.FromURI, @from.FromTag));
            req.Header.Contact = new List<SIPContactHeader>()
                {new SIPContactHeader(sipResponse.Header.From.FromName, sipResponse.Header.From.FromURI)};
            req.Header.UserAgent = ConstString.SIP_USERAGENT_STRING;
            req.Header.Allow = null;
            req.Header.Vias = sipResponse.Header.Vias;
            req.Header.CallId = callId;
            Logger.Debug(
                $"[{Common.LoggerHead}]->回复终止实时流请求状态Bye{sipResponse.RemoteSIPEndPoint}->{req}");
            await Common.SipServer.SipTransport.SendRequestAsync(sipResponse.RemoteSIPEndPoint, req);
        }

        /// <summary>
        /// 回复invite状态
        /// </summary>
        /// <param name="sipResponse"></param>
        /// <param name="sipChannel"></param>
        /// <returns></returns>
        private static async Task InviteOk(SIPResponse sipResponse, SipChannel sipChannel)
        {
            var from = sipResponse.Header.From;
            var to = sipResponse.Header.To;
            string callId = sipResponse.Header.CallId;
            sipChannel.InviteSipResponse = sipResponse;

            SIPRequest req = SIPRequest.GetRequest(SIPMethodsEnum.ACK, sipResponse.Header.To.ToURI,
                new SIPToHeader(to.ToName, to.ToURI, to.ToTag),
                new SIPFromHeader(@from.FromTag, @from.FromURI, @from.FromTag));
            req.Header.Contact = new List<SIPContactHeader>()
                {new SIPContactHeader(sipResponse.Header.From.FromName, sipResponse.Header.From.FromURI)};
            req.Header.UserAgent = ConstString.SIP_USERAGENT_STRING;
            req.Header.Allow = null;
            req.Header.Vias = sipResponse.Header.Vias;
            req.Header.CallId = callId;
            Logger.Debug(
                $"[{Common.LoggerHead}]->回复实时流请求状态ACK{sipResponse.RemoteSIPEndPoint}->{req}");
            await Common.SipServer.SipTransport.SendRequestAsync(sipResponse.RemoteSIPEndPoint, req);
        }

        /// <summary>
        /// 回复回放流invite状态
        /// </summary>
        /// <param name="sipResponse"></param>
        /// <param name="sipChannel"></param>
        /// <returns></returns>
        private static async Task InviteOk(SIPResponse sipResponse, RecordInfo.Item record)
        {
            var from = sipResponse.Header.From;
            var to = sipResponse.Header.To;
            string callId = sipResponse.Header.CallId;
            record.InviteSipResponse = sipResponse;

            SIPRequest req = SIPRequest.GetRequest(SIPMethodsEnum.ACK, sipResponse.Header.To.ToURI,
                new SIPToHeader(to.ToName, to.ToURI, to.ToTag),
                new SIPFromHeader(@from.FromTag, @from.FromURI, @from.FromTag));
            req.Header.Contact = new List<SIPContactHeader>()
                {new SIPContactHeader(sipResponse.Header.From.FromName, sipResponse.Header.From.FromURI)};
            req.Header.UserAgent = ConstString.SIP_USERAGENT_STRING;
            req.Header.Allow = null;
            req.Header.Vias = sipResponse.Header.Vias;
            req.Header.CallId = callId;
            Logger.Debug(
                $"[{Common.LoggerHead}]->回复回放流请求状态ACK{sipResponse.RemoteSIPEndPoint}->{req}");
            await Common.SipServer.SipTransport.SendRequestAsync(sipResponse.RemoteSIPEndPoint, req);
        }

        /// <summary>
        /// 检测sdp是通道时实流还是通道回放流
        /// 
        /// </summary>
        /// <param name="sdp"></param>
        /// <returns>true通道实时流，false通道回放流</returns>
        private static bool CheckIsChannleInviteBySdpString(string sdp)
        {
            string[] sdpTmp = sdp.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
            if (sdpTmp.Length > 0)
            {
                foreach (var sdpparm in sdpTmp)
                {
                    if (!string.IsNullOrEmpty(sdpparm) && sdpparm.Trim().ToUpper().StartsWith("Y="))
                    {
                        var s = sdpparm.Trim().ToUpper().Replace("Y=", "");
                        if (!string.IsNullOrEmpty(s))
                        {
                            if (s.StartsWith("1"))
                            {
                                return  false;
                            }
                        }
                    }
                }
            }

            return true;
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
            var status = sipResponse.Status;
            SIPMethodsEnum method;
            bool ret;
            NeedReturnTask _task;
            switch (status)
            {
                case SIPResponseStatusCodesEnum.Ok:
                    method = sipResponse.Header.CSeqMethod;
                    ret = Common.NeedResponseRequests.TryRemove(sipResponse.Header.CallId,
                        out _task);
                    if (ret && _task != null)
                    {
                        switch (method)
                        {
                            case SIPMethodsEnum.INVITE: //请求实时流成功回复
                                bool   isChannleInvite = CheckIsChannleInviteBySdpString(sipResponse.Body);

                                if (isChannleInvite)
                                {
                                    await InviteOk(sipResponse, _task.SipChannel);
                                }
                                else
                                {
                                    var record =(RecordInfo.Item) _task.Obj;
                                    await InviteOk(sipResponse, record);
                                    Common.NeedResponseRequests.TryAdd(
                                        "MEDIASTATUS"+sipResponse.Header.CallId,
                                        _task); //再次加入等待列表,播放完成时会回调,使用同一个callid
                                    
                                }

                                break;
                            case SIPMethodsEnum.BYE: //停止实时流成功回复
                                isChannleInvite = _task.Obj == null;
                                if (isChannleInvite)
                                {
                                    await InviteEnd(sipResponse, _task.SipChannel);
                                }
                                else
                                {
                                    var record =(RecordInfo.Item) _task.Obj;
                                    await InviteEnd(sipResponse, record);
                                    var ret1=Common.NeedResponseRequests.TryRemove("MEDIASTATUS" + sipResponse.Header.CallId,
                                        out NeedReturnTask _task1);
                                    if (ret1 && _task1 != null)
                                    {
                                        ((RecordInfo.Item) _task1.Obj).PushStatus = PushStatus.IDLE;
                                        ((RecordInfo.Item) _task1.Obj).MediaServerStreamInfo = null;
                                        Task.Run(() =>
                                        {
                                            OnInviteHistoryVideoFinished?.Invoke((RecordInfo.Item)_task1.Obj);
                                        }); //抛线程出去处理
                                        Logger.Debug(
                                            $"[{Common.LoggerHead}]->结束点播->{_task1.SipDevice.DeviceId}->{_task1.SipChannel.DeviceId}->Stream->{((RecordInfo.Item)_task1.Obj).SsrcId}:{((RecordInfo.Item)_task1.Obj).Stream}");


                                    }
                                    

                                }

                                break;
                        }

                        try
                        {
                            var tmpSipDevice =
                                Common.SipDevices.FindLast(x => x.DeviceId.Equals(_task.SipDevice.DeviceId));
                            if (tmpSipDevice != null)
                            {
                                tmpSipDevice.LastSipResponse = sipResponse;
                            }

                            switch (_task.CommandType) //更次入列
                            {
                                case CommandType.RecordInfo:
                                    Common.NeedResponseRequests.TryAdd(
                                        _task.CommandType.ToString().ToUpper() + ":" + _task.SipDevice.DeviceId + ":" +
                                        _task.SipChannel.DeviceId, _task); //再次加入等待列表

                                    break;
                                case CommandType.Catalog:
                                    Common.NeedResponseRequests.TryAdd(
                                        _task.CommandType.ToString().ToUpper() + ":" + _task.SipDevice.DeviceId,
                                        _task); //再次加入等待列表

                                    break;
                               
                            }

                            Logger.Debug(
                                $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的SipResponse->{sipResponse}这个消息是回复消息，callid:{sipResponse.Header.CallId}");
                            _task.AutoResetEvent.Set(); //通知调用者任务完成,凋用者后续要做dispose操作,//这里有问题，会崩溃
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex.Message + "\r\n" + ex.StackTrace);
                        }
                    }
                    else
                    {
                        Logger.Debug(
                            $"[{Common.LoggerHead}]->收到来自{remoteEndPoint}的SipResponse->{sipResponse}");
                    }

                    break;
            }
        }
    }
}