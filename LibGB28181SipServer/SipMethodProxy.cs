using System;
using System.Threading;
using LibCommon.Structs.GB28181;
using LibGB28181SipServer;

namespace LibGB28181SipServer
{
    /// <summary>
    /// sip方法操作代理
    /// </summary>
    public class SipMethodProxy:IDisposable
    {
        private  AutoResetEvent _autoResetEvent =new AutoResetEvent(false);
        private SipServer _sipServer;
        private int _timeout;

        public SipMethodProxy(int timeout=5000)
        {
            _timeout = timeout;
        }
        ~SipMethodProxy()
        {
            Dispose(); //释放非托管资源
        }

        public void Dispose()
        {
            if (_autoResetEvent != null)
            {
                _autoResetEvent.Dispose();
            }
        }


       // public bool 
        /// <summary>
        /// 获取设备目录
        /// </summary>
        /// <param name="sipDevice"></param>
        /// <returns></returns>
        public bool DeviceCatalogQuery(SipDevice sipDevice)
        {
            try
            {
                Common.SipServer.DeviceCatalogQuery(sipDevice, _autoResetEvent, _timeout);
                var isTimeout = _autoResetEvent.WaitOne(_timeout);
                if (!isTimeout)
                {
                    return false;
                }

                return true;
            }
            finally
            {
              Dispose(); 
            }
        }
        
    }
}