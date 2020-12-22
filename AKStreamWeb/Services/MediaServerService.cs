using System;
using AKStreamWeb.WebResponse;
using LibCommon;
using LibCommon.Structs;
using LibZLMediaKitMediaServer;

namespace AKStreamWeb.Services
{
    public static class MediaServerService
    {
        /// <summary>
        /// 保持与流媒体服务器的心跳
        /// </summary>
        /// <param name="req"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static AKStreamWebResponseBase MediaServerKeepAlive(MediaServerKeepAlive req, out ResponseStruct rs)
        {
            ResMediaServerKeepAlive result;
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            if (req == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ParamsIsNotRight,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
                };
            }

            if (Math.Abs((DateTime.Now - req.ServerDateTime).Seconds) > 60) //两边服务器时间大于60秒，则回复注册失败
            {
                result = new ResMediaServerKeepAlive()
                {
                    Rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_TimeExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_TimeExcept],
                    },
                    RecommendTimeSynchronization = true,
                    ServerDateTime = DateTime.Now,
                };
                return result;
            }

            lock (Common.MediaServerLockObj)
            {
                var mediaServer = Common.MediaServerList.FindLast(x => x.MediaServerId.Equals(req.MediaServerId));
                if (mediaServer != null)
                {
                    //已经存在的
                    if ((DateTime.Now - mediaServer.KeepAliveTime).Seconds < 10)//10秒内多次心跳请求直接回复
                    {
                        mediaServer.KeepAliveTime = DateTime.Now;
                        result = new ResMediaServerKeepAlive()
                        {
                            Rs = rs,
                            RecommendTimeSynchronization = false,
                            ServerDateTime = DateTime.Now,
                        };
                        return result;
                    }
                    mediaServer.Secret = req.Secret;
                    mediaServer.IpV4Address = req.IpV4Address;
                    mediaServer.IpV6Address = req.IpV6Address;
                    mediaServer.IsRunning = true;
                    mediaServer.KeeperPort = req.KeeperWebApiPort;
                    mediaServer.RecordPath = req.RecordPath;
                    mediaServer.ZlmediakitPid = req.MediaServerPid;
                    mediaServer.KeepAliveTime = DateTime.Now;
                    mediaServer.MediaServerId = req.MediaServerId;
                    mediaServer.HttpPort = req.ZlmHttpPort;
                    mediaServer.HttpsPort = req.ZlmHttpsPort;
                    mediaServer.RtmpPort = req.ZlmRtmpPort;
                    mediaServer.RtmpsPort = req.ZlmRtmpsPort;
                    mediaServer.RtspPort = req.ZlmRtspPort;
                    mediaServer.RtspsPort = req.ZlmRtspsPort;
                    mediaServer.RtpPortMax = req.RtpPortMax;
                    mediaServer.RtpPortMin = req.RtpPortMin;
                    mediaServer.ServerDateTime = req.ServerDateTime;
                    mediaServer.ZlmRecordFileSec = req.ZlmRecordFileSec;
                    if (req.PerformanceInfo != null) //更新性能信息
                    {
                        mediaServer.PerformanceInfo = req.PerformanceInfo;
                    }
                    result = new ResMediaServerKeepAlive()
                    {
                        Rs = rs,
                        RecommendTimeSynchronization = false,
                        ServerDateTime = DateTime.Now,
                        NeedRestartMediaServer = false,
                    };
                }
                else
                {
                    //没有存在的
                    var tmpMediaServer = new ServerInstance();
                    tmpMediaServer.Secret = req.Secret;
                    tmpMediaServer.IpV4Address = req.IpV4Address;
                    tmpMediaServer.IpV6Address = req.IpV6Address;
                    tmpMediaServer.IsRunning = true;
                    tmpMediaServer.KeeperPort = req.KeeperWebApiPort;
                    tmpMediaServer.RecordPath = req.RecordPath;
                    tmpMediaServer.ZlmediakitPid = req.MediaServerPid;
                    tmpMediaServer.KeepAliveTime = DateTime.Now;
                    tmpMediaServer.MediaServerId = req.MediaServerId;
                    tmpMediaServer.HttpPort = req.ZlmHttpPort;
                    tmpMediaServer.HttpsPort = req.ZlmHttpsPort;
                    tmpMediaServer.RtmpPort = req.ZlmRtmpPort;
                    tmpMediaServer.RtmpsPort = req.ZlmRtmpsPort;
                    tmpMediaServer.RtspPort = req.ZlmRtspPort;
                    tmpMediaServer.RtspsPort = req.ZlmRtspsPort;
                    tmpMediaServer.RtpPortMax = req.RtpPortMax;
                    tmpMediaServer.RtpPortMin = req.RtpPortMin;
                    tmpMediaServer.ServerDateTime = req.ServerDateTime;
                    tmpMediaServer.ZlmRecordFileSec = req.ZlmRecordFileSec;
                    if (req.PerformanceInfo != null) //更新性能信息
                    {
                        mediaServer.PerformanceInfo = req.PerformanceInfo;
                    }
                    tmpMediaServer.WebApiHelper = new WebApiHelper(tmpMediaServer.IpV4Address,
                        tmpMediaServer.UseSsl ? tmpMediaServer.HttpsPort : tmpMediaServer.HttpPort,
                        tmpMediaServer.Secret, "", tmpMediaServer.UseSsl);
                    Common.MediaServerList.Add(tmpMediaServer);
                    result = new ResMediaServerKeepAlive()
                    {
                        Rs = rs,
                        RecommendTimeSynchronization = false,
                        ServerDateTime = DateTime.Now,
                    };
                    if (Common.AkStreamWebConfig.MediaServerFirstToRestart)
                    {
                        result.NeedRestartMediaServer = true;
                    }
                }
            }
            return result;
        }
    }
}