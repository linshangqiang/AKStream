using System;
using System.Collections.Generic;
using System.Linq;
using LibCommon.Enums;
using LibCommon.Structs.GB28181.Sys;
using LibCommon.Structs.GB28181.XML;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SIPSorcery.SIP;

namespace LibCommon.Structs.GB28181
{
    [Serializable]
    public class SipChannel : IDisposable
    {
        private string _parentId = null!;
        private string _deviceId = null!;
        private string _ssrcId;
        private string _stream;
        private SIPEndPoint _remoteEndPoint = null!;
        private SIPEndPoint _localSipEndPoint = null!;
        private PushStatus _pushStatus;
        private DateTime _lastUpdateTime;
        private SipChannelType _sipChannelType;
        private DevStatus _sipChannelStatus;
        private Catalog.Item _sipChannelDesc = null!;
        private MediaServerStreamInfo? _channelMediaServerStreamInfo;
        private SIPRequest _inviteSipRequest; //要把请求实时视频时的req和res存起来，因为在结束时要用到这两个内容
        private SIPResponse _inviteSipResponse; //要把请求实时视频时的req和res存起来，因为在结束时要用到这两个内容
        private SIPRequest _lastSipRequest; //保存最后一次sipRequest

        private List<KeyValuePair<int, RecordInfo.Item>> _lastRecordInfos =
            new List<KeyValuePair<int, RecordInfo.Item>>(); //最后一次获取到的录像文件列表


        /// <summary>
        /// 额外真加SSRCId
        /// 总共9位，1-5位为channel.deivce的2-6位，6-9位为此设备的序列号
        /// 不足9位要前补0
        /// 真正的ssrc是10位，第一位为1时是回放流，第一位是0时的实时流
        /// 流形式位0/1+后9位就是这个流的ssrc
        /// </summary>

        public string SsrcId
        {
            get => _ssrcId;
            set => _ssrcId = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 额外真加Stream
        /// 它是SSRCID的16进制表示方法
        /// </summary>

        public string Stream
        {
            get => _stream;
            set => _stream = value ?? throw new ArgumentNullException(nameof(value));
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
        public PushStatus PushStatus
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
        public DevStatus SipChannelStatus
        {
            get => _sipChannelStatus;
            set => _sipChannelStatus = value;
        }

        /// <summary>
        /// 通道信息
        /// </summary>
        public Catalog.Item SipChannelDesc
        {
            get => _sipChannelDesc;
            set => _sipChannelDesc = value;
        }


        public void Dispose()
        {
            _sipChannelDesc = null!;
        }

        /// <summary>
        /// Sip通道的流媒体相关信息
        /// </summary>
        public MediaServerStreamInfo? ChannelMediaServerStreamInfo
        {
            get => _channelMediaServerStreamInfo;
            set => _channelMediaServerStreamInfo = value;
        }

        /// <summary>
        /// 保存请求实时流时的request,因为在终止实时流的时候要用到
        /// </summary>
        [JsonIgnore]
        public SIPRequest InviteSipRequest
        {
            get => _inviteSipRequest;
            set => _inviteSipRequest = value ?? throw new ArgumentNullException(nameof(value));
        }


        /// <summary>
        /// 保存请求实时流时的response,因为在终止实时流的时候要用到
        /// </summary>
        [JsonIgnore]
        public SIPResponse InviteSipResponse
        {
            get => _inviteSipResponse;
            set => _inviteSipResponse = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 保存最后一次SipRequest
        /// </summary>
        [JsonIgnore]
        public SIPRequest LastSipRequest
        {
            get => _lastSipRequest;
            set => _lastSipRequest = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 最后一次获取到的录像文件列表
        /// </summary>
        public List<KeyValuePair<int, RecordInfo.Item>> LastRecordInfos
        {
            get => _lastRecordInfos;
            set => _lastRecordInfos = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 获取最后查询到的录像文件，按排序规则排序
        /// </summary>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public List<RecordInfo.Item> GetLastRecordInfoList(OrderBy orderBy = OrderBy.ASC)
        {
            List<RecordInfo.Item> result = new List<RecordInfo.Item>();
            foreach (var kv in LastRecordInfos)
            {
                if (kv.Value != null)
                {
                    result.Add(kv.Value);
                }
            }

            switch (orderBy)
            {
                case OrderBy.ASC:
                    result = result.OrderBy(x => x.EndTime).ToList();

                    break;
                case OrderBy.DESC:
                    result = result.OrderByDescending(x => x.EndTime).ToList();

                    break;
            }

            return result;
        }


        ~SipChannel()
        {
            Dispose(); //释放非托管资源
        }
    }
}