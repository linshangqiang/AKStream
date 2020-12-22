using System;
using LibCommon;
using LibZLMediaKitMediaServer.Structs.WebRequest.ZLMediaKit;
using LibZLMediaKitMediaServer.Structs.WebResponse.ZLMediaKit;

namespace LibZLMediaKitMediaServer
{
    public class WebApiHelper
    {
        private string _ipAddress;
        private ushort _webApiPort;
        private string _secret;
        private string _baseUri = "/index/api/";
        private bool _useSSL = false;


        public WebApiHelper(string ipAddress, ushort webApiPort, string secret, string baseUri = "",
            bool useSSL = false)
        {
            _webApiPort = webApiPort;
            _secret = secret;
            _ipAddress = ipAddress.Trim();
            if (!string.IsNullOrEmpty(baseUri))
            {
                _baseUri = baseUri;
            }

            _useSSL = useSSL;
        }

        /// <summary>
        /// 获取各epoll(或select)线程负载以及延时
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        public ResZLMediaKitGetThreadsLoad GetThreadsLoad(out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            string url = _useSSL ? "https://" : "http://" + $"{_ipAddress}{_baseUri}getThreadsLoad";
            try
            {
                var httpRet = NetHelper.HttpGetRequest(url, null, "utf-8", 5000);
                if (!string.IsNullOrEmpty(httpRet))
                {
                    var resZlMediaKitGetThreadsLoad = JsonHelper.FromJson<ResZLMediaKitGetThreadsLoad>(httpRet);
                    if (resZlMediaKitGetThreadsLoad != null)
                    {
                        return resZlMediaKitGetThreadsLoad;
                    }

                    var resError = JsonHelper.FromJson<ResZLMediaKitErrorResponse>(httpRet);
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                        ExceptMessage = httpRet,
                        ExceptStackTrace = JsonHelper.ToJson(resError),
                    };
                }
                else
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                    };
                }
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_WebApiExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
            }

            return null;
        }


        /// <summary>
        /// 获取各后台epoll(或select)线程负载以及延时
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        public ResZLMediaKitGetThreadsLoad GetWorkThreadsLoad(out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            string url = _useSSL ? "https://" : "http://" + $"{_ipAddress}{_baseUri}getWorkThreadsLoad";
            try
            {
                var httpRet = NetHelper.HttpGetRequest(url, null, "utf-8", 5000);
                if (!string.IsNullOrEmpty(httpRet))
                {
                    var resZlMediaKitGetThreadsLoad = JsonHelper.FromJson<ResZLMediaKitGetThreadsLoad>(httpRet);
                    if (resZlMediaKitGetThreadsLoad != null)
                    {
                        return resZlMediaKitGetThreadsLoad;
                    }

                    var resError = JsonHelper.FromJson<ResZLMediaKitErrorResponse>(httpRet);
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                        ExceptMessage = httpRet,
                        ExceptStackTrace = JsonHelper.ToJson(resError),
                    };
                }
                else
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                    };
                }
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_WebApiExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
            }

            return null;
        }


        /// <summary>
        /// 获取服务器配置
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        public ResZLMediaKitConfig GetServerConfig(out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            string url = _useSSL ? "https://" : "http://" + $"{_ipAddress}{_baseUri}getServerConfig";
            try
            {
                var req = new ReqZLMediaKitGetSystemConfig();
                req.Secret = this._secret;
                string reqData = JsonHelper.ToJson(req);
                var httpRet = NetHelper.HttpPostRequest(url, null, reqData, "utf-8", 60000);
                if (!string.IsNullOrEmpty(httpRet))
                {
                    var resZlMediaKitConfig = JsonHelper.FromJson<ResZLMediaKitConfig>(httpRet);
                    if (resZlMediaKitConfig != null)
                    {
                        return resZlMediaKitConfig;
                    }

                    var resError = JsonHelper.FromJson<ResZLMediaKitErrorResponse>(httpRet);
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                        ExceptMessage = httpRet,
                        ExceptStackTrace = JsonHelper.ToJson(resError),
                    };
                }
                else
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                    };
                }
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_WebApiExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
            }

            return null;
        }


        /// <summary>
        /// 重启流媒体服务器（弃用）
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        public ResZLMediaKitRestartServer RestartServer(out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            string url = _useSSL ? "https://" : "http://" + $"{_ipAddress}{_baseUri}restartServer";
            try
            {
                var req = new ReqZLMediaKitRequestBase();
                req.Secret = this._secret;
                string reqData = JsonHelper.ToJson(req);
                var httpRet = NetHelper.HttpPostRequest(url, null, reqData, "utf-8", 60000);
                if (!string.IsNullOrEmpty(httpRet))
                {
                    var resZLMediaKitRestartServer = JsonHelper.FromJson<ResZLMediaKitRestartServer>(httpRet);
                    if (resZLMediaKitRestartServer != null)
                    {
                        return resZLMediaKitRestartServer;
                    }

                    var resError = JsonHelper.FromJson<ResZLMediaKitErrorResponse>(httpRet);
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                        ExceptMessage = httpRet,
                        ExceptStackTrace = JsonHelper.ToJson(resError),
                    };
                }
                else
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                    };
                }
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_WebApiExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
            }

            return null;
        }


        /// <summary>
        /// 获取ZLMediakit的流列表
        /// </summary>
        /// <param name="req"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public ResZLMediaKitMediaList GetMediaList(ResZLMediaKitGetMediaList req, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            string url = _useSSL ? "https://" : "http://" + $"{_ipAddress}{_baseUri}getMediaList";
            try
            {
                req.Secret = this._secret;
                string reqData = JsonHelper.ToJson(req);
                var httpRet = NetHelper.HttpPostRequest(url, null, reqData, "utf-8", 60000);
                if (!string.IsNullOrEmpty(httpRet))
                {
                    var resZLMediaKitMediaList = JsonHelper.FromJson<ResZLMediaKitMediaList>(httpRet);
                    if (resZLMediaKitMediaList != null)
                    {
                        return resZLMediaKitMediaList;
                    }

                    var resError = JsonHelper.FromJson<ResZLMediaKitErrorResponse>(httpRet);
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                        ExceptMessage = httpRet,
                        ExceptStackTrace = JsonHelper.ToJson(resError),
                    };
                }
                else
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                    };
                }
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_WebApiExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
            }

            return null;
        }


        /// <summary>
        /// 请求关闭流
        /// </summary>
        /// <param name="req"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public ResZLMediaKitCloseStreams CloseStreams(ReqZLMediaKitCloseStreams req, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            string url = _useSSL ? "https://" : "http://" + $"{_ipAddress}{_baseUri}close_streams";
            try
            {
                req.Secret = this._secret;
                string reqData = JsonHelper.ToJson(req);
                var httpRet = NetHelper.HttpPostRequest(url, null, reqData, "utf-8", 60000);
                if (!string.IsNullOrEmpty(httpRet))
                {
                    var resZLMediaKitCloseStreams = JsonHelper.FromJson<ResZLMediaKitCloseStreams>(httpRet);
                    if (resZLMediaKitCloseStreams != null)
                    {
                        return resZLMediaKitCloseStreams;
                    }

                    var resError = JsonHelper.FromJson<ResZLMediaKitErrorResponse>(httpRet);
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                        ExceptMessage = httpRet,
                        ExceptStackTrace = JsonHelper.ToJson(resError),
                    };
                }
                else
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                    };
                }
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_WebApiExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
            }

            return null;
        }

        /// <summary>
        /// 获取所有TcpSession列表(获取所有tcp客户端相关信息)
        /// </summary>
        /// <param name="req"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public ResZLMediaKitGetAllSession GetAllSession(ReqZLMediaKitGetAllSession req, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            string url = _useSSL ? "https://" : "http://" + $"{_ipAddress}{_baseUri}getAllSession";
            try
            {
                req.Secret = this._secret;
                string reqData = JsonHelper.ToJson(req);
                var httpRet = NetHelper.HttpPostRequest(url, null, reqData, "utf-8", 60000);
                if (!string.IsNullOrEmpty(httpRet))
                {
                    var resZLMediaKitGetAllSession = JsonHelper.FromJson<ResZLMediaKitGetAllSession>(httpRet);
                    if (resZLMediaKitGetAllSession != null)
                    {
                        return resZLMediaKitGetAllSession;
                    }

                    var resError = JsonHelper.FromJson<ResZLMediaKitErrorResponse>(httpRet);
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                        ExceptMessage = httpRet,
                        ExceptStackTrace = JsonHelper.ToJson(resError),
                    };
                }
                else
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                    };
                }
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_WebApiExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
            }

            return null;
        }


        /// <summary>
        /// 断开tcp连接，比如说可以断开rtsp、rtmp播放器等
        /// </summary>
        /// <param name="req"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public ResZLMediaKitKickSession KickSession(ReqZLMediaKitGetAllSession req, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            string url = _useSSL ? "https://" : "http://" + $"{_ipAddress}{_baseUri}kick_session";
            try
            {
                req.Secret = this._secret;
                string reqData = JsonHelper.ToJson(req);
                var httpRet = NetHelper.HttpPostRequest(url, null, reqData, "utf-8", 60000);
                if (!string.IsNullOrEmpty(httpRet))
                {
                    var resZLMeidakitKickSession = JsonHelper.FromJson<ResZLMediaKitKickSession>(httpRet);
                    if (resZLMeidakitKickSession != null)
                    {
                        return resZLMeidakitKickSession;
                    }

                    var resError = JsonHelper.FromJson<ResZLMediaKitErrorResponse>(httpRet);
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                        ExceptMessage = httpRet,
                        ExceptStackTrace = JsonHelper.ToJson(resError),
                    };
                }
                else
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                    };
                }
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_WebApiExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
            }

            return null;
        }

        /// <summary>
        /// 断开tcp连接，比如说可以断开rtsp、rtmp播放器等(多个)
        /// </summary>
        /// <param name="req"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public ResZLMediaKitKickSessions KickSessions(ReqZLMediaKitKickSessions req, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            string url = _useSSL ? "https://" : "http://" + $"{_ipAddress}{_baseUri}kick_sessions";
            try
            {
                req.Secret = this._secret;
                string reqData = JsonHelper.ToJson(req);
                var httpRet = NetHelper.HttpPostRequest(url, null, reqData, "utf-8", 60000);
                if (!string.IsNullOrEmpty(httpRet))
                {
                    var resZLMediaKitKickSessions = JsonHelper.FromJson<ResZLMediaKitKickSessions>(httpRet);
                    if (resZLMediaKitKickSessions != null)
                    {
                        return resZLMediaKitKickSessions;
                    }

                    var resError = JsonHelper.FromJson<ResZLMediaKitErrorResponse>(httpRet);
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                        ExceptMessage = httpRet,
                        ExceptStackTrace = JsonHelper.ToJson(resError),
                    };
                }
                else
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                    };
                }
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_WebApiExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
            }

            return null;
        }


        /// <summary>
        /// 添加流代理
        /// </summary>
        /// <param name="req"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public ResZLMediaKitAddStreamProxy AddStreamProxy(ReqZLMediaKitAddStreamProxy req, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            string url = _useSSL ? "https://" : "http://" + $"{_ipAddress}{_baseUri}addStreamProxy";
            try
            {
                req.Secret = this._secret;
                string reqData = JsonHelper.ToJson(req);
                var httpRet = NetHelper.HttpPostRequest(url, null, reqData, "utf-8", 60000);
                if (!string.IsNullOrEmpty(httpRet))
                {
                    var resZLMediaKitAddStreamProxy = JsonHelper.FromJson<ResZLMediaKitAddStreamProxy>(httpRet);
                    if (resZLMediaKitAddStreamProxy != null)
                    {
                        return resZLMediaKitAddStreamProxy;
                    }

                    var resError = JsonHelper.FromJson<ResZLMediaKitErrorResponse>(httpRet);
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                        ExceptMessage = httpRet,
                        ExceptStackTrace = JsonHelper.ToJson(resError),
                    };
                }
                else
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                    };
                }
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_WebApiExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
            }

            return null;
        }


        /// <summary>
        /// 添加ffmpeg流代理
        /// </summary>
        /// <param name="req"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public ResZLMediaKitAddFFmpegProxy AddFFmpegSource(ReqZLMediaKitAddFFmpegProxy req, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            string url = _useSSL ? "https://" : "http://" + $"{_ipAddress}{_baseUri}addFFmpegSource";
            try
            {
                req.Secret = this._secret;
                string reqData = JsonHelper.ToJson(req);
                var httpRet = NetHelper.HttpPostRequest(url, null, reqData, "utf-8", 60000);
                if (!string.IsNullOrEmpty(httpRet))
                {
                    var resZLMediaKitAddFFmpegProxy = JsonHelper.FromJson<ResZLMediaKitAddFFmpegProxy>(httpRet);
                    if (resZLMediaKitAddFFmpegProxy != null)
                    {
                        return resZLMediaKitAddFFmpegProxy;
                    }

                    var resError = JsonHelper.FromJson<ResZLMediaKitErrorResponse>(httpRet);
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                        ExceptMessage = httpRet,
                        ExceptStackTrace = JsonHelper.ToJson(resError),
                    };
                }
                else
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                    };
                }
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_WebApiExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
            }

            return null;
        }


        /// <summary>
        /// 获取rtp代理时的某路ssrc rtp信息
        /// </summary>
        /// <param name="req"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public ResZLMediaKitGetRtpInfo GetRtpInfo(ReqZLMediaKitGetRtpInfo req, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            string url = _useSSL ? "https://" : "http://" + $"{_ipAddress}{_baseUri}getRtpInfo";
            try
            {
                req.Secret = this._secret;
                string reqData = JsonHelper.ToJson(req);
                var httpRet = NetHelper.HttpPostRequest(url, null, reqData, "utf-8", 60000);
                if (!string.IsNullOrEmpty(httpRet))
                {
                    var resZLMediaKitGetRtpInfo = JsonHelper.FromJson<ResZLMediaKitGetRtpInfo>(httpRet);
                    if (resZLMediaKitGetRtpInfo != null)
                    {
                        return resZLMediaKitGetRtpInfo;
                    }

                    var resError = JsonHelper.FromJson<ResZLMediaKitErrorResponse>(httpRet);
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                        ExceptMessage = httpRet,
                        ExceptStackTrace = JsonHelper.ToJson(resError),
                    };
                }
                else
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                    };
                }
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_WebApiExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
            }

            return null;
        }


        /// <summary>
        /// 开始录制
        /// </summary>
        /// <param name="req"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public ResZLMediaKitStartRecord StartRecord(ReqZLMediaKitStartRecord req, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            string url = _useSSL ? "https://" : "http://" + $"{_ipAddress}{_baseUri}startRecord";
            try
            {
                req.Secret = this._secret;
                string reqData = JsonHelper.ToJson(req);
                var httpRet = NetHelper.HttpPostRequest(url, null, reqData, "utf-8", 60000);
                if (!string.IsNullOrEmpty(httpRet))
                {
                    var resZLMediaKitStartRecord = JsonHelper.FromJson<ResZLMediaKitStartRecord>(httpRet);
                    if (resZLMediaKitStartRecord != null)
                    {
                        return resZLMediaKitStartRecord;
                    }

                    var resError = JsonHelper.FromJson<ResZLMediaKitErrorResponse>(httpRet);
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                        ExceptMessage = httpRet,
                        ExceptStackTrace = JsonHelper.ToJson(resError),
                    };
                }
                else
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                    };
                }
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_WebApiExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
            }

            return null;
        }


        /// <summary>
        /// 停止录制
        /// </summary>
        /// <param name="req"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public ResZLMediaKitStopRecord StopRecord(ReqZLMediaKitStopRecord req, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            string url = _useSSL ? "https://" : "http://" + $"{_ipAddress}{_baseUri}stopRecord";
            try
            {
                req.Secret = this._secret;
                string reqData = JsonHelper.ToJson(req);
                var httpRet = NetHelper.HttpPostRequest(url, null, reqData, "utf-8", 60000);
                if (!string.IsNullOrEmpty(httpRet))
                {
                    var resZLMediaKitStopRecord = JsonHelper.FromJson<ResZLMediaKitStopRecord>(httpRet);
                    if (resZLMediaKitStopRecord != null)
                    {
                        return resZLMediaKitStopRecord;
                    }

                    var resError = JsonHelper.FromJson<ResZLMediaKitErrorResponse>(httpRet);
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                        ExceptMessage = httpRet,
                        ExceptStackTrace = JsonHelper.ToJson(resError),
                    };
                }
                else
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                    };
                }
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_WebApiExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
            }

            return null;
        }


        /// <summary>
        /// 流是否正在录制
        /// </summary>
        /// <param name="req"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public ResZLMediaKitIsRecording IsRecording(ReqZLMediaKitIsRecording req, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            string url = _useSSL ? "https://" : "http://" + $"{_ipAddress}{_baseUri}isRecording";
            try
            {
                req.Secret = this._secret;
                string reqData = JsonHelper.ToJson(req);
                var httpRet = NetHelper.HttpPostRequest(url, null, reqData, "utf-8", 60000);
                if (!string.IsNullOrEmpty(httpRet))
                {
                    var resZLMediaKitIsRecording = JsonHelper.FromJson<ResZLMediaKitIsRecording>(httpRet);
                    if (resZLMediaKitIsRecording != null)
                    {
                        return resZLMediaKitIsRecording;
                    }

                    var resError = JsonHelper.FromJson<ResZLMediaKitErrorResponse>(httpRet);
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                        ExceptMessage = httpRet,
                        ExceptStackTrace = JsonHelper.ToJson(resError),
                    };
                }
                else
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                    };
                }
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_WebApiExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
            }

            return null;
        }

        /// <summary>
        /// 获取流截图
        /// </summary>
        /// <param name="req"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public string GetSnap(ReqZLMediaKitGetSnap req, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            string url = _useSSL
                ? "https://"
                : "http://" +
                  $"{_ipAddress}{_baseUri}getSnap?secret={this._secret}&url={req.Url}&timeout_sec={req.Timeout_Sec}&expire_sec={req.Expire_Sec}";
            try
            {
                string base64 = "";
                var httpRet = NetHelper.DownloadFileToBase64(url, out base64);
                if (httpRet && !string.IsNullOrEmpty(base64))
                {
                    return base64;
                }
                else
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                    };
                }
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_WebApiExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
            }

            return null;
        }


        /// <summary>
        /// 创建rtp服务
        /// </summary>
        /// <param name="req"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public ResZLMediaKitOpenRtpPort OpenRtpPort(ReqZLMediaKitOpenRtpPort req, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            string url = _useSSL ? "https://" : "http://" + $"{_ipAddress}{_baseUri}openRtpServer";
            try
            {
                req.Secret = this._secret;
                string reqData = JsonHelper.ToJson(req);
                var httpRet = NetHelper.HttpPostRequest(url, null, reqData, "utf-8", 60000);
                if (!string.IsNullOrEmpty(httpRet))
                {
                    var resZLMediaKitOpenRtpPort = JsonHelper.FromJson<ResZLMediaKitOpenRtpPort>(httpRet);
                    if (resZLMediaKitOpenRtpPort != null)
                    {
                        return resZLMediaKitOpenRtpPort;
                    }

                    var resError = JsonHelper.FromJson<ResZLMediaKitErrorResponse>(httpRet);
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                        ExceptMessage = httpRet,
                        ExceptStackTrace = JsonHelper.ToJson(resError),
                    };
                }
                else
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                    };
                }
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_WebApiExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
            }

            return null;
        }


        /// <summary>
        /// 关闭rtp服务
        /// </summary>
        /// <param name="req"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public ResZLMediaKitCloseRtpPort CloseRtpPort(ReqZLMediaKitCloseRtpPort req, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            string url = _useSSL ? "https://" : "http://" + $"{_ipAddress}{_baseUri}closeRtpServer";
            try
            {
                req.Secret = this._secret;
                string reqData = JsonHelper.ToJson(req);
                var httpRet = NetHelper.HttpPostRequest(url, null, reqData, "utf-8", 60000);
                if (!string.IsNullOrEmpty(httpRet))
                {
                    var resZLMediaKitCloseRtpPort = JsonHelper.FromJson<ResZLMediaKitCloseRtpPort>(httpRet);
                    if (resZLMediaKitCloseRtpPort != null)
                    {
                        return resZLMediaKitCloseRtpPort;
                    }

                    var resError = JsonHelper.FromJson<ResZLMediaKitErrorResponse>(httpRet);
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                        ExceptMessage = httpRet,
                        ExceptStackTrace = JsonHelper.ToJson(resError),
                    };
                }
                else
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                    };
                }
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_WebApiExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
            }

            return null;
        }


        /// <summary>
        /// 获取rtp服务列表
        /// </summary>
        /// <param name="req"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public ResZLMediaKitListRtpServer ListRtpServer(ReqZLMediaKitRequestBase req, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            string url = _useSSL ? "https://" : "http://" + $"{_ipAddress}{_baseUri}listRtpServer";
            try
            {
                req.Secret = this._secret;
                string reqData = JsonHelper.ToJson(req);
                var httpRet = NetHelper.HttpPostRequest(url, null, reqData, "utf-8", 60000);
                if (!string.IsNullOrEmpty(httpRet))
                {
                    var resZLMediaKitListRtpServer = JsonHelper.FromJson<ResZLMediaKitListRtpServer>(httpRet);
                    if (resZLMediaKitListRtpServer != null)
                    {
                        return resZLMediaKitListRtpServer;
                    }

                    var resError = JsonHelper.FromJson<ResZLMediaKitErrorResponse>(httpRet);
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                        ExceptMessage = httpRet,
                        ExceptStackTrace = JsonHelper.ToJson(resError),
                    };
                }
                else
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                    };
                }
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_WebApiExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
            }

            return null;
        }

        /// <summary>
        /// 向上级发送rtp数据
        /// </summary>
        /// <param name="req"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public ResZLMediaKitResponseBase StartSendRtp(ReqZLMediaKitStartSendRtp req, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            string url = _useSSL ? "https://" : "http://" + $"{_ipAddress}{_baseUri}startSendRtp";
            try
            {
                req.Secret = this._secret;
                string reqData = JsonHelper.ToJson(req);
                var httpRet = NetHelper.HttpPostRequest(url, null, reqData, "utf-8", 60000);
                if (!string.IsNullOrEmpty(httpRet))
                {
                    var resZLMediaKitResponseBase = JsonHelper.FromJson<ResZLMediaKitResponseBase>(httpRet);
                    if (resZLMediaKitResponseBase != null)
                    {
                        return resZLMediaKitResponseBase;
                    }

                    var resError = JsonHelper.FromJson<ResZLMediaKitErrorResponse>(httpRet);
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                        ExceptMessage = httpRet,
                        ExceptStackTrace = JsonHelper.ToJson(resError),
                    };
                }
                else
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                    };
                }
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_WebApiExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
            }

            return null;
        }

        /// <summary>
        /// 结束向上级发送rtp数据
        /// </summary>
        /// <param name="req"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public ResZLMediaKitResponseBase StopSendRtp(ReqZLMediaKitStopSendRtp req, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };

            string url = _useSSL ? "https://" : "http://" + $"{_ipAddress}{_baseUri}stopSendRtp";
            try
            {
                req.Secret = this._secret;
                string reqData = JsonHelper.ToJson(req);
                var httpRet = NetHelper.HttpPostRequest(url, null, reqData, "utf-8", 60000);
                if (!string.IsNullOrEmpty(httpRet))
                {
                    var resZLMediaKitResponseBase = JsonHelper.FromJson<ResZLMediaKitResponseBase>(httpRet);
                    if (resZLMediaKitResponseBase != null)
                    {
                        return resZLMediaKitResponseBase;
                    }

                    var resError = JsonHelper.FromJson<ResZLMediaKitErrorResponse>(httpRet);
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                        ExceptMessage = httpRet,
                        ExceptStackTrace = JsonHelper.ToJson(resError),
                    };
                }
                else
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.MediaServer_WebApiDataExcept,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiDataExcept],
                    };
                }
            }
            catch (Exception ex)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.MediaServer_WebApiExcept,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.MediaServer_WebApiExcept],
                    ExceptMessage = ex.Message,
                    ExceptStackTrace = ex.StackTrace,
                };
            }

            return null;
        }
    }
}