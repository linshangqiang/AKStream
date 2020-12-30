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
        Sys_WebApi_Except = -1008, //WebApi异常
        Sys_ConfigNotReady = -1009, //配置文件没有就绪
        Sys_DataBaseNotReady = -1010, //数据库没有就绪
        Sys_NetworkPortExcept = -1011, //端口不可用
        Sys_DiskInfoExcept = -1012, //磁盘不可用
        Sys_UrlExcept = -1013, //参数中URL异常
        Sys_ReadIniFileExcept = -1014, //读取ini文件异常
        Sys_WriteIniFileExcept = -1015, //写入ini文件异常
        Sys_SocketPortForRtpExcept = -1016, //查找可用rtp端口时异常，可能已无可用端口
        Sys_SpecifiedFileNotExists = -1017, //指定文件不存在
        Sys_InvalidAccessKey=-1018,//访问密钥失效
        Sys_AKStreamKeeperNotRunning=-1019,//AKStreamKeeper流媒体服务器治理程序没有运行
        Sys_DataBaseLimited=-1020,//数据库操作受限，请检查相关参数，如分页查询时每页不能超过10000行
        Sys_DB_VideoChannelNotExists=-1021,//数据库中不存在指定音视频通道
        Sys_DataBaseExcept=-1022,//数据库执行异常
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
        Sip_AlredayPushStream = -2010, //sip通道已经在推流状态
        Sip_NotOnPushStream = -2011, //Sip通道没有在推流状态
        Sip_Channel_StatusExcept = -2012, //Sip通道设备状态异常
        MediaServer_WebApiExcept = -3000, //访问流媒体服务器WebApi时异常
        MediaServer_WebApiDataExcept = -3001, //访问流媒体服务器WebApi接口返回数据异常
        MediaServer_TimeExcept = -3002, //服务器时间异常，建议同步
        MediaServer_BinNotFound = -3003, //流媒体服务器可执行文件不存在
        MediaServer_ConfigNotFound = -3004, //流媒体服务器配置文件不存在，建议手工运行一次流媒体服务器使其自动生成配置文件模板
        MediaServer_InstanceIsNull = -3005, //流媒体服务实例为空，请先创建流媒体服务实例
        MediaServer_StartUpExcept = -3006, //启动流媒体服务器失败
        MediaServer_ShutdownExcept = -3007, //停止流媒体服务器失败
        MediaServer_RestartExcept = -3008, //重启流媒体服务器失败
        MediaServer_ReloadExcept = -3009, //流媒体服务器配置热加载失败
        MediaServer_NotRunning = -3010, //流媒体服务器没有运行

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
            ErrorDic[ErrorNumber.Sys_ConfigNotReady] = "配置文件没有就绪,请检查配置文件是否正确无误";
            ErrorDic[ErrorNumber.Sys_DataBaseNotReady] = "数据库没有就绪，请检查数据库是否可以正常连接";
            ErrorDic[ErrorNumber.Sys_NetworkPortExcept] = "端口不可用，请检查配置文件";
            ErrorDic[ErrorNumber.Sys_WebApi_Except] = "WebApi异常";
            ErrorDic[ErrorNumber.Sys_DiskInfoExcept] = "磁盘不可用，请检查配置文件";
            ErrorDic[ErrorNumber.Sys_UrlExcept] = "参数中URL异常";
            ErrorDic[ErrorNumber.Sys_ReadIniFileExcept] = "读取ini文件异常";
            ErrorDic[ErrorNumber.Sys_WriteIniFileExcept] = "写入ini文件异常";
            ErrorDic[ErrorNumber.Sys_SocketPortForRtpExcept] = "查找可用rtp端口时异常，可能已无可用端口";
            ErrorDic[ErrorNumber.Sys_SpecifiedFileNotExists] = "指定文件不存在";
            ErrorDic[ErrorNumber.Sys_InvalidAccessKey] = "访问密钥失效";
            ErrorDic[ErrorNumber.Sys_AKStreamKeeperNotRunning] = "AKStreamKeeper流媒体服务器治理程序没有运行";
            ErrorDic[ErrorNumber.Sys_DataBaseLimited] = "数据库操作受限，请检查相关参数，如分页查询时每页不能超过10000行,第一页从1开始而不是从0开始";
            ErrorDic[ErrorNumber.Sys_DB_VideoChannelNotExists] = "数据库中不存在指定音视频通道";
            ErrorDic[ErrorNumber.Sys_DataBaseExcept] = "数据库执行异常";
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
            ErrorDic[ErrorNumber.MediaServer_BinNotFound] = "流媒体服务器可执行文件不存在";
            ErrorDic[ErrorNumber.MediaServer_ConfigNotFound] = "流媒体服务器配置文件不存在，建议手工运行一次流媒体服务器使其自动生成配置文件模板";
            ErrorDic[ErrorNumber.MediaServer_InstanceIsNull] = "流媒体服务实例为空，请先创建流媒体服务实例";
            ErrorDic[ErrorNumber.MediaServer_StartUpExcept] = "启动流媒体服务器失败";
            ErrorDic[ErrorNumber.MediaServer_ShutdownExcept] = "停止流媒体服务器失败";
            ErrorDic[ErrorNumber.MediaServer_RestartExcept] = "重启流媒体服务器失败";
            ErrorDic[ErrorNumber.MediaServer_ReloadExcept] = "流媒体服务器配置热加载失败";
            ErrorDic[ErrorNumber.MediaServer_NotRunning] = "流媒体服务器没有运行";

            ErrorDic[ErrorNumber.Other] = "未知错误";
        }
    }
}