using System;
using System.Diagnostics;
using System.IO;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Kernel;

internal static class VersionFunctions
{
	[KernelFunction(Name = "kernel.fileversion")]
	internal static string FileVersion(string filePath)
	{
		if (File.Exists(filePath))
		{
			return FileVersionInfo.GetVersionInfo(filePath).FileVersion;
		}
		return null;
	}

	[KernelFunction(Name = "kernel.osversion")]
	internal static string OSVersion()
	{
		return Environment.OSVersion.ToString();
	}

	[KernelFunction(Name = "kernel.iswindows7")]
	internal static bool IsWindows7()
	{
		if (Environment.OSVersion.Version.Major == 6)
		{
			return Environment.OSVersion.Version.Minor == 1;
		}
		return false;
	}
}
