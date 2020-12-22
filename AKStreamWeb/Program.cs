using System;
using AKStreamWeb.Misc;
using LibCommon;
using LibGB28181SipServer;
using LibLogger;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace AKStreamWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Common.Init();
            CreateHostBuilder(args).Build().Run();
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>().UseUrls($"http://*:{Common.AkStreamWebConfig.WebApiPort}"); });
    }
}