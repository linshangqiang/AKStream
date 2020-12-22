using System;
using System.Net;
using LibCommon;
using LibLogger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AKStreamWeb.Attributes
{
    /// <summary>
    /// 日志记录类，所有经过httpcontroller的进出日志都会被记录
    /// </summary>
    public class LogAttribute : Attribute, IActionFilter
    {
        /// <summary>
        /// 请求后
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
            string info = $@"StatusCode:{context.HttpContext.Response.StatusCode}";
            string remoteIpAddr = context.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (context.HttpContext.Response.StatusCode == (int) HttpStatusCode.OK)
                {
                    if (!context.HttpContext.Request.Path.Equals("/WebHook/MediaServerRegister"))
                    {
                        info =
                            $@"{info}  Body: {JsonHelper.ToJson(((context.Result as ObjectResult)!).Value)}";
                        Logger.Debug(
                            $@"OUTPUT    {remoteIpAddr}    {context.HttpContext.Request.Method}    {context.HttpContext.Request.Path} ->" +
                            info);
                        /*LogWebApiWriter.WriteWebApiLog(
                           c,
                            info,
                            ConsoleColor.Gray);*/
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $@"OUTPUT    {remoteIpAddr}    {context.HttpContext.Request.Method}    {context.HttpContext.Request.Path} ->" +
                    info + " -> " + ex.Message + " -> " + ex.StackTrace);
            }
        }

        /// <summary>
        /// 请求中
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            string remoteIpAddr = context.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (context.HttpContext.Request.Method.Equals("get", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!context.HttpContext.Request.Path.Equals("/WebHook/MediaServerRegister"))
                    {
                        Logger.Debug(
                            $@"INPUT    {remoteIpAddr}    {context.HttpContext.Request.Method}    {context.HttpContext.Request.Path} ->" +
                            $@"{JsonHelper.ToJson(context.ActionArguments)}");
                    }
                }
                else
                {
                    if (!context.HttpContext.Request.Path.Equals("/WebHook/MediaServerRegister"))
                    {
                        Logger.Debug(
                            $@"INPUT    {remoteIpAddr}    {context.HttpContext.Request.Method}    {context.HttpContext.Request.Path} -> " +
                            $@"{JsonHelper.ToJson(context.ActionArguments)}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $@"INPUT    {remoteIpAddr}    {context.HttpContext.Request.Method}    {context.HttpContext.Request.Path} ->" +
                    remoteIpAddr + " -> " + ex.Message + " -> " + ex.StackTrace);
            }
        }
    }
}