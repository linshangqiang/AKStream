using System;

namespace LibGB28181SipServer
{
    [Serializable]
    /// <summary>
    /// sip服务器配置
    /// </summary>
    public class SipServerConfig
    {
        private string _sipIpAddress;
        private string? _sipIpV6Address;
        private string _msgEncode;
        private string _serverSipDeviceId;
        private ushort _sipPort;
        private string _gbVersion;
        private bool _authentication;
        private string? _sipUsername;
        private string? _sipPassword;
        private string _msgProtocol;
        private int _keepAliveInterval;
        private int _keepAliveLostNumber;
        private bool ipV6enable=false;
        

        /// <summary>
        /// sip服务器ip地址
        /// </summary>
        public string SipIpAddress
        {
            get => _sipIpAddress;
            set => _sipIpAddress = value;
        }

        /// <summary>
        /// ipv6地址
        /// </summary>
        public string? SipIpV6Address
        {
            get => _sipIpV6Address;
            set => _sipIpV6Address = value;
        }

        /// <summary>
        /// sip服务中消息的编码格式
        /// </summary>
        public string MsgEncode
        {
            get => _msgEncode;
            set => _msgEncode = value;
        }

        /// <summary>
        /// sip服务器id
        /// </summary>
        public string ServerSipDeviceId
        {
            get => _serverSipDeviceId;
            set => _serverSipDeviceId = value;
        }

        /// <summary>
        /// sip服务端口
        /// </summary>
        public ushort SipPort
        {
            get => _sipPort;
            set => _sipPort = value;
        }

        /// <summary>
        /// sip执行的版本
        /// </summary>
        public string GbVersion
        {
            get => _gbVersion;
            set => _gbVersion = value;
        }

        /// <summary>
        /// 是否启用认证
        /// </summary>
        public bool Authentication
        {
            get => _authentication;
            set => _authentication = value;
        }

        /// <summary>
        /// sip用户名
        /// </summary>
        public string? SipUsername
        {
            get => _sipUsername;
            set => _sipUsername = value;
        }

        /// <summary>
        /// sip密码
        /// </summary>
        public string? SipPassword
        {
            get => _sipPassword;
            set => _sipPassword = value;
        }

        /// <summary>
        /// sip消息使用的协议
        /// </summary>
        public string MsgProtocol
        {
            get => _msgProtocol;
            set => _msgProtocol = value;
        }

        /// <summary>
        /// 心跳保持周期
        /// </summary>
        public int KeepAliveInterval
        {
            get => _keepAliveInterval;
            set => _keepAliveInterval = value;
        }

        /// <summary>
        /// 多少次心跳丢失后算该设备下线
        /// </summary>
        public int KeepAliveLostNumber
        {
            get => _keepAliveLostNumber;
            set => _keepAliveLostNumber = value;
        }

        /// <summary>
        /// ipv6使能
        /// </summary>
        public bool IpV6Enable
        {
            get => ipV6enable;
            set => ipV6enable = value;
        }
    }
}