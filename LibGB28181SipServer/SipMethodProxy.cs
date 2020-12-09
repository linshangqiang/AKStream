using System;
using System.Threading;
using LibCommon.Structs.GB28181;
using LibGB28181SipServer;

namespace LibGB28181SipServer
{
    /// <summary>
    /// sip方法操作代理
    /// </summary>
    public class SipMethodProxy
    {
        private  AutoResetEvent _autoResetEvent =new AutoResetEvent(false);
        private SipServer _sipServer;
        private int _timeout;

        public SipMethodProxy(int timeout=5000)
        {
            _timeout = timeout;
        }

        /// <summary>
        /// 获取设备目录
        /// </summary>
        /// <param name="sipDevice"></param>
        /// <returns></returns>
        public bool DeviceCatalogQuery(SipDevice sipDevice)
        {
            Common.SipServer.DeviceCatalogQuery(sipDevice,_autoResetEvent,_timeout);
            var isTimeout = _autoResetEvent.WaitOne(_timeout);
            if (!isTimeout)
            {
                return false;
            }
            return true;
        }
        
    }
}