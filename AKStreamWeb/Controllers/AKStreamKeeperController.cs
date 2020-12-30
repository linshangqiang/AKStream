using System.Collections.Generic;
using AKStreamWeb.Attributes;
using AKStreamWeb.Services;
using LibCommon;
using LibCommon.Structs;
using LibCommon.Structs.WebRequest.AKStreamKeeper;
using LibCommon.Structs.WebResponse.AKStreamKeeper;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AKStreamWeb.Controllers
{
    [Log]
    [AuthVerify]
    [ApiController]
    [Route("/AKStreamKeeper")]
    [SwaggerTag("流媒体服务器治理的相关接口")]
    public class AKStreamKeeperController : ControllerBase
    {
        /// <summary>
        /// 添加一个裁剪合并任务
        /// </summary>
        /// <param name="AccessKey"></param>
        /// <param name="mediaServerId"></param>
        /// <param name="reqKeeper"></param>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [Route("AddCutOrMergeTask")]
        [HttpPost]
        public ResKeeperCutMergeTaskResponse AddCutOrMergeTask(
            [FromHeader(Name = "AccessKey")] string AccessKey, string mediaServerId,ReqKeeperCutMergeTask reqKeeper)
        {
            ResponseStruct rs;
            var ret = MediaServerService.AddCutOrMergeTask(mediaServerId,reqKeeper, out rs);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }
        
        /// <summary>
        /// 获取裁剪合并任务状态
        /// </summary>
        /// <param name="AccessKey"></param>
        /// <param name="mediaServerId"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [Route("GetMergeTaskStatus")]
        [HttpGet]
        public ResKeeperCutMergeTaskStatusResponse GetMergeTaskStatus(
            [FromHeader(Name = "AccessKey")] string AccessKey, string mediaServerId,string taskId)
        {
            ResponseStruct rs;
            var ret = MediaServerService.GetMergeTaskStatus(mediaServerId,taskId, out rs);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }
        
        /// <summary>
        /// 获取裁剪合并任务积压列表
        /// </summary>
        /// <param name="AccessKey"></param>
        /// <param name="mediaServerId"></param>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [Route("GetBacklogTaskList")]
        [HttpGet]
        public ResKeeperCutMergeTaskStatusResponseList GetBacklogTaskList(
            [FromHeader(Name = "AccessKey")] string AccessKey, string mediaServerId)
        {
            ResponseStruct rs;
            var ret = MediaServerService.GetBacklogTaskList(mediaServerId, out rs);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }

     
        /// <summary>
        /// 获取一个可用的rtp端口（偶数端口）
        /// </summary>
        /// <param name="AccessKey"></param>
        /// <param name="mediaServerId"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [Route("GuessAnRtpPort")]
        [HttpGet]
        public ushort GuessAnRtpPort(
            [FromHeader(Name = "AccessKey")] string AccessKey, string mediaServerId, ushort? min=0,ushort? max=0)
        {
            ResponseStruct rs;
            var ret = MediaServerService.GuessAnRtpPort(mediaServerId, out rs,min,max);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }
        
        
        /// <summary>
        /// 删除一个指定文件
        /// </summary>
        /// <param name="AccessKey"></param>
        /// <param name="mediaServerId"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [Route("DeleteFile")]
        [HttpGet]
        public bool DeleteFile(
            [FromHeader(Name = "AccessKey")] string AccessKey, string mediaServerId, string filePath)
        {
            ResponseStruct rs;
            var ret = MediaServerService.DeleteFile(mediaServerId, filePath, out rs);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }


        /// <summary>
        /// 获取流媒体治理程序健康状态
        /// </summary>
        /// <param name="AccessKey"></param>
        /// <param name="mediaServerId"></param>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [Route("KeeperHealth")]
        [HttpGet]
        public bool KeeperHealth(
            [FromHeader(Name = "AccessKey")] string AccessKey, string mediaServerId)
        {
            ResponseStruct rs;
            var ret = MediaServerService.KeeperHealth(mediaServerId, out rs);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }


        /// <summary>
        /// 指定文件是否存在
        /// </summary>
        /// <param name="AccessKey"></param>
        /// <param name="mediaServerId"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [Route("FileExists")]
        [HttpGet]
        public bool FileExists(
            [FromHeader(Name = "AccessKey")] string AccessKey, string mediaServerId, string filePath)
        {
            ResponseStruct rs;
            var ret = MediaServerService.FileExists(mediaServerId, filePath, out rs);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }

        /// <summary>
        /// 批量删除文件
        /// </summary>
        /// <param name="AccessKey"></param>
        /// <param name="mediaServerId"></param>
        /// <param name="fileList"></param>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [Route("DeleteFileList")]
        [HttpPost]
        public ResKeeperDeleteFileList DeleteFileList(
            [FromHeader(Name = "AccessKey")] string AccessKey, string mediaServerId, List<string> fileList)
        {
            ResponseStruct rs;
            var ret = MediaServerService.DeleteFileList(mediaServerId, fileList, out rs);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }

        /// <summary>
        /// 清空没有文件的存储目录
        /// </summary>
        /// <param name="AccessKey"></param>
        /// <param name="mediaServerId"></param>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [Route("CleanUpEmptyDir")]
        [HttpGet]
        public bool CleanUpEmptyDir(
            [FromHeader(Name = "AccessKey")] string AccessKey, string mediaServerId, string? filePath = "")
        {
            ResponseStruct rs;
            var ret = MediaServerService.CleanUpEmptyDir(mediaServerId, out rs, filePath);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }

        /// <summary>
        /// 启动流媒体服务器
        /// </summary>
        /// <param name="AccessKey"></param>
        /// <param name="mediaServerId"></param>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [Route("StartMediaServer")]
        [HttpGet]
        public ResKeeperStartMediaServer StartMediaServer(
            [FromHeader(Name = "AccessKey")] string AccessKey, string mediaServerId)
        {
            ResponseStruct rs;
            var ret = MediaServerService.StartMediaServer(mediaServerId, out rs);
            if (ret == null || !rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }

        /// <summary>
        /// 停止流媒体服务器
        /// </summary>
        /// <param name="AccessKey"></param>
        /// <param name="mediaServerId"></param>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [Route("ShutdownMediaServer")]
        [HttpGet]
        public bool ShutdownMediaServer(
            [FromHeader(Name = "AccessKey")] string AccessKey, string mediaServerId)
        {
            ResponseStruct rs;
            var ret = MediaServerService.ShutdownMediaServer(mediaServerId, out rs);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }

        /// <summary>
        /// 重新启动流媒体服务器
        /// </summary>
        /// <param name="AccessKey"></param>
        /// <param name="mediaServerId"></param>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [Route("RestartMediaServer")]
        [HttpGet]
        public ResKeeperRestartMediaServer RestartMediaServer(
            [FromHeader(Name = "AccessKey")] string AccessKey, string mediaServerId)
        {
            ResponseStruct rs;
            var ret = MediaServerService.RestartMediaServer(mediaServerId, out rs);
            if (ret == null || !rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }

        /// <summary>
        /// 流媒体服务器配置文件热加载
        /// </summary>
        /// <param name="AccessKey"></param>
        /// <param name="mediaServerId"></param>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [Route("ReloadMediaServer")]
        [HttpGet]
        public bool ReloadMediaServer(
            [FromHeader(Name = "AccessKey")] string AccessKey, string mediaServerId)
        {
            ResponseStruct rs;
            var ret = MediaServerService.ReloadMediaServer(mediaServerId, out rs);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }


        /// <summary>
        /// 检查流媒体服务器的运行状态
        /// </summary>
        /// <param name="AccessKey"></param>
        /// <param name="mediaServerId"></param>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [Route("CheckMediaServerRunning")]
        [HttpGet]
        public ResKeeperCheckMediaServerRunning CheckMediaServerRunning(
            [FromHeader(Name = "AccessKey")] string AccessKey, string mediaServerId)
        {
            ResponseStruct rs;
            var ret = MediaServerService.CheckMediaServerRunning(mediaServerId, out rs);
            if (ret == null || !rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }
    }
}