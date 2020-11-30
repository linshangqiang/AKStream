using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LibCommon
{
    public static class UtilsHelper
    {
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
            DateTime Time = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));
            long TimeStamp = (time.Ticks - Time.Ticks) / 10000; //除10000调整为13位     
            return TimeStamp;
        }

        /// <summary>  
        /// 将Unix时间戳格式 转换为DateTime时间格式
        /// </summary>  
        /// <param name="time">时间</param>  
        /// <returns>long</returns>  
        public static DateTime ConvertDateTimeToInt(long time)
        {
            DateTime Time = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));
            DateTime dateTime = Time.AddSeconds(time);
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
        public static string? CreateUuid()
        {
            return Guid.NewGuid().ToString("D");
        }

        /// <summary>
        /// 是否为GUID
        /// </summary>
        /// <param name="strSrc"></param>
        /// <returns></returns>
        public static bool IsUuidByError(string strSrc)
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