using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace TONWallet
{
    public class HardwareManager
    {
        public static string? GetWalletPath()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) return null; // todo linux

            // Get a list of connected USB devices
            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_USBHub"))
                collection = searcher.Get();

            // Iterate through each USB device
            foreach (var device in collection)
            {
                var deviceID = device.GetPropertyValue("DeviceID")?.ToString();

                // Ensure the device ID is not null
                if (deviceID != null)
                {
                    // Get the drive letter for the USB device
                    var driveLetter = GetDriveLetterFromDeviceID(deviceID);

                    // Check if the drive is ready and accessible
                    if (!string.IsNullOrEmpty(driveLetter) && Directory.Exists(driveLetter))
                    {
                        // Check if the file exists in the root directory of the USB drive
                        var filePath = Path.Combine(driveLetter, "wallet.json");
                        if (File.Exists(filePath))
                        {
                            return filePath;
                        }
                    }
                }
            }
            return null;
        }

        static string GetDriveLetterFromDeviceID(string deviceID)
        {
            var indexOfInstanceID = deviceID.IndexOf("InstanceId_", StringComparison.Ordinal);
            if (indexOfInstanceID == -1)
                return null;

            var instanceID = deviceID.Substring(indexOfInstanceID + "InstanceId_".Length);
            var driveQuery = new ManagementObjectSearcher(
                @"SELECT * FROM Win32_DiskDrive WHERE DeviceID LIKE '%" + instanceID + "'");
            foreach (var d in driveQuery.Get())
            {
                var drive = new ManagementObjectSearcher(
                    @"ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" + d["DeviceID"]
                    + "'} WHERE AssocClass = Win32_DiskDriveToDiskPartition");
                foreach (var assoc in drive.Get())
                {
                    var partition = new ManagementObjectSearcher(
                        @"ASSOCIATORS OF {Win32_DiskPartition.DeviceID='"
                        + assoc["DeviceID"]
                        + "'} WHERE AssocClass = Win32_LogicalDiskToPartition");
                    foreach (var p in partition.Get())
                    {
                        var driveLetter = p["Name"];
                        return driveLetter?.ToString();
                    }
                }
            }
            return null;
        }
    }
}
