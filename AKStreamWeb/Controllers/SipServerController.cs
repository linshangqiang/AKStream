using System.Collections.Generic;
using AKStreamWeb.Attributes;
using AKStreamWeb.Services;
using LibCommon;
using LibCommon.Structs.GB28181;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;


namespace AKStreamWeb.Controllers
{

    [Log]
    [AuthVerify]
    [ApiController]
    [Route("/SipGate")]
    [SwaggerTag("Sip网关相关接口")]
    public class SipServerController: ControllerBase
    {
        /// <summary>
        /// 通过deviceId获取sip设备实例
        /// </summary>
        /// <param name="AccessKey"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [Route("GetSipDeviceListByDeviceId")]
        [HttpGet]
        public SipDevice GetSipDeviceListByDeviceId(
            [FromHeader(Name = "AccessKey")] string AccessKey,string deviceId)
        {
            ResponseStruct rs;
            var ret = SipServerService.GetSipDeviceListByDeviceId(deviceId,out rs);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }
        
        /// <summary>
        /// 获取Sip设备列表
        /// </summary>
        /// <param name="AccessKey"></param>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
       
        [Route("GetSipDeviceList")]
        [HttpGet]
        public List<SipDevice> GetSipDeviceList(
            [FromHeader(Name = "AccessKey")] string AccessKey)
        {
            ResponseStruct rs;
            var ret = SipServerService.GetSipDeviceList(out rs);
            if (!rs.Code.Equals(ErrorNumber.None))
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }

    }
}