using System;
using System.Collections.Generic;
using GB28181.Sys.XML;
using LibCommon;
using LibCommon.Enums;
using LibCommon.Structs;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SIPSorcery.SIP;

namespace LibGB28181SipServer
{
   
    [Serializable]
    public class SipChannel: IDisposable
    {
        private string _guid = null!;
        private string _parentGuid = null!;
        private string _parentId = null!;
        private string _deviceId = null!;
        private SIPEndPoint _remoteEndPoint = null!;
        private SIPEndPoint _localSipEndPoint = null!;
        private DevicePushStatus _pushStatus;
        private DateTime _lastUpdateTime;
        private SipChannelType _sipChannelType;
        private DevStatus _sipChanneStatus;
        private Catalog.Item _sipChannelDesc = null!;
        private List<MediaServerStreamInfo> _channelMediaServerStreamInfos=new List<MediaServerStreamInfo>();
        

        /// <summary>
        /// 通道在系统中唯一id
        /// </summary>
        public string Guid
        {
            get => _guid;
            set => _guid = value;
        }
        [JsonIgnore]

        /// <summary>
        /// sip设备的ip端口协议
        /// </summary>
        public SIPEndPoint RemoteEndPoint
        {
            get => _remoteEndPoint;
            set => _remoteEndPoint = value;
        }

        [JsonIgnore]
        /// <summary>
        /// sip服务的ip端口协议
        /// </summary>
        public SIPEndPoint LocalSipEndPoint
        {
            get => _localSipEndPoint;
            set => _localSipEndPoint = value;
        }

        /// <summary>
        /// 父节点的guid
        /// </summary>
        public string ParentGuid
        {
            get => _parentGuid;
            set => _parentGuid = value;
        }

        /// <summary>
        /// 父节点的SipDeviceId
        /// </summary>
        public string ParentId
        {
            get => _parentId;
            set => _parentId = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 自己的通道ID
        /// </summary>
        public string DeviceId
        {
            get => _deviceId;
            set => _deviceId = value ?? throw new ArgumentNullException(nameof(value));
        }

        [JsonConverter(typeof(StringEnumConverter))]
        /// <summary>
        /// 通道推流情况，idle,pushon空闲,推流中
        /// </summary>
        public DevicePushStatus PushStatus
        {
            get => _pushStatus;
            set => _pushStatus = value;
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
        /// 通道类型
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public SipChannelType SipChannelType
        {
            get => _sipChannelType;
            set => _sipChannelType = value;
        }

        /// <summary>
        /// 通道状态
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public DevStatus SipChanneStatus
        {
            get => _sipChanneStatus;
            set => _sipChanneStatus = value;
        }

        /// <summary>
        /// 通道信息
        /// </summary>
        public Catalog.Item SipChannelDesc
        {
            get => _sipChannelDesc;
            set => _sipChannelDesc = value;
        }

        /// <summary>
        /// 额外的流媒体信息
        /// </summary>
      
        

        public void Dispose()
        {
            _sipChannelDesc = null!;
        }

        /// <summary>
        /// Sip通道的流媒体相关信息
        /// </summary>
        public List<MediaServerStreamInfo> ChannelMediaServerStreamInfos
        {
            get => _channelMediaServerStreamInfos;
            set => _channelMediaServerStreamInfos = value;
        }
        
        
       // public MediaServerStreamInfo LiveVideo()

        ~SipChannel()
        {
            Dispose(); //释放非托管资源
        }
    }
}