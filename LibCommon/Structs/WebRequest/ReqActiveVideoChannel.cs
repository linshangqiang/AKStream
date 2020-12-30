using System;
using LibCommon.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LibCommon.Structs.WebRequest
{
    [Serializable]
    public class ReqActiveVideoChannel
    {
        private string _mediaServerId;
        private string? _channelName;
        private string? _departmentId;
        private string? _departmentName;
        private string? _pDepartmentId;
        private string? _pDepartmentName;
        private DeviceNetworkType? _deviceNetworkType;
        private VideoDeviceType? _videoDeviceType;
        private bool? _autoVideo;
        private bool? _autoRecord;
        private string? _ipV4Address;
        private string? _ipV6Address;
        private bool? _hasPtz;
        private bool? _rtpWithTcp;
        private bool? _defaultRtpPort;
        private bool? _noPlayerBreak;

        /// <summary>
        /// 流媒体服务器id
        /// </summary>
        public string MediaServerId
        {
            get => _mediaServerId;
            set => _mediaServerId = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 通道名称
        /// </summary>
        public string? ChannelName
        {
            get => _channelName;
            set => _channelName = value;
        }

        /// <summary>
        /// 部门代码
        /// </summary>
        public string? DepartmentId
        {
            get => _departmentId;
            set => _departmentId = value;
        }

        /// <summary>
        /// 部门名称
        /// </summary>
        public string? DepartmentName
        {
            get => _departmentName;
            set => _departmentName = value;
        }

        /// <summary>
        /// 上级部门代码
        /// </summary>
        public string? PDepartmentId
        {
            get => _pDepartmentId;
            set => _pDepartmentId = value;
        }

        /// <summary>
        /// 上级部门名称
        /// </summary>
        public string? PDepartmentName
        {
            get => _pDepartmentName;
            set => _pDepartmentName = value;
        }

        /// <summary>
        /// 设备网络类型
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public DeviceNetworkType? DeviceNetworkType
        {
            get => _deviceNetworkType;
            set => _deviceNetworkType = value;
        }
       
        /// <summary>
        /// 设备类型
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public VideoDeviceType? VideoDeviceType
        {
            get => _videoDeviceType;
            set => _videoDeviceType = value;
        }

        /// <summary>
        /// 是否自动推拉流
        /// </summary>
        public bool? AutoVideo
        {
            get => _autoVideo;
            set => _autoVideo = value;
        }

        /// <summary>
        /// 是否自动录制（有录制计划时配合录制计划）
        /// </summary>
        public bool? AutoRecord
        {
            get => _autoRecord;
            set => _autoRecord = value;
        }

        /// <summary>
        /// ipv4地址
        /// </summary>
        public string? IpV4Address
        {
            get => _ipV4Address;
            set => _ipV4Address = value;
        }

        /// <summary>
        /// ipv6地址
        /// </summary>
        public string? IpV6Address
        {
            get => _ipV6Address;
            set => _ipV6Address = value;
        }

        /// <summary>
        /// 是否有ptz控制
        /// </summary>
        public bool? HasPtz
        {
            get => _hasPtz;
            set => _hasPtz = value;
        }
        

        /// <summary>
        /// gb28181是否采用tcp推流
        /// </summary>
        public bool? RtpWithTcp
        {
            get => _rtpWithTcp;
            set => _rtpWithTcp = value;
        }

     
        /// <summary>
        /// 是否使用默认rtp端口
        /// </summary>
        public bool? DefaultRtpPort
        {
            get => _defaultRtpPort;
            set => _defaultRtpPort = value;
        }

        /// <summary>
        /// 没有观察人时是否断开推拉流
        /// </summary>
        public bool? NoPlayerBreak
        {
            get => _noPlayerBreak;
            set => _noPlayerBreak = value;
        }
    }
}