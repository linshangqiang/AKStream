using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using IniParser;
using IniParser.Model;
using LibCommon;
using LibLogger;

namespace AKStreamKeeper
{
    /// <summary>
    /// 流媒体服务器的进程实例
    /// </summary>
    [Serializable]
    public class MediaServerInstance
    {
        private string _binPath;
        private string _configPath;
        private string _workPath;
        private Process _process;
        private string _secret;
        private string _mediaServerId;
        private static int _pid;
        private bool _isRunning => CheckRunning();
        private ushort _zlmHttpPort;
        private ushort _zlmHttpsPort;
        private ushort _zlmRtspPort;
        private ushort _zlmRtmpPort;
        private ushort _zlmRtspsPort;
        private ushort _zlmRtmpsPort;
        private ushort _zlmRtpProxyPort;
        private uint _zlmRecordFileSec;
        private string _zlmFFMPEGCmd;
        private static bool _isSelfClose = false;

        public static event Common.MediaServerKilled OnMediaKilled = null!;

        private static ProcessHelper _mediaServerProcessHelper =
            new ProcessHelper(p_StdOutputDataReceived, p_ErrOutputDataReceived, p_Process_Exited!);

        /// <summary>
        /// 可执行文件路径
        /// </summary>
        public string BinPath
        {
            get => _binPath;
            set => _binPath = value;
        }

        /// <summary>
        /// 配置文件路径
        /// </summary>
        public string ConfigPath
        {
            get => _configPath;
            set => _configPath = value;
        }

        /// <summary>
        /// 鉴权密钥
        /// </summary>
        public string Secret
        {
            get => _secret;
            set => _secret = value;
        }

        /// <summary>
        /// 流媒体服务器id
        /// </summary>
        public string MediaServerId
        {
            get => _mediaServerId;
            set => _mediaServerId = value;
        }

        /// <summary>
        /// 流媒体进程id
        /// </summary>
        public static int Pid
        {
            get => _pid;
            set => _pid = value;
        }

        public ushort ZlmRtpProxyPort
        {
            get => _zlmRtpProxyPort;
            set => _zlmRtpProxyPort = value;
        }

        /// <summary>
        /// http端口
        /// </summary>
        public ushort ZlmHttpPort
        {
            get => _zlmHttpPort;
            set => _zlmHttpPort = value;
        }

        /// <summary>
        /// https端口
        /// </summary>
        public ushort ZlmHttpsPort
        {
            get => _zlmHttpsPort;
            set => _zlmHttpsPort = value;
        }

        /// <summary>
        /// rtsp端口
        /// </summary>
        public ushort ZlmRtspPort
        {
            get => _zlmRtspPort;
            set => _zlmRtspPort = value;
        }

        /// <summary>
        /// rtmp端口
        /// </summary>
        public ushort ZlmRtmpPort
        {
            get => _zlmRtmpPort;
            set => _zlmRtmpPort = value;
        }

        /// <summary>
        /// rtsps端口
        /// </summary>
        public ushort ZlmRtspsPort
        {
            get => _zlmRtspsPort;
            set => _zlmRtspsPort = value;
        }

        /// <summary>
        /// rtmps端口
        /// </summary>
        public ushort ZlmRtmpsPort
        {
            get => _zlmRtmpsPort;
            set => _zlmRtmpsPort = value;
        }

        /// <summary>
        /// 文件录制时长（秒）
        /// </summary>
        public uint ZlmRecordFileSec
        {
            get => _zlmRecordFileSec;
            set => _zlmRecordFileSec = value;
        }

        /// <summary>
        /// ffmpeg的命令
        /// </summary>
        public string ZlmFFmpegCmd
        {
            get => _zlmFFMPEGCmd;
            set => _zlmFFMPEGCmd = value;
        }

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning
        {
            get => _isRunning;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="binPath"></param>
        /// <param name="configPath"></param>
        public MediaServerInstance(string binPath, string configPath = "")
        {
            _binPath = binPath;
            if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
            {
                _workPath = Path.GetDirectoryName(binPath);
                _configPath = _workPath + "/config.ini";
            }
            else
            {
                _configPath = configPath;
            }

            ResponseStruct rs;
            try
            {
                var ret = GetConfig(out rs);
                if (!ret || !rs.Code.Equals(ErrorNumber.None))
                {
                    throw new AkStreamException(rs);
                }

                var ret2 = SetConfig(out rs);
                if (!ret2 || !rs.Code.Equals(ErrorNumber.None))
                {
                    throw new AkStreamException(rs);
                }
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
                throw new AkStreamException(rs);
            }
        }


        public bool SetConfig(out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (!string.IsNullOrEmpty(_configPath) && File.Exists(_configPath))
            {
                var parser = new FileIniDataParser();
                try
                {
                    IniData data = parser.ReadFile(_configPath, Encoding.UTF8);
                    Uri AKStreamWebUri = new Uri(Common.AkStreamKeeperConfig.AkStreamWebRegisterUrl);
                    string h = AKStreamWebUri.Host.ToString();
                    string p = AKStreamWebUri.Port.ToString();
                    data["hook"].RemoveAllKeys();
                    data["hook"]["enable"] = "1";
                    data["hook"]["on_flow_report"] =
                        $"http://{h}:{p}/MediaServer/WebHook/OnFlowReport"; //流量统计
                    data["hook"]["on_http_access"] =
                        $"http://{h}:{p}/MediaServer/WebHook/OnHttpAccess"; //http事件
                    data["hook"]["on_play"] =
                        $"http://{h}:{p}/MediaServer/WebHook/OnPlay"; //有流被客户端播放时
                    data["hook"]["on_publish"] =
                        $"http://{h}:{p}/MediaServer/WebHook/OnPublish"; //有流发布时
                    data["hook"]["on_record_mp4"] =
                        $"http://{h}:{p}/MediaServer/WebHook/OnRecordMp4"; //当录制mp4完成时
                    data["hook"]["on_record_ts"] =
                        $"http://{h}:{p}/MediaServer/WebHook/OnRecordTs"; //当录制ts完成时
                    data["hook"]["on_rtsp_auth"] =
                        $"http://{h}:{p}/MediaServer/WebHook/OnRtspAuth"; //rtsp鉴权
                    data["hook"]["on_rtsp_realm"] =
                        $"http://{h}:{p}/MediaServer/WebHook/OnRtspRealm"; //rtsp专用鉴权
                    data["hook"]["on_shell_login"] =
                        $"http://{h}:{p}/MediaServer/WebHook/OnShellLogin"; //shell鉴权
                    data["hook"]["on_stream_changed"] =
                        $"http://{h}:{p}/MediaServer/WebHook/OnStreamChanged"; //流注册或注销时
                    data["hook"]["on_stream_none_reader"] =
                        $"http://{h}:{p}/MediaServer/WebHook/OnStreamNoneReader"; //流无人观看时
                    data["hook"]["on_stream_not_found"] =
                        $"http://{h}:{p}/MediaServer/WebHook/OnStreamNotFopund"; //请求没有找到对应流的时候
                    data["hook"]["on_server_started"] =
                        $"http://{h}:{p}/MediaServer/WebHook/OnServerStarted"; //当流媒体启动时
                    data["hook"]["timeoutSec"] = "5"; //httpclient超时时间5秒
                    data["general"]["flowThreshold"] = "1"; //当用户超过1byte流量时，将触发on_flow_report的webhook(/WebHook/OnStop)
                    parser.WriteFile(_configPath, data);
                    return true;
                }
                catch (Exception ex)
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.Sys_WriteIniFileExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_WriteIniFileExcept],
                        ExceptMessage = ex.Message,
                        ExceptStackTrace = ex.StackTrace,
                    };
                    throw new AkStreamException(rs);
                }
            }

            rs = new ResponseStruct()
            {
                Code = ErrorNumber.MediaServer_ConfigNotFound,
                Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_ConfigNotFound],
            };
            return false;
        }

        /// <summary>
        /// 获取和检查流媒体服务器的一些参数
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        /// <exception cref="AkStreamException"></exception>
        public bool GetConfig(out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (!string.IsNullOrEmpty(_configPath) && File.Exists(_configPath))
            {
                var parser = new FileIniDataParser();

                try
                {
                    string[] fileIniStrings = File.ReadAllLines(_configPath);
                    for (int i = 0; i <= fileIniStrings.Length - 1; i++)
                    {
                        if (fileIniStrings[i].Trim().StartsWith('#') || fileIniStrings[i].Trim().StartsWith(';'))
                        {
                            fileIniStrings[i] = fileIniStrings[i].TrimStart('#');
                            fileIniStrings[i] = ";" + fileIniStrings[i];
                        }
                    }

                    File.WriteAllLines(_configPath, fileIniStrings);
                    IniData data = parser.ReadFile(_configPath, Encoding.UTF8);

                    #region 检查MediaServerId

                    var _tmpStr = data["general"]["mediaServerId"];
                    if (string.IsNullOrEmpty(_tmpStr) || _tmpStr.ToUpper().Equals("your_server_id"))
                    {
                        data["general"]["mediaServerId"] = UtilsHelper.generalGuid();
                        try
                        {
                            parser.WriteFile(_configPath, data);
                        }
                        catch (Exception ex)
                        {
                            rs = new ResponseStruct()
                            {
                                Code = ErrorNumber.Sys_WriteIniFileExcept,
                                Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_WriteIniFileExcept],
                                ExceptMessage = ex.Message,
                                ExceptStackTrace = ex.StackTrace,
                            };
                            throw new AkStreamException(rs);
                        }
                    }

                    _mediaServerId = data["general"]["mediaServerId"];
                    _tmpStr = "";
                    _tmpStr = data["api"]["secret"];
                    if (!string.IsNullOrEmpty(_tmpStr))
                    {
                        _secret = _tmpStr;
                    }
                    else
                    {
                        _secret = "";
                    }

                    #endregion

                    #region 检查httpPort

                    _tmpStr = "";
                    _tmpStr = data["http"]["port"];
                    if (string.IsNullOrEmpty(_tmpStr))
                    {
                        rs = new ResponseStruct()
                        {
                            Code = ErrorNumber.Sys_ConfigNotReady,
                            Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ConfigNotReady],
                            ExceptMessage = "http.port=null，http端口不能为空",
                        };
                        return false;
                    }
                    else
                    {
                        ushort tmpUshort;
                        if (ushort.TryParse(_tmpStr, out tmpUshort))
                        {
                            _zlmHttpPort = tmpUshort;
                        }
                        else
                        {
                            rs = new ResponseStruct()
                            {
                                Code = ErrorNumber.Sys_ConfigNotReady,
                                Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ConfigNotReady],
                                ExceptMessage = "http.port不是可接受端口，http端口设置异常",
                            };
                            return false;
                        }
                    }

                    #endregion

                    #region 检查https端口

                    _tmpStr = "";
                    _tmpStr = data["http"]["sslport"];
                    if (string.IsNullOrEmpty(_tmpStr))
                    {
                        _zlmHttpsPort = 0;
                    }
                    else
                    {
                        ushort tmpUshort;
                        if (ushort.TryParse(_tmpStr, out tmpUshort))
                        {
                            _zlmHttpsPort = tmpUshort;
                        }
                        else
                        {
                            _zlmHttpsPort = 0;
                        }
                    }

                    #endregion

                    #region 检查rtsp端口

                    _tmpStr = "";
                    _tmpStr = data["rtsp"]["port"];
                    if (string.IsNullOrEmpty(_tmpStr))
                    {
                        rs = new ResponseStruct()
                        {
                            Code = ErrorNumber.Sys_ConfigNotReady,
                            Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ConfigNotReady],
                            ExceptMessage = "rtsp.port=null，rtsp端口不能为空",
                        };
                        return false;
                    }
                    else
                    {
                        ushort tmpUshort;
                        if (ushort.TryParse(_tmpStr, out tmpUshort))
                        {
                            _zlmRtspPort = tmpUshort;
                        }
                        else
                        {
                            rs = new ResponseStruct()
                            {
                                Code = ErrorNumber.Sys_ConfigNotReady,
                                Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ConfigNotReady],
                                ExceptMessage = "rtsp.port不是可接受端口，rtsp端口设置异常",
                            };
                            return false;
                        }
                    }

                    #endregion

                    #region rtsps端口检查

                    _tmpStr = "";
                    _tmpStr = data["rtsp"]["sslport"];
                    if (string.IsNullOrEmpty(_tmpStr))
                    {
                        _zlmRtspsPort = 0;
                    }
                    else
                    {
                        ushort tmpUshort;
                        if (ushort.TryParse(_tmpStr, out tmpUshort))
                        {
                            _zlmRtspsPort = tmpUshort;
                        }
                        else
                        {
                            _zlmRtspsPort = 0;
                        }
                    }

                    #endregion

                    #region rtmp端口检查

                    _tmpStr = "";
                    _tmpStr = data["rtmp"]["port"];
                    if (string.IsNullOrEmpty(_tmpStr))
                    {
                        rs = new ResponseStruct()
                        {
                            Code = ErrorNumber.Sys_ConfigNotReady,
                            Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ConfigNotReady],
                            ExceptMessage = "rtmp.port=null，rtmp端口不能为空",
                        };
                        return false;
                    }
                    else
                    {
                        ushort tmpUshort;
                        if (ushort.TryParse(_tmpStr, out tmpUshort))
                        {
                            _zlmRtmpPort = tmpUshort;
                        }
                        else
                        {
                            rs = new ResponseStruct()
                            {
                                Code = ErrorNumber.Sys_ConfigNotReady,
                                Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ConfigNotReady],
                                ExceptMessage = "rtmp.port不是可接受端口，rtmp端口设置异常",
                            };
                            return false;
                        }
                    }

                    #endregion

                    #region rtmps端口检查

                    _tmpStr = "";
                    _tmpStr = data["rtmp"]["sslport"];
                    if (string.IsNullOrEmpty(_tmpStr))
                    {
                        _zlmRtmpsPort = 0;
                    }
                    else
                    {
                        ushort tmpUshort;
                        if (ushort.TryParse(_tmpStr, out tmpUshort))
                        {
                            _zlmRtmpsPort = tmpUshort;
                        }
                        else
                        {
                            _zlmRtmpsPort = 0;
                        }
                    }

                    #endregion

                    #region 检查rtpProxy端口

                    _tmpStr = "";
                    _tmpStr = data["rtp_proxy"]["port"];
                    if (string.IsNullOrEmpty(_tmpStr))
                    {
                        rs = new ResponseStruct()
                        {
                            Code = ErrorNumber.Sys_ConfigNotReady,
                            Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ConfigNotReady],
                            ExceptMessage = "rtp.port=null，rtp端口不能为空",
                        };
                        return false;
                    }
                    else
                    {
                        ushort tmpUshort;
                        if (ushort.TryParse(_tmpStr, out tmpUshort))
                        {
                            _zlmRtpProxyPort = tmpUshort;
                        }
                        else
                        {
                            rs = new ResponseStruct()
                            {
                                Code = ErrorNumber.Sys_ConfigNotReady,
                                Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ConfigNotReady],
                                ExceptMessage = "rtp.port不是可接受端口，rtp端口设置异常",
                            };
                            return false;
                        }
                    }

                    #endregion

                    #region 检查录制文件时长（秒）

                    _tmpStr = "";
                    _tmpStr = data["record"]["fileSecond"];
                    if (string.IsNullOrEmpty(_tmpStr))
                    {
                        rs = new ResponseStruct()
                        {
                            Code = ErrorNumber.Sys_ConfigNotReady,
                            Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ConfigNotReady],
                            ExceptMessage = "record.fileSecond=null，fileSecond录制时长不能为空，建议120秒",
                        };
                        return false;
                    }
                    else
                    {
                        ushort tmpUshort;
                        if (ushort.TryParse(_tmpStr, out tmpUshort))
                        {
                            _zlmRecordFileSec = tmpUshort;
                        }
                        else
                        {
                            rs = new ResponseStruct()
                            {
                                Code = ErrorNumber.Sys_ConfigNotReady,
                                Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ConfigNotReady],
                                ExceptMessage = "record.fileSecond不可接受，fileSecond录制时长不能为空，建议120秒",
                            };
                            return false;
                        }
                    }

                    #endregion

                    #region 获取ffmpeg命令

                    _tmpStr = "";
                    _tmpStr = data["ffmpeg"]["cmd"];
                    if (string.IsNullOrEmpty(_tmpStr))
                    {
                        _zlmFFMPEGCmd = "";
                    }
                    else
                    {
                        _zlmFFMPEGCmd = _tmpStr.Trim();
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.Sys_ReadIniFileExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ReadIniFileExcept],
                        ExceptMessage = ex.Message,
                        ExceptStackTrace = ex.StackTrace,
                    };
                    throw new AkStreamException(rs);
                }

                return true;
            }

            rs = new ResponseStruct()
            {
                Code = ErrorNumber.MediaServer_ConfigNotFound,
                Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_ConfigNotFound],
            };
            return false;
        }

        /// <summary>
        /// 流媒体服务器是否正在运行
        /// </summary>
        /// <returns></returns>
        public bool CheckRunning()
        {
            if (_process != null && !_process.HasExited)
            {
                _pid = _process.Id;
                return true;
            }

            return false;
        }


        /// <summary>
        /// 重启流媒体服务
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public int Restart()
        {
            try
            {
                Shutdown();
                Thread.Sleep(300);
                return Startup();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 配置热加载
        /// </summary>
        /// <returns></returns>
        public int Reload()
        {
            if (_isRunning)
            {
                var tmpPro = new ProcessHelper();
                tmpPro.RunProcess("/bin/bash",
                    $"-c 'killall -1 {Path.GetFileNameWithoutExtension(_process.StartInfo.FileName)}'", 1000, out _,
                    out _);
                return _process.Id;
            }

            return -1;
        }

        /// <summary>
        /// 启动流媒体服务器
        /// </summary>
        /// <returns></returns>
        public int Startup()
        {
            if (_isRunning)
            {
                return _pid;
            }

            string binDir = Path.GetDirectoryName(_binPath);
            string configDir = Path.GetDirectoryName(_configPath);
            Process ret;
            if (binDir.Trim().Equals(configDir.Trim()))
            {
                ret = _mediaServerProcessHelper.RunProcess(_binPath, "");
            }
            else
            {
                ret = _mediaServerProcessHelper.RunProcess(_binPath, $"-c {_configPath}");
            }

            if (ret != null && !ret.HasExited)
            {
                _process = ret;
                _pid = _process.Id;
                _isSelfClose = false;
                return _process.Id;
            }

            return -1;
        }

        /// <summary>
        /// 结束流媒体服务器
        /// </summary>
        /// <returns></returns>
        public bool Shutdown()
        {
            _pid = -1;
            _isSelfClose = true;
            return _mediaServerProcessHelper.KillProcess(_process);
        }

        /// <summary>
        /// 获取流媒体服务器pid
        /// </summary>
        /// <returns></returns>
        public int GetPid()
        {
            if (!_process.HasExited)
            {
                _pid = _process.Id;
                return _process.Id;
            }

            _pid = -1;
            return -1;
        }

        public static void p_Process_Exited(object sender, EventArgs e)
        {
            _pid = -1;
            OnMediaKilled?.Invoke(_isSelfClose);
        }

        public static void p_StdOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                Logger.Debug(
                    $"[{Common.LoggerHead}]->[ZLMediaKit]->{e.Data}");
            }
        }

        public static void p_ErrOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                Logger.Error(
                    $"[{Common.LoggerHead}]->[ZLMediaKit]->{e.Data}");
            }
        }
    }
}