using System;
using System.Diagnostics;
using System.Reflection;
using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

public static class FileVersionFunctions
{
	[KernelFunction(Name = "kernel.getfileversion")]
	internal static string GetFileVersion(string fileName)
	{
		try
		{
			return FileVersionInfo.GetVersionInfo(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(fileName)).FileVersion;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error getting file version.", e);
			return string.Empty;
		}
	}

	[KernelFunction(Name = "kernel.getfileversiontable")]
	internal static LuaTable GetFileVersionInfo(string fileName)
	{
		try
		{
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(fileName));
			return new LuaTable(KernelService.Instance.LuaRuntime)
			{
				["file_version"] = versionInfo.FileVersion,
				["company_name"] = versionInfo.CompanyName,
				["comments"] = versionInfo.Comments,
				["build_version"] = versionInfo.FileBuildPart,
				["major_version"] = versionInfo.FileMajorPart,
				["minor_version"] = versionInfo.FileMinorPart,
				["private_version"] = versionInfo.FilePrivatePart,
				["copyright"] = versionInfo.LegalCopyright
			};
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error getting file version.", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.getdotnetassemblyversion")]
	internal static string GetAssemblyVersion(string file)
	{
		string result = string.Empty;
		try
		{
			result = AssemblyInfoHelper.GetVersion(Assembly.LoadFile(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(file)));
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error getting file version.", e);
		}
		return result;
	}

	[KernelFunction(Name = "kernel.getfrontendversion")]
	internal static string GetFrontendVersion()
	{
		return Assembly.GetExecutingAssembly().GetName().Version.ToString();
	}
}
