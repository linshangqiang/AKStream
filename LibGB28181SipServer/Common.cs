using System;
using System.IO;
using System.Threading;
using LibCommon;
using LibSystemInfo;

namespace LibGB28181SipServer
{
    public static class Common
    {
        private static string _loggerHead = "SipServer";
        private static SipServerConfig _sipServerConfig = null;
        private static string _sipServerConfigPath = LibCommon.LibCommon.ConfigPath + "SipServerConfig.json";

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
        /// 返加0说明文件存在并正确加载
        /// 返回1说明文件不存在已新建并加载
        /// 返回-1说明文件创建或读取异常失败
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static int ReadSipServerConfig(out ResponseStruct rs)
        {
            long a = DateTime.Now.Ticks;
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
                finally
                {
                    Console.WriteLine((DateTime.Now.Ticks - a) / 10000);
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