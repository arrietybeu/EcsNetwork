namespace arriety.ecs.components
{
    public class DeviceInfoComponent : IComponent
    {
        public int EntityId { get; set; }
        
        public string Platform { get; set; } = "";
        public int MemorySizeMB { get; set; }
        public string DeviceName { get; set; } = "";
        
        public void InitializeFromEnvironment()
        {
            DeviceName = Environment.MachineName;
            Platform = Environment.OSVersion.Platform.ToString();

            var computerInfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
            ulong memoryBytes = computerInfo.TotalPhysicalMemory;
            MemorySizeMB = (int)(memoryBytes / (1024 * 1024));
        }
    }
} 