using System;
using System.Diagnostics;

namespace LibSystemInfo
{
    public static class MemoryLinuxValue
    {
        private static MemoryInfo _memoryInfo = new MemoryInfo();

        /// <summary>
        /// 获取linux系统总物理内存使用情况(总量/已使用/未使用)(返回单位MB)
        /// </summary>
        /// <returns></returns>
        public static MemoryInfo GetMemoryStatus()
        {
            try
            {
                string output = "";
                var info = new ProcessStartInfo();
                info.FileName = "/bin/bash";
                info.Arguments = "-c \"free -m\"";
                info.RedirectStandardOutput = true;
                using (var process = Process.Start(info))
                {
                    output = process.StandardOutput.ReadToEnd();
                }

                if (string.IsNullOrWhiteSpace(output))
                {
                    return _memoryInfo;
                }

                var lines = output.Trim().Split('\n');
                var memory = lines[1].Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                _memoryInfo.Total = ulong.Parse(memory[1]);
                _memoryInfo.Used = ulong.Parse(memory[2]);
                _memoryInfo.Free = _memoryInfo.Total - _memoryInfo.Used;
                _memoryInfo.FreePercent =
                    Math.Round(
                        double.Parse(_memoryInfo.Used.ToString()) * 100.00 / double.Parse(_memoryInfo.Total.ToString()),
                        3);
                return _memoryInfo;
            }
            catch (Exception ex)
            {
                return _memoryInfo;
            }
        }
    }
}