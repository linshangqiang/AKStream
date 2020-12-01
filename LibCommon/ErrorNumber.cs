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
        Sip_StartExcept = -1003, //启动Sip服务异常
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
            ErrorDic[ErrorNumber.Other] = "未知错误";
        }
    }
}