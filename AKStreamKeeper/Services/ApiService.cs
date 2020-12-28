using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using LibCommon;
using LibCommon.Structs.WebResponse.AKStreamKeeper;
using LibLogger;

namespace AKStreamKeeper.Services
{
    public static class ApiService
    {
        /// <summary>
        /// 选择一个可用的rtp端口，仅使用偶数端口
        /// </summary>
        /// <param name="minPort"></param>
        /// <param name="maxPort"></param>
        /// <returns></returns>
        private static ushort _guessAnRtpPort(ushort minPort, ushort maxPort)
        {
            lock (Common._getRtpPortLock)
            {
                IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
                List<IPEndPoint> tcpIpEndPoints = ipProperties.GetActiveTcpListeners().ToList();
                List<IPEndPoint> udpIpEndPoints = ipProperties.GetActiveUdpListeners().ToList();
                if (minPort > maxPort)
                {
                    for (ushort i = maxPort; i <= minPort; i++)
                    {
                        if (UtilsHelper.IsOdd(i))
                        {
                            continue;
                        }

                        if (tcpIpEndPoints.Count > 0 || udpIpEndPoints.Count > 0)
                        {
                            var ret = tcpIpEndPoints.FindLast(x => x.Port == i);
                            var ret2 = udpIpEndPoints.FindLast(x => x.Port == i);
                            if (ret == null && ret2 == null)
                            {
                                return i;
                            }
                        }
                        else
                        {
                            return i;
                        }
                    }
                }

                if (minPort < maxPort)
                {
                    for (ushort i = minPort; i <= maxPort; i++)
                    {
                        if (UtilsHelper.IsOdd(i))
                        {
                            continue;
                        }

                        if (tcpIpEndPoints.Count > 0 || udpIpEndPoints.Count > 0)
                        {
                            var ret = tcpIpEndPoints.FindLast(x => x.Port == i);
                            var ret2 = udpIpEndPoints.FindLast(x => x.Port == i);
                            if (ret == null && ret2 == null)
                            {
                                return i;
                            }
                        }
                        else
                        {
                            return i;
                        }
                    }
                }

                if (minPort == maxPort)
                {
                    if (UtilsHelper.IsOdd(minPort))
                    {
                        return 0;
                    }

                    if (tcpIpEndPoints.Count > 0 || udpIpEndPoints.Count > 0)
                    {
                        var ret = tcpIpEndPoints.FindLast(x => x.Port == minPort);
                        var ret2 = udpIpEndPoints.FindLast(x => x.Port == minPort);
                        if (ret == null && ret2 == null)
                        {
                            return minPort;
                        }
                    }
                    else
                    {
                        return minPort;
                    }
                }

                return 0;
            }
        }


        /// <summary>
        /// 获取流媒体服务器运行状态
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static int CheckMediaServerRunning(out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (Common.MediaServerInstance != null)
            {
                if (Common.MediaServerInstance.IsRunning)
                {
                    return Common.MediaServerInstance.GetPid();
                }

                return -1;
            }

            return -1;
        }

        /// <summary>
        /// 重新加载流媒体服务器配置文件（热加载）
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static bool ReloadMediaServer(out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (Common.MediaServerInstance != null)
            {
                if (Common.MediaServerInstance.IsRunning)
                {
                    var ret = Common.MediaServerInstance.Reload();
                    if (ret > 0)
                    {
                        Logger.Info($"[{Common.LoggerHead}]->热加载流媒体服务器配置文件成功");
                        return true;
                    }

                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_ReloadExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_ReloadExcept],
                    };
                    Logger.Warn($"[{Common.LoggerHead}]->热加载流媒体服务器配置文件失败");
                    return false;
                }

                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_NotRunning,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_NotRunning],
                };
                Logger.Warn($"[{Common.LoggerHead}]->热加载流媒体服务器配置文件失败->流媒体服务器没有运行");
                return false;
            }

            rs = new ResponseStruct()
            {
                Code = ErrorNumber.MediaServer_InstanceIsNull,
                Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_InstanceIsNull],
            };
            Logger.Warn($"[{Common.LoggerHead}]->热加载流媒体服务器配置文件失败->流媒体服务器实例为空");
            return false;
        }

        /// <summary>
        /// 重启流媒体服务器
        /// </summary>
        /// <param name="rs"></param>
        /// <returns>流媒体服务器pid</returns>
        public static int RestartMediaServer(out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (Common.MediaServerInstance != null)
            {
                var ret = Common.MediaServerInstance.Restart();
                if (ret > 0)
                {
                    Logger.Info($"[{Common.LoggerHead}]->重启流媒体服务器成功->PID:{ret}");
                    return ret;
                }

                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_RestartExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_RestartExcept],
                };
                Logger.Warn($"[{Common.LoggerHead}]->重启流媒体服务器失败");
                return -1;
            }

            rs = new ResponseStruct()
            {
                Code = ErrorNumber.MediaServer_InstanceIsNull,
                Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_InstanceIsNull],
            };
            Logger.Warn($"[{Common.LoggerHead}]->重启流媒体服务器失败->流媒体服务实例为空");
            return -1;
        }

        /// <summary>
        /// 终止流媒体服务器
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static bool ShutdownMediaServer(out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (Common.MediaServerInstance != null)
            {
                if (!Common.MediaServerInstance.IsRunning)
                {
                    Logger.Info($"[{Common.LoggerHead}]->关闭流媒体服务器->流媒体不在运行状态");
                    return true;
                }

                var ret = Common.MediaServerInstance.Shutdown();
                if (ret)
                {
                    Logger.Info($"[{Common.LoggerHead}]->关闭流媒体服务器成功");
                    return ret;
                }

                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_ShutdownExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_ShutdownExcept],
                };
                Logger.Warn($"[{Common.LoggerHead}]->关闭流媒体服务器失败");
                return false;
            }

            rs = new ResponseStruct()
            {
                Code = ErrorNumber.MediaServer_InstanceIsNull,
                Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_InstanceIsNull],
            };
            Logger.Warn($"[{Common.LoggerHead}]->关闭流媒体服务器失败->流媒体服务器实例为空");
            return false;
        }

        /// <summary>
        /// 启动流媒体服务器
        /// </summary>
        /// <param name="rs"></param>
        /// <returns>流媒体服务器pid</returns>
        public static int StartMediaServer(out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (Common.MediaServerInstance != null)
            {
                if (Common.MediaServerInstance.IsRunning)
                {
                    var pid = Common.MediaServerInstance.GetPid();
                    Logger.Info($"[{Common.LoggerHead}]->启动流媒体服务器->流媒体服务器处于启动状态->PID:{pid}");
                    return pid;
                }

                var ret = Common.MediaServerInstance.Startup();
                if (ret > 0)
                {
                    Logger.Info($"[{Common.LoggerHead}]->启动流媒体服务器成功->PID:{ret}");
                    return ret;
                }

                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_StartUpExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_StartUpExcept],
                };
                Logger.Warn($"[{Common.LoggerHead}]->启动流媒体服务器失败");
                return -1;
            }

            rs = new ResponseStruct()
            {
                Code = ErrorNumber.MediaServer_InstanceIsNull,
                Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_InstanceIsNull],
            };
            Logger.Warn($"[{Common.LoggerHead}]->启动流媒体服务器失败->流媒体服务实例为空");
            return -1;
        }

        /// <summary>
        /// 清理空目录
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static bool CleanUpEmptyDir(out ResponseStruct rs, string rootPath = "")
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (string.IsNullOrEmpty(rootPath))
            {
                foreach (var path in Common.AkStreamKeeperConfig.CustomRecordPathList)
                {
                    string dirList = "";
                    if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                    {
                        try
                        {
                            DirectoryInfo dir = new DirectoryInfo(path);
                            DirectoryInfo[] subdirs = dir.GetDirectories("*.*", SearchOption.AllDirectories);
                            foreach (DirectoryInfo subdir in subdirs)
                            {
                                FileSystemInfo[] subFiles = subdir.GetFileSystemInfos();
                                var l = subFiles.Length;
                                if (l == 0)
                                {
                                    subdir.Delete();
                                    dirList += "清理空目录 ->" + subdir + "\r\n";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($@"[{Common.LoggerHead}]->清理空目录时发生异常->{ex.Message}\r\n{ex.StackTrace}");
                        }

                        if (!string.IsNullOrEmpty(dirList))
                        {
                            Logger.Info($@"[{Common.LoggerHead}]->{dirList}");
                        }
                    }
                }
            }
            else
            {
                string dirList = "";
                if (!string.IsNullOrEmpty(rootPath) && Directory.Exists(rootPath))
                {
                    try
                    {
                        DirectoryInfo dir = new DirectoryInfo(rootPath);
                        DirectoryInfo[] subdirs = dir.GetDirectories("*.*", SearchOption.AllDirectories);
                        foreach (DirectoryInfo subdir in subdirs)
                        {
                            FileSystemInfo[] subFiles = subdir.GetFileSystemInfos();
                            var l = subFiles.Length;
                            if (l == 0)
                            {
                                subdir.Delete();
                                dirList += "清理空目录 ->" + subdir + "\r\n";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($@"[{Common.LoggerHead}]->清理空目录时发生异常->{ex.Message}\r\n{ex.StackTrace}");
                    }

                    if (!string.IsNullOrEmpty(dirList))
                    {
                        Logger.Info($@"[{Common.LoggerHead}]->{dirList}");
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 批量删除文件
        /// </summary>
        /// <param name="fileList"></param>
        /// <param name="rs"></param>
        /// <returns>返回未正常删除的文件列表</returns>
        public static ResKeeperDeleteFileList DeleteFileList(List<string> fileList, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            var result = new List<string>();
            if (fileList != null && fileList.Count > 0)
            {
                foreach (var path in fileList)
                {
                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    {
                        try
                        {
                            File.Delete(path);
                        }
                        catch
                        {
                            result.Add(path);
                        }
                    }

                    Thread.Sleep(10);
                }

                return new ResKeeperDeleteFileList()
                {
                    PathList = result,
                };
            }

            rs = new ResponseStruct()
            {
                Code = ErrorNumber.Sys_ParamsIsNotRight,
                Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
            };
            return new ResKeeperDeleteFileList();
        }

        /// <summary>
        /// 文件是否存在
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static bool FileExists(string filePath, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            var ret = File.Exists(filePath);
            if (!ret)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_SpecifiedFileNotExist,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_SpecifiedFileNotExist],
                };
            }

            return ret;
        }

        /// <summary>
        /// 删除一个指定文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static bool DeleteFile(string filePath, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    return true;
                }
                catch (Exception ex)
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.None,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.None],
                        ExceptMessage = ex.Message,
                        ExceptStackTrace = ex.StackTrace,
                    };
                    return false;
                }
            }
            else
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_SpecifiedFileNotExist,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_SpecifiedFileNotExist],
                };
                return false;
            }
        }

        /// <summary>
        /// 找一个可用的rtp端口
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static ushort GuessAnRtpPort(out ResponseStruct rs, ushort? min = 0, ushort? max = 0)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            ushort port = 0;
            if ((min == null || min == 0) && (max == null || max == 0))
            {
                port = _guessAnRtpPort(Common.AkStreamKeeperConfig.MinRtpPort,
                    Common.AkStreamKeeperConfig.MaxRtpPort);
            }
            else
            {
                port = _guessAnRtpPort((ushort) min, (ushort) max);
            }

            if (port > 0)
            {
                return port;
            }

            rs = new ResponseStruct()
            {
                Code = ErrorNumber.Sys_SocketPortForRtpExcept,
                Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_SocketPortForRtpExcept],
            };
            return 0;
        }
    }
}