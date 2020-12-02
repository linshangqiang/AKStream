using System;
using System.Collections.Generic;
using GB28181.Sys.XML;
using SIPSorcery.SIP;

namespace LibGB28181SipServer
{
    public class SipDevice
    {
        private string _guid;
        private SIPEndPoint _remoteEndPoint;
        private SIPEndPoint _localSipEndPoint;
        private string? _ipAddress;
        private List<SipChannel> _sipChannels;
        private DeviceInfo _deviceInfo;
        private DateTime _registerTime;
        private string? _username;
        private string? _password;
        private DateTime _keepAliveTime;
        private int _keepAliveLostTime;

        /// <summary>
        /// 设备在系统中唯一id
        /// </summary>
        public string Guid
        {
            get => _guid;
            set => _guid = value;
        }

        /// <summary>
        /// sip设备ip端口协议
        /// </summary>
        public SIPEndPoint RemoteEndPoint
        {
            get => _remoteEndPoint;
            set => _remoteEndPoint = value;
        }

        /// <summary>
        /// sip服务ip端口协议
        /// </summary>
        public SIPEndPoint LocalSipEndPoint
        {
            get => _localSipEndPoint;
            set => _localSipEndPoint = value;
        }

        /// <summary>
        /// 设备ip地址
        /// </summary>
        public string? IpAddress
        {
            get => _ipAddress;
            set => _ipAddress = value;
        }

        /// <summary>
        /// 设备所属通道信息
        /// </summary>
        public List<SipChannel> SipChannels
        {
            get => _sipChannels;
            set => _sipChannels = value;
        }

        /// <summary>
        /// 设备信息
        /// </summary>
        public DeviceInfo DeviceInfo
        {
            get => _deviceInfo;
            set => _deviceInfo = value;
        }

        /// <summary>
        /// 注册时间
        /// </summary>
        public DateTime RegisterTime
        {
            get => _registerTime;
            set => _registerTime = value;
        }

        /// <summary>
        /// 注册用户名
        /// </summary>
        public string? Username
        {
            get => _username;
            set => _username = value;
        }

        /// <summary>
        /// 注册密码
        /// </summary>
        public string? Password
        {
            get => _password;
            set => _password = value;
        }

        /// <summary>
        /// 最后一次心跳时间
        /// </summary>
        public DateTime KeepAliveTime
        {
            get => _keepAliveTime;
            set => _keepAliveTime = value;
        }

        /// <summary>
        /// 心跳已连续丢失多少次
        /// </summary>
        public int KeepAliveLostTime
        {
            get => _keepAliveLostTime;
            set => _keepAliveLostTime = value;
        }
    }
}