using System;
using System.Threading;
using LibCommon;
using LibCommon.Enums;
using LibCommon.Structs;
using LibCommon.Structs.GB28181;
using LibCommon.Structs.GB28181.XML;
using LibGB28181SipServer;
using LibLogger;
using Newtonsoft.Json;
using SipServer = LibGB28181SipServer.SipServer;

namespace Test_SipGate
{
    class Program
    {
        public static void OnRegister(string sipDeviceJson)
        {
         //   Console.WriteLine("=================设备注册了->" + sipDeviceJson);
        }

        public static void OnUnRegister(string sipDeviceJson)
        {
           // Console.WriteLine("=================设备注销了->" + sipDeviceJson);
        }

        public static void OnKeepalive(string deviceId, DateTime keepAliveTime, int lostTimes)
        {
            /*
            Console.WriteLine("=================收到了设备的心跳->" + deviceId + " 心跳时间：" +
                              keepAliveTime.ToString("yyyy-MM-dd HH:mm:ss" + " 失去的心跳次数：" + lostTimes));
        */
        }

        public static void OnDeviceStatusReceived(SipDevice sipDevice, DeviceStatus deviceStatus)
        {
         //   Console.WriteLine("=================收到设备状态信息->" + sipDevice.DeviceId+"->\r\n"+JsonHelper.ToJson(deviceStatus,Formatting.Indented));  
        }

        public static void OnInviteHistoryVideoFinished(RecordInfo.Item record)
        {
            Console.WriteLine("=================收到回放流结束通知->\r\n" + JsonHelper.ToJson(record,Formatting.Indented));  
        }

        public static void OnDeviceReadyReceived(SipDevice sipDevice)
        {
            ResponseStruct rs;
          //  Console.WriteLine("=================收到设备就绪通知->" + sipDevice.DeviceId);
            SipMethodProxy sipMethodProxy = new SipMethodProxy(5000);
            if (sipMethodProxy.DeviceCatalogQuery(sipDevice, out rs))
            {
                Console.WriteLine("=================设备目录获取成功->" + JsonHelper.ToJson(sipDevice.SipChannels,Formatting.Indented));
            }
            else
            {
                Console.WriteLine("=================设备目录获取失败->" + sipDevice.DeviceId+"\r\n"+JsonHelper.ToJson(rs,Formatting.Indented)); 
 
            }

            SipMethodProxy sipMethodProxy2 = new SipMethodProxy(5000);
            if (sipMethodProxy2.GetSipDeviceInfo(sipDevice, out rs))
            {
                Console.WriteLine("=================获取设备信息成功->" + sipDevice.DeviceId);
            }
            else
            {
                Console.WriteLine("=================获取设备信息失败->" + sipDevice.DeviceId+"\r\n"+JsonHelper.ToJson(rs,Formatting.Indented)); 

            }

            SipMethodProxy sipMethodProxy3 = new SipMethodProxy(5000);
            if (sipMethodProxy3.GetSipDeviceStatus(sipDevice, out rs))
            {
                Console.WriteLine("=================获取设备状态信息成功->" + sipDevice.DeviceId);
            }
            else
            {
                 Console.WriteLine("=================获取设备状态信息失败->" + sipDevice.DeviceId+"\r\n"+JsonHelper.ToJson(rs,Formatting.Indented)); 
            }
        }

        static void Main(string[] args)
        {
            try
            {
#if (DEBUG)


                Console.WriteLine("[Debug]\t当前程序为Debug编译模式");
                Console.WriteLine("[Debug]\t程序启动路径:" + GCommon.BaseStartPath);
                Console.WriteLine("[Debug]\t程序启动全路径:" + GCommon.BaseStartFullPath);
                Console.WriteLine("[Debug]\t程序运行路径:" + GCommon.WorkSpacePath);
                Console.WriteLine("[Debug]\t程序运行全路径:" + GCommon.WorkSpaceFullPath);
                Console.WriteLine("[Debug]\t程序启动命令:" + GCommon.CommandLine);

#endif

                SipServer sipServer = new SipServer();

                SipMsgProcess.OnRegisterReceived += OnRegister;
                SipMsgProcess.OnUnRegisterReceived += OnUnRegister;
                SipMsgProcess.OnKeepaliveReceived += OnKeepalive;
                SipMsgProcess.OnDeviceReadyReceived += OnDeviceReadyReceived;
                SipMsgProcess.OnDeviceStatusReceived += OnDeviceStatusReceived;
                SipMsgProcess.OnInviteHistoryVideoFinished += OnInviteHistoryVideoFinished;
                ResponseStruct rs;
                try
                {
                    sipServer.Start(out rs);
                    if (rs.Code != ErrorNumber.None)
                    {
                        Logger.Info(JsonHelper.ToJson(rs, Formatting.Indented));
                    }


                    while (true)
                    {
                        string cmd = Console.ReadLine();
                        if (string.IsNullOrEmpty(cmd))
                        {
                            continue;
                        }

                        string[] aa = cmd.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                        string type = aa[0];
                        switch (type)
                        {
                            case "M":
                                SipDevice sipDevice2 = null;
                                SipChannel sipChannel2 = null;
                                PTZCommandType ptzCmd = PTZCommandType.UnKnow;
                                PtzCtrl ptzCtrl = null;
                                int ptzSpeed = 0;

                                if (aa.Length == 4)
                                {
                                    //只有dev
                                    var devid = aa[1];
                                    sipDevice2 = Common.SipDevices.FindLast(x => x.DeviceId.Equals(devid));
                                    ptzCmd = (PTZCommandType) Enum.Parse(typeof(PTZCommandType), aa[2], true);
                                    ptzSpeed = int.Parse(aa[3]);
                                    ptzCtrl = new PtzCtrl()
                                    {
                                        PtzCommandType = ptzCmd,
                                        SipDevice = sipDevice2,
                                        SipChannel = null,
                                        Speed = ptzSpeed,
                                    };
                                }

                                if (aa.Length == 5)
                                {
                                    var devid = aa[1];
                                    sipDevice2 = Common.SipDevices.FindLast(x => x.DeviceId.Equals(devid));
                                    var chid = aa[2];
                                    sipChannel2 = sipDevice2.SipChannels.FindLast(x => x.DeviceId.Equals(chid));
                                    ptzCmd = (PTZCommandType) Enum.Parse(typeof(PTZCommandType), aa[3], true);
                                    ptzSpeed = int.Parse(aa[4]);
                                    ptzCtrl = new PtzCtrl()
                                    {
                                        PtzCommandType = ptzCmd,
                                        SipDevice = sipDevice2,
                                        SipChannel = sipChannel2,
                                        Speed = ptzSpeed,
                                    };
                                    //dev和channel
                                }


                                SipMethodProxy sipMethodProxy2 = new SipMethodProxy(5000);
                                var ret3 = sipMethodProxy2.PtzMove(ptzCtrl, out rs);
                                if (ret3 && rs.Code == ErrorNumber.None)
                                {
                                    Logger.Info("----------------------------------------命令->请求PTZ控制成功成功");
                                }
                                else
                                {
                                    Logger.Info("----------------------------------------命令->请求PTZ控制成功失败");
                                }

                                break;
                            case "F":
                                if (aa.Length > 2)
                                {
                                    Logger.Info("命令->查询设备录像文件目录" + " cmd:" + type + " " + "args:" + aa[1]);

                                    var sipDevice = Common.SipDevices.FindLast(x => x.DeviceId.Equals(aa[1]));
                                    if (sipDevice == null)
                                    { Logger.Info("----------------------------------------命令->请求获取设备录像文件列表时发现设备不存在");
                                        break;
                                    }
                                    var sipChannel = sipDevice.SipChannels.FindLast(x => x.DeviceId.Equals(aa[2]));
                                    if (sipDevice != null && sipChannel != null)
                                    {
                                        SipQueryRecordFile sipQueryRecordFile = new SipQueryRecordFile();
                                        sipQueryRecordFile.SipRecordFileQueryType = SipRecordFileQueryType.all;
                                        sipQueryRecordFile.StartTime = DateTime.Parse("2020-12-12 22:00:00");
                                        sipQueryRecordFile.EndTime =  DateTime.Parse("2020-12-13 23:00:00");
                                        SipMethodProxy sipMethodProxy = new SipMethodProxy(5000);
                                        var ret = sipMethodProxy.QueryRecordFileList(sipChannel, sipQueryRecordFile,
                                            out rs);
                                        if (ret)
                                        {
                                            Logger.Info("----------------------------------------命令->请求获取设备录像文件列表成功");
                                         
                                            Logger.Info("----------------------------------------查询收到的录像文件列表\r\n" +
                                                         JsonHelper.ToJson(sipChannel.GetLastRecordInfoList(OrderBy.DESC), Formatting.Indented));
                                        }
                                        else
                                        {
                                            Logger.Info("----------------------------------------命令->请求获取设备录像文件列表失败");
                                        }
                                    }
                                }

                                break;
                            case "K":
                                type = "K";
                                // aa = new string[] {"K", "34020000002220000001","34020000001370000008"};
                                if (aa.Length > 2)
                                {
                                    Logger.Info("命令->查询设备目录" + " cmd:" + type + " " + "args:" + aa[1]);

                                    var sipDevice = Common.SipDevices.FindLast(x => x.DeviceId.Equals(aa[1]));
                                    var sipChannel = sipDevice.SipChannels.FindLast(x => x.DeviceId.Equals(aa[2]));
                                    if (sipDevice != null && sipChannel != null)
                                    {
                                        SipMethodProxy sipMethodProxy = new SipMethodProxy(5000);
                                        var ret = sipMethodProxy.DeInvite(sipChannel, out rs);
                                        if (ret)
                                        {
                                            Logger.Info("----------------------------------------命令->请求终止实时流成功");
                                        }
                                        else
                                        {
                                            Logger.Info("----------------------------------------命令->请求终止实时流失败"+JsonHelper.ToJson(rs,Formatting.Indented));
                                        }
                                    }
                                }

                                break;
                            case "A":
                                type = "C";
                                aa = new string[] {"C", "34020000002220000001"};
                                if (aa.Length > 1)
                                {
                                    Logger.Info("命令->查询设备目录" + " cmd:" + type + " " + "args:" + aa[1]);
                                    var sipDevice = Common.SipDevices.FindLast(x => x.DeviceId.Equals(aa[1]));
                                    if (sipDevice != null)
                                    {
                                        SipMethodProxy sipMethodProxy = new SipMethodProxy(5000);
                                        var ret = sipMethodProxy.DeviceCatalogQuery(sipDevice, out rs);
                                        if (ret)
                                        {
                                            Logger.Info("----------------------------------------命令->查询设备目录成功");
                                        }
                                        else
                                        {
                                            Logger.Info("----------------------------------------命令->查询设备目录失败");
                                        }
                                    }
                                }

                                break;
                           
                            case "EXIT":
                                return;
                            case "L":
                                Logger.Info("-----------------" + JsonHelper.ToJson(Common.SipDevices));
                                break;
                            case "C":

                                foreach (var s in aa)
                                {
                                    Logger.Info(s);
                                }

                                Logger.Info("aa 长度：" + aa.Length);


                                if (aa.Length > 1)
                                {
                                    Logger.Info("命令->查询设备目录" + " cmd:" + type + " " + "args:" + aa[1]);
                                    var sipDevice = Common.SipDevices.FindLast(x => x.DeviceId.Equals(aa[1]));
                                    if (sipDevice != null)
                                    {
                                        SipMethodProxy sipMethodProxy = new SipMethodProxy(5000);
                                        var ret = sipMethodProxy.DeviceCatalogQuery(sipDevice, out rs);
                                        if (ret)
                                        {
                                            Logger.Info("----------------------------------------命令->查询设备目录成功");
                                        }
                                        else
                                        {
                                            Logger.Info("----------------------------------------命令->查询设备目录失败");
                                        }
                                    }
                                }

                                break;
                            case "V":

                                foreach (var s in aa)
                                {
                                    Logger.Info(s);
                                }

                                Logger.Info("aa 长度：" + aa.Length);


                                if (aa.Length > 3)
                                {
                                    Logger.Info("命令->推流命令" + " cmd:" + type + " " + "args:" + aa[1]);

                                    var sipDevice = Common.SipDevices.FindLast(x => x.DeviceId.Equals(aa[1]));
                                    var sipChannel = sipDevice.SipChannels.FindLast(x => x.DeviceId.Equals(aa[2]));
                                    var sipServerIp = aa[3];
                                    if (sipDevice != null && sipChannel != null)
                                    {
                                        PushMediaInfo pushMediaInfo = new PushMediaInfo();
                                        pushMediaInfo.StreamPort = 10000;
                                        pushMediaInfo.MediaServerIpAddress = sipServerIp;
                                        pushMediaInfo.PushStreamSocketType = PushStreamSocketType.UDP;
                                        SipMethodProxy sipMethodProxy = new SipMethodProxy(5000);
                                        var ret = sipMethodProxy.Invite(sipChannel, pushMediaInfo, out rs);
                                        if (ret)
                                        {
                                            Logger.Info("----------------------------------------命令->请求实时流成功"); 
                                           
                                        }
                                        else
                                        {
                                            Logger.Info("----------------------------------------命令->请求实时流失败\r\n" +
                                                         JsonHelper.ToJson(rs));
                                        }
                                    }
                                }

                                break;
                            case "I":
                                if (aa.Length > 4)
                                {
                                   
                                    //I 34020000001110000001 34020000001320000008 1323362910 192.168.2.43
                                    string sipdeviceid = aa[1];
                                    string sipchaid = aa[2];
                                    string recordid = aa[3];
                                    string medip = aa[4];
                                    var tmpDevSip = Common.SipDevices.FindLast(x => x.DeviceId.Equals(sipdeviceid));
                                    if (tmpDevSip != null)
                                    {
                                        var sipchann = tmpDevSip.SipChannels.FindLast(x => x.DeviceId.Equals(sipchaid));
                                        if (sipchann != null)
                                        {
                                            var record =
                                                sipchann.LastRecordInfos.FindLast(x => x.Value.SsrcId.Equals(recordid));
                                            if (record.Value != null)
                                            {
                                                PushMediaInfo pushMediaInfo = new PushMediaInfo();
                                                pushMediaInfo.StreamPort = 10000;
                                                pushMediaInfo.MediaServerIpAddress = medip;
                                                pushMediaInfo.PushStreamSocketType = PushStreamSocketType.UDP;
                                                SipMethodProxy sipMethodProxy = new SipMethodProxy(5000);
                                                var ret = sipMethodProxy.InviteRecord(record.Value, pushMediaInfo, out rs);
                                                if (ret)
                                                {
                                                    Logger.Info("----------------------------------------命令->请求回放流成功"); 
                                           
                                                }
                                                else
                                                {
                                                    Logger.Info("----------------------------------------命令->请求回放流失败\r\n" +
                                                                JsonHelper.ToJson(rs));
                                                } 
                                            }
                                            else
                                            {
                                                Logger.Info("----------------------------------------命令->请求回放流时发生录像文件不存在");   
                                            }
                                        }
                                        else
                                        {
                                            Logger.Info("----------------------------------------命令->请求回放流时发生sip通道不存在");
                                        }
                                    }
                                    else
                                    {
                                        Logger.Info("----------------------------------------命令->请求回放流时发生sip设备不存在");
                                    }
                                }

                                break;
                            case "KI":
                                if (aa.Length > 4)
                                {
                                   
                                    //I 34020000001110000001 34020000001320000008 1323362910 192.168.2.43
                                    string sipdeviceid = aa[1];
                                    string sipchaid = aa[2];
                                    string recordid = aa[3];
                                    string medip = aa[4];
                                    var tmpDevSip = Common.SipDevices.FindLast(x => x.DeviceId.Equals(sipdeviceid));
                                    if (tmpDevSip != null)
                                    {
                                        var sipchann = tmpDevSip.SipChannels.FindLast(x => x.DeviceId.Equals(sipchaid));
                                        if (sipchann != null)
                                        {
                                            var record =
                                                sipchann.LastRecordInfos.FindLast(x => x.Value.SsrcId.Equals(recordid));
                                            if (record.Value != null)
                                            {
                                             
                                                SipMethodProxy sipMethodProxy = new SipMethodProxy(5000);
                                                var ret = sipMethodProxy.DeInvite(record.Value, out rs);
                                                if (ret)
                                                {
                                                    Logger.Info("----------------------------------------命令->请求终止回放流成功"); 
                                           
                                                }
                                                else
                                                {
                                                    Logger.Info("----------------------------------------命令->请求终止回放流失败\r\n" +
                                                                JsonHelper.ToJson(rs));
                                                } 
                                            }
                                            else
                                            {
                                                Logger.Info("----------------------------------------命令->请求终止回放流时发生录像文件不存在");   
                                            }
                                        }
                                        else
                                        {
                                            Logger.Info("----------------------------------------命令->请求终止回放流时发生sip通道不存在");
                                        }
                                    }
                                    else
                                    {
                                        Logger.Info("----------------------------------------命令->请求终止回放流时发生sip设备不存在");
                                    }
                                }

                                break;
                        }
                    }
                }
                catch (AkStreamException ex)
                {
                    Logger.Error(JsonHelper.ToJson(ex, Formatting.Indented));
                }

                Console.WriteLine("Hello World!");
            }
            catch (AkStreamException ex)
            {
                Console.WriteLine(JsonHelper.ToJson(ex, Formatting.Indented));
            }
        }
    }
}