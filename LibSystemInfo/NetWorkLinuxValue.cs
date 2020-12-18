using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using LibCommon;
using LibCommon.Structs;

namespace LibSystemInfo
{
    public static class NetWorkLinuxValue
    {
        private static object lockObj = new object();
        private static string ethName = "";

        public static void GetInfo()
        {
            while (true)
            {
                if (string.IsNullOrEmpty(ethName))
                {
                    Thread.Sleep(1000);
                    continue;
                }

                var lines = File.ReadAllLines("/proc/net/dev");
                if (lines.Length > 0)
                {
                    foreach (var str in lines)
                    {
                        if (str.Contains(ethName))
                        {
                            string[] strTmpArr = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            if (strTmpArr.Length > 0)
                            {
                                long.TryParse(strTmpArr[1], out var tmpRecv);
                                long.TryParse(strTmpArr[9], out var tmpSend);

                                if (tmpRecv > 0 && tmpSend > 0)
                                {
                                    if (_perRecvBytes == 0 && _perSendBytes == 0)
                                    {
                                        lock (lockObj)
                                        {
                                            _perRecvBytes = tmpRecv;
                                            _perSendBytes = tmpSend;
                                            NetWorkStat.CurrentRecvBytes = 0;
                                            NetWorkStat.CurrentSendBytes = 0;
                                            NetWorkStat.TotalRecvBytes = 0;
                                            NetWorkStat.TotalSendBytes = 0;
                                            NetWorkStat.UpdateTime = DateTime.Now;
                                        }
                                    }
                                    else
                                    {
                                        lock (lockObj)
                                        {
                                            NetWorkStat.CurrentRecvBytes = tmpRecv - _perRecvBytes;
                                            NetWorkStat.CurrentSendBytes = tmpSend - _perSendBytes;
                                            _perRecvBytes += NetWorkStat.CurrentRecvBytes;
                                            _perSendBytes += NetWorkStat.CurrentSendBytes;
                                            NetWorkStat.TotalRecvBytes += NetWorkStat.CurrentRecvBytes;
                                            NetWorkStat.TotalSendBytes += NetWorkStat.CurrentSendBytes;
                                            NetWorkStat.UpdateTime = DateTime.Now;
                                        }
                                    }
                                }
                            }

                            break;
                        }
                    }
                }

                Thread.Sleep(1000);
            }
        }

        static NetWorkLinuxValue()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                ProcessHelper tmpProcess = new ProcessHelper(null!, null!, null!);
                tmpProcess.RunProcess("/usr/sbin/route", "", 1000, out string std, out string err);
                bool isFound = false;
                if (!string.IsNullOrEmpty(std))
                {
                    string[] tmpStrArr = std.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    if (tmpStrArr.Length > 0)
                    {
                        foreach (var str in tmpStrArr)
                        {
                            if (isFound)
                            {
                                break;
                            }

                            if (!string.IsNullOrEmpty(str) && str.ToLower().Contains("default"))
                            {
                                string[] s1Arr = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                if (s1Arr.Length > 0)
                                {
                                    string str1 = s1Arr[s1Arr.Length - 1].Trim();
                                    if (!string.IsNullOrEmpty(str1))
                                    {
                                        ethName = str1;
                                        tmpProcess.RunProcess("/usr/sbin/ifconfig", str1, 1000, out string std1,
                                            out string err1);

                                        if (!string.IsNullOrEmpty(std1))
                                        {
                                            string[] tmpStrArr1 = std1.Split('\n',
                                                StringSplitOptions.RemoveEmptyEntries);
                                            if (tmpStrArr1.Length > 0)
                                            {
                                                foreach (var str2 in tmpStrArr1)
                                                {
                                                    if (!string.IsNullOrEmpty(str2) && str2.Contains("ether"))
                                                    {
                                                        var regex = "([0-9a-fA-F]{2})(([/\\s:-][0-9a-fA-F]{2}){5})";
                                                        var mac = Regex.Match(str2, regex);
                                                        if (mac.Value.Trim().Length == 17)
                                                        {
                                                            NetWorkStat.Mac = mac.Value.ToUpper().Replace(":", "-")
                                                                .Trim();
                                                            isFound = true;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                new Thread(new ThreadStart(delegate
                {
                    try
                    {
                        GetInfo();
                    }
                    catch
                    {
                        // ignored
                    }
                })).Start();
            }
        }


        private static long _perSendBytes = 0;
        private static long _perRecvBytes = 0;

        public static NetWorkStat NetWorkStat = new NetWorkStat();

        public static NetWorkStat GetNetworkStat()
        {
            lock (lockObj)
            {
                return NetWorkStat;
            }
        }
    }
}