using AKStreamWeb.Attributes;
using AKStreamWeb.Services;
using LibCommon;
using LibCommon.Structs.WebRequest;
using LibCommon.Structs.WebResponse;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AKStreamWeb.Controllers
{
    [Log]
    [AuthVerify]
    [ApiController]
    [Route("/SystemApi")]
    [SwaggerTag("系统相关API")]
    public class SystemServiceController: ControllerBase
    {
        /// <summary>
        /// 获取部门信息
        /// </summary> 
        /// <param name="AccessKey"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        [Route("GetDeptartmentInfoList")]
        [HttpPost]
        public ResGetDepartmentInfo GetDeptartmentInfoList([FromHeader(Name = "AccessKey")] string AccessKey,ReqGetDepartmentInfo  req)
        {
            ResponseStruct rs;
            var ret = SystemService.GetDeptartmentInfoList(req,out rs);
            if (rs.Code != ErrorNumber.None)
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }
    }
}