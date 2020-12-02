using System;
using System.Collections.Generic;

namespace LibCommon
{
    /// <summary>
    /// 错误代码
    /// </summary>
    [Serializable]
    public enum ErrorNumber : int
    {
        None = 0, //成功
        Sys_GetMacAddressExcept = -1000, //获取Mac地址异常
        Sys_GetIpAddressExcept = -1001, //获取IP地址异常
        Sys_JsonWriteExcept = -1002, //Json写入异常
        Sys_JsonReadExcept = -1003, //Json读取异常
        Sip_StartExcept = -2000, //启动Sip服务异常
        Sip_StopExcept = -2001, //停止Sip服务异常
        Sip_Except_DisposeSipDevice = -2002, //Sip网关内部异常(销毁Sip设备时)
        Sip_Except_RegisterSipDevice = -2003, //Sip网关内部异常(注册Sip设备时)
        Other = -6000 //其他异常
    }

    /// <summary>
    /// 错误代码描述
    /// </summary>
    [Serializable]
    public static class ErrorMessage
    {
        public static Dictionary<ErrorNumber, string>? ErrorDic;

        public static void Init()
        {
            ErrorDic = new Dictionary<ErrorNumber, string>();
            ErrorDic[ErrorNumber.None] = "无错误";
            ErrorDic[ErrorNumber.Sys_GetMacAddressExcept] = "获取Mac地址异常";
            ErrorDic[ErrorNumber.Sys_GetIpAddressExcept] = "获取IP地址异常";
            ErrorDic[ErrorNumber.Sys_JsonWriteExcept] = "Json写入异常";
            ErrorDic[ErrorNumber.Sys_JsonReadExcept] = "Json读取异常";
            ErrorDic[ErrorNumber.Sip_StartExcept] = "启动Sip服务异常";
            ErrorDic[ErrorNumber.Sip_StopExcept] = "停止Sip服务异常";
            ErrorDic[ErrorNumber.Sip_Except_DisposeSipDevice] = "Sip网关内部异常(销毁Sip设备时)";
            ErrorDic[ErrorNumber.Sip_Except_RegisterSipDevice] = "Sip网关内部异常(注册Sip设备时)";

            ErrorDic[ErrorNumber.Other] = "未知错误";
        }
    }
}