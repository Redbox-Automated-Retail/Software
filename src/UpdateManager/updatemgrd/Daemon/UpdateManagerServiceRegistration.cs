#if !NETFRAMEWORK
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using Microsoft.Win32;

namespace Redbox.UpdateManager.Daemon;

// .NET 10 replacement for UpdateManagerServiceInstaller (System.Configuration.Install does not exist on .NET):
// registers updatemgrd$default with the same settings through the Win32 service API.
internal static class UpdateManagerServiceRegistration
{
	private const string ServiceName = "updatemgrd$default";

	private const string DisplayName = "Redbox Store Manager Service";

	private const string Description = "Manage updates and run scheduled tasks.";

	private const uint SC_MANAGER_ALL_ACCESS = 0xF003F;

	private const uint SERVICE_ALL_ACCESS = 0xF01FF;

	private const uint SERVICE_WIN32_OWN_PROCESS = 0x10;

	private const uint SERVICE_INTERACTIVE_PROCESS = 0x100;

	private const uint SERVICE_AUTO_START = 2;

	private const uint SERVICE_ERROR_NORMAL = 1;

	private const uint SERVICE_CONFIG_DESCRIPTION = 1;

	public static void Install(bool forceStart)
	{
		string imagePath = "\"" + System.Environment.ProcessPath + "\"";
		IntPtr manager = OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);
		if (manager == IntPtr.Zero)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		try
		{
			// ServicesDependedOn = { "Tcpip" }; null account = LocalSystem (ServiceProcessInstaller.Account).
			IntPtr service = CreateService(manager, ServiceName, DisplayName, SERVICE_ALL_ACCESS, SERVICE_WIN32_OWN_PROCESS, SERVICE_AUTO_START, SERVICE_ERROR_NORMAL, imagePath, null, IntPtr.Zero, "Tcpip\0\0", null, null);
			if (service == IntPtr.Zero)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
			try
			{
				SERVICE_DESCRIPTION description = new SERVICE_DESCRIPTION { lpDescription = Description };
				ChangeServiceConfig2(service, SERVICE_CONFIG_DESCRIPTION, ref description);
			}
			finally
			{
				CloseServiceHandle(service);
			}
		}
		finally
		{
			CloseServiceHandle(manager);
		}
		// As UpdateManagerServiceInstaller.OnAfterInstall: mark the service interactive, optionally start it.
		string keyName = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\" + ServiceName;
		int type = (int)Registry.GetValue(keyName, "Type", 0)!; // the service key exists right after CreateService
		Registry.SetValue(keyName, "Type", type | (int)SERVICE_INTERACTIVE_PROCESS);
		if (forceStart)
		{
			using ServiceController controller = new ServiceController(ServiceName);
			controller.Start();
		}
	}

	public static void Uninstall()
	{
		IntPtr manager = OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);
		if (manager == IntPtr.Zero)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		try
		{
			IntPtr service = OpenService(manager, ServiceName, SERVICE_ALL_ACCESS);
			if (service == IntPtr.Zero)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
			try
			{
				if (!DeleteService(service))
				{
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}
			}
			finally
			{
				CloseServiceHandle(service);
			}
		}
		finally
		{
			CloseServiceHandle(manager);
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct SERVICE_DESCRIPTION
	{
		public string lpDescription;
	}

	[DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
	private static extern IntPtr OpenSCManager(string? machineName, string? databaseName, uint desiredAccess);

	[DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
	private static extern IntPtr CreateService(IntPtr manager, string serviceName, string displayName, uint desiredAccess, uint serviceType, uint startType, uint errorControl, string binaryPathName, string? loadOrderGroup, IntPtr tagId, string dependencies, string? serviceStartName, string? password);

	[DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
	private static extern IntPtr OpenService(IntPtr manager, string serviceName, uint desiredAccess);

	[DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
	private static extern bool ChangeServiceConfig2(IntPtr service, uint infoLevel, ref SERVICE_DESCRIPTION info);

	[DllImport("advapi32.dll", SetLastError = true)]
	private static extern bool DeleteService(IntPtr service);

	[DllImport("advapi32.dll", SetLastError = true)]
	private static extern bool CloseServiceHandle(IntPtr handle);
}
#endif
