using System;
using System.Collections.Generic;
using LibCommon;
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
        /// 添加一个裁剪合并任务
        /// </summary>
        /// <param name="mediaServerId"></param>
        /// <param name="reqKeeper"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
         public static ResKeeperCutMergeTaskResponse AddCutOrMergeTask(string mediaServerId,ReqKeeperCutMergeTask reqKeeper,out ResponseStruct rs)
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

            if (reqKeeper==null)
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

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning==false || mediaServer.IsMediaServerRunning==false || mediaServer.WebApiHelper==null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_NotRunning,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_NotRunning],
                };
                return null;
            }

            var ret = mediaServer.KeeperWebApi.AddCutOrMergeTask(out rs, reqKeeper);
            if ( !rs.Code.Equals(ErrorNumber.None))
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
        public static ResKeeperCutMergeTaskStatusResponse GetMergeTaskStatus(string mediaServerId,string taskId,out ResponseStruct rs)
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

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning==false|| mediaServer.IsMediaServerRunning==false || mediaServer.WebApiHelper==null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_NotRunning,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_NotRunning],
                };
                return null;
            }

            var ret = mediaServer.KeeperWebApi.GetMergeTaskStatus(out rs, taskId);
            if ( !rs.Code.Equals(ErrorNumber.None))
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
        public static List<ResKeeperCutMergeTaskStatusResponse> GetBacklogTaskList(string mediaServerId,out ResponseStruct rs)
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

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning==false|| mediaServer.IsMediaServerRunning==false || mediaServer.WebApiHelper==null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_NotRunning,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_NotRunning],
                };
                return null;
            }

            var ret = mediaServer.KeeperWebApi.GetBacklogTaskList(out rs);
            if ( !rs.Code.Equals(ErrorNumber.None))
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
        public static ushort GuessAnRtpPort(string mediaServerId,out ResponseStruct rs,ushort? min = 0, ushort? max = 0)
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

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning==false|| mediaServer.IsMediaServerRunning==false || mediaServer.WebApiHelper==null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_NotRunning,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_NotRunning],
                };
                return 0;
            }

            var ret = mediaServer.KeeperWebApi.GuessAnRtpPort(out rs,min,max);
            if ( !rs.Code.Equals(ErrorNumber.None))
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
        public static bool DeleteFile(string mediaServerId,string filePath,out ResponseStruct rs)
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

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning==false|| mediaServer.IsMediaServerRunning==false || mediaServer.WebApiHelper==null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_NotRunning,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_NotRunning],
                };
                return false;
            }

            var ret = mediaServer.KeeperWebApi.DeleteFile(out rs,filePath);
            if ( !rs.Code.Equals(ErrorNumber.None))
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
        public static bool KeeperHealth(string mediaServerId,out ResponseStruct rs)
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

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning==false)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_AKStreamKeeperNotRunning,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_AKStreamKeeperNotRunning],
                };
                return false;
            }

            var ret = mediaServer.KeeperWebApi.KeeperHealth(out rs);
            if ( !rs.Code.Equals(ErrorNumber.None))
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
         public static bool FileExists(string mediaServerId,string filePath,out ResponseStruct rs)
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

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning==false|| mediaServer.IsMediaServerRunning==false || mediaServer.WebApiHelper==null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_NotRunning,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_NotRunning],
                };
                return false;
            }

            var ret = mediaServer.KeeperWebApi.FileExists(out rs,filePath);
            if (  !rs.Code.Equals(ErrorNumber.None))
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
        public static ResKeeperDeleteFileList DeleteFileList(string mediaServerId,List<string> fileList,out ResponseStruct rs)
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

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning==false|| mediaServer.IsMediaServerRunning==false || mediaServer.WebApiHelper==null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_NotRunning,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_NotRunning],
                };
                return null;
            }

            var ret = mediaServer.KeeperWebApi.DeleteFileList(out rs,fileList);
            if ( ret==null || !rs.Code.Equals(ErrorNumber.None))
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
        public static bool CleanUpEmptyDir(string mediaServerId,out ResponseStruct rs,string? filePath="")
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

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning==false|| mediaServer.IsMediaServerRunning==false || mediaServer.WebApiHelper==null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_NotRunning,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_NotRunning],
                };
                return false;
            }

            var ret = mediaServer.KeeperWebApi.CleanUpEmptyDir(out rs,filePath);
            if ( !rs.Code.Equals(ErrorNumber.None))
            {
                return false;
            }

            return ret;
        }
        
        public static ResKeeperStartMediaServer StartMediaServer(string mediaServerId,out ResponseStruct rs)
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

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning==false)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_AKStreamKeeperNotRunning,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_AKStreamKeeperNotRunning],
                };
                return null;
            }

            var ret = mediaServer.KeeperWebApi.StartMediaServer(out rs);
            if (ret==null || !rs.Code.Equals(ErrorNumber.None))
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
        public static bool ShutdownMediaServer(string mediaServerId,out ResponseStruct rs)
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

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning==false|| mediaServer.IsMediaServerRunning==false || mediaServer.WebApiHelper==null)
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
        public static ResKeeperRestartMediaServer RestartMediaServer(string mediaServerId,out ResponseStruct rs)
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

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning==false)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_AKStreamKeeperNotRunning,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_AKStreamKeeperNotRunning],
                };
                return null;
            }

            var ret = mediaServer.KeeperWebApi.RestartMediaServer(out rs);
            if (ret==null || !rs.Code.Equals(ErrorNumber.None))
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
        public static bool ReloadMediaServer(string mediaServerId,out ResponseStruct rs)
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

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning==false|| mediaServer.IsMediaServerRunning==false || mediaServer.WebApiHelper==null)
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
        public static ResKeeperCheckMediaServerRunning CheckMediaServerRunning(string mediaServerId,out ResponseStruct rs)
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

            if (mediaServer.KeeperWebApi == null || mediaServer.IsKeeperRunning==false|| mediaServer.IsMediaServerRunning==false || mediaServer.WebApiHelper==null)
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