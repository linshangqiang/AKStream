using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;
using LibCommon.Structs;
using LibCommon.Structs.GB28181;
using Newtonsoft.Json;

namespace LibCommon
{
    /// <summary>
    /// 常用工具类
    /// </summary>
    public static class UtilsHelper
    {
        /// <summary>
        /// 获取ssrc值
        /// </summary>
        /// <param name="pushMediaInfoToCreateSsrc"></param>
        /// <returns></returns>
        public static KeyValuePair<string, uint> GetSSRCValue(PushMediaInfoToCreateSSRC pushMediaInfoToCreateSsrc)
        {
            var tmpSSRC = pushMediaInfoToCreateSsrc.MediaServerId + pushMediaInfoToCreateSsrc.MediaServerIp +
                          pushMediaInfoToCreateSsrc.Vhost + pushMediaInfoToCreateSsrc.App +
                          pushMediaInfoToCreateSsrc.SipDeviceId +
                          pushMediaInfoToCreateSsrc.SipChannelId + pushMediaInfoToCreateSsrc.PushStreamSocketType;
            if (!string.IsNullOrEmpty(pushMediaInfoToCreateSsrc.StartTime)) //用于回放流
            {
                tmpSSRC += pushMediaInfoToCreateSsrc.StartTime + "-";
            }

            if (!string.IsNullOrEmpty(pushMediaInfoToCreateSsrc.Endtime)) //用于回放流
            {
                tmpSSRC += pushMediaInfoToCreateSsrc.Endtime;
            }


            var uintSSRC = CRC32Helper.GetCRC32(tmpSSRC);
            var stringSSRC = string.Format("{0:X8}", uintSSRC);
            KeyValuePair<string, uint> tmpKeyValuePair = new KeyValuePair<string, uint>(stringSSRC, uintSSRC);
            return tmpKeyValuePair;
        }

        /// <summary>
        /// 判断是否为奇数
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static bool IsOdd(int num)
        {
            return (num & 1) == 1;
        }

        /// <summary>
        /// 获取MD5加密码值
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Md5(string str)
        {
            try
            {
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                byte[] bytValue, bytHash;
                bytValue = System.Text.Encoding.UTF8.GetBytes(str);
                bytHash = md5.ComputeHash(bytValue);
                md5.Clear();
                string sTemp = "";
                for (int i = 0; i < bytHash.Length; i++)
                {
                    sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
                }

                str = sTemp.ToLower();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return str;
        }

        /// <summary>
        /// XML转类实例
        /// </summary>
        /// <param name="xmlBody"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T XMLToObject<T>(XElement xmlBody)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            return (T) xmlSerializer.Deserialize(xmlBody.CreateReader());
        }

        /// <summary>
        /// 生成一个新的序列id
        /// </summary>
        /// <returns></returns>
        public static int CreateNewCSeq()
        {
            var r = new Random();
            return r.Next(1, ushort.MaxValue);
        }

        /// <summary>
        /// 通过mac地址获取ip地址
        /// </summary>
        /// <param name="mac"></param>
        /// <param name="getIPV6"></param>
        /// <returns></returns>
        public static IPInfo GetIpAddressByMacAddress(string mac, bool getIPV6 = false)
        {
            bool found = false;
            string tmpMac = mac.Replace("-", "").Replace(":", "").ToUpper().Trim();
            IPInfo ipInfo = new IPInfo();
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in nics)
            {
                if (found) break;
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                    adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    string macadd = adapter.GetPhysicalAddress().ToString();
                    if (macadd.ToUpper().Trim().Equals(tmpMac))
                    {
                        //获取以太网卡网络接口信息
                        IPInterfaceProperties ip = adapter.GetIPProperties();
                        //获取单播地址集
                        UnicastIPAddressInformationCollection ipCollection = ip.UnicastAddresses;
                        foreach (UnicastIPAddressInformation ipadd in ipCollection)
                        {
                            if (ipadd.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                //判断是否为ipv4
                                ipInfo.IpV4 = ipadd.Address.ToString().Trim();
                            }

                            if (getIPV6)
                            {
                                if (ipadd.Address.AddressFamily == AddressFamily.InterNetworkV6)
                                {
                                    //判断是否为ipv6
                                    ipInfo.IpV6 = ipadd.Address.ToString().Trim();
                                }
                            }

                            if (getIPV6)
                            {
                                if (!string.IsNullOrEmpty(ipInfo.IpV4) && !string.IsNullOrEmpty(ipInfo.IpV6))
                                {
                                    found = true;
                                    break;
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(ipInfo.IpV4))
                                {
                                    found = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (found)
            {
                return ipInfo;
            }

            return null!;
        }

        /// <summary>
        /// 写入Json配置文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool WriteJsonConfig<T>(string filePath, T obj)
        {
            try
            {
                var jsonStr = JsonHelper.ToJson(obj!, Formatting.Indented);
                File.WriteAllText(filePath, jsonStr);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 读取json配置文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static object ReadJsonConfig<T>(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    string tmpStr = File.ReadAllText(filePath);
                    return JsonHelper.FromJson<T>(tmpStr)!;
                }
                catch
                {
                    return null!;
                }
            }

            return null!;
        }


        /// <summary>
        /// 是否为Url
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsUrl(string str)
        {
            try
            {
                string Url = @"^http(s)?://([\w-]+\.)+[\w-]+(:\d*)?(/[\w- ./?%&=]*)?$";
                return Regex.IsMatch(str, Url);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>  
        /// 将 DateTime时间格式转换为Unix时间戳格式  
        /// </summary>  
        /// <param name="time">时间</param>  
        /// <returns>long</returns>  
        public static long ConvertDateTimeToInt(DateTime time)
        {
            DateTime time2 = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));
            long timeStamp = (time.Ticks - time2.Ticks) / 10000; //除10000调整为13位     
            return timeStamp;
        }

        /// <summary>  
        /// 将Unix时间戳格式 转换为DateTime时间格式
        /// </summary>  
        /// <param name="time">时间</param>  
        /// <returns>long</returns>  
        public static DateTime ConvertDateTimeToInt(long time)
        {
            DateTime time2 = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));
            DateTime dateTime = time2.AddSeconds(time);
            return dateTime;
        }

        /// <summary>
        /// 正则获取内容
        /// </summary>
        /// <param name="str"></param>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string GetValue(string str, string s, string e)
        {
            Regex rg = new Regex("(?<=(" + s + "))[.\\s\\S]*?(?=(" + e + "))",
                RegexOptions.Multiline | RegexOptions.Singleline);
            return rg.Match(str).Value;
        }


        /// <summary>
        /// 获取两个时间差的毫秒数
        /// </summary>
        /// <param name="starttime"></param>
        /// <param name="endtime"></param>
        /// <returns></returns>
        public static long GetTimeGoneMilliseconds(DateTime starttime, DateTime endtime)
        {
            TimeSpan ts = endtime.Subtract(starttime);
            return (long) ts.TotalMilliseconds;
        }

        /// <summary>
        /// 获取时间戳(毫秒级)
        /// </summary>
        /// <returns></returns>
        public static long GetTimeStampMilliseconds()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// 检测是否为ip 地址
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsIpAddr(string ip)
        {
            return Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }


        /// <summary>
        /// 生成guid
        /// </summary>
        /// <returns></returns>
        public static string? CreateGUID()
        {
            return Guid.NewGuid().ToString("D");
        }

        /// <summary>
        /// 是否为GUID
        /// </summary>
        /// <param name="strSrc"></param>
        /// <returns></returns>
        public static bool IsUUID(string strSrc)
        {
            if (String.IsNullOrEmpty(strSrc))
            {
                return false;
            }

            bool _result = false;
            try
            {
                Guid _t = new Guid(strSrc);
                _result = true;
            }
            catch
            {
            }

            return _result;
        }


        /// <summary>
        /// 结束自己
        /// </summary>
        public static void KillSelf()
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// 获取pid
        /// </summary>
        /// <param name="processName"></param>
        /// <returns></returns>
        public static int GetProcessPid(string fileName)
        {
            Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(fileName));
            if (processes.Length > 0)
            {
                if (!processes[0].HasExited)
                {
                    return processes[0].Id;
                }
            }

            return -1;
        }


        /// <summary>
        /// 获取随机字符串
        /// </summary>
        /// <returns></returns>
        public static string generalGuid()
        {
            Random rand = new Random((int) DateTime.Now.Ticks);
            string random_str = "";
            for (int i = 0; i < 6; ++i)
            {
                for (int j = 0; j < 8; j++)
                    switch (rand.Next() % 2)
                    {
                        case 1:
                            random_str += (char) ('A' + rand.Next() % 26);
                            break;
                        default:
                            random_str += (char) ('0' + rand.Next() % 10);
                            break;
                    }

                if (i < 5)
                    random_str += "-";
            }

            return random_str;
        }

        /// <summary>
        /// 替换#开头的所有行为;开头
        /// </summary>
        /// <param name="filePath"></param>
        public static void ReplaceSharpWord(string filePath)
        {
            if (File.Exists(filePath))
            {
                var list = File.ReadAllLines(filePath).ToList();
                var tmp_list = new List<string>();
                foreach (var str in list)
                {
                    if (!str.StartsWith('#'))
                    {
                        tmp_list.Add(str);
                    }
                    else
                    {
                        int index = str.IndexOf("#", StringComparison.Ordinal);
                        tmp_list.Add(str.Remove(index, index).Insert(index, ";"));
                    }
                }

                File.WriteAllLines(filePath, tmp_list);
            }
        }


        /// <summary>
        /// 删除List<T>中null的记录
        /// </summary>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        public static void RemoveNull<T>(List<T> list)
        {
            // 找出第一个空元素 O(n)
            int count = list.Count;
            for (int i = 0; i < count; i++)
                if (list[i] == null)
                {
                    // 记录当前位置
                    int newCount = i++;

                    // 对每个非空元素，复制至当前位置 O(n)
                    for (; i < count; i++)
                        if (list[i] != null)
                            list[newCount++] = list[i];

                    // 移除多余的元素 O(n)
                    list.RemoveRange(newCount, count - newCount);
                    break;
                }
        }
    }
}