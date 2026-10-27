using System;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

public sealed class Computerinfo
{
	public long DiskFreeSpace { get; private set; }

	public long InstalledMemory { get; private set; }

	public string Manufacturer { get; private set; }

	public string Model { get; private set; }

	public Computerinfo()
	{
		foreach (ManagementObject instance in new ManagementClass("Win32_ComputerSystem").GetInstances())
		{
			Manufacturer = instance["Manufacturer"].ToString();
			Model = instance["Model"].ToString();
		}
		if (GetPhysicallyInstalledSystemMemory(out var totalMemoryInKilobytes))
		{
			InstalledMemory = totalMemoryInKilobytes;
		}
		else
		{
			LogHelper.Instance.Log("[{0}] Unable to obtain memory: GLE = {1}", GetType().Name, Marshal.GetLastWin32Error());
		}
		DriveInfo[] drives = DriveInfo.GetDrives();
		foreach (DriveInfo driveInfo in drives)
		{
			if (driveInfo.Name.StartsWith("c", StringComparison.CurrentCultureIgnoreCase) && driveInfo.IsReady)
			{
				DiskFreeSpace = driveInfo.TotalFreeSpace;
			}
		}
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetPhysicallyInstalledSystemMemory(out long totalMemoryInKilobytes);
}
