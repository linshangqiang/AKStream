using System;
using System.Collections.Generic;
using LibCommon;
using LibCommon.Enums;
using LibCommon.Structs;
using LibCommon.Structs.DBModels;
using LibCommon.Structs.WebRequest;
using LibCommon.Structs.WebRequest.AKStreamKeeper;
using LibCommon.Structs.WebResponse;
using LibCommon.Structs.WebResponse.AKStreamKeeper;
using LibLogger;
using LibZLMediaKitMediaServer;


namespace AKStreamWeb.Services
{
    public static class MediaServerService
    {
        /// <summary>
        /// 获取音视频流通道列表（支持分页，全表条件）
        /// </summary>
        /// <param name="req"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static ResGetVideoChannelList GetVideoChannelList(ReqGetVideoChannelList req, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };


            if (!string.IsNullOrEmpty(req.ChannelIdLike) && !req.ChannelIdLike.ToLower().Trim().Equals("string"))
            {
                req.ChannelId = null;
            }

            if (!string.IsNullOrEmpty(req.ChannelNameLike) && !req.ChannelNameLike.ToLower().Trim().Equals("string"))
            {
                req.ChannelName = null;
            }

            if (!string.IsNullOrEmpty(req.DepartmentNameLike) &&
                !req.DepartmentNameLike.ToLower().Trim().Equals("string"))
            {
                req.DepartmentName = null;
            }

            if (!string.IsNullOrEmpty(req.IpV4AddressLike) && !req.IpV4AddressLike.ToLower().Trim().Equals("string"))
            {
                req.IpV4AddressLike = null;
            }

            if (!string.IsNullOrEmpty(req.IpV6AddressLike) && !req.IpV6AddressLike.ToLower().Trim().Equals("string"))
            {
                req.IpV6AddressLike = null;
            }

            if (!string.IsNullOrEmpty(req.VideoSrcUrlLike) && !req.VideoSrcUrlLike.ToLower().Trim().Equals("string"))
            {
                req.VideoSrcUrl = null;
            }

            if (!string.IsNullOrEmpty(req.DeviceIdLike) && !req.DeviceIdLike.ToLower().Trim().Equals("string"))
            {
                req.DeviceIdLike = null;
            }

            if (req.IncludeSubDeptartment != null && req.IncludeSubDeptartment == true)
            {
                if (string.IsNullOrEmpty(req.DepartmentId))
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.Sys_ParamsIsNotRight,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight] + ",条件中要求包含子部门,但条件中部门代码为空",
                    };
                    return null!;
                }

                req.DepartmentNameLike = "";
                req.DepartmentName = "";
                req.PDepartmentId = "";
                req.PDepartmentName = "";
            }

            bool isPageQuery = req.PageIndex != null;
            bool haveOrderBy = req.OrderBy != null;
            if (isPageQuery)
            {
                if (req.PageSzie > 10000)
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.Sys_DataBaseLimited,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_DataBaseLimited],
                    };
                    return null!;
                }

                if (req.PageIndex <= 0)
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.Sys_DataBaseLimited,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_DataBaseLimited],
                    };
                    return null!;
                }
            }

            string orderBy = "";
            if (haveOrderBy)
            {
                foreach (var order in req.OrderBy!)
                {
                    if (order != null)
                    {
                        orderBy += order.FieldName + " " + Enum.GetName(typeof(OrderByDir), order.OrderByDir!) + ",";
                    }
                }

                orderBy = orderBy.TrimEnd(',');
            }

            long total = -1;
            List<VideoChannel> retList = null;


            try
            {
                if (isPageQuery)
                {
                    retList = ORMHelper.Db.Select<VideoChannel>().Where("1=1")
                        .WhereIf(req.Id != null && req.Id > 0, x => x.Id.Equals(req.Id))
                        .WhereIf(req.DeviceNetworkType != null, x => x.DeviceNetworkType.Equals(req.DeviceNetworkType))
                        .WhereIf(req.DeviceStreamType != null, x => x.DeviceStreamType.Equals(req.DeviceStreamType))
                        .WhereIf(req.VideoDeviceType != null, x => x.VideoDeviceType.Equals(req.VideoDeviceType))
                        .WhereIf(req.MethodByGetStream != null, x => x.MethodByGetStream.Equals(req.MethodByGetStream))
                        .WhereIf(req.Enabled != null, x => x.Enabled.Equals(req.Enabled))
                        .WhereIf(req.AutoRecord != null, x => x.AutoRecord.Equals(req.AutoRecord))
                        .WhereIf(req.AutoVideo != null, x => x.AutoVideo.Equals(req.AutoVideo))
                        .WhereIf(req.CreateTime != null, x => x.CreateTime.Equals(req.CreateTime))
                        .WhereIf(req.HasPtz != null, x => x.HasPtz.Equals(req.HasPtz))
                        .WhereIf(req.UpdateTime != null, x => x.UpdateTime.Equals(req.UpdateTime))
                        .WhereIf(req.DefaultRtpPort != null, x => x.DefaultRtpPort.Equals(req.DefaultRtpPort))
                        .WhereIf(req.NoPlayerBreak != null, x => x.NoPlayerBreak.Equals(req.NoPlayerBreak))
                        .WhereIf(req.RtpWithTcp != null, x => x.RtpWithTcp.Equals(req.RtpWithTcp))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.ChannelId) && !req.ChannelId.ToLower().Trim().Equals("string"),
                            x => x.ChannelId.Equals(req.ChannelId))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.ChannelName) &&
                            !req.ChannelName.ToLower().Trim().Equals("string"),
                            x => x.ChannelName.Equals(req.ChannelName))
                        .WhereIf(
                            (!string.IsNullOrEmpty(req.DepartmentId) &&
                            !req.DepartmentId.ToLower().Trim().Equals("string") &&( req.IncludeSubDeptartment==null || req.IncludeSubDeptartment==false)),
                            x => x.DepartmentId.Equals(req.DepartmentId))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.DepartmentName) &&
                            !req.DepartmentName.ToLower().Trim().Equals("string"),
                            x => x.DepartmentName.Equals(req.DepartmentName))
                        .WhereIf(!string.IsNullOrEmpty(req.DeviceId) && !req.DeviceId.ToLower().Trim().Equals("string"),
                            x => x.DeviceId.Equals(req.DeviceId))
                        .WhereIf(!string.IsNullOrEmpty(req.MainId) && !req.MainId.ToLower().Trim().Equals("string"),
                            x => x.MainId.Equals(req.MainId))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.IpV4Address) &&
                            !req.IpV4Address.ToLower().Trim().Equals("string"),
                            x => x.IpV4Address.Equals(req.IpV4Address))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.IpV6Address) &&
                            !req.IpV6Address.ToLower().Trim().Equals("string"),
                            x => x.IpV6Address.Equals(req.IpV6Address))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.MediaServerId) &&
                            !req.MediaServerId.ToLower().Trim().Equals("string"),
                            x => x.MediaServerId.Equals(req.MediaServerId))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.PDepartmentId) &&
                            !req.PDepartmentId.ToLower().Trim().Equals("string"),
                            x => x.PDepartmentId.Equals(req.PDepartmentId))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.PDepartmentName) &&
                            !req.PDepartmentName.ToLower().Trim().Equals("string"),
                            x => x.PDepartmentName.Equals(req.PDepartmentName))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.VideoSrcUrl) &&
                            !req.VideoSrcUrl.ToLower().Trim().Equals("string"),
                            x => x.VideoSrcUrl.Equals(req.VideoSrcUrl))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.ChannelNameLike) &&
                            !req.ChannelNameLike.ToLower().Trim().Equals("string"),
                            x => x.ChannelName.Contains(req.ChannelNameLike))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.DepartmentNameLike) &&
                            !req.DepartmentNameLike.ToLower().Trim().Equals("string"),
                            x => x.DepartmentName.Contains(req.DepartmentNameLike))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.IpV4AddressLike) &&
                            !req.IpV4AddressLike.ToLower().Trim().Equals("string"),
                            x => x.IpV4Address.Contains(req.IpV4AddressLike))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.IpV6AddressLike) &&
                            !req.IpV6AddressLike.ToLower().Trim().Equals("string"),
                            x => x.IpV6Address.Contains(req.IpV6AddressLike))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.VideoSrcUrlLike) &&
                            !req.VideoSrcUrlLike.ToLower().Trim().Equals("string"),
                            x => x.VideoSrcUrl.Contains(req.VideoSrcUrlLike))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.DeviceIdLike) &&
                            !req.DeviceIdLike.ToLower().Trim().Equals("string"),
                            x => x.DeviceId.Contains(req.DeviceIdLike))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.ChannelIdLike) &&
                            !req.ChannelIdLike.ToLower().Trim().Equals("string"),
                            x => x.ChannelId.Contains(req.ChannelIdLike))
                        .WhereIf(req.IncludeSubDeptartment != null && req.IncludeSubDeptartment == true,
                            x => x.PDepartmentId.Equals(req.DepartmentId))
                        .OrderBy(orderBy)
                        .Count(out total)
                        .Page((int) req.PageIndex!, (int) req.PageSzie!)
                        .ToList();
                }
                else
                {
                    retList = ORMHelper.Db.Select<VideoChannel>().Where("1=1")
                        .WhereIf(req.Id != null && req.Id > 0, x => x.Id.Equals(req.Id))
                        .WhereIf(req.DeviceNetworkType != null, x => x.DeviceNetworkType.Equals(req.DeviceNetworkType))
                        .WhereIf(req.DeviceStreamType != null, x => x.DeviceStreamType.Equals(req.DeviceStreamType))
                        .WhereIf(req.VideoDeviceType != null, x => x.VideoDeviceType.Equals(req.VideoDeviceType))
                        .WhereIf(req.MethodByGetStream != null, x => x.MethodByGetStream.Equals(req.MethodByGetStream))
                        .WhereIf(req.Enabled != null, x => x.Enabled.Equals(req.Enabled))
                        .WhereIf(req.AutoRecord != null, x => x.AutoRecord.Equals(req.AutoRecord))
                        .WhereIf(req.AutoVideo != null, x => x.AutoVideo.Equals(req.AutoVideo))
                        .WhereIf(req.CreateTime != null, x => x.CreateTime.Equals(req.CreateTime))
                        .WhereIf(req.HasPtz != null, x => x.HasPtz.Equals(req.HasPtz))
                        .WhereIf(req.UpdateTime != null, x => x.UpdateTime.Equals(req.UpdateTime))
                        .WhereIf(req.DefaultRtpPort != null, x => x.DefaultRtpPort.Equals(req.DefaultRtpPort))
                        .WhereIf(req.NoPlayerBreak != null, x => x.NoPlayerBreak.Equals(req.NoPlayerBreak))
                        .WhereIf(req.RtpWithTcp != null, x => x.RtpWithTcp.Equals(req.RtpWithTcp))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.ChannelId) && !req.ChannelId.ToLower().Trim().Equals("string"),
                            x => x.ChannelId.Equals(req.ChannelId))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.ChannelName) &&
                            !req.ChannelName.ToLower().Trim().Equals("string"),
                            x => x.ChannelName.Equals(req.ChannelName))
                        .WhereIf(
                            (!string.IsNullOrEmpty(req.DepartmentId) &&
                             !req.DepartmentId.ToLower().Trim().Equals("string") &&( req.IncludeSubDeptartment==null || req.IncludeSubDeptartment==false)),
                            x => x.DepartmentId.Equals(req.DepartmentId))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.DepartmentName) &&
                            !req.DepartmentName.ToLower().Trim().Equals("string"),
                            x => x.DepartmentName.Equals(req.DepartmentName))
                        .WhereIf(!string.IsNullOrEmpty(req.DeviceId) && !req.DeviceId.ToLower().Trim().Equals("string"),
                            x => x.DeviceId.Equals(req.DeviceId))
                        .WhereIf(!string.IsNullOrEmpty(req.MainId) && !req.MainId.ToLower().Trim().Equals("string"),
                            x => x.MainId.Equals(req.MainId))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.IpV4Address) &&
                            !req.IpV4Address.ToLower().Trim().Equals("string"),
                            x => x.IpV4Address.Equals(req.IpV4Address))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.IpV6Address) &&
                            !req.IpV6Address.ToLower().Trim().Equals("string"),
                            x => x.IpV6Address.Equals(req.IpV6Address))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.MediaServerId) &&
                            !req.MediaServerId.ToLower().Trim().Equals("string"),
                            x => x.MediaServerId.Equals(req.MediaServerId))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.PDepartmentId) &&
                            !req.PDepartmentId.ToLower().Trim().Equals("string"),
                            x => x.PDepartmentId.Equals(req.PDepartmentId))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.PDepartmentName) &&
                            !req.PDepartmentName.ToLower().Trim().Equals("string"),
                            x => x.PDepartmentName.Equals(req.PDepartmentName))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.VideoSrcUrl) &&
                            !req.VideoSrcUrl.ToLower().Trim().Equals("string"),
                            x => x.VideoSrcUrl.Equals(req.VideoSrcUrl))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.ChannelNameLike) &&
                            !req.ChannelNameLike.ToLower().Trim().Equals("string"),
                            x => x.ChannelName.Contains(req.ChannelNameLike))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.DepartmentNameLike) &&
                            !req.DepartmentNameLike.ToLower().Trim().Equals("string"),
                            x => x.DepartmentName.Contains(req.DepartmentNameLike))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.IpV4AddressLike) &&
                            !req.IpV4AddressLike.ToLower().Trim().Equals("string"),
                            x => x.IpV4Address.Contains(req.IpV4AddressLike))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.IpV6AddressLike) &&
                            !req.IpV6AddressLike.ToLower().Trim().Equals("string"),
                            x => x.IpV6Address.Contains(req.IpV6AddressLike))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.VideoSrcUrlLike) &&
                            !req.VideoSrcUrlLike.ToLower().Trim().Equals("string"),
                            x => x.VideoSrcUrl.Contains(req.VideoSrcUrlLike))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.DeviceIdLike) &&
                            !req.DeviceIdLike.ToLower().Trim().Equals("string"),
                            x => x.DeviceId.Contains(req.DeviceIdLike))
                        .WhereIf(
                            !string.IsNullOrEmpty(req.ChannelIdLike) &&
                            !req.ChannelIdLike.ToLower().Trim().Equals("string"),
                            x => x.ChannelId.Contains(req.ChannelIdLike))
                        .WhereIf(req.IncludeSubDeptartment != null && req.IncludeSubDeptartment == true,
                            x => x.PDepartmentId.Equals(req.DepartmentId))
                        .OrderBy(orderBy)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_DataBaseExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_DataBaseExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
                return null;
            }

            ResGetVideoChannelList result = new ResGetVideoChannelList();
            result.VideoChannelList = retList;
            if (!isPageQuery)
            {
                if (retList != null)
                {
                    total = retList.Count;
                }
                else
                {
                    total = 0;
                }
            }

            result.Total = total;
            result.Request = req;
            return result;
        }

        /// <summary>
        /// 修改音视频通道参数
        /// </summary>
        /// <param name="mainId"></param>
        /// <param name="req"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static VideoChannel ModifyVideoChannel(string mainId, ReqModifyVideoChannel req, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            if (string.IsNullOrEmpty(mainId) || mainId.Trim().ToLower().Equals("string"))
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ParamsIsNotRight,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
                };
                return null;
            }

            if (!string.IsNullOrEmpty(req.MediaServerId) && !req.MediaServerId.ToLower().Trim().Equals("string"))
            {
                if (Common.MediaServerList.FindLast(x => x.MediaServerId.Trim().Equals(req.MediaServerId.Trim())) ==
                    null)
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_InstanceIsNull,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_InstanceIsNull],
                    };
                    return null;
                }
            }

            if (!string.IsNullOrEmpty(req.VideoSrcUrl) || !req.VideoSrcUrl.ToLower().Trim().Equals("string"))
            {
                if (req.DeviceStreamType != null && req.DeviceStreamType == DeviceStreamType.GB28181)
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.Sys_ParamsIsNotRight,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight] +
                                  ",VideoSrcUrl有值时，DeviceStreamtype不应该是GB28181",
                    };
                    return null;
                }
            }

            if (req.MethodByGetStream != MethodByGetStream.None)
            {
                if (string.IsNullOrEmpty(req.VideoSrcUrl) || req.VideoSrcUrl.ToLower().Trim().Equals("string") ||
                    req.DeviceStreamType == DeviceStreamType.GB28181)
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.Sys_ParamsIsNotRight,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight] +
                                  ",MethodByGetStream不为None时VideoSrcUrl不能为空，并且DeviceStreamtype不应该是GB28181",
                    };
                    return null;
                }
            }

            if (!string.IsNullOrEmpty(req.DeviceId) || !string.IsNullOrEmpty(req.ChannelId) ||
                !req.DeviceId.ToLower().Trim().Equals("string") || !req.ChannelId.ToLower().Trim().Equals("string"))
            {
                if (req.DeviceStreamType != DeviceStreamType.GB28181)
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.Sys_ParamsIsNotRight,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight] +
                                  ",当DeviceId,ChannleId不为空时,表示此通道为GB28181通道，则DeviceStreamType必须为GB28181",
                    };
                    return null;
                }
            }

            VideoChannel ret = null;
            try
            {
                ret = ORMHelper.Db.Select<VideoChannel>().Where(x => x.MainId.Trim().Equals(mainId.Trim()))
                    .First();
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_DataBaseExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_DataBaseExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
                return null;
            }

            if (ret == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_DB_VideoChannelNotExists,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_DB_VideoChannelNotExists],
                };
                return null;
            }

            try
            {
                var rAffrows = ORMHelper.Db.Update<VideoChannel>()
                    .SetIf(!string.IsNullOrEmpty(req.ChannelName) && !req.ChannelName.Trim().ToLower().Equals("string"),
                        x => x.ChannelName, req.ChannelName)
                    .SetIf(
                        !string.IsNullOrEmpty(req.DepartmentId) && !req.DepartmentId.ToLower().Trim().Equals("string"),
                        x => x.DepartmentId, req.DepartmentId)
                    .SetIf(
                        !string.IsNullOrEmpty(req.DepartmentName) &&
                        !req.DepartmentName.ToLower().Trim().Equals("string"),
                        x => x.DepartmentName, req.DepartmentName)
                    .SetIf(
                        !string.IsNullOrEmpty(req.PDepartmentId) &&
                        !req.PDepartmentId.ToLower().Trim().Equals("string"),
                        x => x.PDepartmentId, req.PDepartmentId)
                    .SetIf(
                        !string.IsNullOrEmpty(req.PDepartmentName) &&
                        !req.PDepartmentName.ToLower().Trim().Equals("string"),
                        x => x.PDepartmentName, req.PDepartmentName)
                    .SetIf(!string.IsNullOrEmpty(req.IpV4Address) && !req.IpV4Address.ToLower().Trim().Equals("string"),
                        x => x.IpV4Address, req.IpV4Address)
                    .SetIf(!string.IsNullOrEmpty(req.IpV6Address) && !req.IpV6Address.ToLower().Trim().Equals("string"),
                        x => x.IpV6Address, req.IpV6Address)
                    .SetIf(
                        !string.IsNullOrEmpty(req.MediaServerId) &&
                        !req.MediaServerId.ToLower().Trim().Equals("string"),
                        x => x.MediaServerId, req.MediaServerId)
                    .SetIf(!string.IsNullOrEmpty(req.VideoSrcUrl) && !req.VideoSrcUrl.ToLower().Trim().Equals("string"),
                        x => x.VideoSrcUrl, req.VideoSrcUrl)
                    .SetIf(!string.IsNullOrEmpty(req.DeviceId) && !req.DeviceId.ToLower().Trim().Equals("string"),
                        x => x.DeviceId, req.DeviceId)
                    .SetIf(!string.IsNullOrEmpty(req.ChannelId) && !req.ChannelId.ToLower().Trim().Equals("string"),
                        x => x.ChannelId, req.ChannelId)
                    .SetIf(req.AutoVideo != null, x => x.AutoVideo, req.AutoVideo)
                    .SetIf(req.HasPtz != null, x => x.HasPtz, req.HasPtz)
                    .SetIf(req.DefaultRtpPort != null, x => x.DefaultRtpPort, req.DefaultRtpPort)
                    .SetIf(req.DeviceNetworkType != null, x => x.DeviceNetworkType, req.DeviceNetworkType)
                    .SetIf(req.NoPlayerBreak != null, x => x.NoPlayerBreak, req.NoPlayerBreak)
                    .SetIf(req.RtpWithTcp != null, x => x.RtpWithTcp, req.RtpWithTcp)
                    .SetIf(req.VideoDeviceType != null, x => x.VideoDeviceType, req.VideoDeviceType)
                    .SetIf(req.AutoRecord != null, x => x.AutoRecord, req.AutoRecord)
                    .SetIf(req.Enable != null, x => x.Enabled, req.Enable)
                    .SetIf(req.DeviceStreamType != null, x => x.DeviceStreamType, req.DeviceStreamType)
                    .SetIf(req.MethodByGetStream != null, x => x.MethodByGetStream, req.MethodByGetStream)
                    .Set(x => x.UpdateTime, DateTime.Now)
                    .Where("1=1")
                    .Where(x => x.MainId.Trim().Equals(mainId.Trim())).ExecuteAffrows();
                if (rAffrows > 0)
                {
                    return ORMHelper.Db.Select<VideoChannel>().Where(x => x.MainId.Trim().Equals(mainId.Trim()))
                        .First();
                }
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_DataBaseExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_DataBaseExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
                return null;
            }

            rs = new ResponseStruct()
            {
                Code = ErrorNumber.Sys_DataBaseExcept,
                Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_DataBaseExcept],
                ExceptMessage = "数据库可能异常，具体原因不明"
            };
            return null;
        }

        /// <summary>
        /// 激活音视频通道
        /// </summary>
        /// <param name="mainId"></param>
        /// <param name="req"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static VideoChannel ActiveVideoChannel(string mainId, ReqActiveVideoChannel req, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            if (string.IsNullOrEmpty(mainId) || mainId.Trim().ToLower().Equals("string"))
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ParamsIsNotRight,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
                };
                return null;
            }

            if (string.IsNullOrEmpty(req.MediaServerId) || req.MediaServerId.ToLower().Trim().Equals("string"))
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ParamsIsNotRight,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
                };
                return null;
            }

            if (Common.MediaServerList.FindLast(x => x.MediaServerId.Trim().Equals(req.MediaServerId.Trim())) == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_InstanceIsNull,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_InstanceIsNull],
                };
                return null;
            }

            VideoChannel ret = null;
            try
            {
                ret = ORMHelper.Db.Select<VideoChannel>().Where(x => x.MainId.Trim().Equals(mainId.Trim()))
                    .Where(x => x.Enabled.Equals(false))
                    .Where(x => x.MediaServerId.Contains("unknown_server")).First();
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_DataBaseExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_DataBaseExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
                return null;
            }

            if (ret == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_DB_VideoChannelNotExists,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_DB_VideoChannelNotExists] + ",此设备可能已激活",
                };
                return null;
            }

            try
            {
                var rAffrows = ORMHelper.Db.Update<VideoChannel>()
                    .SetIf(!string.IsNullOrEmpty(req.ChannelName) && !req.ChannelName.Trim().ToLower().Equals("string"),
                        x => x.ChannelName, req.ChannelName)
                    .SetIf(
                        !string.IsNullOrEmpty(req.DepartmentId) && !req.DepartmentId.ToLower().Trim().Equals("string"),
                        x => x.DepartmentId, req.DepartmentId)
                    .SetIf(
                        !string.IsNullOrEmpty(req.DepartmentName) &&
                        !req.DepartmentName.ToLower().Trim().Equals("string"),
                        x => x.DepartmentName, req.DepartmentName)
                    .SetIf(
                        !string.IsNullOrEmpty(req.PDepartmentId) &&
                        !req.PDepartmentId.ToLower().Trim().Equals("string"),
                        x => x.PDepartmentId, req.PDepartmentId)
                    .SetIf(
                        !string.IsNullOrEmpty(req.PDepartmentName) &&
                        !req.PDepartmentName.ToLower().Trim().Equals("string"),
                        x => x.PDepartmentName, req.PDepartmentName)
                    .SetIf(!string.IsNullOrEmpty(req.IpV4Address) && !req.IpV4Address.ToLower().Trim().Equals("string"),
                        x => x.IpV4Address, req.IpV4Address)
                    .SetIf(!string.IsNullOrEmpty(req.IpV6Address) && !req.IpV6Address.ToLower().Trim().Equals("string"),
                        x => x.IpV6Address, req.IpV6Address)
                    .SetIf(req.AutoVideo != null, x => x.AutoVideo, req.AutoVideo)
                    .SetIf(req.HasPtz != null, x => x.HasPtz, req.HasPtz)
                    .SetIf(req.DefaultRtpPort != null, x => x.DefaultRtpPort, req.DefaultRtpPort)
                    .SetIf(req.DeviceNetworkType != null, x => x.DeviceNetworkType, req.DeviceNetworkType)
                    .SetIf(req.NoPlayerBreak != null, x => x.NoPlayerBreak, req.NoPlayerBreak)
                    .SetIf(req.RtpWithTcp != null, x => x.RtpWithTcp, req.RtpWithTcp)
                    .SetIf(req.VideoDeviceType != null, x => x.VideoDeviceType, req.VideoDeviceType)
                    .SetIf(req.AutoRecord != null, x => x.AutoRecord, req.AutoRecord)
                    .Set(x => x.MediaServerId, req.MediaServerId)
                    .Set(x => x.UpdateTime, DateTime.Now)
                    .Set(x => x.Enabled, true)
                    .Where("1=1")
                    .Where(x => x.MainId.Trim().Equals(mainId.Trim())).ExecuteAffrows();
                if (rAffrows > 0)
                {
                    return ORMHelper.Db.Select<VideoChannel>().Where(x => x.MainId.Trim().Equals(mainId.Trim()))
                        .First();
                }
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_DataBaseExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_DataBaseExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
                return null;
            }

            rs = new ResponseStruct()
            {
                Code = ErrorNumber.Sys_DataBaseExcept,
                Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_DataBaseExcept],
                ExceptMessage = "数据库可能异常，具体原因不明"
            };
            return null;
        }

        /// <summary>
        /// 获取未激活视频通道列表（支持分页）
        /// </summary>
        /// <param name="req"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static ResGetWaitForActiveVideoChannelList GetWaitForActiveVideoChannelList(ReqPaginationBase req,
            out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            bool isPageQuery = req.PageIndex != null;
            bool haveOrderBy = req.OrderBy != null;
            if (isPageQuery)
            {
                if (req.PageSzie > 10000)
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.Sys_DataBaseLimited,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_DataBaseLimited],
                    };
                    return null!;
                }

                if (req.PageIndex <= 0)
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.Sys_DataBaseLimited,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_DataBaseLimited],
                    };
                    return null!;
                }
            }

            string orderBy = "";
            if (haveOrderBy)
            {
                foreach (var order in req.OrderBy!)
                {
                    if (order != null)
                    {
                        orderBy += order.FieldName + " " + Enum.GetName(typeof(OrderByDir), order.OrderByDir!) + ",";
                    }
                }

                orderBy = orderBy.TrimEnd(',');
            }

            long total = -1;
            List<VideoChannel> retList = null!;
            try
            {
                if (!isPageQuery)
                {
                    retList = ORMHelper.Db.Select<VideoChannel>().Where("1=1")
                        .Where(x => x.Enabled == false).Where(x => x.MediaServerId.Contains("unknown_server"))
                        .OrderBy(orderBy)
                        .ToList();
                }
                else
                {
                    retList = ORMHelper.Db.Select<VideoChannel>().Where("1=1")
                        .Where(x => x.Enabled == false).Where(x => x.MediaServerId.Contains("unknown_server"))
                        .OrderBy(orderBy)
                        .Count(out total)
                        .Page((int) req.PageIndex!, (int) req.PageSzie!)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_DataBaseExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_DataBaseExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
                return null;
            }

            ResGetWaitForActiveVideoChannelList result = new ResGetWaitForActiveVideoChannelList();
            result.VideoChannelList = retList;
            if (!isPageQuery)
            {
                if (retList != null)
                {
                    total = retList.Count;
                }
                else
                {
                    total = 0;
                }
            }

            result.Total = total;
            result.Request = req;
            return result;
        }

        /// <summary>
        /// 通过MediaServerId获取流媒体服务器实例
        /// </summary>
        /// <param name="mediaServerId"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static ServerInstance GetMediaServerByMediaServerId(string mediaServerId, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            return Common.MediaServerList.FindLast(x => x.MediaServerId.Trim().Equals(mediaServerId.Trim()));
        }

        /// <summary>
        /// 获取流媒体服务器列表
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static List<ServerInstance> GetMediaServerList(out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            return Common.MediaServerList;
        }

        /// <summary>
        /// 添加一个裁剪合并任务
        /// </summary>
        /// <param name="mediaServerId"></param>
        /// <param name="reqKeeper"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static ResKeeperCutMergeTaskResponse AddCutOrMergeTask(string mediaServerId,
            ReqKeeperCutMergeTask reqKeeper, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (string.IsNullOrEmpty(mediaServerId))
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ParamsIsNotRight,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
                };
                return null;
            }

            if (reqKeeper == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ParamsIsNotRight,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
                };
                return null;
            }

            var mediaServer = Common.MediaServerList.FindLast(x => x.MediaServerId.Trim().Equals(mediaServerId.Trim()));
            if (mediaServer == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_InstanceIsNull,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_InstanceIsNull],
                };
                return null;
            }

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning == false ||
                mediaServer.IsMediaServerRunning == false || mediaServer.WebApiHelper == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_NotRunning,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_NotRunning],
                };
                return null;
            }

            var ret = mediaServer.KeeperWebApi.AddCutOrMergeTask(out rs, reqKeeper);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                return null;
            }

            return ret;
        }

        /// <summary>
        /// 获取裁剪合并任务状态
        /// </summary>
        /// <param name="mediaServerId"></param>
        /// <param name="taskId"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static ResKeeperCutMergeTaskStatusResponse GetMergeTaskStatus(string mediaServerId, string taskId,
            out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (string.IsNullOrEmpty(mediaServerId))
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ParamsIsNotRight,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
                };
                return null;
            }

            if (string.IsNullOrEmpty(taskId))
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ParamsIsNotRight,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
                };
                return null;
            }

            var mediaServer = Common.MediaServerList.FindLast(x => x.MediaServerId.Trim().Equals(mediaServerId.Trim()));
            if (mediaServer == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_InstanceIsNull,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_InstanceIsNull],
                };
                return null;
            }

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning == false ||
                mediaServer.IsMediaServerRunning == false || mediaServer.WebApiHelper == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_NotRunning,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_NotRunning],
                };
                return null;
            }

            var ret = mediaServer.KeeperWebApi.GetMergeTaskStatus(out rs, taskId);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                return null;
            }

            return ret;
        }

        /// <summary>
        /// 获取裁剪合并任务积压情况
        /// </summary>
        /// <param name="mediaServerId"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static ResKeeperCutMergeTaskStatusResponseList GetBacklogTaskList(string mediaServerId,
            out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (string.IsNullOrEmpty(mediaServerId))
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ParamsIsNotRight,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
                };
                return null;
            }


            var mediaServer = Common.MediaServerList.FindLast(x => x.MediaServerId.Trim().Equals(mediaServerId.Trim()));
            if (mediaServer == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_InstanceIsNull,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_InstanceIsNull],
                };
                return null;
            }

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning == false ||
                mediaServer.IsMediaServerRunning == false || mediaServer.WebApiHelper == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_NotRunning,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_NotRunning],
                };
                return null;
            }

            var ret = mediaServer.KeeperWebApi.GetBacklogTaskList(out rs);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                return null;
            }

            return ret;
        }

        /// <summary>
        /// 获取一个可用的rtp端口（偶数端口）
        /// </summary>
        /// <param name="mediaServerId"></param>
        /// <param name="rs"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static ushort GuessAnRtpPort(string mediaServerId, out ResponseStruct rs, ushort? min = 0,
            ushort? max = 0)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (string.IsNullOrEmpty(mediaServerId))
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ParamsIsNotRight,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
                };
                return 0;
            }


            var mediaServer = Common.MediaServerList.FindLast(x => x.MediaServerId.Trim().Equals(mediaServerId.Trim()));
            if (mediaServer == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_InstanceIsNull,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_InstanceIsNull],
                };
                return 0;
            }

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning == false ||
                mediaServer.IsMediaServerRunning == false || mediaServer.WebApiHelper == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_NotRunning,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_NotRunning],
                };
                return 0;
            }

            var ret = mediaServer.KeeperWebApi.GuessAnRtpPort(out rs, min, max);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                return 0;
            }

            return ret;
        }

        /// <summary>
        /// 删除一个指定文件
        /// </summary>
        /// <param name="mediaServerId"></param>
        /// <param name="filePath"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static bool DeleteFile(string mediaServerId, string filePath, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (string.IsNullOrEmpty(mediaServerId))
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ParamsIsNotRight,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
                };
                return false;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ParamsIsNotRight,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
                };
                return false;
            }

            var mediaServer = Common.MediaServerList.FindLast(x => x.MediaServerId.Trim().Equals(mediaServerId.Trim()));
            if (mediaServer == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_InstanceIsNull,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_InstanceIsNull],
                };
                return false;
            }

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning == false ||
                mediaServer.IsMediaServerRunning == false || mediaServer.WebApiHelper == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_NotRunning,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_NotRunning],
                };
                return false;
            }

            var ret = mediaServer.KeeperWebApi.DeleteFile(out rs, filePath);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                return false;
            }

            return ret;
        }

        /// <summary>
        /// 获取流媒体治理程序健康状态
        /// </summary>
        /// <param name="mediaServerId"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static bool KeeperHealth(string mediaServerId, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (string.IsNullOrEmpty(mediaServerId))
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ParamsIsNotRight,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
                };
                return false;
            }


            var mediaServer = Common.MediaServerList.FindLast(x => x.MediaServerId.Trim().Equals(mediaServerId.Trim()));
            if (mediaServer == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_InstanceIsNull,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_InstanceIsNull],
                };
                return false;
            }

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning == false)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_AKStreamKeeperNotRunning,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_AKStreamKeeperNotRunning],
                };
                return false;
            }

            var ret = mediaServer.KeeperWebApi.KeeperHealth(out rs);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                return false;
            }

            return ret;
        }

        /// <summary>
        /// 指定文件是否存在
        /// </summary>
        /// <param name="mediaServerId"></param>
        /// <param name="filePath"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static bool FileExists(string mediaServerId, string filePath, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (string.IsNullOrEmpty(mediaServerId))
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ParamsIsNotRight,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
                };
                return false;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ParamsIsNotRight,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
                };
                return false;
            }

            var mediaServer = Common.MediaServerList.FindLast(x => x.MediaServerId.Trim().Equals(mediaServerId.Trim()));
            if (mediaServer == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_InstanceIsNull,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_InstanceIsNull],
                };
                return false;
            }

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning == false ||
                mediaServer.IsMediaServerRunning == false || mediaServer.WebApiHelper == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_NotRunning,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_NotRunning],
                };
                return false;
            }

            var ret = mediaServer.KeeperWebApi.FileExists(out rs, filePath);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                return false;
            }

            return ret;
        }

        /// <summary>
        /// 删除文件列表
        /// </summary>
        /// <param name="mediaServerId"></param>
        /// <param name="fileList"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static ResKeeperDeleteFileList DeleteFileList(string mediaServerId, List<string> fileList,
            out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (string.IsNullOrEmpty(mediaServerId))
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ParamsIsNotRight,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
                };
                return null;
            }

            if (fileList == null || fileList.Count <= 0)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ParamsIsNotRight,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
                };
                return null;
            }

            var mediaServer = Common.MediaServerList.FindLast(x => x.MediaServerId.Trim().Equals(mediaServerId.Trim()));
            if (mediaServer == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_InstanceIsNull,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_InstanceIsNull],
                };
                return null;
            }

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning == false ||
                mediaServer.IsMediaServerRunning == false || mediaServer.WebApiHelper == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_NotRunning,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_NotRunning],
                };
                return null;
            }

            var ret = mediaServer.KeeperWebApi.DeleteFileList(out rs, fileList);
            if (ret == null || !rs.Code.Equals(ErrorNumber.None))
            {
                return null;
            }

            return ret;
        }


        /// <summary>
        /// 清空空目录
        /// </summary>
        /// <param name="mediaServerId"></param>
        /// <param name="rs"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool CleanUpEmptyDir(string mediaServerId, out ResponseStruct rs, string? filePath = "")
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (string.IsNullOrEmpty(mediaServerId))
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ParamsIsNotRight,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
                };
                return false;
            }

            var mediaServer = Common.MediaServerList.FindLast(x => x.MediaServerId.Trim().Equals(mediaServerId.Trim()));
            if (mediaServer == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_InstanceIsNull,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_InstanceIsNull],
                };
                return false;
            }

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning == false ||
                mediaServer.IsMediaServerRunning == false || mediaServer.WebApiHelper == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_NotRunning,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_NotRunning],
                };
                return false;
            }

            var ret = mediaServer.KeeperWebApi.CleanUpEmptyDir(out rs, filePath);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                return false;
            }

            return ret;
        }

        public static ResKeeperStartMediaServer StartMediaServer(string mediaServerId, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (string.IsNullOrEmpty(mediaServerId))
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ParamsIsNotRight,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
                };
                return null;
            }

            var mediaServer = Common.MediaServerList.FindLast(x => x.MediaServerId.Trim().Equals(mediaServerId.Trim()));
            if (mediaServer == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_InstanceIsNull,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_InstanceIsNull],
                };
                return null;
            }

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning == false)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_AKStreamKeeperNotRunning,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_AKStreamKeeperNotRunning],
                };
                return null;
            }

            var ret = mediaServer.KeeperWebApi.StartMediaServer(out rs);
            if (ret == null || !rs.Code.Equals(ErrorNumber.None))
            {
                return null;
            }

            return ret;
        }

        /// <summary>
        /// 停止流媒体服务器
        /// </summary>
        /// <param name="mediaServerId"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static bool ShutdownMediaServer(string mediaServerId, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (string.IsNullOrEmpty(mediaServerId))
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ParamsIsNotRight,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
                };
                return false;
            }

            var mediaServer = Common.MediaServerList.FindLast(x => x.MediaServerId.Trim().Equals(mediaServerId.Trim()));
            if (mediaServer == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_InstanceIsNull,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_InstanceIsNull],
                };
                return false;
            }

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning == false ||
                mediaServer.IsMediaServerRunning == false || mediaServer.WebApiHelper == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_NotRunning,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_NotRunning],
                };
                return false;
            }

            var ret = mediaServer.KeeperWebApi.ShutdownMediaServer(out rs);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                return false;
            }

            return ret;
        }


        /// <summary>
        /// 重新启动流媒体服务器
        /// </summary>
        /// <param name="mediaServerId"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static ResKeeperRestartMediaServer RestartMediaServer(string mediaServerId, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (string.IsNullOrEmpty(mediaServerId))
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ParamsIsNotRight,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
                };
                return null;
            }

            var mediaServer = Common.MediaServerList.FindLast(x => x.MediaServerId.Trim().Equals(mediaServerId.Trim()));
            if (mediaServer == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_InstanceIsNull,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_InstanceIsNull],
                };
                return null;
            }

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning == false)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_AKStreamKeeperNotRunning,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_AKStreamKeeperNotRunning],
                };
                return null;
            }

            var ret = mediaServer.KeeperWebApi.RestartMediaServer(out rs);
            if (ret == null || !rs.Code.Equals(ErrorNumber.None))
            {
                return null;
            }

            return ret;
        }

        /// <summary>
        /// 热加载流媒体服务器配置文件
        /// </summary>
        /// <param name="mediaServerId"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static bool ReloadMediaServer(string mediaServerId, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (string.IsNullOrEmpty(mediaServerId))
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ParamsIsNotRight,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
                };
                return false;
            }

            var mediaServer = Common.MediaServerList.FindLast(x => x.MediaServerId.Trim().Equals(mediaServerId.Trim()));
            if (mediaServer == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_InstanceIsNull,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_InstanceIsNull],
                };
                return false;
            }

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning == false ||
                mediaServer.IsMediaServerRunning == false || mediaServer.WebApiHelper == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_NotRunning,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_NotRunning],
                };
                return false;
            }

            var ret = mediaServer.KeeperWebApi.ReloadMediaServer(out rs);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                return false;
            }

            return ret;
        }

        /// <summary>
        /// 获取流媒体服务器运行状态
        /// </summary>
        /// <param name="mediaServerId"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static ResKeeperCheckMediaServerRunning CheckMediaServerRunning(string mediaServerId,
            out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (string.IsNullOrEmpty(mediaServerId))
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ParamsIsNotRight,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
                };
                return null;
            }

            var mediaServer = Common.MediaServerList.FindLast(x => x.MediaServerId.Trim().Equals(mediaServerId.Trim()));
            if (mediaServer == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_InstanceIsNull,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_InstanceIsNull],
                };
                return null;
            }

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning == false ||
                mediaServer.IsMediaServerRunning == false || mediaServer.WebApiHelper == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_NotRunning,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_NotRunning],
                };
                return null;
            }

            var ret = mediaServer.KeeperWebApi.CheckMediaServerRunning(out rs);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                return null;
            }

            return ret;
        }

        /// <summary>
        /// 保持与流媒体服务器的心跳
        /// </summary>
        /// <param name="req"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static ResMediaServerKeepAlive MediaServerKeepAlive(ReqMediaServerKeepAlive req, out ResponseStruct rs)
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
                    if (req.FirstPost)
                    {
                        mediaServer.Dispose();
                        Common.MediaServerList.Remove(mediaServer);
                        result = new ResMediaServerKeepAlive()
                        {
                            Rs = rs,
                            RecommendTimeSynchronization = false,
                            ServerDateTime = DateTime.Now,
                            NeedRestartMediaServer = true,
                        };
                        Logger.Debug(
                            $"[{Common.LoggerHead}]->清理MediaServerList中的的流媒体服务器实例,要求重启流媒体服务器->当前流媒体服务器数量:{Common.MediaServerList.Count}");
                        return result;
                    }

                    //已经存在的
                    if ((DateTime.Now - mediaServer.KeepAliveTime).Seconds < 5) //10秒内多次心跳请求直接回复
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
                    mediaServer.IsKeeperRunning = true;
                    mediaServer.IsMediaServerRunning = req.MediaServerIsRunning;
                    mediaServer.KeeperPort = req.KeeperWebApiPort;
                    mediaServer.RecordPathList = req.RecordPathList;
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
                    mediaServer.AccessKey = req.AccessKey;
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
                    tmpMediaServer.IsKeeperRunning = true;
                    tmpMediaServer.IsMediaServerRunning = req.MediaServerIsRunning;
                    tmpMediaServer.KeeperPort = req.KeeperWebApiPort;
                    tmpMediaServer.RecordPathList = req.RecordPathList;
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
                    tmpMediaServer.AccessKey = req.AccessKey;

                    if (req.PerformanceInfo != null) //更新性能信息
                    {
                        tmpMediaServer.PerformanceInfo = req.PerformanceInfo;
                    }

                    tmpMediaServer.WebApiHelper = new WebApiHelper(tmpMediaServer.IpV4Address,
                        tmpMediaServer.UseSsl ? tmpMediaServer.HttpsPort : tmpMediaServer.HttpPort,
                        tmpMediaServer.Secret, "", tmpMediaServer.UseSsl);
                    tmpMediaServer.KeeperWebApi = new KeeperWebApi(tmpMediaServer.IpV4Address,
                        tmpMediaServer.KeeperPort, tmpMediaServer.AccessKey);
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