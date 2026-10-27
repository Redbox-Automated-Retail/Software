using System.Management;

namespace Redbox.UpdateManager.Kernel;

public static class SystemRestoreFunctions
{
	[KernelFunction(Name = "kernel.systemrestoredisable")]
	internal static void Disable(string drive)
	{
		ManagementScope scope = new ManagementScope("\\\\localhost\\root\\default");
		ManagementPath path = new ManagementPath("SystemRestore");
		ObjectGetOptions options = new ObjectGetOptions();
		ManagementClass managementClass = new ManagementClass(scope, path, options);
		ManagementBaseObject methodParameters = managementClass.GetMethodParameters("Disable");
		methodParameters["Drive"] = drive;
		managementClass.InvokeMethod("Disable", methodParameters, null);
	}

	[KernelFunction(Name = "kernel.systemrestoreenable")]
	internal static void Enable(string drive)
	{
		ManagementScope scope = new ManagementScope("\\\\localhost\\root\\default");
		ManagementPath path = new ManagementPath("SystemRestore");
		ObjectGetOptions options = new ObjectGetOptions();
		ManagementClass managementClass = new ManagementClass(scope, path, options);
		ManagementBaseObject methodParameters = managementClass.GetMethodParameters("Enable");
		methodParameters["Drive"] = drive;
		managementClass.InvokeMethod("Enable", methodParameters, null);
	}
}
