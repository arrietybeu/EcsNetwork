using System;

namespace arriety.utils
{
    public static class DeviceInfo
    {
        public static int MemorySizeMB { get; private set; }
        public static string DeviceName { get; private set; }
        public static string Platform { get; private set; }

        static DeviceInfo()
        {
            Init();
        }

        private static void Init()
        {
            DeviceName = Environment.MachineName;
            Platform = Environment.OSVersion.Platform.ToString();

            var computerInfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
            ulong memoryBytes = computerInfo.TotalPhysicalMemory;
            MemorySizeMB = (int)(memoryBytes / (1024 * 1024));
        }
    }
}
