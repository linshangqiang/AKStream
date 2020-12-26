using System;
using AKStreamWeb.Attributes;
using AKStreamWeb.Services;
using LibCommon;
using LibCommon.Structs;
using LibCommon.Structs.WebResponse;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AKStreamWeb.Controllers.WebHook
{
    [ApiController]
    [Route("/MediaServer")]
    [SwaggerTag("流媒体相关接口")]
    public class MediaServerController : ControllerBase
    {
        /// <summary>
        /// 流媒体心跳
        /// </summary>
        /// <returns></returns>
        [Route("WebHook/MediaServerKeepAlive")]
        [HttpPost]
        [Log]
        public ResMediaServerKeepAlive MediaServerKeepAlive(MediaServerKeepAlive req)
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