using System;
using System.Collections.Generic;
using System.Net;
using System.Timers;
using GB28181.Sys.XML;
using Newtonsoft.Json;
using SIPSorcery.SIP;

namespace LibGB28181SipServer
{
    [Serializable]
    public class SipDevice : IDisposable
    {
        private string? _guid;
        private IPAddress? _ipAddress;
        private int _port;
        private string _deviceId=null!;
        private SIPEndPoint? _remoteEndPoint;
        private SIPEndPoint? _localSipEndPoint;
        private List<SipChannel>? _sipChannels=new List<SipChannel>();
        private DeviceInfo? _deviceInfo=new DeviceInfo();
        private DateTime _registerTime;
        private string? _username;
        private string? _password;
        private DateTime _keepAliveTime;
        private int _keepAliveLostTime;
        private Timer _keepAliveCheckTimer = null;
        private SIPRequest? _firstSipRequest;
        private SIPURI? _contactUri;
        private SIPChannel? _sipChannelLayout;

        public event Common.DoKickSipDevice KickMe = null!;

        
        /// <summary>
        /// 对sip通道操作时的锁
        /// </summary>
        [JsonIgnore]
        public  object SipChannelOptLock = new object();
        public void Dispose()
        {
            
            if (_keepAliveCheckTimer != null)
            {
                _keepAliveCheckTimer.Dispose();
                _keepAliveCheckTimer = null!;
            }
        }

        ~SipDevice()
        {
            Dispose(); //释放非托管资源
        }

        /// <summary>
        /// 设备在系统中唯一id
        /// </summary>
        public string? Guid
        {
            get => _guid;
            set => _guid = value;
        }

        /// <summary>
        /// 设备ID
        /// </summary>
        public string DeviceId
        {
            get => _deviceId;
            set => _deviceId = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 设备ip地址
        /// </summary>
        public IPAddress? IpAddress
        {
            get => _ipAddress;
            set => _ipAddress = value;
        }

        /// <summary>
        /// 设备端口
        /// </summary>
        public int Port
        {
            get => _port;
            set => _port = value;
        }


       
        /// <summary>
        /// sip设备ip端口协议
        /// </summary>
        [JsonIgnore]
        public SIPEndPoint? RemoteEndPoint
        {
            get => _remoteEndPoint;
            set => _remoteEndPoint = value;
        }
       
        /// <summary>
        /// sip服务ip端口协议
        /// </summary>
        [JsonIgnore]
        public SIPEndPoint? LocalSipEndPoint
        {
            get => _localSipEndPoint;
            set => _localSipEndPoint = value;
        }

       

        /// <summary>
        /// 设备所属通道信息
        /// </summary>
        public List<SipChannel>? SipChannels
        {
            get => _sipChannels;
            set => _sipChannels = value;
        }

        /// <summary>
        /// 设备信息
        /// </summary>
        public DeviceInfo? DeviceInfo
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

        public SipDevice()
        {
            _keepAliveCheckTimer = new Timer(Common.SipServerConfig.KeepAliveInterval * 1000);
            _keepAliveCheckTimer.Enabled = true; //启动Elapsed事件触发
            _keepAliveCheckTimer.Elapsed += OnTimedEvent; //添加触发事件的函数
            _keepAliveCheckTimer.AutoReset = true; //需要自动reset
            _keepAliveCheckTimer.Start(); //启动计时器
        }
        
        /// <summary>
        /// 注册时的sipRequest
        /// </summary>

        [JsonIgnore]
        public SIPRequest? FirstSipRequest
        {
            get => _firstSipRequest;
            set => _firstSipRequest = value;
        }

       
        /// <summary>
        /// 注册时候的uri
        /// </summary>
        [JsonIgnore]
        public SIPURI? ContactUri
        {
            get => _contactUri;
            set => _contactUri = value;
        }

       
        /// <summary>
        /// 设备所在的Sip通道实例
        /// </summary>
        [JsonIgnore]
        public SIPChannel? SipChannelLayout
        {
            get => _sipChannelLayout;
            set => _sipChannelLayout = value;
        }

        /// <summary>
        /// 定时触发，检查心跳周期并计算失去多少次心跳
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if ((DateTime.Now - _keepAliveTime).Seconds > Common.SipServerConfig.KeepAliveInterval + 1)
            {
                _keepAliveLostTime++;
            }
            else
            {
                _keepAliveLostTime--;
                if (_keepAliveLostTime < 0)
                {
                    _keepAliveLostTime = 0;
                }
            }

            //做踢除
            if (_keepAliveLostTime >= Common.SipServerConfig.KeepAliveLostNumber)
            {
                if (KickMe != null)
                {
                    _keepAliveCheckTimer.Enabled = false;
                    _keepAliveCheckTimer.Stop();
                    KickMe?.Invoke(_guid!);
                }
            }
        }
    }
}