using System;
using LibCommon.Structs.GB28181;
using LibCommon.Structs.GB28181.XML;

namespace AKStreamWeb.Misc
{
    /// <summary>
    /// sip设备回调类
    /// </summary>
    public static class SipServerCallBack
    {
        public static void OnRegister(string sipDeviceJson)
        {
            //设备注册时
        }

        public static void OnUnRegister(string sipDeviceJson)
        {
            //设备注销时
        }

        public static void OnKeepalive(string deviceId, DateTime keepAliveTime, int lostTimes)
        {
            //设备有心跳时
        }

        public static void OnDeviceStatusReceived(SipDevice sipDevice, DeviceStatus deviceStatus)
        {
            //获取到设备状态时
        }

        public static void OnInviteHistoryVideoFinished(RecordInfo.Item record)
        {
            //收到设备的录像文件列表时
        }

        public static void OnDeviceReadyReceived(SipDevice sipDevice)
        {
            //设备就绪时
        }
    }
}