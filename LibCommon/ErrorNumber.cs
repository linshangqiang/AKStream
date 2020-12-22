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
        Sys_ConfigDirNotExists = -1004, //配置文件目录不存在
        Sys_ConfigFileNotExists = -1005, //配置文件不存在
        Sys_ParamsNotEnough = -1006, //参数不足
        Sys_ParamsIsNotRight = -1007, //参数不正确
        Sys_WebApi_Except=-1008,//WebApi异常
        Sys_ConfigNotReady=-1009, //配置文件没有就绪
        Sys_DataBaseNotReady=-1010,//数据库没有就绪
        Sip_StartExcept = -2000, //启动Sip服务异常
        Sip_StopExcept = -2001, //停止Sip服务异常
        Sip_Except_DisposeSipDevice = -2002, //Sip网关内部异常(销毁Sip设备时)
        Sip_Except_RegisterSipDevice = -2003, //Sip网关内部异常(注册Sip设备时)
        Sip_ChannelNotExists = -2004, //Sip音视频通道不存在
        Sip_DeviceNotExists = -2005, //Sip设备不存在
        Sip_OperationNotAllowed = -2006, //该设备类型下不允许这个操作
        Sip_DeInviteExcept = -2007, //结束推流时异常
        Sip_InviteExcept = -2008, //推流时异常
        Sip_SendMessageExcept = -2009, //发送sip消息时异常
        Sip_AlredayPushStream=-2010,//sip通道已经在推流状态
        Sip_NotOnPushStream=-2011,//Sip通道没有在推流状态
        Sip_Channel_StatusExcept=-2012,//Sip通道设备状态异常
        MediaServer_WebApiExcept=-3000,//访问流媒体服务器WebApi时异常
        MediaServer_WebApiDataExcept=-3001,//访问流媒体服务器WebApi接口返回数据异常
        MediaServer_TimeExcept=-3002,//服务器时间异常，建议同步
       


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
            ErrorDic[ErrorNumber.Sys_ConfigDirNotExists] = "配置文件目录不存在";
            ErrorDic[ErrorNumber.Sys_ConfigFileNotExists] = "配置文件不存在";
            ErrorDic[ErrorNumber.Sys_ParamsNotEnough] = "传入参数不足";
            ErrorDic[ErrorNumber.Sys_ParamsIsNotRight] = "传入参数不正确";
            ErrorDic[ErrorNumber.Sys_ConfigNotReady] = "配置文件没有就绪";
            ErrorDic[ErrorNumber.Sys_DataBaseNotReady] = "数据库没有就绪";
            ErrorDic[ErrorNumber.Sip_StartExcept] = "启动Sip服务异常";
            ErrorDic[ErrorNumber.Sip_StopExcept] = "停止Sip服务异常";
            ErrorDic[ErrorNumber.Sip_Except_DisposeSipDevice] = "Sip网关内部异常(销毁Sip设备时)";
            ErrorDic[ErrorNumber.Sip_Except_RegisterSipDevice] = "Sip网关内部异常(注册Sip设备时)";
            ErrorDic[ErrorNumber.Sip_ChannelNotExists] = "Sip音视频通道不存在";
            ErrorDic[ErrorNumber.Sip_DeviceNotExists] = "Sip设备不存在";
            ErrorDic[ErrorNumber.Sip_OperationNotAllowed] = "该类型的设备不允许做这个操作";
            ErrorDic[ErrorNumber.Sip_DeInviteExcept] = "结束推流时发生异常";
            ErrorDic[ErrorNumber.Sip_InviteExcept] = "推流时发生异常";
            ErrorDic[ErrorNumber.Sip_SendMessageExcept] = "发送Sip消息时异常";
            ErrorDic[ErrorNumber.Sip_AlredayPushStream] = "Sip通道(回放录像)已经在推流状态";
            ErrorDic[ErrorNumber.Sip_NotOnPushStream] = "Sip通道(回放录像)没有在推流状态";
            ErrorDic[ErrorNumber.Sip_Channel_StatusExcept] = "Sip通道状态异常";
            ErrorDic[ErrorNumber.MediaServer_WebApiExcept] = "访问流媒体服务器WebApi接口时异常";
            ErrorDic[ErrorNumber.MediaServer_WebApiDataExcept] = "访问流媒体服务器WebApi接口返回数据异常";
            ErrorDic[ErrorNumber.MediaServer_TimeExcept] = "流媒体服务器时间异常，建议同步";
            ErrorDic[ErrorNumber.Sys_WebApi_Except] = "WebApi异常";
            
            


            ErrorDic[ErrorNumber.Other] = "未知错误";
        }
    }
}