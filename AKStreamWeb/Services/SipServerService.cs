using System.Collections.Generic;
using AKStreamWeb.WebRequest;
using LibCommon;
using LibCommon.Structs.GB28181;

namespace AKStreamWeb.Services
{
    public static class SipServerService
    {

        /// <summary>
        /// 通过DeviceId获取Device设备实例
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static SipDevice GetSipDeviceListByDeviceId(string deviceId, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (string.IsNullOrEmpty(deviceId))
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ParamsIsNotRight,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
                };
                return null;
            }
            var ret = LibGB28181SipServer.Common.SipDevices.FindLast(x => x.DeviceId.Equals(deviceId));
            return ret;
        }
        /// <summary>
        /// 获取所有Sip设备列表
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static List<SipDevice> GetSipDeviceList(out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            return LibGB28181SipServer.Common.SipDevices;
        }
    }
}