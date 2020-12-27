using System;
using FreeSql.DataAnnotations;
using LibCommon.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LibCommon.Structs.DBModels
{
    /// <summary>
    /// 摄像头通道实例
    /// </summary>
    [Serializable]
    [Table(Name = "VideoChannels")]
    [Index("idx_vcs_maid", "MainId", true)]
    [Index("idx_vcs_chnn", "ChannelName", false)]
    [Index("idx_vcs_msid", "MediaServerId", false)]
    [Index("idx_vcs_dept", "DepartmentId", false)]
    [Index("idx_vcs_ipv4", "IpV4Address", false)]
    [Index("idx_vcs_ipv6", "IpV6Address", false)]
    [Index("idx_vcs_enbl", "Enabled", false)]
    public class VideoChannels
    {
        private long _id;
        private string _mainId;
        private string _mediaServerId;
        private string? _channelName;
        private string? _departmentId;
        private string? _departmentName;
        private string? _pDepartmentId;
        private string? _pDepartmentName;
        private DeviceNetworkType _deviceNetworkType;
        private DeviceStreamType _deviceStreamType;
        private MethodByGetStream _methodByGetStream;
        private bool _autoVideo;
        private bool _autoRecord;
        private string _ipV4Address;
        private string? _ipV6Address;
        private bool _hasPtz;
        private string? _deviceId;
        private string? _channelId;
        private bool? _rtpWithTcp;
        private string? _videoSrcUrl;
        private bool? _defaultRtpPort;
        private DateTime _createTime;
        private DateTime _updateTime;
        private bool? _enabled;
        private bool? _noPlayerBreak;


        /// <summary>
        /// 数据库主键
        /// </summary>
        [Column(IsPrimary = true, IsIdentity = true)]
        public long Id
        {
            get => _id;
            set => _id = value;
        }

        /// <summary>
        /// 设备的唯一ID
        /// </summary>
        [Column(DbType = "varchar(32) NOT NULL")]
        public string MainId
        {
            get => _mainId;
            set => _mainId = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 流媒体服务器ID
        /// </summary>
        [Column(DbType = "varchar(64) NOT NULL")]
        public string MediaServerId
        {
            get => _mediaServerId;
            set => _mediaServerId = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 通道名称，整个系统唯一
        /// </summary>
        [Column(DbType = "varchar(64)")]
        public string? ChannelName
        {
            get => _channelName;
            set => _channelName = value;
        }

        /// <summary>
        /// 部门代码
        /// </summary>
        [Column(DbType = "varchar(20)")]
        public string? DepartmentId
        {
            get => _departmentId;
            set => _departmentId = value;
        }

        /// <summary>
        /// 部门名称
        /// </summary>
        [Column(DbType = "varchar(64)")]
        public string? DepartmentName
        {
            get => _departmentName;
            set => _departmentName = value;
        }

        /// <summary>
        /// 上级部门代码
        /// </summary>
        [Column(DbType = "varchar(20)")]
        public string? PDepartmentId
        {
            get => _pDepartmentId;
            set => _pDepartmentId = value;
        }

        /// <summary>
        /// 上级部门名称
        /// </summary>
        [Column(DbType = "varchar(64)")]
        public string? PDepartmentName
        {
            get => _pDepartmentName;
            set => _pDepartmentName = value;
        }


        /// <summary>
        /// 设备的网络类型
        /// </summary>
        [Column(MapType = typeof(string))]
        [JsonConverter(typeof(StringEnumConverter))]
        public DeviceNetworkType DeviceNetworkType
        {
            get => _deviceNetworkType;
            set => _deviceNetworkType = value;
        }

        /// <summary>
        /// 设备的流类型
        /// </summary>
        [Column(MapType = typeof(string))]
        [JsonConverter(typeof(StringEnumConverter))]
        public DeviceStreamType DeviceStreamType
        {
            get => _deviceStreamType;
            set => _deviceStreamType = value;
        }

        /// <summary>
        /// 使用哪种方式拉取非rtp设备的流
        /// </summary>
        [Column(MapType = typeof(string))]
        [JsonConverter(typeof(StringEnumConverter))]
        public MethodByGetStream MethodByGetStream
        {
            get => _methodByGetStream;
            set => _methodByGetStream = value;
        }

        /// <summary>
        /// 是否自动启用推拉流
        /// </summary>
        public bool AutoVideo
        {
            get => _autoVideo;
            set => _autoVideo = value;
        }

        /// <summary>
        /// 是否自动启用录制计划
        /// </summary>
        public bool AutoRecord
        {
            get => _autoRecord;
            set => _autoRecord = value;
        }

        /// <summary>
        /// 设备的ipv4地址
        /// </summary>
        [Column(DbType = "varchar(16)")]
        public string IpV4Address
        {
            get => _ipV4Address;
            set => _ipV4Address = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 设备的ipv6地址
        /// </summary>
        [Column(DbType = "varchar(64)")]
        public string? IpV6Address
        {
            get => _ipV6Address;
            set => _ipV6Address = value;
        }

        /// <summary>
        /// 设备是否有云台控制
        /// </summary>
        public bool HasPtz
        {
            get => _hasPtz;
            set => _hasPtz = value;
        }

        /// <summary>
        /// GB28181设备的SipDevice.DeviceId
        /// </summary>
        [Column(DbType = "varchar(20)")]
        public string? DeviceId
        {
            get => _deviceId;
            set => _deviceId = value;
        }

        /// <summary>
        /// GB28181设备的SipChannel.DeviceId
        /// </summary>
        [Column(DbType = "varchar(20)")]
        public string? ChannelId
        {
            get => _channelId;
            set => _channelId = value;
        }

        /// <summary>
        /// Rtp设备是否使用Tcp推流
        /// </summary>
        public bool? RtpWithTcp
        {
            get => _rtpWithTcp;
            set => _rtpWithTcp = value;
        }

        /// <summary>
        /// 非Rtp设备的视频流源地址
        /// </summary>
        [Column(DbType = "varchar(255)")]
        public string? VideoSrcUrl
        {
            get => _videoSrcUrl;
            set => _videoSrcUrl = value;
        }

        /// <summary>
        /// Rtp设备是否使用流媒体默认rtp端口，如10000端口
        /// </summary>
        public bool? DefaultRtpPort
        {
            get => _defaultRtpPort;
            set => _defaultRtpPort = value;
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime
        {
            get => _createTime;
            set => _createTime = value;
        }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime
        {
            get => _updateTime;
            set => _updateTime = value;
        }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool? Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }


        /// <summary>
        /// 无人观察时断开流端口，此字段为true时AutoVideo字段必须为Flase
        /// 如果AutoVideo为true,则此字段无效
        /// </summary>
        public bool? NoPlayerBreak
        {
            get => _noPlayerBreak;
            set => _noPlayerBreak = value;
        }
    }
}