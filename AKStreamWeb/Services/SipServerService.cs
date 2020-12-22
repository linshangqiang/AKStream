using System.Collections.Generic;
using AKStreamWeb.WebRequest;
using LibCommon;
using LibCommon.Structs.GB28181;

namespace AKStreamWeb.Services
{
    public static class SipServerService
    {

        /// <summary>
        /// 获取所有Sip设备列表
        /// </summary>
        /// <returns></returns>
        public static List<SipDevice> GetSipDevice(ReqGetSipDeviceList req,out ResponseStruct rs)
        {
            if (req.Port != null && req.IsReday != null && !string.IsNullOrEmpty(req.IpV4Address) &&
                !string.IsNullOrEmpty(req.DeviceId))
            {
               // var result=
            }

            rs = null;
            return null;
        }
            
    }
}