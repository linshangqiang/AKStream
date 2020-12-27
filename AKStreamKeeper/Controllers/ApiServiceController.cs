using System.Collections.Generic;
using AKStreamKeeper.Attributes;
using AKStreamKeeper.Services;
using LibCommon;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AKStreamKeeper.Controllers
{
    [ApiController]
    [Route("/ApiService")]
    [SwaggerTag("流媒体服务器相关接口")]
    public class ApiServiceController : ControllerBase
    {
        /// <summary>
        /// 获取流媒体服务器运行状态
        /// </summary>
        /// <returns>返回pid,大于0说明正在运行，否则为未运行</returns>
        /// <exception cref="AkStreamException"></exception>
        [Route("CheckMediaServerRunning")]
        [HttpGet]
        public int CheckMediaServerRunning()
        {
            ResponseStruct rs;
            var ret = ApiService.CheckMediaServerRunning(out rs);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }

        /// <summary>
        /// 热加载流媒体服务器配置
        /// </summary>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [Route("ReloadMediaServer")]
        [HttpGet]
        public bool ReloadMediaServer()
        {
            ResponseStruct rs;
            var ret = ApiService.ReloadMediaServer(out rs);
            if (!ret || !rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }

        /// <summary>
        /// 重启流媒体服务器
        /// </summary>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [Route("RestartMediaServer")]
        [HttpGet]
        public int RestartMediaServer()
        {
            ResponseStruct rs;
            var ret = ApiService.RestartMediaServer(out rs);
            if (ret <= 0 || !rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }

        /// <summary>
        /// 终止流媒体服务器
        /// </summary>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [Route("ShutdownMediaServer")]
        [HttpGet]
        public bool ShutdownMediaServer()
        {
            ResponseStruct rs;
            var ret = ApiService.ShutdownMediaServer(out rs);
            if (!ret || !rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }

        /// <summary>
        /// 启动流媒体服务器
        /// </summary>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [Route("StartMediaServer")]
        [HttpGet]
        public int StartMediaServer()
        {
            ResponseStruct rs;
            var ret = ApiService.StartMediaServer(out rs);
            if (ret <= 0 || !rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }

        /// <summary>
        /// 清理空目录
        /// </summary>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [Route("CleanUpEmptyDir")]
        [HttpGet]
        public bool CleanUpEmptyDir(string? filePath = "")
        {
            ResponseStruct rs;
            var ret = ApiService.CleanUpEmptyDir(out rs);
            if (!ret || !rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }

        /// <summary>
        /// 批量删除文件
        /// </summary>
        /// <param name="fileList"></param>
        /// <returns>当有文件未正常删除时返回这些文件列表</returns>
        /// <exception cref="AkStreamException"></exception>
        [Route("DeleteFileList")]
        [HttpPost]
        public List<string> DeleteFileList(List<string> fileList)
        {
            ResponseStruct rs;
            var ret = ApiService.DeleteFileList(fileList, out rs);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }

        /// <summary>
        /// 文件是否存在
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Route("FileExists")]
        [HttpGet]
        public bool FileExists(string filePath)
        {
            ResponseStruct rs;
            var ret = ApiService.FileExists(filePath, out rs);
            if (!ret || !rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }

        /// <summary>
        /// 健康检测
        /// </summary>
        /// <returns></returns>
        [Route("WebApiHealth")]
        [HttpGet]
        [Log]
        public string WebApiHealth()
        {
            return "OK";
        }

        /// <summary>
        /// 删除一个指定的文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [Route("DeleteFile")]
        [HttpGet]
        [Log]
        public bool DeleteFile(string filePath)
        {
            ResponseStruct rs;
            var ret = ApiService.DeleteFile(filePath, out rs);
            if (!ret || !rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }

        /// <summary>
        /// 获取一个可用的rtp端口（配置文件中minPort-maxPort的范围内的偶数端口）
        /// </summary>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [Route("GuessAnRtpPort")]
        [HttpGet]
        [Log]
        public ushort GuessAnRtpPort(ushort? min = 0, ushort? max = 0)
        {
            ResponseStruct rs;
            var ret = ApiService.GuessAnRtpPort(out rs, min, max);
            if (ret == 0 || !rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }
    }
}