using System;
using System.Management;
using Microsoft.Win32;

namespace ReverseScroll
{
    internal static class Program
    {
        private static void Main()
        {
            Console.WriteLine("Applying registry magic!");
            Console.WriteLine();

            using var searcher = new ManagementObjectSearcher(@"SELECT * FROM Win32_PnPEntity WHERE PNPClass = 'Mouse'");
            using var mgmtObjs = searcher.Get();

            foreach (var obj in mgmtObjs)
            {
                var device = Device.From(obj);
                device.ReverseScroll();
            }

            Console.WriteLine("Press any key to quit...");
            Console.ReadKey();
        }
    }

    class Device
    {
        public string Type { get; init; }
        public string Path { get; init; }

        public static Device From(ManagementBaseObject mgmtObject)
        {
            var deviceId = (string)mgmtObject.GetPropertyValue("DeviceID");
            var splitDeviceId = deviceId.Split(@"\");

            return new Device
            {
                Type = splitDeviceId[0],
                Path = splitDeviceId[1] + @"\" + splitDeviceId[2]
            };
        }

        public void ReverseScroll()
        {
            if (IsPhysicalMouse)
            {
                Console.WriteLine($"Setting FlipFlopWheel for {Path}...");

                using var key = Registry.LocalMachine.OpenSubKey(@$"SYSTEM\CurrentControlSet\Enum\HID\{Path}\Device Parameters", true);
                key.SetValue("FlipFlopWheel", 1, RegistryValueKind.DWord);

                Console.WriteLine("Done");
                Console.WriteLine();
            }
        }

        private bool IsPhysicalMouse => "HID".Equals(Type) && Path.StartsWith("VID_");
    }
}