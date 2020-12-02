using System;
using GB28181.Sys.XML;
using LibCommon;
using SIPSorcery.SIP;

namespace LibGB28181SipServer
{
   
    [Serializable]
    public class SipChannel
    {
        private string _guid;
        private SIPEndPoint _remoteEndPoint;
        private SIPEndPoint _localSipEndPoint;
        private SipDevice _parentSipDevice;
        private string _ipAddress;
        private DevicePushStatus _pushStatus;
        private DateTime? _pushOnTime;
        private DateTime _lastUpdateTime;
        private long? _pushOnlineTime;
        private Catalog.Item _sipChannelItem;

        /// <summary>
        /// 通道在系统中唯一id
        /// </summary>
        public string Guid
        {
            get => _guid;
            set => _guid = value;
        }

        /// <summary>
        /// sip设备的ip端口协议
        /// </summary>
        public SIPEndPoint RemoteEndPoint
        {
            get => _remoteEndPoint;
            set => _remoteEndPoint = value;
        }

        /// <summary>
        /// sip服务的ip端口协议
        /// </summary>
        public SIPEndPoint LocalSipEndPoint
        {
            get => _localSipEndPoint;
            set => _localSipEndPoint = value;
        }

        /// <summary>
        /// 通道所属的sip设备信息
        /// </summary>
        public SipDevice ParentSipDevice
        {
            get => _parentSipDevice;
            set => _parentSipDevice = value;
        }

        /// <summary>
        /// ip地址
        /// </summary>
        public string IpAddress
        {
            get => _ipAddress;
            set => _ipAddress = value;
        }

        /// <summary>
        /// 通道推流情况，idle,pushon空闲,推流中
        /// </summary>
        public DevicePushStatus PushStatus
        {
            get => _pushStatus;
            set => _pushStatus = value;
        }

        /// <summary>
        /// 推流时间
        /// </summary>
        public DateTime? PushOnTime
        {
            get => _pushOnTime;
            set => _pushOnTime = value;
        }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdateTime
        {
            get => _lastUpdateTime;
            set => _lastUpdateTime = value;
        }

        /// <summary>
        /// 本次推流时长
        /// </summary>
        public long? PushOnlineTime
        {
            get => _pushOnlineTime;
            set => _pushOnlineTime = value;
        }

        /// <summary>
        /// 通道信息
        /// </summary>
        public Catalog.Item SipChannelItem
        {
            get => _sipChannelItem;
            set => _sipChannelItem = value;
        }
    }
}