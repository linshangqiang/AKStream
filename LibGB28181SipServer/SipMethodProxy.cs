using System;
using System.Threading;
using LibCommon;
using LibCommon.Structs;
using LibCommon.Structs.GB28181;
using LibCommon.Structs.GB28181.XML;


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
        private CommandType _commandType;
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

        public CommandType CommandType
        {
            get => _commandType;
            set => _commandType = value;
        }


        /// <summary>
        /// 获取设备的状态信息
        /// </summary>
        /// <param name="sipDevice"></param>
        /// <returns></returns>
        public bool GetSipDeviceStatus(SipDevice sipDevice,out ResponseStruct rs)
        {
            try
            {
              
                _commandType = CommandType.DeviceStatus;
                Common.SipServer.GetDeviceStatus(sipDevice , _autoResetEvent,out rs, _timeout);
                var isTimeout = _autoResetEvent.WaitOne(_timeout);
                if (!isTimeout || !rs.Code.Equals(ErrorNumber.None))
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
        
        /// <summary>
        /// 获取SIP设备信息
        /// </summary>
        /// <param name="sipDevice"></param>
        /// <returns></returns>
        public bool GetSipDeviceInfo(SipDevice sipDevice,out ResponseStruct rs)
        {
            try
            {
               
                _commandType = CommandType.DeviceInfo;
                Common.SipServer.GetDeviceInfo(sipDevice , _autoResetEvent,out rs, _timeout);
                var isTimeout = _autoResetEvent.WaitOne(_timeout);
                if (!isTimeout || !rs.Code.Equals(ErrorNumber.None))
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

        
        /// <summary>
        /// 获取通道录像文件列表
        /// </summary>
        /// <param name="sipChannel"></param>
        /// <param name="queryRecordFile"></param>
        /// <returns></returns>
        public bool QueryRecordFileList(SipChannel sipChannel,SipQueryRecordFile queryRecordFile,out ResponseStruct rs)
        {
            try
            {
                _commandType = CommandType.RecordInfo;
                Common.SipServer.GetRecordFileList(sipChannel ,queryRecordFile, _autoResetEvent,out rs, _timeout);
                var isTimeout = _autoResetEvent.WaitOne(_timeout);
                if (!isTimeout || !rs.Code.Equals(ErrorNumber.None))
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
        
        
        /// <summary>
        /// 请求终止时实流
        /// </summary>
        /// <param name="sipChannel"></param>
        /// <returns></returns>
        public bool DeInvite(SipChannel sipChannel,out ResponseStruct rs)
        {
            try
            {
               
                Common.SipServer.DeInvite(sipChannel , _autoResetEvent,out rs, _timeout);
                _commandType = CommandType.Unknown;
                var isTimeout = _autoResetEvent.WaitOne(_timeout);
                if (!isTimeout || !rs.Code.Equals(ErrorNumber.None))
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
        /// <summary>
        /// 请求实时流
        /// </summary>
        /// <param name="sipChannel"></param>
        /// <param name="pushMediaInfo"></param>
        /// <returns></returns>
        public bool Invite(SipChannel sipChannel,PushMediaInfo pushMediaInfo,out ResponseStruct rs)
        {
            try
            {
                _commandType = CommandType.Unknown;
                Common.SipServer.Invite(sipChannel,pushMediaInfo , _autoResetEvent,out rs, _timeout);
                var isTimeout = _autoResetEvent.WaitOne(_timeout);
                if (!isTimeout || !rs.Code.Equals(ErrorNumber.None))
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
        /// <summary>
        /// 获取设备目录
        /// </summary>
        /// <param name="sipDevice"></param>
        /// <returns></returns>
        public bool DeviceCatalogQuery(SipDevice sipDevice,out ResponseStruct rs )
        {
            try
            {
                _commandType = CommandType.Catalog;
                Common.SipServer.DeviceCatalogQuery(sipDevice, _autoResetEvent, out rs,_timeout);
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