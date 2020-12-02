﻿using System;
using System.IO;

namespace LibCommon
{
    public static class LibCommon
    {
     
        public static string BasePath = AppDomain.CurrentDomain.BaseDirectory + "/";
        public static string ConfigPath = BasePath + "Config/";
        
       
        static LibCommon()
        {
            
            if (!Directory.Exists(ConfigPath))//如果配置文件目录不存在，则创建目录
            {
                Directory.CreateDirectory(ConfigPath);
            }
            //初始化错误代码
            ErrorMessage.Init();
        }
    }
}