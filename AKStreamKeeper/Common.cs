using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using LibCommon;
using LibCommon.Structs;
using LibCommon.Structs.WebResponse;
using LibLogger;
using LibSystemInfo;
using Newtonsoft.Json;
using JsonHelper = LibCommon.JsonHelper;
using Timer = System.Timers.Timer;

namespace AKStreamKeeper
{
    public static class Common
    {
        private static string _configPath = GCommon.ConfigPath + "AKStreamKeeper.json";
        private static AKStreamKeeperConfig _akStreamKeeperConfig;
        private static SystemInfo _keeperSystemInfo = new SystemInfo();
        public static PerformanceInfo KeeperPerformanceInfo = new PerformanceInfo();
        private static object _performanceInfoLock = new object();
        private static Timer _perFormanceInfoTimer;
        private static string _loggerHead = "AKStreamKeeper";

        private static List<KeyValuePair<double, string>> _akStreamDiskInfoOfRecordMap =
            new List<KeyValuePair<double, string>>();

        private static object _akStreamDiskInfoOfRecordMapLock = new object();
        private static long _counter1 = 0;
        private static bool _firstPost = true;

        public delegate void MediaServerKilled(bool self = false);


        /// <summary>
        /// 配置文件路径
        /// </summary>
        public static string ConfigPath
        {
            get => _configPath;
            set => _configPath = value;
        }

        /// <summary>
        /// 配置实例
        /// </summary>
        public static AKStreamKeeperConfig AkStreamKeeperConfig
        {
            get => _akStreamKeeperConfig;
            set => _akStreamKeeperConfig = value;
        }

        /// <summary>
        /// 日志头
        /// </summary>
        public static string LoggerHead
        {
            get => _loggerHead;
            set => _loggerHead = value;
        }

        public static ZLMediaKitServerInstance MediaKitServerInstance;

        /// <summary>
        /// 启动流媒体服务器
        /// </summary>
        /// <returns></returns>
        private static int StartupMediaServer()
        {
            ProcessHelper.KillProcess(_akStreamKeeperConfig.MediaServerPath);
            if (MediaKitServerInstance == null)
            {
                MediaKitServerInstance = new ZLMediaKitServerInstance(_akStreamKeeperConfig.MediaServerPath);
            }

            return MediaKitServerInstance.Startup();
        }

        /// <summary>
        /// 流媒体服务器进程被结束时触发
        /// </summary>
        /// <param name="self"></param>
        private static void OnMediaServerKilled(bool self = false)
        {
            if (!self)
            {
                Logger.Error(
                    $"[{LoggerHead}]->流媒体服务器进程被意外关闭->1秒后重新尝试启动流媒体服务器");
                Thread.Sleep(1000);
                while (StartupMediaServer() <= 0)
                {
                    Logger.Error(
                        $"[{LoggerHead}]->尝试重新启动流媒体服务器启动失败，开始循环尝试，直至启动成功");
                    Thread.Sleep(1000);
                }

                Logger.Info(
                    $"[{LoggerHead}]->尝试重新启动流媒体服务器启动成功->进程ID:{MediaKitServerInstance.GetPid()}");
            }
        }

        private static void GetDiskSpaceToRecordMap()
        {
            var tmpList = new List<KeyValuePair<double, string>>();
            foreach (var path in _akStreamKeeperConfig.CustomRecordPathList)
            {
                var obj = KeeperPerformanceInfo.DriveInfo.FindLast(x => x.Name.Trim().Equals(path));
                if (obj != null)
                {
                    tmpList.Add(new KeyValuePair<double, string>((double) obj.Free, path));
                }
                else
                {
                    if (KeeperPerformanceInfo.SystemType.Trim().ToUpper().Equals("WINDOWS"))
                    {
                        var rootPath = Path.GetPathRoot(path).TrimEnd(':').ToUpper();
                        var winobj = KeeperPerformanceInfo.DriveInfo.FindLast(x =>
                            x.Name.Trim().TrimEnd(':').ToUpper().Equals(rootPath));
                        if (winobj != null)
                        {
                            tmpList.Add(new KeyValuePair<double, string>((double) obj.Free, path));
                        }
                    }
                    else
                    {
                        DriveInfoDiy objUnix = null;
                        if (path.Trim().Equals("/"))
                        {
                            objUnix = KeeperPerformanceInfo.DriveInfo.FindLast(x =>
                                x.Name.Trim().Equals("/"));
                            if (objUnix != null)
                            {
                                tmpList.Add(new KeyValuePair<double, string>((double) objUnix.Free, path));
                            }
                        }
                        else
                        {
                            char[] tmpChars = path.ToCharArray(0, path.Length);
                            List<KeyValuePair<double, string>> subTmpList = new List<KeyValuePair<double, string>>();
                            for (int i = 0; i <= tmpChars.Length - 1; i++)
                            {
                                string tmpStr = "";
                                if (tmpChars[i] == '/')
                                {
                                    if (i == 0)
                                    {
                                        continue;
                                    }

                                    tmpStr = path.Substring(0, i);
                                    objUnix = KeeperPerformanceInfo.DriveInfo.FindLast(x =>
                                        x.Name.Trim().Equals(tmpStr.Trim()));
                                    if (objUnix != null)
                                    {
                                        subTmpList.Add(new KeyValuePair<double, string>((double) objUnix.Free, path));
                                        break;
                                    }
                                }
                            }

                            bool foundDir = false;
                            for (int j = 0; j <= path.Length - 1; j++) //检查多级目录中至少有一层目录是存在的，除根目录以外
                            {
                                if (path[j] == '/')
                                {
                                    string tmpStrPath = path.Substring(0, j);
                                    if (Directory.Exists(tmpStrPath))
                                    {
                                        foundDir = true;
                                        break;
                                    }
                                }
                            }

                            if (subTmpList.Count == 0 && tmpChars[0] == '/' && foundDir)
                            {
                                //如果subTmpList是空的，并且首字符是根目录，同时还满足多级目录中至少有一级目录是存在的情况下，将根目录作为其所在目录获取剩余容量
                                objUnix = KeeperPerformanceInfo.DriveInfo.FindLast(x =>
                                    x.Name.Trim().Equals("/"));
                                if (objUnix != null)
                                {
                                    subTmpList.Add(new KeyValuePair<double, string>((double) objUnix.Free, path));
                                }
                            }

                            tmpList.AddRange(subTmpList);
                        }
                    }
                }
            }

            _akStreamDiskInfoOfRecordMap = tmpList;
            UtilsHelper.RemoveNull(_akStreamDiskInfoOfRecordMap); //去null
            _akStreamDiskInfoOfRecordMap = _akStreamDiskInfoOfRecordMap.Distinct().ToList(); //去重
            for (int i = _akStreamKeeperConfig.CustomRecordPathList.Count - 1;
                i >= 0;
                i--)
            {
                //保证去除完全不存在的目录
                string dir = _akStreamKeeperConfig.CustomRecordPathList[i];
                var objExt = _akStreamDiskInfoOfRecordMap.FindLast(x => x.Value.Trim().Equals(dir.Trim()));
                if (objExt.Value == null)
                {
                    _akStreamKeeperConfig.CustomRecordPathList[i] = null;
                }
            }

            UtilsHelper.RemoveNull(_akStreamKeeperConfig.CustomRecordPathList); //去null
            _akStreamKeeperConfig.CustomRecordPathList =
                _akStreamKeeperConfig.CustomRecordPathList.Distinct().ToList(); //去重
        }

        /// <summary>
        /// 初始化配置文件
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        private static bool InitConfig(out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            _akStreamKeeperConfig = new AKStreamKeeperConfig();


            int i = 0;
            while (KeeperPerformanceInfo == null && KeeperPerformanceInfo.NetWorkStat == null && i < 50)
            {
                i++;
                Thread.Sleep(20);
            }

            string macAddr = "";
            if (KeeperPerformanceInfo != null && KeeperPerformanceInfo.NetWorkStat != null)
            {
                macAddr = KeeperPerformanceInfo.NetWorkStat.Mac;
            }

            if (string.IsNullOrEmpty(macAddr))
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_GetMacAddressExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_GetMacAddressExcept],
                };
                return false; //mac 地址没找到了，报错出去
            }

            IPInfo ipInfo = UtilsHelper.GetIpAddressByMacAddress(macAddr, true);
            if (ipInfo != null)
            {
                _akStreamKeeperConfig.IpV4Address = ipInfo.IpV4;
                _akStreamKeeperConfig.IpV6Address = ipInfo.IpV6;
                _akStreamKeeperConfig.WebApiPort = 6880;
                _akStreamKeeperConfig.UseSsl = false;
                _akStreamKeeperConfig.MaxRtpPort = 20000;
                _akStreamKeeperConfig.MinRtpPort = 10001;
                _akStreamKeeperConfig.AkStreamWebRegisterUrl =
                    $"http://127.0.0.1:5800/MediaServer/WebHook/MediaServerKeepAlive";
                _akStreamKeeperConfig.CustomRecordPathList = new List<string>();
                _akStreamKeeperConfig.CustomRecordPathList.Add("请正确配置用于存储录制文件的目录或盘符");
                _akStreamKeeperConfig.CustomRecordPathList.Add("目前可用设备如下:");
                foreach (var disk in KeeperPerformanceInfo.DriveInfo)
                {
                    string tmp = "目标名称:" + disk.Name + "    空闲空间:" + disk.Free + "    总空间:" + disk.Total +
                                 "    可用空间率:" + disk.FreePercent + "%" + "    是否就绪:" +
                                 disk.IsReady;
                    _akStreamKeeperConfig.CustomRecordPathList.Add(tmp);
                }

                _akStreamKeeperConfig.CustomRecordPathList.Add("配置时仅输入目标名称，支持多个，按照Json格式一行一个");
                _akStreamKeeperConfig.CustomRecordPathList.Add("例:");
                _akStreamKeeperConfig.CustomRecordPathList.Add("/disk1/record");
                _akStreamKeeperConfig.CustomRecordPathList.Add("/disk2/record");
                _akStreamKeeperConfig.CustomRecordPathList.Add("/disk3/record");
                _akStreamKeeperConfig.MediaServerPath =
                    "请正确填写流媒体服务器(ZLMediaKit的MediaServer)的绝对路径，如/opt/MediaServer，并确保其已经执行过一次，在MediaServer的同级目录下已经生成了config.ini文件";
                try
                {
                    string configStr = JsonHelper.ToJson(_akStreamKeeperConfig, Formatting.Indented);
                    File.WriteAllText(_configPath, configStr);
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.Sys_ConfigNotReady,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ConfigNotReady],
                    };
                    return true;
                }
                catch (Exception ex)
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.Sys_JsonWriteExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_JsonWriteExcept],
                    };
                    return false;
                }
            }
            else
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_GetIpAddressExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_GetIpAddressExcept],
                };
                return false;
            }
        }


        /// <summary>
        /// 检测流媒体服务器的配置文件是否存在
        /// </summary>
        /// <param name="MediaServerBinPath"></param>
        /// <returns></returns>
        private static string CheckMediaServerConfig(string MediaServerBinPath)
        {
            string dir = Path.GetDirectoryName(MediaServerBinPath) + "/";
            if (Directory.Exists(dir))
            {
                DirectoryInfo di = new DirectoryInfo(dir);
                if (di != null)
                {
                    foreach (var file in di.GetFiles())
                    {
                        if (file != null && file.Extension.ToLower().Equals(".ini"))
                        {
                            return file.FullName;
                        }
                    }
                }
            }

            return null!;
        }


        /// <summary>
        /// 检测配置文件是否正常
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        private static bool CheckConfig(out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (_akStreamKeeperConfig == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ConfigNotReady,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ConfigNotReady],
                };
                return false;
            }

            if (!UtilsHelper.IsPortOK(_akStreamKeeperConfig.WebApiPort.ToString()))
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_NetworkPortExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_NetworkPortExcept],
                };
                return false;
            }


            if (_akStreamKeeperConfig.CustomRecordPathList != null &&
                _akStreamKeeperConfig.CustomRecordPathList.Count > 0)
            {
                foreach (var path in _akStreamKeeperConfig.CustomRecordPathList)
                {
                    if (!Directory.Exists(path))
                    {
                        try
                        {
                            var ret = Directory.CreateDirectory(path);
                            if (ret == null)
                            {
                                rs = new ResponseStruct()
                                {
                                    Code = ErrorNumber.Sys_DiskInfoExcept,
                                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_DiskInfoExcept],
                                };
                                return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            rs = new ResponseStruct()
                            {
                                Code = ErrorNumber.Sys_DiskInfoExcept,
                                Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_DiskInfoExcept],
                                ExceptMessage = ex.Message,
                                ExceptStackTrace = ex.StackTrace,
                            };
                            return false;
                        }
                    }
                }
            }

            if (!File.Exists(_akStreamKeeperConfig.MediaServerPath))
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_BinNotFound,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_BinNotFound],
                };
                return false;
            }

            string MediaServerConfig = CheckMediaServerConfig(_akStreamKeeperConfig.MediaServerPath);
            if (string.IsNullOrEmpty(MediaServerConfig))
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_ConfigNotFound,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_ConfigNotFound],
                };
                return false;
            }

            if (!UtilsHelper.IsUrl(_akStreamKeeperConfig.AkStreamWebRegisterUrl))
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_UrlExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_UrlExcept],
                };
                return false;
            }

            return true;
        }

        /// <summary>
        /// 读取配置文件
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        private static bool ReadConfig(out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            lock (_performanceInfoLock)
            {
                KeeperPerformanceInfo = _keeperSystemInfo.GetSystemInfoObject();
            }

            if (!File.Exists(_configPath))
            {
                //创建文件 
                var ret = InitConfig(out rs);
                if (ret == false || !rs.Code.Equals(ErrorNumber.None))
                {
                    return false;
                }

                return true;
            }
            else
            {
                //读取文件
                try
                {
                    _akStreamKeeperConfig = JsonHelper.FromJson<AKStreamKeeperConfig>(File.ReadAllText(_configPath));
                    _akStreamKeeperConfig.CustomRecordPathList =
                        _akStreamKeeperConfig.CustomRecordPathList.Distinct().ToList(); //去重

                    for (int i = 0; i <= _akStreamKeeperConfig.CustomRecordPathList.Count - 1; i++)
                    {
                        while (_akStreamKeeperConfig.CustomRecordPathList[i].Trim().EndsWith('/') &&
                               _akStreamKeeperConfig.CustomRecordPathList[i].Trim().Length > 1)
                        {
                            _akStreamKeeperConfig.CustomRecordPathList[i].TrimEnd('/');
                        }
                    }

                    GetDiskSpaceToRecordMap();
                    return CheckConfig(out rs);
                }
                catch (Exception ex)
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.Sys_JsonReadExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_JsonReadExcept],
                        ExceptMessage = ex.Message,
                        ExceptStackTrace = ex.StackTrace,
                    };
                }
            }

            return false;
        }

        private static void startTimer()
        {
            if (_perFormanceInfoTimer == null)
            {
                _perFormanceInfoTimer = new Timer(1000);
                _perFormanceInfoTimer.Enabled = true; //启动Elapsed事件触发
                _perFormanceInfoTimer.Elapsed += OnTimedEvent; //添加触发事件的函数
                _perFormanceInfoTimer.AutoReset = true; //需要自动reset
                _perFormanceInfoTimer.Start(); //启动计时器
            }
        }


        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            lock (_performanceInfoLock)
            {
                KeeperPerformanceInfo = _keeperSystemInfo.GetSystemInfoObject();
            }


            _counter1++;
            if (_counter1 % 60 == 0) //1分钟一次获取磁盘用量情况
            {
                lock (_akStreamDiskInfoOfRecordMapLock)
                {
                    GetDiskSpaceToRecordMap();
                }

                if (_counter1 > 10000000)
                {
                    _counter1 = 0;
                }
            }

            if (_counter1 % 5 == 0 && MediaKitServerInstance != null) //发心跳给服务器
            {
                
                MediaServerKeepAlive tmpKeepAlive = new MediaServerKeepAlive();
                if (_firstPost)
                {
                    tmpKeepAlive.FirstPost = true;
                    _firstPost = false;
                }
                else
                {
                    tmpKeepAlive.FirstPost = false;
                }
                tmpKeepAlive.Secret = MediaKitServerInstance.Secret;
                tmpKeepAlive.PerformanceInfo = KeeperPerformanceInfo;
                tmpKeepAlive.UseSsl = _akStreamKeeperConfig.UseSsl;
                IPInfo ip = UtilsHelper.GetIpAddressByMacAddress(KeeperPerformanceInfo.NetWorkStat.Mac, true);
                tmpKeepAlive.IpV4Address = ip.IpV4;
                tmpKeepAlive.IpV6Address = ip.IpV6 == null ? "" : ip.IpV6;
                tmpKeepAlive.MediaServerId = MediaKitServerInstance.MediaServerId;
                tmpKeepAlive.MediaServerPid = MediaKitServerInstance.GetPid();
                tmpKeepAlive.RecordPathList = _akStreamDiskInfoOfRecordMap;
                tmpKeepAlive.RtpPortMax = _akStreamKeeperConfig.MaxRtpPort;
                tmpKeepAlive.RtpPortMin = _akStreamKeeperConfig.MinRtpPort;
                tmpKeepAlive.ServerDateTime = DateTime.Now;
                tmpKeepAlive.ZlmHttpPort = MediaKitServerInstance.ZlmHttpPort;
                tmpKeepAlive.ZlmHttpsPort = MediaKitServerInstance.ZlmHttpsPort;
                tmpKeepAlive.ZlmRtmpPort = MediaKitServerInstance.ZlmRtmpPort;
                tmpKeepAlive.ZlmRtmpsPort = MediaKitServerInstance.ZlmRtmpsPort;
                tmpKeepAlive.ZlmRtspPort = MediaKitServerInstance.ZlmRtspPort;
                tmpKeepAlive.ZlmRtspsPort = MediaKitServerInstance.ZlmRtspsPort;
                tmpKeepAlive.KeeperWebApiPort = _akStreamKeeperConfig.WebApiPort;
                tmpKeepAlive.ZlmRecordFileSec = MediaKitServerInstance.ZlmRecordFileSec;
                string reqData = JsonHelper.ToJson(tmpKeepAlive,Formatting.Indented);
                try
                {
                    var httpRet = NetHelper.HttpPostRequest(_akStreamKeeperConfig.AkStreamWebRegisterUrl, null, reqData,
                        "utf-8", 5000);
                    if (!string.IsNullOrEmpty(httpRet))
                    {
                        var resMediaServerKeepAlive = JsonHelper.FromJson<ResMediaServerKeepAlive>(httpRet);
                        if (resMediaServerKeepAlive != null)
                        {
                            if (resMediaServerKeepAlive.NeedRestartMediaServer)
                            {
                                Logger.Info(
                                    $"[{LoggerHead}]->控制服务器反馈，要求重启流媒体服务器,马上重启");
                                MediaKitServerInstance.Restart();
                            }

                            if (resMediaServerKeepAlive.RecommendTimeSynchronization)
                            {
                                Logger.Warn(
                                    $"[{LoggerHead}]->控制服务器反馈，流媒体服务器与控制服务器时间一致性过差，建议手工同步时间->控制服务器当前时间->{resMediaServerKeepAlive.ServerDateTime.ToString("yyyy-MM-dd HH:mm:ss")}");
                            }
                        }
                        else
                        {
                            Logger.Warn(
                                $"[{LoggerHead}]->访问控制服务器异常->\r\n{httpRet}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(
                        $"[{LoggerHead}]->与控制服务器保持心跳时异常->\r\n{ex.Message}\r\n{ex.StackTrace}");
                }
            }
        }


        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            Logger.Info(
                $"[{LoggerHead}]->Let's Go...");
            ResponseStruct rs;
            startTimer();
            var ret = ReadConfig(out rs);
            if (!ret || !rs.Code.Equals(ErrorNumber.None))
            {
                Logger.Error(
                    $"[{LoggerHead}]->获取AKStreamKeeper配置文件时异常,系统无法运行->\r\n{JsonHelper.ToJson(rs, Formatting.Indented)}");
                Environment.Exit(0); //退出程序 
            }

            ProcessHelper.KillProcess(_akStreamKeeperConfig.MediaServerPath); //启动前先删除掉所有流媒体进程
            while (StartupMediaServer() <= 0)
            {
                Logger.Error(
                    $"[{LoggerHead}]->流媒体服务器启动失败->1秒后重试");
                Thread.Sleep(1000);
            }

            Logger.Info(
                $"[{LoggerHead}]->流媒体服务器启动成功->进程ID:{MediaKitServerInstance.GetPid()}");
        }

        static Common()
        {
            ZLMediaKitServerInstance.OnMediaKilled += OnMediaServerKilled;
        }
    }
}