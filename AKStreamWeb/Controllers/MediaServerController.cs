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
    public class MediaServerController
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
            try
            {
                return MediaServerService.MediaServerKeepAlive(req, out rs);
            }
            catch (Exception ex)
            {
                ResponseStruct exRs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_WebApi_Except,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_WebApi_Except],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
                throw  new AkStreamException(exRs );
            }
        }
    }
}