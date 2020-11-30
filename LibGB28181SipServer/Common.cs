using System;
using System.IO;
using System.Threading;
using LibSystemInfo;

namespace LibGB28181SipProxy
{
    public static class Common
    {
        private static SipServerConfig _sipServerConfig = null;
        private static string _sipServerConfigPath = LibCommon.LibCommon.ConfigPath + "SipServerConfig.json";


        private static SipServerConfig initSipServerConfig()
        {
            SystemInfo systemInfo = new SystemInfo();
            string macAddr = "";
            string ipAddr = "";
            var sys = systemInfo.GetSystemInfoObject();
            int i = 0;
            while (sys == null || sys.NetWorkStat == null || i < 50)
            {
                i++;
                Thread.Sleep(20);
            }

            if (sys != null && sys.NetWorkStat != null)
            {
                macAddr = sys.NetWorkStat.Mac;
                systemInfo.Dispose();
            }

            if (string.IsNullOrEmpty(macAddr))
            {
                return null; //mac 地址没找到了，报错出去
            }

            return null;
        }

        /// <summary>
        /// 返加0说明文件存在并正确加载
        /// 返回1说明文件不存在已新建
        /// 返回-1说明文件创建或读取异常失败
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static int readSipServerConfig(string filePath)
        {
            if (!File.Exists(filePath))
            {
                //配置文件不存在,要创建
            }
            else
            {
                try
                {
                }
                catch (Exception ex)
                {
                }
            }

            return 1;
        }

        static Common()
        {
        }
    }
}