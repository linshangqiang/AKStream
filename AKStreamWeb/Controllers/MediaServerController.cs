using System.Collections.Generic;
using AKStreamWeb.Attributes;
using AKStreamWeb.Services;
using LibCommon;
using LibCommon.Structs.DBModels;
using LibCommon.Structs.WebRequest;
using LibCommon.Structs.WebResponse;
using LibZLMediaKitMediaServer;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AKStreamWeb.Controllers
{
    [Log]
    [ApiController]
    [Route("/MediaServer")]
    [SwaggerTag("流媒体相关接口")]
    public class MediaServerController : ControllerBase
    {
        
        /// <summary>
        /// 获取音视频列表（支持分页，全表条件）
        /// </summary>
        /// <param name="AccessKey"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [AuthVerify]
        [Route("GetVideoChannelList")]
        [HttpPost]
        public ResGetVideoChannelList GetVideoChannelList([FromHeader(Name = "AccessKey")] string AccessKey,ReqGetVideoChannelList  req)
        {
            ResponseStruct rs;
            var ret = MediaServerService.GetVideoChannelList(req,out rs);
            if (rs.Code != ErrorNumber.None)
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }
        
        
        /// <summary>
        /// 修改音视频通道参数
        /// </summary>
        /// <param name="AccessKey"></param>
        /// <param name="mainId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [AuthVerify]
        [Route("ModifyVideoChannel")]
        [HttpPost]
        public VideoChannel ModifyVideoChannel([FromHeader(Name = "AccessKey")] string AccessKey,string mainId, ReqModifyVideoChannel req)
        {
            ResponseStruct rs;
            var ret = MediaServerService.ModifyVideoChannel(mainId,req,out rs);
            if (rs.Code != ErrorNumber.None)
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }
        
       
        /// <summary>
        /// 激活音视频通道
        /// </summary>
        /// <param name="AccessKey"></param>
        /// <param name="mainId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [AuthVerify]
        [Route("ActiveVideoChannel")]
        [HttpPost]
        public VideoChannel ActiveVideoChannel([FromHeader(Name = "AccessKey")] string AccessKey,string mainId, ReqActiveVideoChannel req)
        {
            ResponseStruct rs;
            var ret = MediaServerService.ActiveVideoChannel(mainId,req,out rs);
            if (rs.Code != ErrorNumber.None)
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }
        
        
        /// <summary>
        /// 获取未激活视频通道列表（支持分页）
        /// </summary>
        /// <param name="AccessKey"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [AuthVerify]
        [Route("GetWaitForActiveVideoChannelList")]
        [HttpPost]
        public ResGetWaitForActiveVideoChannelList GetWaitForActiveVideoChannelList([FromHeader(Name = "AccessKey")] string AccessKey,ReqPaginationBase req)
        {
            ResponseStruct rs;
            var ret = MediaServerService.GetWaitForActiveVideoChannelList(req,out rs);
            if (rs.Code != ErrorNumber.None)
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }

        
        
        /// <summary>
        /// 获取流媒体服务器列表
        /// </summary>
        /// <param name="AccessKey"></param>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [AuthVerify]
        [Route("GetMediaServerList")]
        [HttpGet]
        public List<ServerInstance> GetMediaServerList( [FromHeader(Name = "AccessKey")] string AccessKey)
        {
            ResponseStruct rs;
         
            var ret = MediaServerService.GetMediaServerList( out rs);
            if ( !rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }
        
        /// <summary>
        /// 通过MediaserverId获取流媒体服务器实例
        /// </summary>
        /// <param name="AccessKey"></param>
        /// <param name="mediaServerId"></param>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [AuthVerify]
        [Route("GetMediaServerByMediaServerId")]
        [HttpGet]
        public ServerInstance GetMediaServerByMediaServerId( [FromHeader(Name = "AccessKey")] string AccessKey,string mediaServerId )
        {
            ResponseStruct rs;
            var ret = MediaServerService.GetMediaServerByMediaServerId(mediaServerId, out rs);
            if ( !rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }
        
        /// <summary>
        /// 流媒体心跳
        /// </summary>
        /// <returns></returns>
        [Route("WebHook/MediaServerKeepAlive")]
        [HttpPost]
        public ResMediaServerKeepAlive MediaServerKeepAlive(ReqMediaServerKeepAlive req)
        {
            ResponseStruct rs;
            var ret = MediaServerService.MediaServerKeepAlive(req, out rs);
            if (ret == null || !rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }
    }
}