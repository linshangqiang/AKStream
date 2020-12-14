using System;
using System.Collections.Generic;
using System.Threading;
using LibCommon;
using LibCommon.Structs;
using LibCommon.Structs.GB28181;
using LibCommon.Structs.GB28181.XML;


namespace LibGB28181SipServer
{
    /// <summary>
    /// sip方法操作代理
    /// </summary>
    public class SipMethodProxy : IDisposable
    {
        private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private SipServer _sipServer;
        private int _timeout;
        private CommandType _commandType;

        public SipMethodProxy(int timeout = 5000)
        {
            _timeout = timeout;
        }

        ~SipMethodProxy()
        {
            Dispose(); //释放非托管资源
        }

        public void Dispose()
        {
            if (_autoResetEvent != null)
            {
                _autoResetEvent.Dispose();
            }
        }

        public CommandType CommandType
        {
            get => _commandType;
            set => _commandType = value;
        }


        /// <summary>
        /// 获取设备的状态信息
        /// </summary>
        /// <param name="sipDevice"></param>
        /// <returns></returns>
        public bool GetSipDeviceStatus(SipDevice sipDevice, out ResponseStruct rs)
        {
            try
            {
                _commandType = CommandType.DeviceStatus;
                Common.SipServer.GetDeviceStatus(sipDevice, _autoResetEvent, out rs, _timeout);
                var isTimeout = _autoResetEvent.WaitOne(_timeout);
                if (!isTimeout || !rs.Code.Equals(ErrorNumber.None))
                {
                    return false;
                }

                return true;
            }
            finally
            {
                Dispose();
            }
        }

        /// <summary>
        /// 获取SIP设备信息
        /// </summary>
        /// <param name="sipDevice"></param>
        /// <returns></returns>
        public bool GetSipDeviceInfo(SipDevice sipDevice, out ResponseStruct rs)
        {
            try
            {
                _commandType = CommandType.DeviceInfo;
                Common.SipServer.GetDeviceInfo(sipDevice, _autoResetEvent, out rs, _timeout);
                var isTimeout = _autoResetEvent.WaitOne(_timeout);
                if (!isTimeout || !rs.Code.Equals(ErrorNumber.None))
                {
                    return false;
                }

                return true;
            }
            finally
            {
                Dispose();
            }
        }


        /// <summary>
        /// 获取通道录像文件列表
        /// </summary>
        /// <param name="sipChannel"></param>
        /// <param name="queryRecordFile"></param>
        /// <returns></returns>
        public bool QueryRecordFileList(SipChannel sipChannel, SipQueryRecordFile queryRecordFile,
            out ResponseStruct rs)
        {
            AutoResetEvent _autoResetEvent2 = null;
            try
            {
                _commandType = CommandType.RecordInfo;
                _autoResetEvent2 = new AutoResetEvent(false);
                Common.SipServer.GetRecordFileList(sipChannel, queryRecordFile, _autoResetEvent, _autoResetEvent2,
                    out rs, _timeout);
                var isTimeout = _autoResetEvent.WaitOne(_timeout);
                if (!isTimeout || !rs.Code.Equals(ErrorNumber.None))
                {
                    return false;
                }

                isTimeout = _autoResetEvent2.WaitOne(_timeout);
                if (isTimeout)
                {
                    return true;
                }

                return false;
            }
            finally
            {
                _autoResetEvent2.Dispose();
                Dispose();
            }
        }

        /// <summary>
        /// ptz控制
        /// </summary>
        /// <param name="ptzCtrl"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public bool PtzMove(PtzCtrl ptzCtrl, out ResponseStruct rs)
        {
            try
            {
                Common.SipServer.PtzMove(ptzCtrl, _autoResetEvent, out rs, _timeout);
                _commandType = CommandType.DeviceControl;
                var isTimeout = _autoResetEvent.WaitOne(_timeout);
                if (!isTimeout || !rs.Code.Equals(ErrorNumber.None))
                {
                    return false;
                }

                return true;
            }
            finally
            {
                Dispose();
            }
        }


        /// <summary>
        /// 请求终止时实流
        /// </summary>
        /// <param name="sipChannel"></param>
        /// <returns></returns>
        public bool DeInvite(SipChannel sipChannel, out ResponseStruct rs)
        {
            try
            {
                Common.SipServer.DeInvite(sipChannel, _autoResetEvent, out rs, _timeout);
                _commandType = CommandType.Unknown;
                var isTimeout = _autoResetEvent.WaitOne(_timeout);
                if (!isTimeout || !rs.Code.Equals(ErrorNumber.None))
                {
                    return false;
                }

                sipChannel.PushStatus = DevicePushStatus.IDLE;
                sipChannel.ChannelMediaServerStreamInfo = null;
                return true;
            }
            finally
            {
                Dispose();
            }
        }

        /// <summary>
        /// 请求实时流
        /// </summary>
        /// <param name="sipChannel"></param>
        /// <param name="pushMediaInfo"></param>
        /// <returns></returns>
        public bool Invite(SipChannel sipChannel, PushMediaInfo pushMediaInfo, out ResponseStruct rs)
        {
            try
            {
                _commandType = CommandType.Unknown;
                
                Common.SipServer.Invite(sipChannel, pushMediaInfo, _autoResetEvent, out rs, _timeout);
                var isTimeout = _autoResetEvent.WaitOne(_timeout);
                if (!isTimeout || !rs.Code.Equals(ErrorNumber.None))
                {
                    return false;
                }

                sipChannel.PushStatus = DevicePushStatus.PUSHON;
                sipChannel.ChannelMediaServerStreamInfo= new MediaServerStreamInfo()
                {
                    App = pushMediaInfo.App,
                    MediaServerId = pushMediaInfo.MediaServerIpAddress,
                    MediaServerIp = pushMediaInfo.MediaServerIpAddress,
                    PlayerList = new List<MediaServerStreamPlayerInfo>(),
                    PlayUrl= new List<string>(new []
                    {
                        $"http://{pushMediaInfo.MediaServerIpAddress}:{pushMediaInfo.Http_WsMediaServerPlayPort}/{pushMediaInfo.App}/{pushMediaInfo.Stream}.flv",
                        $"ws://{pushMediaInfo.MediaServerIpAddress}:{pushMediaInfo.Http_WsMediaServerPlayPort}/{pushMediaInfo.App}/{pushMediaInfo.Stream}.flv",
                        $"rtmp://{pushMediaInfo.MediaServerIpAddress}:{pushMediaInfo.RtmpMediaServerPlayPort}/{pushMediaInfo.App}/{pushMediaInfo.Stream}",
                        $"rtsp://{pushMediaInfo.MediaServerIpAddress}:{pushMediaInfo.RtspMediaServerPlayPort}/{pushMediaInfo.App}/{pushMediaInfo.Stream}",
                        $"http://{pushMediaInfo.MediaServerIpAddress}:{pushMediaInfo.Http_WsMediaServerPlayPort}/{pushMediaInfo.App}/{pushMediaInfo.Stream}/hls.m3u8",
                        $"http://{pushMediaInfo.MediaServerIpAddress}:{pushMediaInfo.Http_WsMediaServerPlayPort}/{pushMediaInfo.App}/{pushMediaInfo.Stream}.ts",
                        $"http://{pushMediaInfo.MediaServerIpAddress}:{pushMediaInfo.Http_WsMediaServerPlayPort}/{pushMediaInfo.App}/{pushMediaInfo.Stream}.mp4",
                        pushMediaInfo.RtmpsMediaServerPlayPort>0? $"rtmps://{pushMediaInfo.MediaServerIpAddress}:{pushMediaInfo.RtmpsMediaServerPlayPort}/{pushMediaInfo.App}/{pushMediaInfo.Stream}"
                            :null,
                        pushMediaInfo.RtspsMediaServerPlayPort>0? $"rtsps://{pushMediaInfo.MediaServerIpAddress}:{pushMediaInfo.RtspsMediaServerPlayPort}/{pushMediaInfo.App}/{pushMediaInfo.Stream}"
                            :null,
                        pushMediaInfo.HttpsWssMediaServerPlayPort>0? $"https://{pushMediaInfo.MediaServerIpAddress}:{pushMediaInfo.HttpsWssMediaServerPlayPort}/{pushMediaInfo.App}/{pushMediaInfo.Stream}.flv"
                            :null,
                        pushMediaInfo.HttpsWssMediaServerPlayPort>0? $"wss://{pushMediaInfo.MediaServerIpAddress}:{pushMediaInfo.HttpsWssMediaServerPlayPort}/{pushMediaInfo.App}/{pushMediaInfo.Stream}.flv"
                            :null,
                        pushMediaInfo.HttpsWssMediaServerPlayPort>0? $"https://{pushMediaInfo.MediaServerIpAddress}:{pushMediaInfo.HttpsWssMediaServerPlayPort}/{pushMediaInfo.App}/{pushMediaInfo.Stream}/hls.m3u8"
                            :null,
                        pushMediaInfo.HttpsWssMediaServerPlayPort>0? $"https://{pushMediaInfo.MediaServerIpAddress}:{pushMediaInfo.HttpsWssMediaServerPlayPort}/{pushMediaInfo.App}/{pushMediaInfo.Stream}.ts"
                            :null,
                        pushMediaInfo.HttpsWssMediaServerPlayPort>0? $"https://{pushMediaInfo.MediaServerIpAddress}:{pushMediaInfo.HttpsWssMediaServerPlayPort}/{pushMediaInfo.App}/{pushMediaInfo.Stream}.mp4"
                            :null,
                    }),
                    
                     StartTime = DateTime.Now,
                     PushSocketType = pushMediaInfo.PushStreamSocketType,
                     Ssrc = pushMediaInfo.Ssrc,
                     Stream = pushMediaInfo.Stream,
                     StreamBytes = 0,
                     Vhost = pushMediaInfo.Vhost,
                };
                UtilsHelper.RemoveNull(sipChannel.ChannelMediaServerStreamInfo.PlayUrl);
                
                return true;
            }
            finally
            {
                Dispose();
            }
        }

        /// <summary>
        /// 获取设备目录
        /// </summary>
        /// <param name="sipDevice"></param>
        /// <returns></returns>
        public bool DeviceCatalogQuery(SipDevice sipDevice, out ResponseStruct rs)
        {
            try
            {
                _commandType = CommandType.Catalog;
                Common.SipServer.DeviceCatalogQuery(sipDevice, _autoResetEvent, out rs, _timeout);
                var isTimeout = _autoResetEvent.WaitOne(_timeout);
                if (!isTimeout || !rs.Code.Equals(ErrorNumber.None))
                {
                    return false;
                }

                return true;
            }
            finally
            {
                Dispose();
            }
        }
    }
}