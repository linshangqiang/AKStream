using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using LibCommon;
using LibSystemInfo;
using SIPSorcery.SIP;

namespace LibGB28181SipServer
{
    public static class Common
    {
        public delegate void DoKickSipDevice(string guid);

        public delegate void RegisterDelegate(string sipDeviceJson);

        public delegate void UnRegisterDelegate(string sipDeviceJson);

        public delegate void DeviceAlarmSubscribeDelegate(SIPTransaction sipTransaction);


        public const int SIP_REGISTER_MIN_INTERVAL_SEC = 30; //最小Sip设备注册间隔
        private static string _loggerHead = "SipServer";
        private static SipServerConfig _sipServerConfig = null;
        private static string _sipServerConfigPath = GCommon.ConfigPath + "SipServerConfig.json";

        private static List<SipDevice> _sipDevices = new List<SipDevice>();


        /// <summary>
        /// 用于操作_sipDevices时的锁
        /// </summary>
        public static object SipDevicesLock = new object();

        /// <summary>
        /// 对sip通道操作时的锁
        /// </summary>
        public static object SipChannelOptLock = new object();

        /// <summary>
        /// sip设备列表
        /// </summary>
        public static List<SipDevice> SipDevices
        {
            get => _sipDevices;
            set => _sipDevices = value;
        }


        public static SipServer SipServer = null;

        /// <summary>
        /// Sip网关配置实例
        /// </summary>
        /// 
        public static SipServerConfig SipServerConfig
        {
            get => _sipServerConfig;
            set => _sipServerConfig = value;
        }

        /// <summary>
        /// Sip网关配置文件路径
        /// </summary>
        public static string SipServerConfigPath
        {
            get => _sipServerConfigPath;
            set => _sipServerConfigPath = value;
        }

        public static string LoggerHead
        {
            get => _loggerHead;
            set => _loggerHead = value;
        }

        private static ConcurrentDictionary<string, SIPRequest> _needResponseRequests =
            new ConcurrentDictionary<string, SIPRequest>();

        /// <summary>
        /// 需要信息回复的消息列表
        /// </summary>
        public static ConcurrentDictionary<string, SIPRequest> NeedResponseRequests
        {
            get => _needResponseRequests;
            set => _needResponseRequests = value;
        }

        /// <summary>
        /// Sip通道类型
        /// </summary>
        public enum SipChannelType
        {
            /// <summary>
            /// 音视频流通道
            /// </summary>
            VIDEOCHANNEL,

            /// <summary>
            /// 报警通道
            /// </summary>
            ALARMCHANNEL,

            /// <summary>
            /// 音频流通道
            /// </summary>
            AUDIOCHANNEL,

            /// <summary>
            /// 其他通道
            /// </summary>
            OTHERCHANNEL,
            /// <summary>
            /// id位数不等于20,设置为未知设备
            /// </summary>
            UNKNOW,
        }

        /// <summary>
        /// 初始化一SipServerConfig
        /// </summary>
        /// <returns></returns>
        private static SipServerConfig initSipServerConfig(out ResponseStruct rs)
        {
            try
            {
                SipServerConfig sipServerConfig = null;
                SystemInfo systemInfo = new SystemInfo();
                string macAddr = "";
                string ipAddr = "";
                var sys = systemInfo.GetSystemInfoObject();
                int i = 0;
                while (sys == null || sys.NetWorkStat == null || i < 50)
                {
                    i++;
                    Thread.Sleep(20);
                }

                if (sys != null && sys.NetWorkStat != null)
                {
                    macAddr = sys.NetWorkStat.Mac;
                    systemInfo.Dispose();
                }

                if (string.IsNullOrEmpty(macAddr))
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.Sys_GetMacAddressExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_GetMacAddressExcept],
                    };
                    return null; //mac 地址没找到了，报错出去
                }

                IPInfo ipInfo = UtilsHelper.GetIpAddressByMacAddress(macAddr, true);
                if (!string.IsNullOrEmpty(ipInfo.IpV4))
                {
                    sipServerConfig = new SipServerConfig();
                    sipServerConfig.Authentication = false;
                    sipServerConfig.SipUsername = "admin";
                    sipServerConfig.SipPassword = "123456";
                    sipServerConfig.MsgEncode = "GB2312";
                    sipServerConfig.GbVersion = "GB-2016";
                    sipServerConfig.MsgProtocol = "UDP";
                    sipServerConfig.SipPort = 5060;
                    sipServerConfig.IpV6Enable = !string.IsNullOrEmpty(ipInfo.IpV6);
                    if (sipServerConfig.IpV6Enable)
                    {
                        sipServerConfig.SipIpV6Address = ipInfo.IpV6;
                    }

                    sipServerConfig.SipIpAddress = ipInfo.IpV4;
                    sipServerConfig.KeepAliveInterval = 5;
                    sipServerConfig.KeepAliveLostNumber = 3;
                    /*SipDeviceID 20位编码规则
                    *1-2省级 33 浙江省
                    *3-4市级 02 宁波市
                    *5-6区级 00 宁波市区
                    *7-8村级 00 宁波市区
                    *9-10行业 02 社会治安内部接入
                    *11-13设备类型 118 NVR
                    *14 网络类型 0 监控专用网
                    *15-20 设备序号 000001 1号设备 
                    */
                    sipServerConfig.ServerSipDeviceId = "33020000021180000001";
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.None,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.None],
                    };
                    return sipServerConfig;
                }
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_GetMacAddressExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_GetMacAddressExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
                return null;
            }

            rs = new ResponseStruct()
            {
                Code = ErrorNumber.Other,
                Message = ErrorMessage.ErrorDic![ErrorNumber.Other],
            };
            return null;
        }

        /// <summary>
        /// 通过通道id获取通道类型
        /// </summary>
        /// <param name="sipChannelId"></param>
        /// <returns></returns>
        public static SipChannelType GetSipChannelType(string sipChannelId)
        {
            if (sipChannelId.Trim().Length != 20)
            {
                return SipChannelType.UNKNOW;
            }
            int extId = int.Parse(sipChannelId.Substring(10, 3));
            if (extId == 131 || extId == 132 || extId == 137 || extId==138||extId==139)
            {
                return SipChannelType.VIDEOCHANNEL;
            }
            if (extId == 135 || extId==205)
            {
                return SipChannelType.ALARMCHANNEL;
            }
            if (extId == 137)
            {
                return SipChannelType.AUDIOCHANNEL;
            }

            if (extId >= 140 && extId <= 199)
            {
                return SipChannelType.VIDEOCHANNEL;
            }

            return SipChannelType.OTHERCHANNEL;

        }
        /// <summary>
        /// 返加0说明文件存在并正确加载
        /// 返回1说明文件不存在已新建并加载
        /// 返回-1说明文件创建或读取异常失败
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static int ReadSipServerConfig(out ResponseStruct rs)
        {
            if (!File.Exists(_sipServerConfigPath))
            {
                var config = initSipServerConfig(out rs);
                if (config != null && rs.Code.Equals(ErrorNumber.None))
                {
                    _sipServerConfig = config;
                    if (UtilsHelper.WriteJsonConfig(_sipServerConfigPath, _sipServerConfig))
                    {
                        rs = new ResponseStruct()
                        {
                            Code = ErrorNumber.None,
                            Message = ErrorMessage.ErrorDic![ErrorNumber.None],
                        };
                        return 1;
                    }

                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.Sys_JsonWriteExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_JsonWriteExcept],
                    };
                    return -1;
                }
            }
            else
            {
                try
                {
                    _sipServerConfig =
                        UtilsHelper.ReadJsonConfig<SipServerConfig>(_sipServerConfigPath) as SipServerConfig;
                    if (_sipServerConfig != null)
                    {
                        rs = new ResponseStruct()
                        {
                            Code = ErrorNumber.None,
                            Message = ErrorMessage.ErrorDic![ErrorNumber.None],
                        };
                        return 0;
                    }

                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.Sys_JsonReadExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_JsonReadExcept],
                    };
                    return -1;
                }
                catch (Exception ex)
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.Other,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.Other],
                        ExceptMessage = ex.Message,
                        ExceptStackTrace = ex.StackTrace,
                    };
                    return -1;
                }
            }

            rs = new ResponseStruct()
            {
                Code = ErrorNumber.Other,
                Message = ErrorMessage.ErrorDic![ErrorNumber.Other],
            };
            return 1;
        }

        static Common()
        {
        }
    }
}