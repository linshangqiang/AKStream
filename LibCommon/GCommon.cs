using System;
using System.IO;

namespace LibCommon
{
    public static class GCommon
    {
        public static string BaseStartPath = Environment.CurrentDirectory; //程序启动的目录
        public static string BaseStartFullPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;//程序启动的全路径
        public static string? WorkSpacePath = AppDomain.CurrentDomain.BaseDirectory; //程序运行的目录
        public static string? WorkSpaceFullPath = Environment.GetCommandLineArgs()[0];//程序运行的全路径
        public static string? CommandLine = Environment.CommandLine;//程序启动命令
        public static string ConfigPath = BaseStartPath + "/Config/";
        

        static GCommon()
        {
            if (!Directory.Exists(ConfigPath)) //如果配置文件目录不存在，则创建目录
            {
                Directory.CreateDirectory(ConfigPath);
            }

            //初始化错误代码
            ErrorMessage.Init();
        }
    }
}