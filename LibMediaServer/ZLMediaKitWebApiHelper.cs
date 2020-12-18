using System;
using LibMediaServer.Structs.WebRequest;
using LibMediaServer.Structs.WebResponse;

namespace LibCommon.Structs
{
    public  class ZLMediaKitWebApiHelper
    {
        private string _ipAddress;
        private ushort _webApiPort;
        private string _secret;
        private string _baseUri = "/index/api/";
        private bool _useSSL = false;
       

        public ZLMediaKitWebApiHelper(string ipAddress,ushort webApiPort, string secret, string baseUri="" ,bool useSSL=false)
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
                var reqParams = new ReqZLMediaKitGetSystemConfig();
                reqParams.Secret = this._secret;
                string reqData = JsonHelper.ToJson(reqParams);
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
        /// 重启流媒体服务器
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
                var reqParams = new ReqZLMediaKitRequestBase();
                reqParams.Secret = this._secret;
                string reqData = JsonHelper.ToJson(reqParams);
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
    }
}