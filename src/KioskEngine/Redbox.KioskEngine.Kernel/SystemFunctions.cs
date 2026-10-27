using System;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Kernel;

internal static class SystemFunctions
{
	[KernelFunction(Name = "kernel.systemrenamecomputer")]
	internal static void RenameComputer(string name)
	{
		using ManagementClass managementClass = new ManagementClass("Win32_ComputerSystem");
		using ManagementObjectCollection managementObjectCollection = managementClass.GetInstances();
		foreach (ManagementObject item in managementObjectCollection)
		{
			using ManagementBaseObject managementBaseObject = item.GetMethodParameters("Rename");
			managementBaseObject.SetPropertyValue("Name", name);
			item.InvokeMethod("Rename", managementBaseObject, new InvokeMethodOptions());
		}
	}

	[KernelFunction(Name = "kernel.trimworkingset")]
	internal static bool TrimWorkingSet()
	{
		return SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
	}

	[KernelFunction(Name = "kernel.setworkingsetsize")]
	internal static bool SetWorkingSetSize(int minimumSizeInBytes, int maximumSizeInBytes)
	{
		return SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, minimumSizeInBytes, maximumSizeInBytes);
	}

	[DllImport("kernel32.dll")]
	private static extern bool SetProcessWorkingSetSize(IntPtr hProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);
}
